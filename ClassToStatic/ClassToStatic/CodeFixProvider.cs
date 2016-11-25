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

namespace ClassToStatic
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassToStaticCodeFixProvider)), Shared]
    public class ClassToStaticCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Convert this class to static class";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ClassToStaticAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ConvertClassToStatic(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> ConvertClassToStatic(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            var syntaxTokenList = typeDecl.Modifiers.Any() ? typeDecl.Modifiers.Insert(1, SyntaxFactory.ParseToken("static")) : typeDecl.Modifiers.Add(SyntaxFactory.ParseToken("static"));

            var staticClass = SyntaxFactory.ClassDeclaration(typeDecl.AttributeLists,
                                                            syntaxTokenList,
                                                            typeDecl.Identifier,
                                                            typeDecl.TypeParameterList,
                                                            typeDecl.BaseList,
                                                            typeDecl.ConstraintClauses,
                                                            typeDecl.Members)
                                            .WithLeadingTrivia(typeDecl.GetLeadingTrivia())
                                            .WithTrailingTrivia(typeDecl.GetTrailingTrivia())
                                            .WithAdditionalAnnotations(Formatter.Annotation);

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var newStaticClass = semanticModel.SyntaxTree.GetRoot()
                                              .ReplaceNode(typeDecl, staticClass);

            var newDocument = document.WithSyntaxRoot(newStaticClass);

            return newDocument;
        }
    }

}