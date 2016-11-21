using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UsingCollectorCS
{
    public class UsingCollector : CSharpSyntaxWalker
    {
        public readonly List<UsingDirectiveSyntax> Usings = new List<UsingDirectiveSyntax>();

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Name.ToString() != "System" && !node.Name.ToString().StartsWith("System."))
            {
                Usings.Add(node);
            }
        }
    }
}
