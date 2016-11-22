using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CustomVisitor
{
    public class CodeDomWalker : CSharpSyntaxWalker
    {
        public List<string> NamespaceCollection { get; set; }
        public List<string> ClassList { get; set; }

        public CodeDomWalker() : base(SyntaxWalkerDepth.Trivia)
        {
            NamespaceCollection = new List<string>();
            ClassList = new List<string>();
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var namespaceName = node.Name;
            NamespaceCollection.Add(namespaceName.ToString());

            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var className = node.Identifier;
            ClassList.Add(className.Text);

            base.VisitClassDeclaration(node);
        }
    }
}
