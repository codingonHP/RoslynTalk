using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EmptyArrayInitialization
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyArrayInitializationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EmptyArrayInitialization";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, 
            Title, 
            MessageFormat,
            Category, 
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol,SyntaxKind.ArrayType);
        }

        private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            try
            {
                var field = context.Node as ArrayTypeSyntax;
                var variableDeclaration = field?.Parent;
                var arrayCreationExpression = variableDeclaration?.DescendantNodes().OfType<ArrayCreationExpressionSyntax>();
                if (arrayCreationExpression != null && arrayCreationExpression.Any() )
                {
                    var arrayCreationExpressionSyntax = arrayCreationExpression.ToList()[0];

                    if (arrayCreationExpressionSyntax?.Initializer?.Expressions != null && arrayCreationExpressionSyntax.Initializer?.Expressions.Count == 0)
                    {
                        var diagnostic = Diagnostic.Create(Rule, location: field.GetLocation(), messageArgs: "Remove empty array initilizers");
                        context.ReportDiagnostic(diagnostic);
                    }
                }
               
            }
            catch (Exception)
            {
               
            }
        }
    }

    public class TestClass
    {
        public int[] EmptyArray = new int[] {};
        public int[] EmptyArray02 = new int[0];


    }
}
