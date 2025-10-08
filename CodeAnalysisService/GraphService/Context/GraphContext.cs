using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphService.Nodes;

namespace CodeAnalysisService.GraphService.Context
{
    /// <summary>
    /// Provides controlled access to the node registry
    /// Caches syntax roots and hides raw compilation internals.
    /// </summary>
    public class GraphContext
    {
        private readonly NodeRegistry _registry;
        private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels;
        private readonly Dictionary<SyntaxTree, SyntaxNode> _roots;

        public GraphContext(Dictionary<SyntaxTree, SemanticModel> semanticModels, NodeRegistry registry)
        {
            _semanticModels = semanticModels;
            _registry = registry;

            _roots = new Dictionary<SyntaxTree, SyntaxNode>();
            foreach (var tree in semanticModels.Keys)
            {
                _roots[tree] = tree.GetRoot();
            }
        }

        public SyntaxNode GetRoot(SyntaxTree tree) => _roots[tree];
    }
}
