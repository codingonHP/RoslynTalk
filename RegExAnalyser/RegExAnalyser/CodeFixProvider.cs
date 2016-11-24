using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace RegExAnalyser
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RegExAnalyserCodeFixProvider)), Shared]
    public class RegExAnalyserCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Fix Regular Expression";
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RegExAnalyserAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var invocation = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument:  c =>  FixRegexAsync(context.Document, invocation, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> FixRegexAsync(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken cancellationToken)
        {
            var argumentList = invocationExpr.ArgumentList;
            var regexLiteral = argumentList.Arguments[1].Expression as LiteralExpressionSyntax;

            var newLiteral = SyntaxFactory.ParseExpression("\"TODO : any valid regex goes here..\"")
                .WithLeadingTrivia(regexLiteral?.GetLeadingTrivia())
                .WithTrailingTrivia(regexLiteral?.GetTrailingTrivia())
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(regexLiteral, newLiteral);
            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }
    }
}