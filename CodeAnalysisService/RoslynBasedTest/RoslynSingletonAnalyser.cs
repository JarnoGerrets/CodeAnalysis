using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisService.RoslynBasedTest
{
    /// <summary>
    /// Roslyn-only Singleton detector replicating the logic from SingletonRuleFactory.
    /// </summary>
    public static class RoslynSingletonAnalyser
    {
        public static async Task<(int found, long elapsedMs)> AnalyseAsync(Compilation compilation)
        {
            var stopwatch = Stopwatch.StartNew();
            int totalFound = 0;

            var count = new ConcurrentBag<int>();

            await Task.Run(() =>
            {
                Parallel.ForEach(compilation.SyntaxTrees, tree =>
                {
                    var model = compilation.GetSemanticModel(tree);
                    var root = tree.GetRoot();

                    int localFound = 0;

                    foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                    {
                        var symbol = model.GetDeclaredSymbol(classDecl);
                        if (symbol != null && IsSingleton(symbol))
                            localFound++;
                    }

                    count.Add(localFound);
                });
            });

            totalFound = count.Sum();
            stopwatch.Stop();

            Console.WriteLine($"[Roslyn Baseline] Found {totalFound} singleton(s) in {stopwatch.ElapsedMilliseconds} ms");
            return (totalFound, stopwatch.ElapsedMilliseconds);
        }


        // Helpers
        private static bool IsSingleton(INamedTypeSymbol symbol)
        {
            var privateCtorStpWtch = Stopwatch.StartNew();
            bool hasPrivateCtor = symbol.Constructors.Any(c => c.DeclaredAccessibility == Accessibility.Private);
            privateCtorStpWtch.Stop();
            Console.WriteLine($"[Singleton] Time for each hasPrivateCtor call: {privateCtorStpWtch.ElapsedMilliseconds} ms");

            bool hasStaticField = symbol.GetMembers().OfType<IFieldSymbol>()
                .Any(f => f.IsStatic && SymbolEqualityComparer.Default.Equals(f.Type, symbol));

            bool hasStaticAccessor = symbol.GetMembers().Any(m =>
                m.IsStatic && m switch
                {
                    IPropertySymbol p => SymbolEqualityComparer.Default.Equals(p.Type, symbol),
                    IMethodSymbol method => SymbolEqualityComparer.Default.Equals(method.ReturnType, symbol),
                    _ => false
                });

            return hasPrivateCtor && (hasStaticField || hasStaticAccessor);
        }
    }
}
