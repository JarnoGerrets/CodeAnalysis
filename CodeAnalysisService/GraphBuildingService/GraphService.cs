using CodeAnalysisService.GraphBuildingService;
using CodeAnalysisService.GraphBuildingService.Export;
using CodeAnalysisService.GraphBuildingService.Printer;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisService.GraphBuildingService
{
    public class GraphService
    {
        private readonly GraphBuilder _builder;

        public GraphService(Dictionary<SyntaxTree, SemanticModel> semanticModels)
        {
            _builder = new GraphBuilder(semanticModels);
        }

        public GraphBuilder Build()
        {
            _builder.BuildGraph();
            return _builder;
        }

        public void Print(GraphBuilder graph, string outputDir)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = $"graph-output-{timestamp}.txt";

            var printer = new GraphPrinter(outputDir, fileName);
            printer.PrintGraph(graph.Registry);
        }

        public void Export(GraphBuilder graph, string outputPath)
        {
            GraphJsonExporter.Export(graph.Registry, outputPath);
        }
    }
}
