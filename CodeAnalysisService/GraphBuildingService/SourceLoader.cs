using System.Collections.Concurrent;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;


namespace CodeAnalysisService.GraphBuildingService
{
    public class CodeBaseLoader
    {
        public IReadOnlyList<SyntaxTree> SyntaxTrees { get; private set; } = [];
        public Dictionary<SyntaxTree, SemanticModel> SemanticModels { get; private set; } = [];
        public List<(string FilePath, string Reason)> FailedFiles { get; } = [];

        public async Task LoadAsync(string rootPath)
        {
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException($"Path not found: {rootPath}");

            var files = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);
            var syntaxTrees = new ConcurrentBag<SyntaxTree>();
            var failedFiles = new ConcurrentBag<(string, string)>();

            await Parallel.ForEachAsync(files, async (file, token) =>
            {
                try
                {
                    var text = await File.ReadAllTextAsync(file, Encoding.UTF8, token);
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(text, path: file));
                }
                catch (Exception ex)
                {
                    failedFiles.Add((file, ex.Message));
                }
            });

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .GroupBy(a => a.Location)
                .Select(g => MetadataReference.CreateFromFile(g.Key))
                .ToList();

            var compilation = CSharpCompilation.Create("Analysis", syntaxTrees, references);

            var semanticModels = new Dictionary<SyntaxTree, SemanticModel>();
            foreach (var tree in syntaxTrees)
                semanticModels[tree] = compilation.GetSemanticModel(tree, ignoreAccessibility: true);

            SyntaxTrees = syntaxTrees.ToList();
            SemanticModels = semanticModels;
            FailedFiles.AddRange(failedFiles);
        }
    }
}
