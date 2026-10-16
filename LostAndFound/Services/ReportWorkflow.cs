using System.Text.Json;
using LostAndFound.Models;

namespace LostAndFound.Services;

// Immutable results keep private review data behind staff-only query methods.
public sealed record ReviewRecord(int Id, int LostId, int FoundId, bool Approved,
    string StaffId, string Reason, DateTime ReviewedAt);
public sealed record ClaimRecord(int Id, int ReviewId, string Claimant, string Evidence,
    string Status, string? StaffId, string? Reason, DateTime SubmittedAt, DateTime? ReviewedAt);
public sealed record AuditRecord(int Id, string Entity, int EntityId, string Before,
    string After, string StaffId, string Reason, DateTime ChangedAt);
public sealed record ReportFilter(string? Text = null, string? Category = null,
    ItemReportStatus? Status = null, DateTime? From = null, DateTime? To = null,
    string? Location = null);
public sealed record ReportListing(string Kind, int Id, string Name, string Category,
    string Location, DateTime Date, ItemReportStatus Status)
{
    public string Summary => $"{Kind} #{Id}: {Name} | {Category} | {Location} | {Date:yyyy-MM-dd} | {Status}";
}

public partial class PrototypeDataService
{
    private List<ReviewRecord> _reviews = [];
    private List<ClaimRecord> _claims = [];
    private List<AuditRecord> _history = [];

    private static string RequireStaff()
    {
        if (!StaffSessionService.IsAuthenticated || string.IsNullOrWhiteSpace(StaffSessionService.CurrentStaffId))
            throw new UnauthorizedAccessException("Sign in as staff to use this action.");
        return StaffSessionService.CurrentStaffId;
    }

    private ReportSnapshot CreateSnapshot() => new(LostReports.ToList(), FoundReports.ToList())
    { Reviews = _reviews.ToList(), Claims = _claims.ToList(), History = _history.ToList() };

    private void RestoreWorkflow(ReportSnapshot snapshot)
    {
        _reviews = snapshot.Reviews ?? [];
        _claims = snapshot.Claims ?? [];
        _history = snapshot.History ?? [];
    }

    // Every workflow change and its audit entries share the same atomic snapshot write.
    private T Change<T>(Func<T> action)
    {
        lock (_gate)
        {
            var reviews = _reviews.ToList(); var claims = _claims.ToList(); var history = _history.ToList();
            var lost = LostReports.Select(r => (Report:r, r.Status)).ToList();
            var found = FoundReports.Select(r => (Report:r, r.Status, r.ReturnedAt)).ToList();
            try { var result = action(); Save(); return result; }
            catch
            {
                _reviews = reviews; _claims = claims; _history = history;
                foreach (var x in lost) x.Report.Status = x.Status;
                foreach (var x in found) { x.Report.Status = x.Status; x.Report.ReturnedAt = x.ReturnedAt; }
                throw;
            }
        }
    }

    private void Audit(string entity, int id, string before, string after, string actor, string reason) =>
        _history.Add(new(_history.Count + 1, entity, id, before, after, actor, reason, DateTime.UtcNow));

    private static string Required(string? text, string field)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException($"Enter {field}.");
        if (text.Trim().Length > 4000) throw new ArgumentException($"{field} must be at most 4,000 characters.");
        return text.Trim();
    }

    public bool TrySubmitPublicFoundReport(FoundItemReport? report, out string error)
    {
        if (report is not null) { report.RegisteredByStaffID = 0; report.StorageLocation = "Pending staff intake"; }
        return TrySubmitFoundReport(report, out error);
    }

    public bool TryRegisterFoundReport(FoundItemReport? report, out string error)
    {
        RequireStaff();
        if (report is not null) report.RegisteredByStaffID = 1;
        return TrySubmitFoundReport(report, out error);
    }

    public ReviewRecord ReviewMatch(int lostId, int foundId, bool approve, string reason)
    {
        var actor = RequireStaff(); reason = Required(reason, "a review reason");
        return Change(() =>
        {
            var lost = LostReports.SingleOrDefault(x => x.ID == lostId) ?? throw new ArgumentException("Lost report not found.");
            var found = FoundReports.SingleOrDefault(x => x.ID == foundId) ?? throw new ArgumentException("Found report not found.");
            if (!IsEligible(lost.Status) || !IsEligible(found.Status)) throw new InvalidOperationException("Both reports must be active.");
            if (_matching.CalculateMatchScore(lost, found) <= 0) throw new InvalidOperationException("Choose a compatible suggested pair.");
            if (_reviews.Any(x => x.LostId == lostId && x.FoundId == foundId)) throw new InvalidOperationException("This pair has already been reviewed.");
            var record = new ReviewRecord(_reviews.Count + 1, lostId, foundId, approve, actor, reason, DateTime.UtcNow);
            _reviews.Add(record);
            var next = approve ? ItemReportStatus.Matched : ItemReportStatus.UnderReview;
            Audit("Lost", lostId, lost.Status.ToString(), next.ToString(), actor, reason);
            Audit("Found", foundId, found.Status.ToString(), next.ToString(), actor, reason);
            lost.Status = found.Status = next;
            return record;
        });
    }

    // Staff records claimant evidence during the ownership check, never on public listings.
    public ClaimRecord RecordClaim(int reviewId, string claimant, string evidence)
    {
        var actor = RequireStaff(); claimant = Required(claimant, "the claimant"); evidence = Required(evidence, "ownership evidence");
        return Change(() =>
        {
            var review = _reviews.SingleOrDefault(x => x.Id == reviewId && x.Approved) ?? throw new ArgumentException("Choose an approved match.");
            if (FoundReports.Single(x => x.ID == review.FoundId).Status != ItemReportStatus.Matched)
                throw new InvalidOperationException("This item is not awaiting an ownership claim.");
            if (_claims.Any(x => x.ReviewId == reviewId && x.Status != "Rejected"))
                throw new InvalidOperationException("This match already has an active claim.");
            var claim = new ClaimRecord(_claims.Count + 1, reviewId, claimant, evidence, "Submitted", null, null, DateTime.UtcNow, null);
            _claims.Add(claim); Audit("Claim", claim.Id, "None", "Submitted", actor, "Ownership evidence recorded");
            return claim;
        });
    }

    public ClaimRecord DecideClaim(int claimId, bool approve, string reason)
    {
        var actor = RequireStaff(); reason = Required(reason, "a decision reason");
        return Change(() =>
        {
            var i = _claims.FindIndex(x => x.Id == claimId);
            if (i < 0) throw new ArgumentException("Claim not found.");
            var claim = _claims[i];
            if (claim.Status != "Submitted") throw new InvalidOperationException("This claim has already been decided.");
            var review = _reviews.Single(x => x.Id == claim.ReviewId);
            var lost = LostReports.Single(x => x.ID == review.LostId);
            var found = FoundReports.Single(x => x.ID == review.FoundId);
            if (lost.Status != ItemReportStatus.Matched || found.Status != ItemReportStatus.Matched)
                throw new InvalidOperationException("The matched reports are no longer available for this decision.");
            var next = approve ? ItemReportStatus.Claimed : ItemReportStatus.Matched;
            var decided = claim with { Status = approve ? "Approved" : "Rejected", StaffId = actor, Reason = reason, ReviewedAt = DateTime.UtcNow };
            _claims[i] = decided;
            Audit("Claim", claimId, claim.Status, decided.Status, actor, reason);
            Audit("Lost", lost.ID, lost.Status.ToString(), next.ToString(), actor, reason);
            Audit("Found", found.ID, found.Status.ToString(), next.ToString(), actor, reason);
            lost.Status = found.Status = next;
            return decided;
        });
    }

    public void ReturnReviewedItem(int foundId, string reason)
    {
        var actor = RequireStaff(); reason = Required(reason, "a handover reason");
        Change(() =>
        {
            var found = FoundReports.SingleOrDefault(x => x.ID == foundId) ?? throw new ArgumentException("Found report not found.");
            if (found.Status is ItemReportStatus.Returned or ItemReportStatus.Closed)
                throw new InvalidOperationException("This item has already left the active workflow.");
            var review = _reviews.SingleOrDefault(x => x.FoundId == foundId && x.Approved);
            if (review is not null)
            {
                if (!_claims.Any(x => x.ReviewId == review.Id && x.Status == "Approved"))
                    throw new InvalidOperationException("Approve ownership evidence before returning a matched item.");
                var lost = LostReports.Single(x => x.ID == review.LostId);
                Audit("Lost", lost.ID, lost.Status.ToString(), "Returned", actor, reason);
                lost.Status = ItemReportStatus.Returned;
            }
            Audit("Found", found.ID, found.Status.ToString(), "Returned", actor, reason);
            found.Status = ItemReportStatus.Returned; found.ReturnedAt = DateTime.UtcNow;
            return true;
        });
    }

    public void CloseReport(string kind, int id, string reason)
    {
        var actor = RequireStaff(); reason = Required(reason, "a closure reason");
        Change(() =>
        {
            // A paired item closes only after handover so one side cannot strand a live claim.
            if (_reviews.Any(x => x.Approved && ((kind == "Lost" && x.LostId == id) || (kind == "Found" && x.FoundId == id))))
            {
                var status = kind == "Lost" ? LostReports.Single(x => x.ID == id).Status : FoundReports.Single(x => x.ID == id).Status;
                if (status != ItemReportStatus.Returned) throw new InvalidOperationException("Return the approved pair before closure.");
            }
            if (kind == "Lost")
            {
                var report = LostReports.SingleOrDefault(x => x.ID == id) ?? throw new ArgumentException("Lost report not found.");
                if (report.Status == ItemReportStatus.Closed) throw new InvalidOperationException("Report already closed.");
                Audit(kind, id, report.Status.ToString(), "Closed", actor, reason); report.Status = ItemReportStatus.Closed;
            }
            else if (kind == "Found")
            {
                var report = FoundReports.SingleOrDefault(x => x.ID == id) ?? throw new ArgumentException("Found report not found.");
                if (report.Status == ItemReportStatus.Closed) throw new InvalidOperationException("Report already closed.");
                Audit(kind, id, report.Status.ToString(), "Closed", actor, reason); report.Status = ItemReportStatus.Closed;
            }
            else throw new ArgumentException("Choose Lost or Found.");
            return true;
        });
    }

    public IReadOnlyList<ReviewRecord> GetReviews() { RequireStaff(); lock (_gate) return _reviews.ToArray(); }
    public IReadOnlyList<ClaimRecord> GetClaims() { RequireStaff(); lock (_gate) return _claims.ToArray(); }
    public IReadOnlyList<AuditRecord> GetHistory() { RequireStaff(); lock (_gate) return _history.ToArray(); }

    public IReadOnlyList<ReportListing> SearchReports(ReportFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        if (filter.From.HasValue && filter.To.HasValue && filter.From.Value.Date > filter.To.Value.Date)
            throw new ArgumentException("Start date must be on or before end date.");
        static bool Has(string value, string? query) => string.IsNullOrWhiteSpace(query) || value.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase);
        lock (_gate)
        {
            var rows = LostReports.Select(r => new ReportListing("Lost", r.ID, r.Name, r.Category, r.Location, r.DateLost, r.Status))
                .Concat(FoundReports.Select(r => new ReportListing("Found", r.ID, r.Name, r.Category, r.Location, r.DateFound, r.Status)));
            return rows.Where(r => Has(r.Name, filter.Text) && Has(r.Location, filter.Location)
                && (string.IsNullOrWhiteSpace(filter.Category) || string.Equals(r.Category, filter.Category.Trim(), StringComparison.OrdinalIgnoreCase))
                && (!filter.Status.HasValue || r.Status == filter.Status)
                && (!filter.From.HasValue || r.Date.Date >= filter.From.Value.Date)
                && (!filter.To.HasValue || r.Date.Date <= filter.To.Value.Date))
                .OrderByDescending(r => r.Date).ThenBy(r => r.Kind).ThenBy(r => r.Id).ToArray();
        }
    }
}
