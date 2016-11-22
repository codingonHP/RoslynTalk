using System;
using Microsoft.CodeAnalysis.CSharp;

namespace CustomVisitor
{
    public class Program
    {
        static void Main(string[] args)
        {
            var code = @"
                namespace FirstAnalyzerCSContainer
                {
                    public class FirstAnalyzerCSAnalyzerClass : DiagnosticAnalyzer
                    {
                        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

                        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

                        public override void Initialize(AnalysisContext context)
                        {
                            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LocalDeclarationStatement);
                        }

                        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
                        {
                            throw new NotImplementedException();
                        }
                    }

                    namespace FirstAnalyzerCS02Container{
                        public class FirstAnalyzerCSAnalyzerClass{}
                    }
                }
            ";

            var csharpCode = CSharpSyntaxTree.ParseText(code);
            var root = csharpCode.GetCompilationUnitRoot();

            CodeDomWalker domWalker = new CodeDomWalker();
            domWalker.Visit(root);

            foreach (var @namespace in domWalker.NamespaceCollection)
            {
                Console.WriteLine($"Namespace : {@namespace}");
            }

            foreach (var @class in domWalker.ClassList)
            {
                Console.WriteLine($"class : {@class}");
            }
        }
    }
}
