using CodeAnalysisService.Enums;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.PatternAnalyser.RuleResult;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Helpers;
using Microsoft.CodeAnalysis;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using System.Collections.Generic;
using System.Linq;

namespace CodeAnalysisService.PatternAnalyser.Checks
{
    /// <summary>
    /// Detects classes that hold IEnumerable&lt;T&gt;/collection members (non-primitive T),
    /// marking them as Subject roles for further pattern analysis.
    /// </summary>
    public static class CollectionChecks
    {
        public static RuleStep HasObserverCollection()
        {
            return new RuleStep
            {
                Description = "Class has IEnumerable<T> field/property of non-primitive type",
                MustPass = true,
                Check = node =>
                {
                    var roles = new List<PatternRole>();

                    bool IsCollectionType(ITypeSymbol symbol) =>
                        TypeHelper.GetElementType(symbol) != null;

                    foreach (var edge in node.OutgoingEdges
                        .Where(e => e.Type == EdgeType.HasField || e.Type == EdgeType.HasProperty))
                    {
                        ITypeSymbol? type = edge.Target switch
                        {
                            FieldNode f => f.Symbol.Type,
                            PropertyNode p => p.Symbol.Type,
                            _ => null
                        };

                        if (type != null && IsCollectionType(type))
                        {
                            roles.Add(new PatternRole("Subject", node));
                        }
                    }

                    if (!roles.Any())
                        return RuleStepResult.Empty;

                    return new RuleStepResult(100, true, roles);
                }
            };
        }
    }
}
