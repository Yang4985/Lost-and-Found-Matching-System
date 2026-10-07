using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using LostAndFound.Models;
using LostAndFound.Services;

var data = new PrototypeDataService();
data.LostReports.Clear(); data.FoundReports.Clear();
data.AddLostReport(new LostItemReport { Name="Black wallet", Category="Wallet", Description="black leather wallet silver zip", Location="Library", DateLost=new(2026,10,1) });
string[] categories = ["Wallet","Electronics","Keys","Clothing","Bag","Other"];
for (int i=0; i<1000; i++)
    data.AddFoundReport(new FoundItemReport {
        Name=$"Found item {i}", Category=categories[i%6],
        Description=i%4==0 ? "black leather wallet silver zip" : "blue fabric object labelled campus",
        Location=i%3==0 ? "Library" : "Gym",
        DateFound=new DateTime(2026,10,1).AddDays(i%10), StorageLocation="QA fixture"
    });
for (int i=0;i<5;i++) data.GenerateMatches();
var samples = new List<object>();
var timings = new List<double>();
int? count=null;
for(int i=1;i<=20;i++) {
    var sw=Stopwatch.StartNew();
    var results=data.GenerateMatches();
    sw.Stop();
    if(count.HasValue && count!=results.Count) throw new Exception("Non-repeatable candidate count");
    count=results.Count;
    if(results.Any(r=>r.Score<0 || r.Score>100)) throw new Exception("Invalid score range");
    for(int k=1;k<results.Count;k++) if(results[k-1].Score<results[k].Score) throw new Exception("Unsorted results");
    timings.Add(sw.Elapsed.TotalMilliseconds);
    samples.Add(new { iteration=i, milliseconds=sw.Elapsed.TotalMilliseconds, candidateCount=results.Count });
}
var sorted=timings.Order().ToArray();
double percentile(double p)=>sorted[(int)Math.Ceiling(p*sorted.Length)-1];
var report=new {
    observedAtUtc=DateTime.UtcNow,
    scope="PrototypeDataService.GenerateMatches, one lost report and 1000 found reports; scoring, explanations and sorting; no UI, storage or network",
    fixture="Deterministic index-based mix of six categories, two locations, two descriptions, dates 2026-10-01 to 2026-10-10; all Submitted",
    operatingSystem=RuntimeInformation.OSDescription, runtime=RuntimeInformation.FrameworkDescription,
    architecture=RuntimeInformation.ProcessArchitecture.ToString(), logicalProcessors=Environment.ProcessorCount,
    stopwatchFrequency=Stopwatch.Frequency, configuration="Release", warmupRuns=5, measuredRuns=20,
    lostReports=1, foundReports=1000, p50Milliseconds=percentile(.5), p95Milliseconds=percentile(.95),
    maxMilliseconds=sorted[^1], meanMilliseconds=timings.Average(),
    referenceThresholdMilliseconds=2000, allServiceRunsWithinReference=sorted[^1]<=2000,
    limitation="Service-only reference comparison; does not verify end-to-end 2-second NFR, device-independent performance or matching correctness. Existing defects remain open.", samples
};
string output=args.Length>0?args[0]:"qa/evidence/performance.json";
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
File.WriteAllText(output,JsonSerializer.Serialize(report,new JsonSerializerOptions {WriteIndented=true}));
Console.WriteLine($"20 runs; p50={percentile(.5):F3} ms; p95={percentile(.95):F3} ms; max={sorted[^1]:F3} ms; candidates={count}");
Console.WriteLine(Path.GetFullPath(output));
