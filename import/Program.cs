using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CommandLine;

class FuncDoc
{
    public string name { get; set; } = string.Empty;
    public string doc { get; set; } = string.Empty;
}

class GoDoc
{
    public List<FuncDoc> funDocs { get; set; } = new List<FuncDoc>();
}

class Options
{
    [Option('c', "cs", Required = true, HelpText = "C# source file path")]
    public string CsFile { get; set; } = string.Empty;

    [Option('j', "json", Required = true, HelpText = "Go comments JSON file path")]
    public string JsonFile { get; set; } = string.Empty;
}

class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
           .WithParsed(options =>
           {
               SyncComments(options.CsFile, options.JsonFile);
           });
    }

    static void SyncComments(string csFilePath, string jsonFilePath)
    {
        if (!File.Exists(csFilePath))
        {
            Console.WriteLine("CSharpFile.cs does not exist");
            return;
        }

        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine("GoJsonFile.json does not exist");
            return;
        }

        var json = File.ReadAllText(jsonFilePath);
        var goDoc = JsonSerializer.Deserialize<GoDoc>(json);

        string code = File.ReadAllText(csFilePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        var newRoot = root;

        foreach (var method in methods)
        {
            var methodName = method.Identifier.Text;

            if (goDoc?.funDocs == null) continue;

            var docEntry = goDoc.funDocs.Find(f => f.name == methodName);
            if (docEntry == null) continue;

            var paramLines = method.ParameterList.Parameters
                                 .Select(p =>
                                    {
                                        var paramName = p.Identifier.Text;
                                        var paramType = p.Type?.ToString() ?? "UnknownType";
                                        return $"/// <param name=\"{paramName}\"><see cref=\"{paramType}\"/>parameter</param>";
                                    });
            var docLines = docEntry.doc.Split('\n')
                                       .Select(line => "/// " + line);
            var xmlCommentText = "/// <summary>\n"
                         + string.Join("\n", docLines) + "\n"
                         + "/// </summary>\n"
                         + string.Join("\n", paramLines) + "\n";

            var leadingTrivia = SyntaxFactory.ParseLeadingTrivia(xmlCommentText);

            var targetMethod = newRoot.DescendantNodes()
                              .OfType<MethodDeclarationSyntax>()
                              .First(m => m.Identifier.Text == methodName);

            var newMethod = targetMethod.WithLeadingTrivia(leadingTrivia);
            newRoot = newRoot.ReplaceNode(targetMethod, newMethod);
        }

        File.WriteAllText(csFilePath, newRoot.NormalizeWhitespace().ToFullString());
        Console.WriteLine("Comments synchronized successfully.");
    }
}
