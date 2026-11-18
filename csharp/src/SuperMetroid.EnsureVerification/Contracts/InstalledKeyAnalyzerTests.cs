using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2511 written-out installed key rule, SME6460.</summary>
internal static class InstalledKeyAnalyzerTests
{
    /// <summary>The marker, an installed-key record and the enum the fixtures use.</summary>
    private const string Types = """
        namespace SuperMetroid.Core
        {
            [System.AttributeUsage(System.AttributeTargets.Parameter | System.AttributeTargets.Property |
                System.AttributeTargets.Field | System.AttributeTargets.Method, Inherited = false)]
            public sealed class InstalledKeyAttribute : System.Attribute;
        }
        enum Glow { Dim, Bright }
        readonly record struct Shade([SuperMetroid.Core.InstalledKey] string Effect, int Step);

        """;

    /// <summary>The advice every SME6460 message ends with.</summary>
    private const string Advice = "; write the key out, as a literal or in a table of the identity's keys, so renaming the member never changes a name on disk";

    /// <summary>One positive per way an enum name reaches an installed key, and the written-out keys the rule leaves alone.</summary>
    internal static void VerifyInstalledKeyAnalyzer()
    {
        // An interpolation hole, plain, in either of its name formats, or of a nullable enum.
        Expect(Types + "class Art { Shade Of(Glow glow) => new($\"glow-{glow}\", 0); }",
            "'{glow}' writes the name of a 'Glow' member into the installed key 'Effect'" + Advice);
        Expect(Types + "class Art { Shade Of(Glow glow) => new($\"glow-{glow:G}\", 0); }", "'{glow:G}'");
        Expect(Types + "class Art { Shade Of(Glow? glow) => new($\"glow-{glow}\", 0); }", "a 'Glow' member");
        // ToString, with no format or a name format, and through a string transform (#2409's TorizoLook.Key).
        Expect(Types + "class Art { Shade Of(Glow glow) => new(glow.ToString(), 0); }", "'glow.ToString()'");
        Expect(Types + "class Art { Shade Of(Glow glow) => new(\"glow-\" + glow.ToString(\"F\"), 0); }", "'glow.ToString(\"F\")'");
        Expect(Types + "class Art { Shade Of(Glow glow) => new(glow.ToString().ToLowerInvariant(), 0); }", "'glow.ToString()'");
        // A concatenation, string.Concat, Format and Join, Enum.GetName and nameof of a member.
        Expect(Types + "class Art { Shade Of(Glow glow) => new(\"glow-\" + glow, 0); }", "'\"glow-\" + glow'");
        Expect(Types + "class Art { Shade Of(Glow glow) => new(string.Concat(\"glow-\", glow), 0); }", "a 'Glow' member");
        Expect(Types + "class Art { Shade Of(Glow glow) => new(string.Format(\"glow-{0}\", glow), 0); }", "a 'Glow' member");
        Expect(Types + "class Art { Shade Of(Glow glow) => new(string.Join(\"-\", \"glow\", glow), 0); }", "a 'Glow' member");
        Expect(Types + "class Art { Shade Of(Glow glow) => new(System.Enum.GetName(glow)!, 0); }", "a 'Glow' member");
        Expect(Types + "class Art { Shade Dim => new(nameof(Glow.Dim), 0); }", "'nameof(Glow.Dim)'");
        // string.Create's interpolated string handler, and a hole nested in a hole.
        Expect(Types + "class Art { Shade Of(Glow glow) => new(string.Create(System.Globalization.CultureInfo.InvariantCulture, $\"glow-{glow}\"), 0); }",
            "a 'Glow' member");
        Expect(Types + "class Art { Shade Of(Glow glow) => new($\"glow-{$\"{glow}\"}\", 0); }", "'{glow}'");
        // A conditional's branch, a coalesce and a switch expression's arm.
        Expect(Types + "class Art { Shade Of(Glow glow, bool lit) => new(lit ? $\"lit-{glow}\" : \"dark\", 0); }", "'{glow}'");
        Expect(Types + "class Art { Shade Of(Glow glow, string? named) => new(named ?? glow.ToString(), 0); }", "'glow.ToString()'");
        Expect(Types + "class Art { Shade Of(Glow glow, int place) => new(place switch { 0 => $\"first-{glow}\", _ => \"other\" }, 0); }",
            "'{glow}'");
        // A marked method's or property's result, a marked property's assignment and a marked field's initializer.
        Expect(Types + "class Art { [SuperMetroid.Core.InstalledKey] string Key(Glow glow) => $\"glow-{glow}\"; }",
            "into the installed key 'Key'" + Advice);
        Expect(Types + "class Art { [SuperMetroid.Core.InstalledKey] string Key(Glow glow) { return glow.ToString(); } }", "'Key'");
        Expect(Types + "class Art { Glow _glow; [SuperMetroid.Core.InstalledKey] string Key => $\"glow-{_glow}\"; }", "'Key'");
        Expect(Types + "class Art { [SuperMetroid.Core.InstalledKey] string Key { get; set; } = \"\"; void Set(Glow glow) => Key = $\"glow-{glow}\"; }",
            "'Key'");
        Expect(Types + "class Art { [SuperMetroid.Core.InstalledKey] static readonly string Key = $\"glow-{Glow.Dim}\"; }", "'Key'");
        // A marked parameter of a helper, and a local handed to a key.
        Expect(Types + "class Art { void Name([SuperMetroid.Core.InstalledKey] string name) { } void Run(Glow glow) => Name($\"glow-{glow}\"); }",
            "'name'");
        Expect(Types + "class Art { Shade Of(Glow glow) { string key = $\"glow-{glow}\"; return new(key, 0); } }", "'{glow}'");
        Expect(Types + "class Art { Shade Of(Glow glow) { string key; key = glow.ToString(); return new(key, 0); } }", "'glow.ToString()'");
        // The import pipeline is read too.
        Expect(Types + "class Importer { Shade Of(Glow glow) => new($\"glow-{glow}\", 0); }", "'{glow}'",
            assemblyName: InstalledKeyAnalyzer.ExtractionAssembly);

        // A written-out key, a table lookup and a constant are what the rule asks for.
        Expect(Types + """
            class Art
            {
                static readonly System.Collections.Generic.Dictionary<Glow, string> Keys = new() { [Glow.Dim] = "glow-Dim", [Glow.Bright] = "glow-Bright" };
                Shade Of(Glow glow) => new(Keys[glow], 0);
                Shade Lit(Glow glow) => new($"lit-{Keys[glow]}", 0);
                Shade Dim => new("glow-Dim", 0);
            }
            """, count: 0);
        // A numeric format writes the value, not the name; a number is not an enum.
        Expect(Types + "class Art { Shade Of(Glow glow, int step) => new($\"glow-{glow:D}-{step}\", step); }", count: 0);
        Expect(Types + "class Art { Shade Of(Glow glow) => new(glow.ToString(\"D\"), 0); }", count: 0);
        // Text that is no installed key: an exception message, an unmarked parameter, a diagnostic name.
        Expect(Types + "class Art { Shade Of(Glow glow) => throw new System.ArgumentException($\"No shade for {glow}.\"); }", count: 0);
        Expect(Types + "class Art { void Log(string text) { } void Run(Glow glow) => Log($\"glow {glow}\"); public override string ToString() => $\"{Glow.Dim}\"; }",
            count: 0);
        // An enum read in a condition chooses between written keys; a local never handed to a key is not one.
        Expect(Types + "class Art { Shade Of(Glow glow) => new($\"{(glow == Glow.Dim ? \"dim\" : \"bright\")}\", 0); }", count: 0);
        Expect(Types + "class Art { void Run(Glow glow) { string text = $\"glow {glow}\"; Log(text); } void Log(string text) { } }", count: 0);
        // A lambda inside a marked member returns its own result, not the member's.
        Expect(Types + "class Art { [SuperMetroid.Core.InstalledKey] string Key(Glow glow) { System.Func<string> text = () => $\"{glow}\"; return \"key\"; } }",
            count: 0);
        // Outside Core and the import the rule does not apply.
        Expect(Types + "class Tests { Shade Of(Glow glow) => new($\"glow-{glow}\", 0); }", count: 0, assemblyName: "SuperMetroid.Verification");
    }

    /// <summary>Compiles one source and compares the SME6460 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Frontend/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new InstalledKeyAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != InstalledKeyAnalyzer.EnumNameInKeyId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {InstalledKeyAnalyzer.EnumNameInKeyId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
