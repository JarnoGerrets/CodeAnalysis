using System.Diagnostics;
using CodeAnalysis;
using CodeAnalysisService;
using CodeAnalysisService.PatternAnalyser;

class Program
{
    static async Task Main(string[] args)
    {   
        RepoPrinter.PrintRepo(@"C:\Users\jarno\OneDrive\Documenten\POC CodeAnalysis\CodeAnalysis");   
        var stopwatch = Stopwatch.StartNew();

        var service = new CodeAnalysisServiceRunner();

        await service.AnalyzeAsync(
            @"C:\Users\jarno\OneDrive\Documenten\POC CodeAnalysis"
        );

        service.BuildGraph();

        stopwatch.Stop();

        if (service.Graph != null)
        {
            service.PrintGraph();
            service.CreateGraphView();

            var manager = new PatternAnalyserManager(service.Graph);
            var allResults = manager.AnalyseAll();
            manager.PrintResults(allResults);
        }

        Console.WriteLine($"Total Execution time: {stopwatch.ElapsedMilliseconds} ms");
    }
}
