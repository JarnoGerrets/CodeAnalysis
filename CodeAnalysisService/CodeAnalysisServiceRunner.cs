using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Printer;
using CodeAnalysisService.GraphService.Export;

namespace CodeAnalysisService
{
    /// <summary>
    /// Runs Roslyn analysis on a project directory.
    /// Loads C# files, builds a compilation and semantic models,
    /// and creates a symbol graph via <see cref="GraphBuilder"/>.
    /// Tracks files that failed to load in <see cref="FailedFiles"/>.
    /// </summary>
    public class CodeAnalysisServiceRunner
    {
        private readonly List<SyntaxTree> _syntaxTrees;
        private readonly Compilation _compilation;
        private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels;

        public GraphBuilder? Graph { get; private set; }
        public List<(string FilePath, string Reason)> FailedFiles { get; } = new();

        public CodeAnalysisServiceRunner(string sourceDirectory)
        {
            var codeFiles = Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories);

            var syntaxTrees = new List<SyntaxTree>();

            foreach (var file in codeFiles)
            {
                try
                {
                    string text = File.ReadAllText(file, Encoding.UTF8);
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(text, path: file));
                }
                catch (DecoderFallbackException ex)
                {
                    FailedFiles.Add((file, $"Encoding error: {ex.Message}"));
                }
                catch (IOException ex)
                {
                    FailedFiles.Add((file, $"IO error: {ex.Message}"));
                }
                catch (Exception ex)
                {
                    FailedFiles.Add((file, $"Unexpected error: {ex.Message}"));
                }
            }

            _syntaxTrees = syntaxTrees;

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            _compilation = CSharpCompilation.Create(
                "Analysis",
                syntaxTrees: _syntaxTrees,
                references: references
            );

            _semanticModels = _syntaxTrees.ToDictionary(
                tree => tree,
                tree => _compilation.GetSemanticModel(tree)
            );
        }

        public SemanticModel? GetSemanticModel(SyntaxTree tree)
            => _semanticModels.TryGetValue(tree, out var model) ? model : null;

        public void BuildGraph()
        {
            Graph = new GraphBuilder(_compilation, _semanticModels);
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

            var printer = new GraphPrinter(Path.Combine(Directory.GetCurrentDirectory(), "PrinterLogs"), fileName);
            if (Graph != null)
            {
                printer.PrintGraph(Graph.Registry);
            }

            Console.WriteLine($"Graph written to: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrinterLogs", "graph-output.txt")}");
            Console.WriteLine($"Total syntax trees: {_syntaxTrees.Count}");
        }

        public void CreateGraphView()
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), @"CodeAnalysisService\GraphService", "Export", "ShowResultInView");
            if(dir != null)
            {
                Directory.CreateDirectory(dir);

            var outputPath = Path.Combine(dir, "graph.json");
            if (outputPath != null && Graph != null)
            {
                GraphJsonExporter.Export(Graph.Registry, outputPath);
            }

            Console.WriteLine($"Graph exported to {outputPath}");
            }
        }
    }
}
