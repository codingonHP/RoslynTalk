using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MoreSemanticAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = @"using System;
                                                        using System.Collections.Generic;
                                                        using System.Text;

                                                        namespace HelloWorld
                                                        {
                                                            class Program
                                                            {
                                                                static void Main(string[] args)
                                                                {
                                                                    Console.WriteLine(""Hello, World!"");
                                                                }
                                                            }
                                                        }";

            var codeTree = CSharpSyntaxTree.ParseText(code);
            var compiledResult = CompileSyntaxTree(codeTree);

            var diagnostics = compiledResult.GetDiagnostics();
            foreach (var item in diagnostics)
            {
                Console.WriteLine(item.ToString());
            }

            SemanticModel semanticModel = compiledResult.GetSemanticModel(codeTree);
            SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(codeTree.GetCompilationUnitRoot());

            ISymbol sym = symbolInfo.Symbol;
             
        }

        private static CSharpCompilation CompileSyntaxTree(SyntaxTree codeTree)
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var system = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);

            var compilation = CSharpCompilation.Create("HelloWorld")
                            .AddReferences(mscorlib, system)
                            .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication));


            return compilation;

        }
    }
}
