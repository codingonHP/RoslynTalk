using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TraversingTrees
{
    //https://github.com/dotnet/roslyn/wiki/Getting-Started-C%23-Syntax-Analysis
    class Program
    {
        static void Main(string[] args)
        {
            string code = @"using System;
                            using System.Collections;
                            using System.Linq;
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

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

            var root = syntaxTree.GetCompilationUnitRoot();

            var firstMember = root.Members[0];

            var helloWorldDeclaration = (NamespaceDeclarationSyntax)firstMember;

            var programDeclaration = (ClassDeclarationSyntax)helloWorldDeclaration.Members[0];

            var mainDeclaration = (MethodDeclarationSyntax)programDeclaration.Members[0];

            var argsParameter = mainDeclaration.ParameterList.Parameters[0];

            //LINQ way of doing things.
            var firstParameters = from methodDeclaration in root.DescendantNodes()
                                                    .OfType<MethodDeclarationSyntax>()
                                  where methodDeclaration.Identifier.ValueText == "Main"
                                  select methodDeclaration.ParameterList.Parameters.First();

            var argsParameter2 = firstParameters.Single();
        }
    }
}
