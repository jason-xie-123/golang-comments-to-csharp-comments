using System.Collections.Generic;
using Xunit;

public class SyncCommentsCodeTests
{
    private static GoDoc DocWith(params (string name, string doc)[] entries)
    {
        var goDoc = new GoDoc();
        foreach (var (name, doc) in entries)
        {
            goDoc.funComments.Add(new FuncDoc { name = name, doc = doc });
        }
        return goDoc;
    }

    [Fact]
    public void InjectsSummaryForMethodWithoutExistingDoc()
    {
        const string code = @"
class Sample
{
    public void DoThing() { }
}
";
        var goDoc = DocWith(("DoThing", "Does the thing."));

        var result = Program.SyncCommentsCode(code, goDoc, overwrite: false);

        Assert.Contains("/// <summary>", result);
        Assert.Contains("Does the thing.", result);
        Assert.Contains("/// </summary>", result);
    }

    [Fact]
    public void InjectsParamAndReturnsTags()
    {
        const string code = @"
class Sample
{
    public int Add(int left, int right) { return left + right; }
}
";
        var goDoc = DocWith(("Add", "Adds two numbers."));

        var result = Program.SyncCommentsCode(code, goDoc, overwrite: false);

        // Roslyn's NormalizeWhitespace reformats attribute spacing inside the doc
        // comment text (e.g. `name="left"` -> `name = "left"`), so assert on the
        // pieces rather than one exact literal string.
        Assert.Contains("<param name", result);
        Assert.Contains("\"left\"", result);
        Assert.Contains("<param name", result);
        Assert.Contains("\"right\"", result);
        Assert.Contains("<returns>", result);
    }

    [Fact]
    public void LeavesMethodWithExistingDocUntouchedWhenNotOverwritingAndNoMatch()
    {
        const string code = @"
class Sample
{
    /// <summary>
    /// Original doc.
    /// </summary>
    public void DoThing() { }
}
";
        var goDoc = DocWith(); // no matching entry for DoThing

        var result = Program.SyncCommentsCode(code, goDoc, overwrite: false);

        Assert.Contains("Original doc.", result);
    }

    [Fact]
    public void OverwritesExistingDocWhenOverwriteIsTrue()
    {
        const string code = @"
class Sample
{
    /// <summary>
    /// Original doc.
    /// </summary>
    public void DoThing() { }
}
";
        var goDoc = DocWith(("DoThing", "Updated doc."));

        var result = Program.SyncCommentsCode(code, goDoc, overwrite: true);

        Assert.Contains("Updated doc.", result);
        Assert.DoesNotContain("Original doc.", result);
    }

    [Fact]
    public void UsesInnermostGenericTypeInReturnsTag()
    {
        const string code = @"
using System.Threading.Tasks;

class Sample
{
    public Task<Widget> Build() { return null!; }
}
";
        var goDoc = DocWith(("Build", "Builds a widget."));

        var result = Program.SyncCommentsCode(code, goDoc, overwrite: false);

        // The <returns> tag should reference the innermost type (Widget), not the
        // full generic type (Task<Widget>).
        Assert.Contains("<see cref", result);
        Assert.Contains("\"Widget\"", result);
        Assert.DoesNotContain("cref = \"Task", result);
        Assert.DoesNotContain("cref=\"Task", result);
    }
}
