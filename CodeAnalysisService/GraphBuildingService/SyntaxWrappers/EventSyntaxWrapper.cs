using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisService.GraphBuildingService.SyntaxWrappers
{
    public readonly struct EventSyntaxWrapper
    {
        public EventDeclarationSyntax? EventDecl { get; }
        public EventFieldDeclarationSyntax? EventFieldDecl { get; }

        public SyntaxNode Syntax => (SyntaxNode?)EventDecl ?? EventFieldDecl!;

        public EventSyntaxWrapper(EventDeclarationSyntax decl)
        {
            EventDecl = decl;
            EventFieldDecl = null;
        }

        public EventSyntaxWrapper(EventFieldDeclarationSyntax decl)
        {
            EventDecl = null;
            EventFieldDecl = decl;
        }

        public bool IsFieldStyle => EventFieldDecl != null;
        public bool IsDeclarationStyle => EventDecl != null;
    }
}
