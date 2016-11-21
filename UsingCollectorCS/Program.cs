using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace UsingCollectorCS
{
    /// <summary>
    /// Program to collection using statements that do not start with System
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(
                                                        @"using System;
                                                        using System.Collections.Generic;
                                                        using System.Linq;
                                                        using System.Text;
                                                        using Microsoft.CodeAnalysis;
                                                        using Microsoft.CodeAnalysis.CSharp;

                                                        namespace TopLevel
                                                        {
                                                            using Microsoft;
                                                            using System.ComponentModel;

                                                            namespace Child1
                                                            {
                                                                using Microsoft.Win32;
                                                                using System.Runtime.InteropServices;

                                                                class Foo { }
                                                            }

                                                            namespace Child2
                                                            {
                                                                using System.CodeDom;
                                                                using Microsoft.CSharp;

                                                                class Bar { }
                                                            }
                                                        }");

            var root = tree.GetRoot();
            var collector = new UsingCollector();
            collector.Visit(root);

            foreach (var directive in collector.Usings)
            {
                Console.WriteLine(directive.Name);
            }
        }
    }
}