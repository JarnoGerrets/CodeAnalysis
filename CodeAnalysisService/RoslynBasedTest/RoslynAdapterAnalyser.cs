using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisService.RoslynBasedTest
{
    /// <summary>
    /// Roslyn-only Adapter detector replicating AdapterRuleFactory logic.
    /// </summary>
    public static class RoslynAdapterAnalyser
    {
        public static async Task<(int found, long elapsedMs)> AnalyseAsync(Compilation compilation)
        {
            var stopwatch = Stopwatch.StartNew();
            var totalFound = new ConcurrentBag<int>();

            await Task.Run(() =>
            {
                int totalClasses = 0;
                int potentialAdapters = 0;
                int analysedInDepth = 0;

                Parallel.ForEach(compilation.SyntaxTrees, tree =>
                {
                    int localFound = 0;
                    var model = compilation.GetSemanticModel(tree);
                    var root = tree.GetRoot();

                    // Alle klassen tellen
                    var allClasses = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                    Interlocked.Add(ref totalClasses, allClasses.Count);

                    // Vind alle interfaces of abstracte basisklassen (targets)
                    var potentialTargets = root.DescendantNodes()
                        .OfType<TypeDeclarationSyntax>()
                        .Select(n => model.GetDeclaredSymbol(n))
                        .OfType<INamedTypeSymbol>()
                        .Where(t => t.TypeKind == TypeKind.Interface || t.IsAbstract)
                        .ToList();

                    foreach (var target in potentialTargets)
                    {
                        var adapters = FindImplementors(compilation, target).ToList();
                        if (!adapters.Any()) continue;

                        Interlocked.Add(ref potentialAdapters, adapters.Count);

                        foreach (var adapter in adapters)
                        {
                            var adapteeTypes = GetHeldTypes(adapter).ToList();
                            if (!adapteeTypes.Any()) continue;

                            var ifaceMethods = target.GetMembers().OfType<IMethodSymbol>().ToList();

                            Interlocked.Increment(ref analysedInDepth);

                            foreach (var adaptee in adapteeTypes)
                            {
                                if (adaptee == null || adaptee.TypeKind != TypeKind.Class)
                                    continue;

                                if (DelegatesTo(adapter, adaptee, ifaceMethods, compilation))
                                {
                                    localFound++;
                                    break;
                                }
                            }
                        }
                    }

                    totalFound.Add(localFound);
                });

                Console.WriteLine($"[Adapter] Total classes scanned: {totalClasses}");
                Console.WriteLine($"[Adapter] Potential adapters (implements/inherits): {potentialAdapters}");
                Console.WriteLine($"[Adapter] Analysed in depth: {analysedInDepth}");
            });


            stopwatch.Stop();
            int foundAdapters = totalFound.Sum();
            Console.WriteLine($"[Roslyn Baseline] Found {foundAdapters} adapter-like classes in {stopwatch.ElapsedMilliseconds} ms");
            return (foundAdapters, stopwatch.ElapsedMilliseconds);
        }

        // Helpers
        private static IEnumerable<INamedTypeSymbol> FindImplementors(
            Compilation compilation,
            INamedTypeSymbol target)
        {
            var results = new List<INamedTypeSymbol>();

            foreach (var tree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(tree);
                var root = tree.GetRoot();

                foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (model.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol type)
                        continue;

                    bool implements = target.TypeKind == TypeKind.Interface &&
                                      type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, target));

                    bool inherits = target.TypeKind != TypeKind.Interface && InheritsFrom(type, target);

                    if (implements || inherits)
                        results.Add(type);
                }
            }

            return results.Cast<ISymbol>()
                          .Distinct(SymbolEqualityComparer.Default)
                          .Cast<INamedTypeSymbol>();
        }

        private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol potentialBase)
        {
            for (var b = type.BaseType; b is not null; b = b.BaseType)
                if (SymbolEqualityComparer.Default.Equals(b, potentialBase))
                    return true;
            return false;
        }


        private static IEnumerable<INamedTypeSymbol?> GetHeldTypes(INamedTypeSymbol type)
        {
            foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
            {
                if (field.Type is INamedTypeSymbol fType && fType.TypeKind == TypeKind.Class)
                    yield return fType;
            }

            foreach (var prop in type.GetMembers().OfType<IPropertySymbol>())
            {
                if (prop.Type is INamedTypeSymbol pType && pType.TypeKind == TypeKind.Class)
                    yield return pType;
            }
        }

        private static bool DelegatesTo(
            INamedTypeSymbol adapter,
            INamedTypeSymbol adaptee,
            List<IMethodSymbol> ifaceMethods,
            Compilation compilation)
        {
            foreach (var method in adapter.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.DeclaringSyntaxReferences.Length == 0)
                    continue;

                bool implementsInterface = ifaceMethods.Any(ifaceMethod =>
                    SymbolEqualityComparer.Default.Equals(
                        adapter.FindImplementationForInterfaceMember(ifaceMethod),
                        method));

                if (!implementsInterface)
                    continue;

                var mSyntax = method.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax;
                if (mSyntax?.Body is null && mSyntax?.ExpressionBody is null)
                    continue;

                var methodModel = compilation.GetSemanticModel(mSyntax.SyntaxTree);
                var body = (SyntaxNode?)mSyntax.Body ?? mSyntax.ExpressionBody;
                if (body is null) continue;

                foreach (var call in body.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    var smybolInfoStopWatch = Stopwatch.StartNew();
                    var sym = methodModel.GetSymbolInfo(call).Symbol as IMethodSymbol;
                    smybolInfoStopWatch.Stop();
                    Console.WriteLine($"[Adapter] Time for each SymbolInfo call: {smybolInfoStopWatch.ElapsedMilliseconds} ms");
                    if (sym is null) continue;

                    var calledType = sym.ContainingType;
                    if (calledType is null) continue;

                    bool matchesAdaptee =
                        SymbolEqualityComparer.Default.Equals(calledType, adaptee) ||
                        adaptee.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, calledType)) ||
                        InheritsFrom(adaptee, calledType);

                    if (matchesAdaptee)
                        return true;
                }
            }

            return false;
        }



    }
}
