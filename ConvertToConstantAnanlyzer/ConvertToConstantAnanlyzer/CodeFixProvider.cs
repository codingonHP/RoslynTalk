﻿using System.Collections.Immutable;
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

namespace ConvertToConstantAnanlyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConvertToConstantAnanlyzerCodeFixProvider)), Shared]
    public class ConvertToConstantAnanlyzerCodeFixProvider : CodeFixProvider
    {
        private const string Title = "consider making this a constant";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ConvertToConstantAnanlyzerAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ConvertToConstAsync(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> ConvertToConstAsync(Document document, VariableDeclarationSyntax variableDeclaration, CancellationToken cancellationToken)
        {
            var variableDeclarationWithConst = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName($"const {variableDeclaration}"))
                                                            .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(variableDeclaration, variableDeclarationWithConst);
            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }
    }
}