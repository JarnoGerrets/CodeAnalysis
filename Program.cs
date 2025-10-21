using System.Diagnostics;
using CodeAnalysis;
using CodeAnalysisService;
using CodeAnalysisService.PatternAnalyser;
using CodeAnalysisService.RoslynBasedTest;

class Program
{
    static async Task Main(string[] args)
    {
        // Console.WriteLine("=== Running Roslyn Baseline (empirical comparison) ===");
        // string rootPath = @"C:\Users\jarno\OneDrive\Documenten\POC CodeAnalysis";
        // var compilation = await CompilationBuilder.BuildCompilationAsync(rootPath);

        // var (singletonFound, singletonTime) = await RoslynSingletonAnalyser.AnalyseAsync(compilation);
        // var (adapterFound, adapterTime) = await RoslynAdapterAnalyser.AnalyseAsync(compilation);
        // var (observerFound, observerTime) = await RoslynObserverAnalyser.AnalyseAsync(compilation);

        // Console.WriteLine();
        // Console.WriteLine("=== Roslyn Baseline Summary ===");
        // Console.WriteLine($"Singleton: {singletonFound} found in {singletonTime} ms");
        // Console.WriteLine($"Adapter:   {adapterFound} found in {adapterTime} ms");
        // Console.WriteLine($"Observer:  {observerFound} found in {observerTime} ms");
        // Console.WriteLine($"Total runtime: {singletonTime + adapterTime + observerTime} ms");

        // Either run the above part for pure roslyn detection, or below for graph based detection.

        RepoPrinter.PrintRepo(@"C:\Users\jarno\OneDrive\Documenten\POC CodeAnalysis\CodeAnalysis");


        var service = new CodeAnalysisServiceRunner();
        var graphStopwatch = Stopwatch.StartNew();
        await service.Setup(
            @"C:\Users\jarno\OneDrive\Documenten\POC CodeAnalysis"
        );

        service.BuildGraph();
        graphStopwatch.Stop();

        var stopwatch = Stopwatch.StartNew();
        if (service.Graph != null)
        {
            // service.PrintGraph();
            // service.CreateGraphView();

            var manager = new PatternAnalyserManager(service.Graph);
            var allResults = manager.AnalyseAll();
            stopwatch.Stop();
            manager.PrintResults(allResults);
        }
        Console.WriteLine($"Total Execution time building graph: {graphStopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total Execution time analysing patterns: {stopwatch.ElapsedMilliseconds} ms");
    }
}
