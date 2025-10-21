
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class CompilationBuilder
{
    public static async Task<Compilation> BuildCompilationAsync(string rootPath)
    {
        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException($"Path not found: {rootPath}");
        var files = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);
        var syntaxTrees = new ConcurrentBag<SyntaxTree>();

        await Parallel.ForEachAsync(files, async (file, token) =>
        {
            try
            {
                var text = await File.ReadAllTextAsync(file, Encoding.UTF8, token);
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(text, path: file));
            }
            catch { /* ignoring errors, class is for benchmarking */ }
        });

        var refWatch = Stopwatch.StartNew();
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .DistinctBy(a => a.Location)
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();
        refWatch.Stop();

        var compilation = CSharpCompilation.Create(
            "RoslynBaseline",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return compilation;
    }
}