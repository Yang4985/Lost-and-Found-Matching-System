using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Pages;

public sealed class SearchReportsPage : ContentPage
{
    private readonly PrototypeDataService _data;
    private readonly Entry _text = new() { Placeholder = "Item name" };
    private readonly Entry _location = new() { Placeholder = "Location" };
    private readonly Picker _category = new() { Title = "All categories" };
    private readonly Picker _status = new() { Title = "All statuses" };
    private readonly CheckBox _dates = new();
    private readonly DatePicker _from = new() { Date = DateTime.Today.AddDays(-30) };
    private readonly DatePicker _to = new() { Date = DateTime.Today };
    private readonly Label _results = new() { LineBreakMode = LineBreakMode.WordWrap };
    private readonly Label _message = new();
    public SearchReportsPage(PrototypeDataService data)
    {
        _data = data; Title = "Search reports";
        _category.ItemsSource = new[] { "All categories", "Wallet", "Electronics", "Keys", "Clothing", "Bag", "Other" }; _category.SelectedIndex = 0;
        _status.ItemsSource = new[] { "All statuses" }.Concat(Enum.GetNames<ItemReportStatus>()).ToArray(); _status.SelectedIndex = 0;
        var stack = new VerticalStackLayout { Padding = 24, Spacing = 12, MaximumWidthRequest = 1000 };
        stack.Add(new Label { Text = "Search lost and found reports", FontSize = 28, FontAttributes = FontAttributes.Bold });
        foreach (var (label, view) in new (string, View)[] { ("Item name", _text), ("Category", _category), ("Status", _status), ("Location", _location), ("Use date range", _dates), ("From date", _from), ("To date", _to) })
        { stack.Add(new Label { Text = label }); SemanticProperties.SetDescription(view, label); stack.Add(view); }
        var search = new Button { Text = "Search reports", MinimumHeightRequest = 48 }; search.Clicked += (_, _) => Search(); stack.Add(search);
        stack.Add(_message); stack.Add(_results); Content = new ScrollView { Content = stack };
    }
    protected override void OnAppearing() { base.OnAppearing(); Search(); }
    private void Search()
    {
        try
        {
            var filter = new ReportFilter(_text.Text, _category.SelectedIndex > 0 ? _category.SelectedItem.ToString() : null,
                _status.SelectedIndex > 0 ? Enum.Parse<ItemReportStatus>(_status.SelectedItem.ToString()!) : null,
                _dates.IsChecked ? _from.Date : null, _dates.IsChecked ? _to.Date : null, _location.Text);
            var rows = _data.SearchReports(filter); _message.Text = $"{rows.Count} report(s) found";
            _results.Text = rows.Count == 0 ? "No reports match your filters." : string.Join("\n\n", rows.Select(x => x.Summary));
        }
        catch (ArgumentException ex) { _message.Text = ex.Message; _results.Text = string.Empty; }
    }
}
