using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClassToStatic
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassToStaticAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ClassToStatic";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSemanticModelAction(AnalyseCodeTreeAction);
        }

        private void AnalyseCodeTreeAction(SemanticModelAnalysisContext context)
        {

            TreeWalker treeWalker = new TreeWalker();
            treeWalker.Visit(context.SemanticModel.SyntaxTree.GetRoot());

            if (treeWalker.CanBeConvertedToStatic)
            {
                var diagnostic = Diagnostic.Create(Rule, context.SemanticModel.SyntaxTree.GetRoot().GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    class TreeWalker : CSharpSyntaxWalker
    {
        public bool CanBeConvertedToStatic { get; private set; }

        public TreeWalker()
        {
            CanBeConvertedToStatic = true;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (!CanBeConvertedToStatic)
            {
                return;
            }

            if (node.Modifiers.All(m => m.Text != "static"))
            {
                CanBeConvertedToStatic = false;
            }


            base.VisitMethodDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (!CanBeConvertedToStatic)
            {
                return;
            }

            if (node.Modifiers.All(m => m.Text != "static"))
            {
                CanBeConvertedToStatic = false;
            }

            base.VisitPropertyDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (!CanBeConvertedToStatic)
            {
                return;
            }

            if (node.Modifiers.All(m => m.Text != "static"))
            {
                CanBeConvertedToStatic = false;
            }

            base.VisitFieldDeclaration(node);
        }
    }
}
