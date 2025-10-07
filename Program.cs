using System;
using System.Diagnostics;
using CodeAnalysisService;
using CodeAnalysisService.GraphService.Export;
using CodeAnalysisService.PatternAnalyser;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;

class Program
{
    static void Main(string[] args)
    {      
        var stopwatch = Stopwatch.StartNew();
        var service = new CodeAnalysisServiceRunner(@"C:\Users\jarno\OneDrive\Documenten\POC CodeAnalysis\CodeAnalysis");
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


        Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
    }
}

