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

class Godoc
{
    public List<FuncDoc> funcs { get; set; } = new List<FuncDoc>();
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
        var godoc = JsonSerializer.Deserialize<Godoc>(json);

        string code = File.ReadAllText(csFilePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        var newRoot = root;

        foreach (var method in methods)
        {
            var methodName = method.Identifier.Text;

            if (godoc?.funcs == null) continue;

            var docEntry = godoc.funcs.Find(f => f.name == methodName);
            if (docEntry == null) continue;

            var docLines = docEntry.doc.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(line => "/// " + line.Trim());
            var xmlCommentText = "/// <summary>\n" + string.Join("\n", docLines) + "\n/// </summary>\n";

            var leadingTrivia = SyntaxFactory.ParseLeadingTrivia(xmlCommentText);

            var newMethod = method.WithLeadingTrivia(leadingTrivia);
            newRoot = newRoot.ReplaceNode(method, newMethod);
        }

        File.WriteAllText(csFilePath, newRoot.NormalizeWhitespace().ToFullString());
        Console.WriteLine("Comments synchronized successfully.");
    }
}
