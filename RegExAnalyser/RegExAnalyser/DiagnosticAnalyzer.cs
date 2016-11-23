using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RegExAnalyser
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RegExAnalyserAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RegExAnalyser";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "ExpressionValidation";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeRegExInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeRegExInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = context.Node as InvocationExpressionSyntax;
            var memberAccessExpressionSyntax = invocationExpr?.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax?.Name.Identifier.Text != "Match")
            {
                return;
            }

            var methodSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol as IMethodSymbol;
            if (!methodSymbol?.ToString().StartsWith("System.Text.RegularExpressions.Regex.Match") ?? true)
            {
                return;
            }

            var argumentList = invocationExpr.ArgumentList;
            if ((argumentList?.Arguments.Count ?? 0) < 2)
            {
                return;
            }

            var regexLiteral = argumentList?.Arguments[1].Expression as LiteralExpressionSyntax;
            if (regexLiteral == null)
            {
                return;
            }

            var regexOpt = context.SemanticModel.GetConstantValue(regexLiteral);
            if (!regexOpt.HasValue)
            {
                return;
            }

            var regex = regexOpt.Value as string;
            if (regex == null)
            {
                return;
            }

            try
            {
                Regex.Match("", regex);
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(Rule, invocationExpr.GetLocation(), e.Message);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}