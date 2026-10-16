using LostAndFound.Models;
using LostAndFound.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LostAndFound.QA;

[TestClass]
[DoNotParallelize]
public sealed class WorkflowAcceptanceTests
{
    private string _dir = null!;
    private string Store => Path.Combine(_dir, "reports.json");
    [TestInitialize] public void Start() { _dir = Path.Combine(Path.GetTempPath(), "lf-workflow-" + Guid.NewGuid()); Directory.CreateDirectory(_dir); StaffSessionService.SignOut(); }
    [TestCleanup] public void End() { StaffSessionService.SignOut(); Directory.Delete(_dir, true); }
    private static void Login() => Assert.IsTrue(StaffSessionService.SignIn("STAFF001", "Campus123!"));
    private PrototypeDataService Pair()
    {
        var data = new PrototypeDataService(Store);
        data.AddLostReport(new() { Name = "Samsung phone", Category = "Electronics", Location = "Library", Description = "Black phone", DateLost = DateTime.Today });
        data.AddFoundReport(new() { Name = "Samsung phone", Category = "Electronics", Location = "Library", Description = "Black phone", StorageLocation = "Desk", DateFound = DateTime.Today });
        return data;
    }
    private static ReviewRecord Approve(PrototypeDataService d) { Login(); return d.ReviewMatch(1, 1, true, "Same distinctive case and details"); }

    [TestMethod]
    public void FullWorkflow_ApprovesEvidenceReturnsBothReportsAndReloadsAudit()
    {
        var d = Pair(); var review = Approve(d);
        Assert.AreEqual(ItemReportStatus.Matched, d.LostReports[0].Status);
        Assert.IsEmpty(d.GenerateMatches());
        var claim = d.RecordClaim(review.Id, "Test claimant", "Correct unlock and case marks");
        var decided = d.DecideClaim(claim.Id, true, "Ownership confirmed in person");
        Assert.AreEqual("Approved", decided.Status); Assert.IsNotNull(decided.ReviewedAt);
        d.ReturnReviewedItem(1, "Collected at security desk");
        d.CloseReport("Lost", 1, "Handover complete"); d.CloseReport("Found", 1, "Handover complete");
        var reload = new PrototypeDataService(Store);
        Assert.AreEqual(ItemReportStatus.Closed, reload.LostReports[0].Status);
        Assert.AreEqual(ItemReportStatus.Closed, reload.FoundReports[0].Status);
        Assert.IsNotNull(reload.FoundReports[0].ReturnedAt);
        Assert.AreEqual("Correct unlock and case marks", reload.GetClaims()[0].Evidence);
        Assert.HasCount(10, reload.GetHistory());
        Assert.IsTrue(reload.GetHistory().All(x => x.StaffId == "STAFF001" && x.ChangedAt.Kind == DateTimeKind.Utc));
    }

    [TestMethod]
    public void StudentCannotMutateWorkflowOrReadPrivateEvidence()
    {
        var d = Pair(); Approve(d); d.RecordClaim(1, "Private claimant", "Secret code"); StaffSessionService.SignOut();
        var before = File.ReadAllText(Store);
        Action[] actions = [() => d.ReviewMatch(1, 1, false, "No"), () => d.RecordClaim(1, "X", "Y"),
            () => d.DecideClaim(1, true, "Yes"), () => d.ReturnReviewedItem(1, "Collected"),
            () => d.CloseReport("Found", 1, "Closed"), () => d.GetClaims(), () => d.GetReviews(), () => d.GetHistory()];
        foreach (var action in actions) Assert.ThrowsExactly<UnauthorizedAccessException>(action);
        Assert.AreEqual(before, File.ReadAllText(Store));
        var publicJson = System.Text.Json.JsonSerializer.Serialize(d.SearchReports(new()));
        Assert.DoesNotContain("Secret code", publicJson); Assert.DoesNotContain("Private claimant", publicJson);
    }

    [TestMethod]
    public void RejectedPairDoesNotReappearAndOtherCandidatesRemain()
    {
        var d = Pair(); d.AddFoundReport(new() { Name = "Second phone", Category = "Electronics", DateFound = DateTime.Today }); Login();
        var r = d.ReviewMatch(1, 1, false, "Different serial number");
        Assert.IsFalse(r.Approved); Assert.AreEqual(ItemReportStatus.UnderReview, d.FoundReports.Single(x => x.ID == 1).Status);
        Assert.IsFalse(d.GenerateMatches().Any(x => x.FoundReport.ID == 1));
        Assert.IsTrue(d.GenerateMatches().Any(x => x.FoundReport.ID == 2));
        Assert.IsFalse(new PrototypeDataService(Store).GenerateMatches().Any(x => x.FoundReport.ID == 1));
    }

    [TestMethod]
    public void ApprovedPairCannotBeAssignedToAnotherLostReport()
    {
        var d = Pair(); d.AddLostReport(new() { Name = "Other phone", Category = "Electronics", DateLost = DateTime.Today }); Approve(d);
        Assert.ThrowsExactly<InvalidOperationException>(() => d.ReviewMatch(2, 1, true, "Second owner"));
        Assert.HasCount(1, d.GetReviews());
    }

    [TestMethod]
    public void MatchApprovalDoesNotAllowReturnBeforeOwnershipApproval()
    {
        var d = Pair(); Approve(d);
        Assert.ThrowsExactly<InvalidOperationException>(() => d.ReturnReviewedItem(1, "No evidence"));
        Assert.ThrowsExactly<InvalidOperationException>(() => d.CloseReport("Found", 1, "No evidence"));
        Assert.IsNull(d.FoundReports[0].ReturnedAt);
    }

    [TestMethod]
    public void ClaimRejectionAllowsCorrectedEvidenceWithoutReopeningPair()
    {
        var d = Pair(); Approve(d); var c = d.RecordClaim(1, "Claimant A", "Insufficient details");
        d.DecideClaim(c.Id, false, "Cannot demonstrate ownership");
        Assert.AreEqual(ItemReportStatus.Matched, d.FoundReports[0].Status);
        Assert.ThrowsExactly<InvalidOperationException>(() => d.DecideClaim(c.Id, true, "Repeated"));
        var next = d.RecordClaim(1, "Claimant B", "Device unlock confirmed");
        d.DecideClaim(next.Id, true, "Verified"); Assert.AreEqual(ItemReportStatus.Claimed, d.LostReports[0].Status);
    }

    [TestMethod]
    public void DuplicateClaimDoesNotConsumeAnIdentifierOrWrite()
    {
        var d = Pair(); Approve(d); d.RecordClaim(1, "A", "Evidence"); var before = File.ReadAllText(Store);
        Assert.ThrowsExactly<InvalidOperationException>(() => d.RecordClaim(1, "B", "Evidence"));
        Assert.AreEqual(before, File.ReadAllText(Store)); Assert.HasCount(1, d.GetClaims());
    }

    [TestMethod]
    public void ReturnCannotBeRepeated()
    {
        var d = Pair(); Approve(d); d.RecordClaim(1, "A", "Evidence"); d.DecideClaim(1, true, "Verified"); d.ReturnReviewedItem(1, "Collected");
        var before = File.ReadAllText(Store);
        Assert.ThrowsExactly<InvalidOperationException>(() => d.ReturnReviewedItem(1, "Again"));
        Assert.AreEqual(before, File.ReadAllText(Store));
    }

    [TestMethod]
    public void MismatchedCategoryCannotBeApproved()
    {
        var d = Pair(); d.FoundReports[0].Category = "Wallet"; Login();
        Assert.ThrowsExactly<InvalidOperationException>(() => d.ReviewMatch(1, 1, true, "Wrong category"));
        Assert.IsEmpty(d.GetHistory());
    }

    [TestMethod]
    [DataRow("")][DataRow(" ")]
    public void EmptyReviewReasonCannotChangeStatus(string reason)
    {
        var d = Pair(); Login();
        Assert.ThrowsExactly<ArgumentException>(() => d.ReviewMatch(1, 1, true, reason));
        Assert.AreEqual(ItemReportStatus.Submitted, d.LostReports[0].Status);
    }

    [TestMethod]
    public void EmptyOwnershipEvidenceIsRejected()
    {
        var d = Pair(); Approve(d);
        Assert.ThrowsExactly<ArgumentException>(() => d.RecordClaim(1, "A", " "));
        Assert.ThrowsExactly<ArgumentException>(() => d.RecordClaim(1, " ", "Evidence"));
        Assert.IsEmpty(d.GetClaims());
    }

    [TestMethod]
    public void MissingReportsAndClaimsGiveControlledErrors()
    {
        var d = Pair(); Login();
        Assert.ThrowsExactly<ArgumentException>(() => d.ReviewMatch(9, 1, true, "Reason"));
        Assert.ThrowsExactly<ArgumentException>(() => d.DecideClaim(9, true, "Reason"));
        Assert.ThrowsExactly<ArgumentException>(() => d.RecordClaim(9, "A", "Evidence"));
        Assert.ThrowsExactly<ArgumentException>(() => d.CloseReport("Other", 1, "Reason"));
    }

    [TestMethod]
    public void WorkflowWriteFailureRollsBackStatusDecisionAndHistory()
    {
        var blocked = Path.Combine(_dir, "blocked"); Directory.CreateDirectory(blocked);
        // File.Exists(directory) is false, allowing an isolated in-memory setup before Save fails.
        var d = new PrototypeDataService(); d.AddLostReport(new() { Category = "Keys" }); d.AddFoundReport(new() { Category = "Keys" });
        var field = typeof(PrototypeDataService).GetField("_storagePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        field.SetValue(d, blocked); Login();
        var failure = Assert.Throws<Exception>(() => d.ReviewMatch(1, 1, true, "Verified"));
        Assert.IsTrue(failure is IOException or UnauthorizedAccessException);
        Assert.AreEqual(ItemReportStatus.Submitted, d.LostReports[0].Status);
        Assert.AreEqual(ItemReportStatus.Submitted, d.FoundReports[0].Status);
        Assert.IsEmpty(d.GetReviews()); Assert.IsEmpty(d.GetHistory());
    }

    [TestMethod]
    public void LegacySnapshotLoadsWithoutNewCollections()
    {
        File.WriteAllText(Store, "{\"Lost\":[],\"Found\":[]}"); var d = new PrototypeDataService(Store); Login();
        Assert.IsEmpty(d.GetReviews()); Assert.IsEmpty(d.GetClaims()); Assert.IsEmpty(d.GetHistory());
    }

    [TestMethod]
    public void SearchCombinesCategoryStatusLocationNameAndInclusiveDates()
    {
        var d = Pair(); d.AddFoundReport(new() { Name = "Wallet", Category = "Wallet", Location = "Library", DateFound = DateTime.Today });
        var rows = d.SearchReports(new("PHONE", "electronics", ItemReportStatus.Submitted, DateTime.Today, DateTime.Today, "library"));
        Assert.HasCount(2, rows); Assert.IsTrue(rows.All(x => x.Category == "Electronics"));
        Assert.IsEmpty(d.SearchReports(new(From: DateTime.Today.AddDays(1))));
        Assert.IsEmpty(d.SearchReports(new(To: DateTime.Today.AddDays(-1))));
        Assert.HasCount(1, d.SearchReports(new(Category: "Wallet")));
    }

    [TestMethod]
    public void SearchRejectsInvertedDatesAndReturnsSafeImmutableListings()
    {
        var d = Pair();
        Assert.ThrowsExactly<ArgumentException>(() => d.SearchReports(new(From: DateTime.Today, To: DateTime.Today.AddDays(-1))));
        var rows = d.SearchReports(new()); Assert.HasCount(2, rows);
        Assert.IsFalse(typeof(ReportListing).GetProperties().Any(x => x.Name.Contains("Evidence") || x.Name.Contains("Email") || x.Name.Contains("Claimant")));
    }

    [TestMethod]
    public void InvalidSignInClearsAnEarlierSession()
    {
        Login(); Assert.IsFalse(StaffSessionService.SignIn("invalid", "invalid"));
        Assert.IsFalse(StaffSessionService.IsAuthenticated); Assert.IsNull(StaffSessionService.CurrentStaffId);
    }

    [TestMethod]
    public void FailedClaimDecisionPreservesEvidenceAndPreviousStatus()
    {
        var d = Pair(); Approve(d); d.RecordClaim(1, "Claimant", "Verified identifying detail");
        var before = File.ReadAllText(Store); BlockWrites(d);
        var error = Assert.Throws<Exception>(() => d.DecideClaim(1, true, "Approved"));
        Assert.IsTrue(error is IOException or UnauthorizedAccessException);
        Assert.AreEqual("Submitted", d.GetClaims()[0].Status);
        Assert.AreEqual(ItemReportStatus.Matched, d.FoundReports[0].Status);
        Assert.HasCount(3, d.GetHistory()); Assert.AreEqual(before, File.ReadAllText(Store));
    }

    [TestMethod]
    public void FailedReturnPreservesBothReportStatesAndTimestamp()
    {
        var d = Pair(); Approve(d); d.RecordClaim(1, "Claimant", "Verified detail"); d.DecideClaim(1, true, "Approved");
        var before = File.ReadAllText(Store); BlockWrites(d);
        var error = Assert.Throws<Exception>(() => d.ReturnReviewedItem(1, "Collected"));
        Assert.IsTrue(error is IOException or UnauthorizedAccessException);
        Assert.AreEqual(ItemReportStatus.Claimed, d.LostReports[0].Status);
        Assert.AreEqual(ItemReportStatus.Claimed, d.FoundReports[0].Status);
        Assert.IsNull(d.FoundReports[0].ReturnedAt); Assert.HasCount(6, d.GetHistory());
        Assert.AreEqual(before, File.ReadAllText(Store));
    }

    private void BlockWrites(PrototypeDataService d)
    {
        var blocked = Path.Combine(_dir, "blocked"); Directory.CreateDirectory(blocked);
        typeof(PrototypeDataService).GetField("_storagePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(d, blocked);
    }

    [TestMethod]
    public void PublicIntakeCannotImpersonateStaffRegistration()
    {
        var d = new PrototypeDataService(); StaffSessionService.SignOut();
        var report = new FoundItemReport { Name = "Keys", Category = "Keys", Location = "Library", Description = "Three keys", DateFound = DateTime.Today, RegisteredByStaffID = 1, StorageLocation = "Staff-only room" };
        Assert.IsTrue(d.TrySubmitPublicFoundReport(report, out _));
        Assert.AreEqual(0, report.RegisteredByStaffID); Assert.AreEqual("Pending staff intake", report.StorageLocation);
    }
}
