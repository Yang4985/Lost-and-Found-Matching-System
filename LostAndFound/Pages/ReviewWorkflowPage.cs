using LostAndFound.Services;

namespace LostAndFound.Pages;

public sealed class ReviewWorkflowPage : ContentPage
{
    private readonly PrototypeDataService _data;
    private readonly VerticalStackLayout _body = new() { Spacing = 12, Padding = 24, MaximumWidthRequest = 900 };
    private readonly Picker _pairs = new() { Title = "Suggested lost and found pair" };
    private readonly Entry _reason = new() { Placeholder = "Reason for the review decision" };
    private readonly Picker _reviews = new() { Title = "Approved match for ownership evidence" };
    private readonly Entry _claimant = new() { Placeholder = "Claimant name or student reference" };
    private readonly Editor _evidence = new() { Placeholder = "Private ownership evidence checked by staff", HeightRequest = 90 };
    private readonly Picker _claims = new() { Title = "Claim awaiting a decision" };
    private readonly Entry _claimReason = new() { Placeholder = "Ownership decision reason" };
    private readonly Picker _returns = new() { Title = "Found item with approved ownership" };
    private readonly Picker _close = new() { Title = "Report to close" };
    private readonly Entry _handover = new() { Placeholder = "Handover or closure note" };
    private readonly Label _message = new() { TextColor = Color.FromArgb("#9E1B32") };
    private readonly Label _history = new() { LineBreakMode = LineBreakMode.WordWrap };
    private List<PrototypeMatch> _pairRows = [];
    private List<ReviewRecord> _reviewRows = [];
    private List<ClaimRecord> _claimRows = [];
    private List<ReportListing> _returnRows = [];
    private List<ReportListing> _closeRows = [];

    public ReviewWorkflowPage(PrototypeDataService data)
    {
        _data = data; Title = "Staff review and claims";
        AddHeading("Staff review and ownership");
        _body.Add(new Label { Text = "Review the suggested pair, record identifying evidence, then approve ownership before handover." });
        _body.Add(_message);
        AddHeading("1 Review a suggested match"); AddField(_pairs, "Suggested pair"); AddField(_reason, "Match decision reason");
        AddButton("Approve selected match", () => Review(true)); AddButton("Reject selected match", () => Review(false));
        AddHeading("2 Record private ownership evidence"); AddField(_reviews, "Approved match"); AddField(_claimant, "Claimant"); AddField(_evidence, "Private ownership evidence");
        AddButton("Save ownership claim", () =>
        {
            var review = Selected(_reviewRows, _reviews); _data.RecordClaim(review.Id, _claimant.Text, _evidence.Text);
            _claimant.Text = _evidence.Text = string.Empty;
        });
        AddHeading("3 Decide the claim"); AddField(_claims, "Ownership claim"); AddField(_claimReason, "Claim decision reason");
        AddButton("Approve ownership claim", () => Decide(true)); AddButton("Reject ownership claim", () => Decide(false));
        AddHeading("4 Record handover and closure"); AddField(_returns, "Item ready for handover"); AddField(_handover, "Handover or closure reason");
        AddButton("Confirm item returned", () => _data.ReturnReviewedItem(Selected(_returnRows, _returns).Id, _handover.Text));
        AddField(_close, "Report for closure");
        AddButton("Close selected report", () => { var r = Selected(_closeRows, _close); _data.CloseReport(r.Kind, r.Id, _handover.Text); });
        AddHeading("Recent status history"); _body.Add(_history);
        AddButton("Refresh review lists", () => { });
        Content = new ScrollView { Content = _body };
    }

    private void AddHeading(string text) => _body.Add(new Label { Text = text, FontSize = 23, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 12, 0, 0) });
    private void AddField(View field, string label)
    {
        _body.Add(new Label { Text = label }); SemanticProperties.SetDescription(field, label); _body.Add(field);
    }
    private void AddButton(string text, Action action)
    {
        var button = new Button { Text = text, MinimumHeightRequest = 48 };
        button.Clicked += (_, _) =>
        {
            try { action(); Refresh(); _message.Text = "Action completed and saved."; }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException or UnauthorizedAccessException)
            { _message.Text = ex.Message; }
        };
        _body.Add(button);
    }
    private static T Selected<T>(IReadOnlyList<T> rows, Picker picker) =>
        picker.SelectedIndex >= 0 && picker.SelectedIndex < rows.Count ? rows[picker.SelectedIndex] : throw new ArgumentException("Choose an entry first.");
    private void Review(bool approve) { var p = Selected(_pairRows, _pairs); _data.ReviewMatch(p.LostReport.ID, p.FoundReport.ID, approve, _reason.Text); }
    private void Decide(bool approve) => _data.DecideClaim(Selected(_claimRows, _claims).Id, approve, _claimReason.Text);

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!StaffSessionService.IsAuthenticated) { _body.IsVisible = false; await Shell.Current.GoToAsync("//staff-login"); return; }
        _body.IsVisible = true; Refresh();
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Discard evidence text and private lists when this view is left.
        _claimant.Text = _evidence.Text = _history.Text = string.Empty;
        _claimRows.Clear(); _claims.ItemsSource = null;
    }
    private void Refresh()
    {
        _pairRows = _data.GenerateMatches(); _pairs.ItemsSource = _pairRows.Select(x => x.Title).ToList();
        _reviewRows = _data.GetReviews().Where(x => x.Approved).ToList();
        _reviews.ItemsSource = _reviewRows.Select(x => $"Match #{x.Id}: lost #{x.LostId} / found #{x.FoundId}").ToList();
        _claimRows = _data.GetClaims().Where(x => x.Status == "Submitted").ToList();
        _claims.ItemsSource = _claimRows.Select(x => $"Claim #{x.Id}: {x.Claimant} / {x.Evidence}").ToList();
        _returnRows = _data.SearchReports(new(Status: Models.ItemReportStatus.Claimed)).Where(x => x.Kind == "Found").ToList();
        _returns.ItemsSource = _returnRows.Select(x => x.Summary).ToList();
        _closeRows = _data.SearchReports(new()).Where(x => x.Status is Models.ItemReportStatus.Submitted or Models.ItemReportStatus.UnderReview or Models.ItemReportStatus.Returned).ToList();
        _close.ItemsSource = _closeRows.Select(x => x.Summary).ToList();
        _history.Text = string.Join("\n\n", _data.GetHistory().TakeLast(20).Reverse().Select(x => $"{x.ChangedAt:u}  {x.Entity} #{x.EntityId}: {x.Before} to {x.After}\nStaff {x.StaffId}: {x.Reason}"));
    }
}
