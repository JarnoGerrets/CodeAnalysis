using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisService.RoslynBasedTest
{
    /// <summary>
    /// Roslyn-only Observer detector that semantically mirrors the Graph-based ObserverRuleFactory.
    /// </summary>
    public static class RoslynObserverAnalyser
    {
        public static async Task<(int found, long elapsedMs)> AnalyseAsync(Compilation compilation)
        {
            var stopwatch = Stopwatch.StartNew();
            var totalFound = new ConcurrentBag<int>();

            await Task.Run(() =>
            {
                int totalClasses = 0;
                int eliminatedEarly = 0;
                int analysedInDepth = 0;

                Parallel.ForEach(compilation.SyntaxTrees, tree =>
                {
                    int localFound = 0;
                    var model = compilation.GetSemanticModel(tree);
                    var root = tree.GetRoot();

                    foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                    {
                        Interlocked.Increment(ref totalClasses);

                        var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                        if (symbol == null) continue;

                        // Check syntactisch of er potentie is om een observer te zijn
                        var hasPotential = symbol.GetMembers()
                            .Where(m => m is IFieldSymbol or IPropertySymbol)
                            .Select(m => GetCollectionElementType(m))
                            .Any(t => t != null && t.TypeKind == TypeKind.Interface);

                        if (!hasPotential)
                        {
                            Interlocked.Increment(ref eliminatedEarly);
                            continue;
                        }

                        Interlocked.Increment(ref analysedInDepth);

                        if (IsObserverLike(symbol, model, compilation))
                            localFound++;
                    }

                    totalFound.Add(localFound);
                });

                Console.WriteLine($"[Observer] Total classes scanned: {totalClasses}");
                Console.WriteLine($"[Observer] Eliminated early (no observer field): {eliminatedEarly}");
                Console.WriteLine($"[Observer] Analysed in depth: {analysedInDepth}");
            });


            stopwatch.Stop();
            int found = totalFound.Sum();

            Console.WriteLine($"[Roslyn Baseline] Found {found} observer-like classes in {stopwatch.ElapsedMilliseconds} ms");
            return (found, stopwatch.ElapsedMilliseconds);
        }

        // Helpers
        private static bool IsObserverLike(INamedTypeSymbol subject, SemanticModel model, Compilation compilation)
        {
            var observerTypes = subject
                .GetMembers()
                .Where(m => m is IFieldSymbol or IPropertySymbol)
                .Select(m => GetCollectionElementType(m))
                .Where(t => t != null && t.TypeKind == TypeKind.Interface)
                .Distinct(SymbolEqualityComparer.Default)
                .ToList();

            if (!observerTypes.Any())
                return false;

            bool notifiesObservers = false;
            bool hasAttachDetach = false;

            foreach (var method in subject.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.MethodKind != MethodKind.Ordinary) continue;
                if (method.DeclaringSyntaxReferences.Length == 0) continue;

                var mSyntax = method.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax;
                if (mSyntax?.Body == null) continue;

                var mModel = compilation.GetSemanticModel(mSyntax.SyntaxTree);

                foreach (var invocation in mSyntax.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    var smybolInfoStopWatch = Stopwatch.StartNew();
                    var called = mModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                    smybolInfoStopWatch.Stop();
                    Console.WriteLine($"[Observer] Time for each SymbolInfo call: {smybolInfoStopWatch.ElapsedMilliseconds} ms");
                    if (called == null) continue;

                    if (observerTypes.Any(o => called.ContainingType.AllInterfaces.Contains(o) ||
                                               SymbolEqualityComparer.Default.Equals(called.ContainingType, o)))
                    {
                        notifiesObservers = true;
                        break;
                    }
                }

                if (notifiesObservers) break;
            }

            foreach (var method in subject.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.Parameters.Any(p => observerTypes.Any(o => SymbolEqualityComparer.Default.Equals(p.Type, o))))
                {
                    hasAttachDetach = true;
                    break;
                }

                if (method.Name.Contains("Attach", StringComparison.OrdinalIgnoreCase) ||
                    method.Name.Contains("Detach", StringComparison.OrdinalIgnoreCase) ||
                    method.Name.Contains("Subscribe", StringComparison.OrdinalIgnoreCase) ||
                    method.Name.Contains("Unsubscribe", StringComparison.OrdinalIgnoreCase))
                {
                    hasAttachDetach = true;
                    break;
                }
            }

            string[] commonNames = { "Update", "Notify", "Changed", "OnChange" };
            bool hasCommonMethod = subject
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Any(m => commonNames.Contains(m.Name, StringComparer.OrdinalIgnoreCase));

            var concreteObservers = new List<INamedTypeSymbol>();
            foreach (var iface in observerTypes)
            {
                foreach (var type in compilation.GlobalNamespace.GetAllTypes())
                {
                    if (type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iface)))
                        concreteObservers.Add(type);
                }
            }

            return notifiesObservers || hasAttachDetach || hasCommonMethod || concreteObservers.Any();
        }
        private static INamedTypeSymbol? GetCollectionElementType(ISymbol member)
        {
            var type = member switch
            {
                IFieldSymbol f => f.Type,
                IPropertySymbol p => p.Type,
                _ => null
            };

            if (type is not INamedTypeSymbol named)
                return null;

            if (named.TypeArguments.Length == 1)
                return named.TypeArguments[0] as INamedTypeSymbol;

            var ienum = named.AllInterfaces
                .FirstOrDefault(i => i.Name == "IEnumerable" && i.TypeArguments.Length == 1);

            return ienum?.TypeArguments[0] as INamedTypeSymbol;
        }

        private static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol ns)
        {
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol nestedNs)
                {
                    foreach (var t in GetAllTypes(nestedNs))
                        yield return t;
                }
                else if (member is INamedTypeSymbol t)
                {
                    yield return t;
                    foreach (var nested in t.GetTypeMembers())
                        yield return nested;
                }
            }
        }
    }
}
