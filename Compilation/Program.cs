using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Compilation
{
    /// <summary>
    /// Example - Creating a compilation
    //This example shows how to create a Compilation by adding 
    //assembly references and source files.
    //Like the syntax trees, everything in the Symbols API and 
    //the Binding API is immutable.
    //https://github.com/dotnet/roslyn/wiki/Getting-Started-C%23-Semantic-Analysis
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(
                                                        @"using System;
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
                                                        }");

            var root = (CompilationUnitSyntax)tree.GetRoot();
            var compilation = CSharpCompilation.Create("HelloWorld")
                                               .AddReferences( MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                                               .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);

            var nameInfo = model.GetSymbolInfo(root.Usings[0].Name);

            var systemSymbol = (INamespaceSymbol)nameInfo.Symbol;

            foreach (var ns in systemSymbol.GetNamespaceMembers())
            {
                Console.WriteLine(ns.Name);
            }

            var helloWorldString = root.DescendantNodes()
                                       .OfType<LiteralExpressionSyntax>()
                                       .First();

            var literalInfo = model.GetTypeInfo(helloWorldString);

            var stringTypeSymbol = (INamedTypeSymbol)literalInfo.Type;

            Console.Clear();

            foreach (var name in (from method in stringTypeSymbol.GetMembers()
                                                                 .OfType<IMethodSymbol>()
                                  where method.ReturnType.Equals(stringTypeSymbol) &&
                                        method.DeclaredAccessibility == Accessibility.Public
                                  select method.Name).Distinct())
            {
                Console.WriteLine(name);
            }
        }
    }
}
