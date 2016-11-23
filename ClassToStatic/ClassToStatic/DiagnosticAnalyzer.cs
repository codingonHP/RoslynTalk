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
            context.RegisterSyntaxNodeAction(AnalyseCodeTreeAction, SyntaxKind.ClassDeclaration);
        }

        private void AnalyseCodeTreeAction(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = context.Node as ClassDeclarationSyntax;
            if (classDeclaration == null || classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            {
                return;
            }

            var allMembersAreStatic = classDeclaration.Members.All(IsStaticMember);

            if (allMembersAreStatic)
            {
                var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }

        public bool IsStaticMember(MemberDeclarationSyntax memberDeclaration)
        {
            var kind = memberDeclaration.Kind();
            switch (kind)
            {
                case SyntaxKind.PropertyDeclaration:
                    return ((PropertyDeclarationSyntax)memberDeclaration).Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword || m.Kind() == SyntaxKind.ConstKeyword);
                case SyntaxKind.FieldDeclaration:
                    return ((FieldDeclarationSyntax)memberDeclaration).Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword || m.Kind() == SyntaxKind.ConstKeyword);
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)memberDeclaration).Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword || m.Kind() == SyntaxKind.ConstKeyword);
            }

            return false;
        }
    }
}
