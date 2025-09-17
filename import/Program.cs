using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CommandLine;
using System.Text.Json.Serialization;

class FuncDoc
{
    [JsonPropertyName("name")]
    public string name { get; set; } = string.Empty;
    [JsonPropertyName("doc")]
    public string doc { get; set; } = string.Empty;
}

class GoDoc
{
    [JsonPropertyName("funComments")]
    public List<FuncDoc> funComments { get; set; } = new List<FuncDoc>();
}

class Options
{
    [Option('c', "cs", Required = true, HelpText = "C# source file path")]
    public string CsFile { get; set; } = string.Empty;

    [Option('j', "json", Required = true, HelpText = "Go comments JSON file path")]
    public string JsonFile { get; set; } = string.Empty;

    [Option("overwrite", Default = false, HelpText = "Overwrite existing comments (true) or keep old comments if GoDoc missing (false)")]
    public bool Overwrite { get; set; }

}

class Program
{
    private static readonly string eolSign = "\r\n";

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
           .WithParsed(options =>
           {
               SyncComments(options.CsFile, options.JsonFile, options.Overwrite);
           });
    }

    static void SyncComments(string csFilePath, string jsonFilePath, bool overwrite)
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

        var newRoot = root;

        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            var methodName = method.Identifier.Text;

            var existingTrivia = method.GetLeadingTrivia().ToFullString().Trim();
            FuncDoc docEntry = goDoc?.funComments?.Find(f => f.name == methodName);

            if (!overwrite && !string.IsNullOrEmpty(existingTrivia))
            {
                if (docEntry == null)
                {
                    continue;
                }
            }

            string summaryText = "/// <summary>" + eolSign;
            if (docEntry != null)
            {
                var docLines = docEntry.doc.Split('\n')
                                       .Select(line => "/// " + line);
                summaryText += string.Join(eolSign, docLines) + eolSign;
            }
            else
            {
                summaryText += "/// " + eolSign;
            }
            summaryText += "/// </summary>" + eolSign;

            string paramSection = string.Empty;
            if (method.ParameterList.Parameters.Count > 0)
            {
                var paramLines = method.ParameterList.Parameters
                                     .Select(p =>
                                        {
                                            var paramName = p.Identifier.Text;
                                            var paramType = p.Type?.ToString() ?? "UnknownType";
                                            return $"/// <param name=\"{paramName}\"><see cref=\"{paramType}\"/>parameter</param>";
                                        });
                paramSection = string.Join(eolSign, paramLines);
            }

            var returnType = method.ReturnType.ToString();
            string returnsLine = string.Empty;
            if (returnType != "void")
            {
                returnsLine = $"/// <returns><see cref=\"{returnType}\"/> return value</returns>";
            }

            var xmlCommentText = summaryText;

            if (!string.IsNullOrEmpty(paramSection))
            {
                xmlCommentText += paramSection + eolSign;
            }

            if (!string.IsNullOrEmpty(returnsLine))
            {
                xmlCommentText += returnsLine + eolSign;
            }

            var leadingTrivia = SyntaxFactory.ParseLeadingTrivia(xmlCommentText);

            var targetMethod = newRoot.DescendantNodes()
                              .OfType<MethodDeclarationSyntax>()
                              .First(m => m.Identifier.Text == methodName);

            var newMethod = targetMethod.WithLeadingTrivia(leadingTrivia);
            newRoot = newRoot.ReplaceNode(targetMethod, newMethod);
        }

        var delegates = root.DescendantNodes().OfType<DelegateDeclarationSyntax>();
        foreach (var del in delegates)
        {
            var delegateName = del.Identifier.Text;

            var existingTrivia = del.GetLeadingTrivia().ToFullString().Trim();
            FuncDoc docEntry = goDoc?.funComments?.Find(f => f.name == delegateName);

            if (!overwrite && !string.IsNullOrEmpty(existingTrivia))
            {
                if (docEntry == null)
                {
                    continue;
                }
            }

            string summaryText = "/// <summary>" + eolSign;
            if (docEntry != null && !string.IsNullOrWhiteSpace(docEntry.doc))
            {
                var docLines = docEntry.doc.Split('\n')
                                           .Select(line => "/// " + line.Trim());
                summaryText += string.Join(eolSign, docLines) + eolSign;
            }
            else
            {
                summaryText += "/// " + eolSign;
            }
            summaryText += "/// </summary>" + eolSign;

            string paramSection = string.Empty;
            if (del.ParameterList.Parameters.Count > 0)
            {
                var paramLines = del.ParameterList.Parameters
                    .Select(p =>
                    {
                        var paramName = p.Identifier.Text;
                        var paramType = p.Type?.ToString() ?? "UnknownType";
                        return $"/// <param name=\"{paramName}\"><see cref=\"{paramType}\"/> parameter</param>";
                    });
                paramSection = string.Join(eolSign, paramLines);
            }

            var returnType = del.ReturnType.ToString();
            string returnsLine = string.Empty;
            if (returnType != "void")
            {
                returnsLine = $"/// <returns><see cref=\"{returnType}\"/> return value</returns>";
            }

            string xmlCommentText = summaryText;
            if (!string.IsNullOrEmpty(paramSection))
                xmlCommentText += paramSection + eolSign;
            if (!string.IsNullOrEmpty(returnsLine))
                xmlCommentText += returnsLine + eolSign;

            var leadingTrivia = SyntaxFactory.ParseLeadingTrivia(xmlCommentText);

            var targetDelegate = newRoot.DescendantNodes()
                                  .OfType<DelegateDeclarationSyntax>()
                                  .First(d => d.Identifier.Text == delegateName);

            var newDelegate = targetDelegate.WithLeadingTrivia(leadingTrivia);
            newRoot = newRoot.ReplaceNode(targetDelegate, newDelegate);
        }

        var formattedCode = newRoot.NormalizeWhitespace(indentation: "    ", eol: eolSign)
                               .ToFullString();

        File.WriteAllText(csFilePath, formattedCode);
        Console.WriteLine("Comments synchronized successfully.");
    }
}
