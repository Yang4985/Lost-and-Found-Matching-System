using System.Collections.ObjectModel;
using System.Text.Json;
using LostAndFound.Models;

namespace LostAndFound.Services;

public class PrototypeDataService
{
    private readonly string? _storagePath;
    private readonly object _gate = new();
    private readonly MatchingService _matching = new();
    private int _nextLostId = 1;
    private int _nextFoundId = 1;
    public ObservableCollection<LostItemReport> LostReports { get; } = [];
    public ObservableCollection<FoundItemReport> FoundReports { get; } = [];

    // An omitted path is an isolated in-memory mode for tests; the app supplies its data path.
    public PrototypeDataService(string? storagePath = null)
    {
        _storagePath = storagePath;
        if (storagePath is null || !File.Exists(storagePath)) return;
        var snapshot = JsonSerializer.Deserialize<ReportSnapshot>(File.ReadAllText(storagePath))
            ?? throw new InvalidDataException("The report store is empty or invalid.");
        if (snapshot.Lost is null || snapshot.Found is null)
            throw new InvalidDataException("The report store is missing report collections.");
        foreach (var report in snapshot.Lost) LostReports.Add(report);
        foreach (var report in snapshot.Found) FoundReports.Add(report);
        _nextLostId = checked(LostReports.Select(r => r.ID).DefaultIfEmpty(0).Max() + 1);
        _nextFoundId = checked(FoundReports.Select(r => r.ID).DefaultIfEmpty(0).Max() + 1);
    }

    public void AddLostReport(LostItemReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        lock (_gate)
        {
            report.ID = _nextLostId;
            report.ReportedAt = DateTime.UtcNow;
            report.Status = ItemReportStatus.Submitted;
            LostReports.Insert(0, report);
            try { Save(); _nextLostId++; }
            catch { LostReports.Remove(report); throw; }
        }
    }

    public void AddFoundReport(FoundItemReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        lock (_gate)
        {
            report.ID = _nextFoundId;
            report.ReportedAt = DateTime.UtcNow;
            report.Status = ItemReportStatus.Submitted;
            FoundReports.Insert(0, report);
            try { Save(); _nextFoundId++; }
            catch { FoundReports.Remove(report); throw; }
        }
    }

    // Call after changing existing reports. One app instance owns this local snapshot.
    public void Save()
    {
        lock (_gate)
        {
            if (_storagePath is null) return;
            var path = Path.GetFullPath(_storagePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var bytes = JsonSerializer.SerializeToUtf8Bytes(new ReportSnapshot(LostReports.ToList(), FoundReports.ToList()));
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes);
                    stream.Flush(flushToDisk: true);
                }
                File.Move(temporary, path, overwrite: true);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }

    public List<PrototypeMatch> GenerateMatches()
    {
        lock (_gate)
        {
            var results = new List<PrototypeMatch>();
            foreach (var lost in LostReports.Where(r => IsEligible(r.Status)))
            foreach (var found in FoundReports.Where(r => IsEligible(r.Status)))
            {
                var score = _matching.CalculateMatchScore(lost, found);
                if (score > 0)
                    results.Add(new PrototypeMatch(lost, found, score, _matching.ExplainMatch(lost, found)));
            }
            return results.OrderByDescending(r => r.Score)
                .ThenBy(r => r.LostReport.ID).ThenBy(r => r.FoundReport.ID).ToList();
        }
    }

    public static bool IsEligible(ItemReportStatus status) => status is
        ItemReportStatus.Submitted or ItemReportStatus.Available or ItemReportStatus.UnderReview;
    public record ReportSnapshot(List<LostItemReport> Lost, List<FoundItemReport> Found);
}

public record PrototypeMatch(LostItemReport LostReport, FoundItemReport FoundReport, int Score, string Explanation)
{
    public string Title => $"{LostReport.Name} ↔ {FoundReport.Name}";
    public string ScoreText => $"Match score: {Score}/100";
}
