using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

namespace ConsoleApplication1
{
    public class Program
    {
        static void Main(string[] args)
        {
            var code = @"
                            namespace ConsoleApplication1
                            {
                                using System;
                                class Program
                                {
                                    static void Main(string[] args)
                                    {
                                        int a = 100 , b = 200, c;
                                        c = a + b;
                                        Console.WriteLine(""Compiled using Compilation API of roslyn"" + c);
                                        Console.WriteLine(a);
                                        Console.WriteLine(b);
                                        Console.WriteLine(c);
                                        Console.WriteLine(""Hello world"");
                                        Console.ReadKey();
                                    }
                                }
                            }
                        ";

            IEnumerable<MetadataReference> references = GetAllReferences();

            CSharpCompilation compilation;
            SyntaxTree syntaxTree;

            EmitResult compile = CompileCode(code, references, out compilation, out syntaxTree);
            if (compile.Success)
            {
                Console.WriteLine($"{compilation.Language}, {compilation.LanguageVersion},\nActual Program Compiled \n\n{compilation.SyntaxTrees.First().GetRoot().NormalizeWhitespace() }");
            }

            var semanticModel = GetSemanticModel(compilation, syntaxTree);

            ConsoleWriteLineWalker consoleWriteLineWalker = new ConsoleWriteLineWalker();
            consoleWriteLineWalker.Visit(syntaxTree.GetRoot());

            foreach (var expressionSyntax in consoleWriteLineWalker.ExpressionCollection)
            {
                var parsedExpression = semanticModel.GetConstantValue(expressionSyntax);
                if (parsedExpression.HasValue)
                {
                    Console.WriteLine(parsedExpression.Value);
                }
                else
                {
                    Console.WriteLine("No value found");
                }
                
            }

        }

        private static IEnumerable<MetadataReference> GetAllReferences()
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var system = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
            IEnumerable<MetadataReference> references = new[] { mscorlib, system };
            return references;
        }

        private static EmitResult CompileCode(string code, IEnumerable<MetadataReference> references, out CSharpCompilation compilationResult, out SyntaxTree syntaxTree)
        {
            syntaxTree = GetSyntaxTree(code);

            compilationResult = CSharpCompilation.Create("Demo")
                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(references);

            var compile = compilationResult.Emit("Demo.exe");
            return compile;
        }

        private static SyntaxTree GetSyntaxTree(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            return syntaxTree;
        }

        private static SemanticModel GetSemanticModel(CSharpCompilation compilation, SyntaxTree tree)
        {
            return compilation.GetSemanticModel(tree);
        }
    }

    public class ConsoleWriteLineWalker : CSharpSyntaxWalker
    {
        public ConsoleWriteLineWalker()
        {
            ExpressionCollection = new List<ExpressionSyntax>();
        }

        public List<ExpressionSyntax> ExpressionCollection { get; }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            MemberAccessExpressionSyntax member = node.Expression as MemberAccessExpressionSyntax;

            var type = member?.Expression as IdentifierNameSyntax;

            if (type != null && type.Identifier.Text == "Console" && member.Name.Identifier.Text == "WriteLine")
            {
                if (node.ArgumentList.Arguments.Count == 1)
                {
                    var value = node.ArgumentList.Arguments.Single().Expression;
                    ExpressionCollection.Add(value);
                    return;
                }
            }


            base.VisitInvocationExpression(node);
        }
    }
}

namespace ConsoleApplication1
{
    using System;
    class Program12
    {
        static void Main01(string[] args)
        {
            int a = 100, b = 200, c;
            c = a + b;
            Console.WriteLine("Compiled using Compilation API of roslyn");
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);
            Console.WriteLine("Hello world");
            Console.ReadKey();
        }
    }
}
