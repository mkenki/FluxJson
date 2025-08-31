using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluxJson.Generator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
                !classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                (classDeclarationSyntax.TypeParameterList?.Parameters.Count ?? 0) == 0)
            {
                CandidateClasses.Add(classDeclarationSyntax);
            }
        }
    }
}
