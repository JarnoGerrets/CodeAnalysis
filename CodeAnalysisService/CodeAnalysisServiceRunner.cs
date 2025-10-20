using Microsoft.CodeAnalysis;
using System.Diagnostics;
using CodeAnalysisService.GraphBuildingService;

namespace CodeAnalysisService
{
    /// <summary>
    /// Service runner that loads .cs files, builds syntax trees and semantic models
    /// then starts the analyser to detect patterns
    /// </summary>
    public class CodeAnalysisServiceRunner
    {
        private readonly CodeBaseLoader _loader = new();
        private GraphBuilder? _graph;
        public GraphBuilder? Graph => _graph;

        public async Task AnalyzeAsync(string rootPath)
        {
            var sw = Stopwatch.StartNew();
            await _loader.LoadAsync(rootPath);
            sw.Stop();
            Console.WriteLine($"Roslyn setup: {sw.ElapsedMilliseconds} ms");
        }

        public void BuildGraph()
        {
            if (!_loader.SemanticModels.Any()) return;
            var service = new GraphService(_loader.SemanticModels);
            _graph = service.Build();

            if (_loader.FailedFiles.Any())
            {
                Console.WriteLine("Some files could not be processed:");
                foreach (var (file, reason) in _loader.FailedFiles)
                    Console.WriteLine($"   {file} - {reason}");
            }
        }

        public void PrintGraph() =>
            _graph?.Let(g => new GraphService(_loader.SemanticModels)
                .Print(g, Path.Combine(Directory.GetCurrentDirectory(), "PrinterLogs")));

        public void CreateGraphView() =>
            _graph?.Let(g => new GraphService(_loader.SemanticModels)
                .Export(g, Path.Combine(Directory.GetCurrentDirectory(), "graph.json")));
    }


    internal static class Extensions
    {
        public static void Let<T>(this T? value, Action<T> action) where T : class
        {
            if (value != null) action(value);
        }
    }
}
