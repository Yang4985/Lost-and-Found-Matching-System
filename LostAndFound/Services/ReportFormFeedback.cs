namespace LostAndFound.Services;

public static class ReportFormFeedback
{
    public static void Show(ContentPage page, string errors)
    {
        foreach (var (control, prefix) in new[] { ("ItemNameEntry", "Item name"), ("CategoryPicker", "Category"),
            ("LocationEntry", "Location"), ("DescriptionEditor", "Description"), ("StorageEntry", "Storage location"),
            ("LostDatePicker", "Lost date"), ("FoundDatePicker", "Found date") })
        {
            var label = page.FindByName<Label>(control + "Error");
            if (label is null) continue;
            label.Text = string.Join("\n", errors.Split('\n').Where(x => x.StartsWith(prefix, StringComparison.Ordinal)));
            label.IsVisible = !string.IsNullOrEmpty(label.Text);
        }
    }
}
