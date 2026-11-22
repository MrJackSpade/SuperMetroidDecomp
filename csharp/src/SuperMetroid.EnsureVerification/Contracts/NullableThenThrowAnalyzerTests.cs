using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2554 rule that a required value is never held nullable and thrown on, SME6475.</summary>
internal static class NullableThenThrowAnalyzerTests
{
    /// <summary>The types the fixtures use.</summary>
    private const string Types = """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        sealed class Body { }
        sealed class Look { public Body? Band { get; init; } public Body Required { get; init; } = new(); }

        """;

    /// <summary>The advice every SME6475 message ends with.</summary>
    private const string Advice = "is held nullable and thrown on when absent";

    /// <summary>One positive per form a throw on absence takes and per kind of value, and the reads the rule leaves alone.</summary>
    internal static void VerifyNullableThenThrowAnalyzer()
    {
        // A field, a property, the field keyword, a parameter and a local, coalesced into a throw.
        Expect(Types + "class Boss { Body? _body; Body Read() => _body ?? throw new InvalidOperationException(); }", "'_body' " + Advice);
        Expect(Types + "class Boss { Body Read(Look look) => look.Band ?? throw new InvalidOperationException(); }", "'look.Band' " + Advice);
        Expect(Types + "class Boss { Body? Shown { get => field ?? throw new InvalidOperationException(); set; } }", "'field' " + Advice);
        Expect(Types + "class Boss { Body Read(Body? body) => body ?? throw new ArgumentNullException(nameof(body)); }", "'body' " + Advice);
        Expect(Types + "class Boss { Body Read(Body[] all) { Body? first = null; foreach (Body b in all) first ??= b; return first ?? throw new InvalidOperationException(); } }",
            "'first' " + Advice);
        // A source method's result, a delegate's, a downcast and an element of a source collection.
        Expect(Types + "class Boss { Body? Find() => null; Body Read() => Find() ?? throw new InvalidOperationException(); }", "'Find()' " + Advice);
        Expect(Types + "class Boss { Body Read(Func<int, Body?> find) => find(1) ?? throw new InvalidOperationException(); }", "'find(1)' " + Advice);
        Expect(Types + "class Boss { Body Read(object shown) => shown as Body ?? throw new InvalidOperationException(); }", "'shown as Body' " + Advice);
        Expect(Types + "class Boss { Body?[] _bodies = []; Body Read(int i) => _bodies[i] ?? throw new InvalidOperationException(); }", "'_bodies[i]' " + Advice);
        Expect(Types + "class Boss { Dictionary<int, Body?> _bodies = []; Body Read(int i) => _bodies[i] ?? throw new InvalidOperationException(); }",
            "'_bodies[i]' " + Advice);
        Expect(Types + "class Boss { Look? _look; Body Read() => _look?.Band ?? throw new InvalidOperationException(); }", "'_look?.Band' " + Advice);
        // An if statement, a conditional expression, a positive pattern, a case and a switch arm.
        Expect(Types + "class Boss { Body? _body; Body Read() { if (_body is null) throw new InvalidOperationException(); return _body; } }", "'_body' " + Advice);
        Expect(Types + "class Boss { Body? _body; Body Read() { if (_body == null) { throw new InvalidOperationException(); } return _body; } }", "'_body' " + Advice);
        Expect(Types + "class Boss { Body? _body; Body Read() { if (_body is not { } body) throw new InvalidOperationException(); return body; } }", "'_body' " + Advice);
        Expect(Types + "class Boss { Body? _body; Body Read() { if (_body is not Body body) throw new InvalidOperationException(); return body; } }", "'_body' " + Advice);
        Expect(Types + "class Boss { Body? _body; Body Read() => _body is { } body ? body : throw new InvalidOperationException(); }", "'_body' " + Advice);
        Expect(Types + "class Boss { Body? _body; Body Read() => _body != null ? _body : throw new InvalidOperationException(); }", "'_body' " + Advice);
        Expect(Types + "class Boss { int? _band; int Read() { switch (_band) { case null: throw new InvalidOperationException(); default: return _band.Value; } } }",
            "'_band' " + Advice);
        Expect(Types + "class Boss { int? _band; int Read() => _band switch { null => throw new InvalidOperationException(), int band => band }; }",
            "'_band' " + Advice);
        // A value type held nullable is the same shape, and the import is held to the rule alongside Core.
        Expect(Types + "class Boss { int? _band; int Read() => _band ?? throw new InvalidOperationException(); }", "'_band' " + Advice);
        Expect(Types + "class Importer { Body? _body; Body Read() => _body ?? throw new InvalidOperationException(); }", "'_body' " + Advice,
            assemblyName: NullableThenThrowAnalyzer.ExtractionAssembly);

        // A library's nullable result is that library's contract, checked where it is called.
        Expect(Types + "class Boss { string Read(string path) => System.IO.Path.GetDirectoryName(path) ?? throw new InvalidOperationException(); }", count: 0);
        Expect(Types + "class Boss { Body Read(List<Body> all) => all.FirstOrDefault() ?? throw new InvalidOperationException(); }", count: 0);
        Expect(Types + "class Boss { Body Read(Dictionary<int, Body> all) => all.GetValueOrDefault(1) ?? throw new InvalidOperationException(); }", count: 0);
        // A guard whose parameter is marked to be non-null on return exists to throw on null.
        Expect(Types + "class Guard { static T NotNull<T>([System.Diagnostics.CodeAnalysis.NotNull] T? value) where T : class => value ?? throw new ArgumentNullException(nameof(value)); }",
            count: 0);
        // A value whose type the source declares present is not held nullable, even where a test checks it against outside data.
        Expect(Types + "class Importer { Body Read(Look look) => look.Required ?? throw new InvalidOperationException(); }", count: 0,
            assemblyName: NullableThenThrowAnalyzer.ExtractionAssembly);
        Expect(Types + "class Importer { Body[] _bodies = []; Body Read(int i) => _bodies[i] ?? throw new InvalidOperationException(); }", count: 0,
            assemblyName: NullableThenThrowAnalyzer.ExtractionAssembly);
        Expect(Types + "class Boss { Body Read(Body body) { if (body is null) throw new InvalidOperationException(); return body; } }", count: 0);
        // An absence that is not thrown on, and a throw on something other than absence, are other code.
        Expect(Types + "class Boss { Body? _body; Body Read() => _body ?? new Body(); Body Other() { if (_body is null) return new Body(); return _body; } }", count: 0);
        Expect(Types + "class Boss { Body? _body; bool _ready; Body Read() { if (!_ready) throw new InvalidOperationException(); return _body ?? new Body(); } }", count: 0);
        Expect(Types + "class Boss { object? _shown; Body Read() { if (_shown is not Body body) throw new InvalidOperationException(); return body; } }", count: 0);
        Expect(Types + "class Boss { int? _band; int Read() => _band switch { null => 0, int band => band }; }", count: 0);
        // Outside Core and the import the rule does not apply.
        Expect(Types + "class Host { Body? _body; Body Read() => _body ?? throw new InvalidOperationException(); }", count: 0,
            assemblyName: "SuperMetroid.Desktop");
    }

    /// <summary>Compiles one source and compares the SME6475 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Bosses/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new NullableThenThrowAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != NullableThenThrowAnalyzer.NullableThenThrowId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {NullableThenThrowAnalyzer.NullableThenThrowId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
