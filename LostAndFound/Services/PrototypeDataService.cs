using System.Collections.ObjectModel;
using LostAndFound.Models;

namespace LostAndFound.Services
{
    public class PrototypeDataService
    {
        private int _nextLostId = 2;
        private int _nextFoundId = 2;

        public ObservableCollection<LostItemReport> LostReports { get; } = [];
        public ObservableCollection<FoundItemReport> FoundReports { get; } = [];

        public PrototypeDataService()
        {
            LostReports.Add(new LostItemReport
            {
                ID = 1,
                Name = "Black wallet",
                Category = "Wallet",
                Description = "Black leather wallet with a silver zip",
                Location = "Library",
                DateLost = DateTime.Today.AddDays(-1),
                DistinguishingFeatures = "Small scratch on the back"
            });

            FoundReports.Add(new FoundItemReport
            {
                ID = 1,
                Name = "Leather wallet",
                Category = "Wallet",
                Description = "Black wallet found beside the printers",
                Location = "Library",
                DateFound = DateTime.Today,
                StorageLocation = "Security desk"
            });
        }

        public void AddLostReport(LostItemReport report)
        {
            report.ID = _nextLostId++;
            report.ReportedAt = DateTime.UtcNow;
            report.Status = ItemReportStatus.Submitted;
            LostReports.Insert(0, report);
        }

        public void AddFoundReport(FoundItemReport report)
        {
            report.ID = _nextFoundId++;
            report.ReportedAt = DateTime.UtcNow;
            report.Status = ItemReportStatus.Submitted;
            FoundReports.Insert(0, report);
        }

        public List<PrototypeMatch> GenerateMatches()
        {
            var results = new List<PrototypeMatch>();

            foreach (var lost in LostReports.Where(r => r.Status != ItemReportStatus.Closed))
            {
                foreach (var found in FoundReports.Where(r => r.Status != ItemReportStatus.Closed))
                {
                    var reasons = new List<string>();
                    var score = 0;

                    if (EqualsText(lost.Category, found.Category))
                    {
                        score += 40;
                        reasons.Add("same category");
                    }

                    if (EqualsText(lost.Location, found.Location))
                    {
                        score += 25;
                        reasons.Add("same location");
                    }

                    var sharedKeywords = Keywords(lost.Name + " " + lost.Description)
                        .Intersect(Keywords(found.Name + " " + found.Description))
                        .Take(3)
                        .ToList();

                    if (sharedKeywords.Count > 0)
                    {
                        score += Math.Min(25, sharedKeywords.Count * 10);
                        reasons.Add("shared words: " + string.Join(", ", sharedKeywords));
                    }

                    var dayDifference = Math.Abs((lost.DateLost.Date - found.DateFound.Date).Days);
                    if (dayDifference <= 3)
                    {
                        score += 10;
                        reasons.Add($"dates within {dayDifference} day(s)");
                    }

                    if (score > 0)
                    {
                        results.Add(new PrototypeMatch(lost, found, Math.Min(score, 100),
                            string.Join("; ", reasons)));
                    }
                }
            }

            return results.OrderByDescending(result => result.Score).ToList();
        }

        private static bool EqualsText(string first, string second) =>
            !string.IsNullOrWhiteSpace(first) &&
            string.Equals(first.Trim(), second.Trim(), StringComparison.OrdinalIgnoreCase);

        private static IEnumerable<string> Keywords(string value) => value
            .ToLowerInvariant()
            .Split([' ', ',', '.', '-', '/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 2)
            .Distinct();
    }

    public record PrototypeMatch(
        LostItemReport LostReport,
        FoundItemReport FoundReport,
        int Score,
        string Explanation)
    {
        public string Title => $"{LostReport.Name} ↔ {FoundReport.Name}";
        public string ScoreText => $"Match score: {Score}%";
    }
}
