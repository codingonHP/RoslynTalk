using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TypeInferenceRewriter
{
    public class TypeInferenceRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;

        public TypeInferenceRewriter(SemanticModel semanticModel)
        {
            this._semanticModel = semanticModel;
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (node.Declaration.Variables.Count > 1)
            {
                return node;
            }
            if (node.Declaration.Variables[0].Initializer == null)
            {
                return node;
            }

            var declarator = node.Declaration.Variables.First();
            var variableTypeName = node.Declaration.Type;
            var variableType = (ITypeSymbol)_semanticModel.GetSymbolInfo(variableTypeName).Symbol;
            var initializerInfo = _semanticModel.GetTypeInfo(declarator.Initializer.Value);

            if (Equals(variableType, initializerInfo.Type))
            {
                TypeSyntax varTypeName = IdentifierName("var").WithLeadingTrivia(variableTypeName.GetLeadingTrivia())
                                                              .WithTrailingTrivia(variableTypeName.GetTrailingTrivia());

                return node.ReplaceNode(variableTypeName, varTypeName);
            }

            return node;
        }
    }
}
