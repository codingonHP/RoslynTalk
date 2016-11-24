using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SyntaxTransformation
{
    //https://github.com/dotnet/roslyn/wiki/Getting-Started-C%23-Syntax-Transformation
    class Program
    {
        static void Main(string[] args)
        {
            NameSyntax name = SyntaxFactory.IdentifierName("System");
            Console.WriteLine(name.ToString());

            name = SyntaxFactory.QualifiedName(name, SyntaxFactory.IdentifierName("Collections"));
            Console.WriteLine(name.ToString());

            name = SyntaxFactory.QualifiedName(name, SyntaxFactory.IdentifierName("Generic"));
            Console.WriteLine(name.ToString());

            SyntaxTree tree = CSharpSyntaxTree.ParseText(
                                                        @"
                                                        using System;
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
                                                        }");

            //Get the first using `System.Collections` from the code and replace it with System.Collection.Generic
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var oldUsing = root.Usings[1];
            var newUsing = oldUsing.WithName(name);
            Console.WriteLine(newUsing.ToString());

            var modifiedRoot = root.ReplaceNode(oldUsing, newUsing);
            Console.WriteLine("Old code");
            Console.WriteLine(root.ToString().Normalize());
            Console.WriteLine("Modified code");
            Console.WriteLine(modifiedRoot.ToString().Normalize());

        }
    }
}
