using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Text;
using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Printer;
using CodeAnalysisService.GraphService.Export;
using System.Collections.Concurrent;

namespace CodeAnalysisService
{
    /// <summary>
    /// Service runner that loads .cs files, builds syntax trees and semantic models
    /// then starts the analyser to detect patterns
    /// </summary>
    public class CodeAnalysisServiceRunner
    {
        private readonly List<SyntaxTree> _syntaxTrees = new();
        private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels = new();

        public GraphBuilder? Graph { get; private set; }
        public List<(string FilePath, string Reason)> FailedFiles { get; } = new();

        public async Task AnalyzeAsync(string rootPath)
        {
            var stopwatch = Stopwatch.StartNew();
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException($"Path not found: {rootPath}");

            var allFiles = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);

            var syntaxTrees = new ConcurrentBag<SyntaxTree>();
            var failedFiles = new ConcurrentBag<(string FilePath, string Reason)>();

            await Parallel.ForEachAsync(allFiles, async (file, token) =>
            {
                try
                {
                    var text = await File.ReadAllTextAsync(file, Encoding.UTF8, token);
                    var tree = CSharpSyntaxTree.ParseText(text, path: file);
                    syntaxTrees.Add(tree);
                }
                catch (Exception ex)
                {
                    failedFiles.Add((file, ex.Message));
                }
            });

            _syntaxTrees.Clear();
            _syntaxTrees.AddRange(syntaxTrees);
            FailedFiles.Clear();
            FailedFiles.AddRange(failedFiles);

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                    !a.IsDynamic &&
                    !string.IsNullOrEmpty(a.Location) &&
                    File.Exists(a.Location))
                .GroupBy(a => a.Location)
                .Select(g => MetadataReference.CreateFromFile(g.Key))
                .ToList();

            var compilation = CSharpCompilation.Create(
                "Analysis",
                syntaxTrees: _syntaxTrees,
                references: references
            );

            foreach (var tree in _syntaxTrees)
                _semanticModels[tree] = compilation.GetSemanticModel(tree, ignoreAccessibility: true);

            stopwatch.Stop();
            Console.WriteLine($"Roslyn building execution time (parallel parse): {stopwatch.ElapsedMilliseconds} ms");
        }


        public void BuildGraph()
        {
            if (!_semanticModels.Any()) return;

            Graph = new GraphBuilder(_semanticModels.Values.First().Compilation, _semanticModels);
            Graph.BuildGraph();

            if (FailedFiles.Any())
            {
                Console.WriteLine("Some files could not be processed:");
                foreach (var (file, reason) in FailedFiles)
                    Console.WriteLine($"   {file} - {reason}");
            }
        }

        public void PrintGraph()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"graph-output-{timestamp}.txt";
            string logDir = Path.Combine(Directory.GetCurrentDirectory(), "PrinterLogs");

            var printer = new GraphPrinter(logDir, fileName);
            Graph?.Let(g => printer.PrintGraph(g.Registry));

            Console.WriteLine($"Total syntax trees: {_syntaxTrees.Count}");
        }

        public void CreateGraphView()
        {
            if (Graph == null) return;
            var dir = Path.Combine(Directory.GetCurrentDirectory(),
                @"CodeAnalysisService\GraphService", "Export", "ShowResultInView");
            Directory.CreateDirectory(dir);

            var outputPath = Path.Combine(dir, "graph.json");

            GraphJsonExporter.Export(Graph.Registry, outputPath);
        }
    }

    internal static class Extensions
    {
        public static void Let<T>(this T? value, Action<T> action) where T : class
        {
            if (value != null) action(value);
        }
    }
}
