using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2511 written-out installed key rule, SME6460, and its #2520 sibling SME6462.</summary>
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

    /// <summary>The usings the #2520 fixtures' collections and JSON need, ahead of <see cref="Types"/>.</summary>
    private const string Usings = """
        using System.Collections.Generic;
        using System.Linq;
        using System.Text.Json.Serialization;

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

        // #2520: a member overriding a marked one is marked with it.
        Expect(Types + "abstract class Image { [SuperMetroid.Core.InstalledKey] public abstract string Frame { get; } } " +
            "sealed class Icon(Glow glow) : Image { public override string Frame => $\"icon-{glow}\"; }", "into the installed key 'Frame'" + Advice);
        // A value typed System.Enum writes its member's name too.
        Expect(Types + "class Art { [SuperMetroid.Core.InstalledKey] string Key(System.Enum reaction) => reaction.ToString(); }",
            "a 'Enum' member into the installed key 'Key'");
        // A JSON document writer writes what it is handed into the document.
        Expect(Types + "class Art { void Write(System.Text.Json.Utf8JsonWriter writer, Glow glow) => writer.WriteStringValue(glow.ToString()); }",
            "into the installed key 'WriteStringValue'");
        // A selector's result, and the collection gathered from it, reach the key they are handed to.
        Expect(Usings + Types + "class Art { [SuperMetroid.Core.InstalledKey] string[] Names(Glow[] glows) => [.. glows.Select(glow => glow.ToString())]; }",
            "'glow.ToString()' writes the name of a 'Glow' member into the installed key 'Names'");
        Expect(Usings + Types + "class Art { [SuperMetroid.Core.InstalledKey] string[] Names(IEnumerable<Glow> glows) => glows.Select(glow => $\"{glow}\").ToArray(); }",
            "into the installed key 'Names'");
        Expect(Usings + Types + "record Doc([SuperMetroid.Core.InstalledKey] IReadOnlyList<string> Glows); " +
            "class Art { Doc Of(Glow[] glows) => new([.. glows.Where(glow => glow != Glow.Dim).Select(glow => glow.ToString())]); }",
            "into the installed key 'Glows'");

        // #2544: a name helper whose result a caller hands to a key: string.Create's interpolated text, a property, a local
        // function, a helper returning another helper's result, a result held in a local, and a collection's key.
        Expect(Types + "static class Names { public static string Of(Glow glow) => string.Create(System.Globalization.CultureInfo.InvariantCulture, $\"glow {glow}\"); } " +
            "class Art { Shade Of(Glow glow) => new(Names.Of(glow), 0); }",
            "'{glow}' writes the name of a 'Glow' member into the installed key 'Effect'" + Advice);
        Expect(Types + "class Art { Glow _glow; string Name => $\"glow-{_glow}\"; Shade Of() => new(Name, 0); }", "into the installed key 'Effect'");
        Expect(Types + "class Art { Shade Of(Glow glow) { string Name() => glow.ToString(); return new(Name(), 0); } }", "'glow.ToString()'");
        Expect(Types + "static class Names { public static string Inner(Glow glow) => $\"{glow}\"; public static string Outer(Glow glow) => \"glow-\" + Inner(glow); } " +
            "class Art { Shade Of(Glow glow) => new(Names.Outer(glow), 0); }", "'{glow}'");
        Expect(Types + "static class Names { public static string Of(Glow glow) { string name = $\"glow-{glow}\"; return name; } } " +
            "class Art { Shade Of(Glow glow) { string key = Names.Of(glow); return new(key, 0); } }", "'{glow}'");
        Expect(Types + "static class Names { public static string Of(Glow glow) => $\"{glow}\"; } " +
            "class Art { void Name([SuperMetroid.Core.InstalledKey] string name) { } void Run(Glow glow) => Name(Names.Of(glow)); }", "'name'");
        ExpectKeyed(Usings + Types + "static class Names { public static string Of(Glow glow) => glow.ToString(); } " +
            "class Art { readonly Dictionary<string, int> anchors = []; bool Has(Glow glow) => anchors.ContainsKey(Names.Of(glow)); }",
            "'glow.ToString()' keys the collection of 'anchors'");
        // A helper whose result no caller hands to a key, a helper spelling a written-out key, and one whose caller only
        // shows its text are none; a helper of another assembly is that assembly's to report.
        Expect(Types + "static class Names { public static string Of(Glow glow) => $\"glow {glow}\"; } " +
            "class Art { void Log(string text) { } void Run(Glow glow) => Log(Names.Of(glow)); }", count: 0);
        Expect(Types + "static class Names { static readonly System.Collections.Generic.Dictionary<Glow, string> Keys = new() { [Glow.Dim] = \"Dim\", [Glow.Bright] = \"Bright\" }; " +
            "public static string Of(Glow glow) => string.Create(System.Globalization.CultureInfo.InvariantCulture, $\"glow {Keys[glow]}\"); } " +
            "class Art { Shade Of(Glow glow) => new(Names.Of(glow), 0); }", count: 0);
        Expect(Types + "static class Names { public static string Of(Glow glow) => $\"glow {glow}\"; } " +
            "class Art { Shade Of(Glow glow) => throw new System.ArgumentException(Names.Of(glow)); }", count: 0);

        // SME6462: an enum member's name made a dictionary's or set's key, through a local or a member, read or written.
        ExpectKeyed(Usings + Types + "class Art { void Run(Glow glow) { var anchors = new Dictionary<string, int>(); anchors.Add(glow.ToString(), 1); } }",
            "'glow.ToString()' keys the collection of 'anchors': it spells a 'Glow' by its members' names");
        ExpectKeyed(Usings + Types + "class Art { readonly Dictionary<string, int> anchors = []; bool Has(Glow glow) => anchors.ContainsKey($\"{glow}\"); }",
            "'{glow}' keys the collection of 'anchors'");
        ExpectKeyed(Usings + Types + "class Art { int At(Dictionary<string, int> anchors, Glow glow) => anchors[glow.ToString()]; }",
            "keys the collection of 'anchors'");
        ExpectKeyed(Usings + Types + "class Art { bool Has(IReadOnlyDictionary<string, int> anchors) => anchors.TryGetValue(nameof(Glow.Dim), out _); }",
            "'nameof(Glow.Dim)'");
        ExpectKeyed(Usings + Types + "class Art { void Run(Glow glow) { var seen = new HashSet<string>(); string key = glow.ToString(); seen.Add(key); } }",
            "keys the collection of 'seen'");
        ExpectKeyed(Usings + Types + "class Art { Dictionary<string, Glow> ByName => System.Enum.GetValues<Glow>().ToDictionary(glow => glow.ToString()); }",
            "'glow.ToString()' keys the collection of 'System.Enum.GetValues<Glow>().ToDictionary(glow => glow.ToString())'");
        // A key selector of any query names it so, the repository's span queries among them.
        ExpectKeyed(Usings + Types + "static class Spans { public static Dictionary<TKey, T> Keyed<T, TKey>(this T[] items, System.Func<T, TKey> keySelector) " +
            "where TKey : notnull => items.ToDictionary(keySelector); } class Art { Dictionary<string, Glow> Of(Glow[] glows) => glows.Keyed(glow => $\"{glow}\"); }",
            "'{glow}' keys the collection of");
        // Enum.Parse, Enum.TryParse and Enum.GetNames read and list an identity by its names.
        ExpectKeyed(Types + "class Art { Glow Of(string name) => System.Enum.Parse<Glow>(name); }", "reads text: it spells a 'Glow'");
        ExpectKeyed(Types + "class Art { bool Of(string name, out Glow glow) => System.Enum.TryParse(name, ignoreCase: false, out glow); }",
            "reads text: it spells a 'Glow'");
        ExpectKeyed(Types + "class Art { string[] Names => System.Enum.GetNames<Glow>(); }", "lists an identity's names: it spells a 'Glow'");
        // JsonStringEnumConverter in any form: constructed, generic, or named by a converter attribute.
        ExpectKeyed(Types + "class Art { System.Text.Json.JsonSerializerOptions Options => new() { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } }; }",
            "converts JSON: it spells a 'enum'");
        ExpectKeyed(Types + "class Art { object Converter => new System.Text.Json.Serialization.JsonStringEnumConverter<Glow>(); }",
            "converts JSON: it spells a 'Glow'");
        ExpectKeyed(Usings + Types + "class Doc { [JsonConverter(typeof(JsonStringEnumConverter<Glow>))] public Glow Glow { get; init; } }",
            "converts JSON: it spells a 'Glow'");
        ExpectKeyed(Types + "class Importer { Glow Of(string name) => System.Enum.Parse<Glow>(name); }", "reads text", assemblyName: InstalledKeyAnalyzer.ExtractionAssembly);

        // A collection keyed by the identity itself, a written-out key, a numeric format, a value and a message are none.
        ExpectKeyed(Usings + Types + """
            static class GlowKeys { public static string Of(Glow glow) => glow == Glow.Dim ? "Dim" : "Bright"; }
            class Art
            {
                readonly Dictionary<Glow, int> byGlow = [];
                readonly Dictionary<string, string> byKey = [];
                void Run(Glow glow)
                {
                    byGlow.Add(glow, 1);
                    byKey.Add(GlowKeys.Of(glow), "value");
                    byKey.Add(glow.ToString("D"), "number");
                    byKey.Add("value", glow.ToString());
                    _ = System.Enum.GetValues<Glow>().ToDictionary(GlowKeys.Of);
                    if (!byKey.ContainsKey(GlowKeys.Of(glow)))
                        throw new System.InvalidOperationException($"No entry for {glow}.");
                }
            }
            """, count: 0);
        // A converter of the identity's own keys is the codec the rule asks for; outside Core and the import it does not apply.
        ExpectKeyed(Types + "sealed class GlowJson : System.Text.Json.Serialization.JsonConverter<Glow> { " +
            "public override Glow Read(ref System.Text.Json.Utf8JsonReader reader, System.Type type, System.Text.Json.JsonSerializerOptions options) => Glow.Dim; " +
            "public override void Write(System.Text.Json.Utf8JsonWriter writer, Glow value, System.Text.Json.JsonSerializerOptions options) => writer.WriteStringValue(\"Dim\"); }",
            count: 0);
        ExpectKeyed(Types + "class Tests { Glow Of(string name) => System.Enum.Parse<Glow>(name); }", count: 0, assemblyName: "SuperMetroid.Verification");
    }

    /// <summary>Compiles one source and compares the SME6460 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly) =>
        Expect(InstalledKeyAnalyzer.EnumNameInKeyId, source, message, count, assemblyName);

    /// <summary>Compiles one source and compares the SME6462 count and an optional message fragment.</summary>
    private static void ExpectKeyed(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly) =>
        Expect(InstalledKeyAnalyzer.EnumNameKeyedId, source, message, count, assemblyName);

    /// <summary>Compiles one source and compares the count of rule <paramref name="id"/>'s findings and an optional message fragment.</summary>
    private static void Expect(string id, string source, string? message, int count, string assemblyName)
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
            diagnostics.Any(d => d.Id != id || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {id} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
