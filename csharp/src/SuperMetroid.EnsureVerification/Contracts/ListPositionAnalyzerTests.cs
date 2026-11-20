using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2556 list-position rule, SME6482.</summary>
internal static class ListPositionAnalyzerTests
{
    /// <summary>The place type and attributes the fixtures use.</summary>
    private const string Types = """
        using System;
        using System.Collections.Generic;
        sealed class EncodingOwnerAttribute(string format) : Attribute { public string Format { get; } = format; }
        sealed class CompatibilityServiceAttribute(string behaviour) : Attribute { public string Behaviour { get; } = behaviour; }
        sealed class Place(int ordinal) { public int Ordinal { get; } = ordinal; public Place[] All { get; } = []; }

        """;

    /// <summary>The advice every SME6482 message ends with.</summary>
    private const string Advice = "reads a place's position in its list";

    /// <summary>One positive per way a position is read, and the reads the rule leaves alone.</summary>
    internal static void VerifyListPositionAnalyzer()
    {
        // A partner found by counting places from another.
        Expect(Types + "class Behavior { Place Partner(Place part) => part.All[part.Ordinal - 1]; }", "'part.Ordinal'");
        // A place number handed to a lookup, stored, or compared.
        Expect(Types + "class Behavior { bool Done(Func<int, bool> loaded, Place place) => loaded(place.Ordinal); }", "'place.Ordinal'");
        Expect(Types + "class Behavior { int _held; void Hold(Place place) => _held = place.Ordinal; }", "'place.Ordinal'");
        Expect(Types + "class Behavior { bool First(Place place) => place.Ordinal == 0; }", Advice);
        // Spelling the number in an interpolated message is not a read, nor is the declaration's own assignment.
        Expect(Types + "class Behavior { string Name(Place place) => $\"enemy place {place.Ordinal}\"; }", count: 0);
        Expect(Types + "class Behavior { string Name(Place place, int offset) => $\"place {place.Ordinal + offset}\"; }", count: 0);
        Expect(Types + "sealed class Slot { public Slot(int ordinal) { Ordinal = ordinal; } public int Ordinal { get; } }", count: 0);
        // The single owner of the listing and a compatibility service may read it.
        Expect(Types + "[EncodingOwner(\"a population's listing\")] static class Listing { public static Place Next(Place part) => part.All[part.Ordinal + 1]; }", count: 0);
        Expect(Types + "[CompatibilityService(\"a reach by placement\")] static class Reach { public static int Of(Place part) => part.Ordinal; }", count: 0);
        // Another member named Ordinal that is no position, and a member of another assembly, are not read here.
        Expect(Types + "class Behavior { StringComparer Comparer => StringComparer.Ordinal; }", count: 0);
        // Outside Core the rule does not apply.
        Expect(Types + "class Importer { int Read(Place place) => place.Ordinal; }", count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>Compiles one source and compares the SME6482 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Enemies/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new ListPositionAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != ListPositionAnalyzer.ListPositionId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {ListPositionAnalyzer.ListPositionId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
