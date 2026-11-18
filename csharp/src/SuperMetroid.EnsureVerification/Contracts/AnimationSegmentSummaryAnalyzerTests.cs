using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2309 animation segment summary rule, SME6459.</summary>
internal static class AnimationSegmentSummaryAnalyzerTests
{
    /// <summary>The advice every SME6459 message ends with.</summary>
    private const string Advice = "' only restates its name; say what this run of the animation shows";

    /// <summary>One positive per form a restating summary takes, and the summaries and enums the rule leaves alone.</summary>
    internal static void VerifyAnimationSegmentSummaryAnalyzer()
    {
        // The name as an animation, the name alone, and the loop.
        Expect(Segment("/// <summary>The facing left shooting animation.</summary>", "FacingLeftShooting"),
            "The summary of 'FacingLeftShooting" + Advice);
        Expect(Segment("/// <summary>The idle.</summary>", "Idle"), "The summary of 'Idle" + Advice);
        Expect(Segment("/// <summary>The loop.</summary>", "FollowerLoop"), "The summary of 'FollowerLoop" + Advice);
        // A run of capitals and a digit split into words, case and spacing ignored, and a summary over several lines.
        Expect(Segment("/// <summary>the  ui idle 2 animation.</summary>", "UIIdle2"), "The summary of 'UIIdle2" + Advice);
        Expect(Segment("/// <summary>\n    /// The power up\n    /// facing right animation.\n    /// </summary>", "PowerUpFacingRight"),
            "The summary of 'PowerUpFacingRight" + Advice);
        // The name's words in any order and punctuation, with the actor's name, articles, possessives and "animation"
        // (#2465), and a part named twice.
        Expect(Segment("/// <summary>Zoa body animation: facing left, reset.</summary>", "BodyFacingLeftReset"),
            "The summary of 'BodyFacingLeftReset" + Advice);
        Expect(Segment("/// <summary>Zoa tail animation: facing left, fake tail whip.</summary>", "TailFacingLeftInitialFakeWhip"),
            "The summary of 'TailFacingLeftInitialFakeWhip" + Advice);
        Expect(Segment("/// <summary>Zoa animation: delete.</summary>", "Delete"), "The summary of 'Delete" + Advice);
        Expect(Segment("/// <summary>His left-facing idle animation.</summary>", "IdleFacingLeft"), "The summary of 'IdleFacingLeft" + Advice);
        Expect(Segment("/// <summary>Ceres Zoa's left-facing lunge.</summary>", "CeresLungeFacingLeft"), "The summary of 'CeresLungeFacingLeft" + Advice);

        // A summary that says what the run shows, one that only begins with the name, one holding a reference, one of
        // only the actor's and restating words, and one missing are not restatements.
        Expect(Segment("/// <summary>Zoa animation: a rest on the last frame.</summary>", "Rest"), count: 0);
        Expect(Segment("/// <summary>The Zoa animation.</summary>", "Rest"), count: 0);
        Expect(Segment("/// <summary>Darting left, its tail tucked, then kicking, repeating.</summary>", "FacingLeftShooting"), count: 0);
        Expect(Segment("/// <summary>The idle pose held while it waits for Samus.</summary>", "Idle"), count: 0);
        Expect(Segment("/// <summary>The idle animation of <see cref=\"Other\"/>.</summary>", "Idle"), count: 0);
        Expect("enum ZoaSegment { Idle }\nclass Other { }", count: 0);
        // An enum that is not an animation segment's, and the suffix alone, are not read.
        Expect("enum Facing\n{\n    /// <summary>The left.</summary>\n    Left,\n}", count: 0);
        Expect("enum Segment\n{\n    /// <summary>The left.</summary>\n    Left,\n}", count: 0);
        // Outside Core the rule does not apply.
        Expect(Segment("/// <summary>The idle animation.</summary>", "Idle"), count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>A segment enum whose one member carries <paramref name="documentation"/>, with a class its references can name.</summary>
    private static string Segment(string documentation, string member) =>
        $"enum ZoaSegment\n{{\n    {documentation}\n    {member},\n}}\nclass Other {{ }}";

    /// <summary>Compiles one source and compares the SME6459 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview, DocumentationMode.Diagnose),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Enemies/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new AnimationSegmentSummaryAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != AnimationSegmentSummaryAnalyzer.RestatedSegmentNameId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {AnimationSegmentSummaryAnalyzer.RestatedSegmentNameId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
