using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>#2602: the game's code uses no reflection; each form the rule names is reported, and what is not reflection is not.</summary>
internal static class GameReflectionAnalyzerTests
{
    private const string Usings = "using System; using System.Collections.Generic; using System.Reflection; " +
        "using System.Runtime.CompilerServices; using System.Text.Json; using System.Text.Json.Nodes; " +
        "using System.Text.Json.Serialization.Metadata; ";

    /// <summary>The fixture's own types.</summary>
    private const string Owned = "sealed class C { private int x; public int P { get; set; } public int X => x; } enum E { A, B } ";

    /// <summary>Positive and negative cases for SME6489.</summary>
    internal static void VerifyGameReflectionAnalyzer()
    {
        // Member lookups, by name or by scan, and the System.Reflection members they hand back.
        Expect("static class T { static object? R() => typeof(C).GetProperty(\"P\"); }", 1, "Type.GetProperty");
        Expect("static class T { static object R() => typeof(C).GetMembers(); }", 1);
        Expect("static class T { static object? R(PropertyInfo p, C c) => p.GetValue(c); }", 1, "PropertyInfo.GetValue");
        Expect("static class T { static object? R(Type t) => t.GetField(\"x\", BindingFlags.Instance | BindingFlags.NonPublic); }", 3);
        // Type scanning and type lookup by name.
        Expect("static class T { static object R() => typeof(C).Assembly; }", 1, "carries Assembly");
        Expect("static class T { static object? R() => Type.GetType(\"C\"); }", 1, "Type.GetType");
        Expect("static class T { static object? R() => typeof(C).BaseType; }", 1, "Type.BaseType");
        Expect("static class T { static bool R(Type t) => typeof(C).IsAssignableFrom(t); }", 1, "Type.IsAssignableFrom");
        Expect("static class T { static object R(Action a) => a.Method; }", 1, "carries MethodInfo");
        // Construction and invocation that bypass the code.
        Expect("static class T { static object? R() => Activator.CreateInstance(typeof(C)); }", 1, "Activator.CreateInstance");
        Expect("static class T { static object R() => RuntimeHelpers.GetUninitializedObject(typeof(C)); }", 1, "GetUninitializedObject");
        Expect("static class T { static object? R(Delegate d) => d.DynamicInvoke(); }", 1, "DynamicInvoke");
        Expect("static class T { [UnsafeAccessor(UnsafeAccessorKind.Field, Name = \"x\")] static extern ref int X(C c); }", 1, "UnsafeAccessor");
        Expect("static class T { static void R() { dynamic d = new C(); d.P = 1; } }", 1, "dynamic");
        // Reflection-based serialization; serialization given a source-generated context's metadata is not.
        Expect("static class T { static string R() => JsonSerializer.Serialize(new C()); }", 1, "JsonSerializer.Serialize");
        Expect("static class T { static C? R(string s) => JsonSerializer.Deserialize<C>(s); }", 1, "JsonSerializer.Deserialize");
        Expect("static class T { static object R() => new DefaultJsonTypeInfoResolver(); }", 1, "DefaultJsonTypeInfoResolver");
        Expect("static class T { static string R(JsonTypeInfo<C> info) => JsonSerializer.Serialize(new C(), info); }");
        Expect("static class T { static C? R(string s, JsonTypeInfo<C> info) => JsonSerializer.Deserialize(s, info); }");
        // Not reflection: a type's name or identity, typeof, nameof, an enum's values, an applied attribute, the JSON DOM.
        Expect("static class T { static string R(object o) => o.GetType().Name + typeof(C).Name; }");
        Expect("static class T { static bool R(object o) => o.GetType() == typeof(C); }");
        Expect("static class T { static string R() => nameof(C.P); }");
        Expect("static class T { static E[] R() => Enum.GetValues<E>(); }");
        Expect("[Obsolete(\"fixture\")] static class T { }");
        Expect("static class T { static JsonNode? R() => JsonNode.Parse(\"{}\"); }");
        // The code-analysis tooling reflects by its nature.
        Expect("static class T { static object? R() => typeof(C).GetProperty(\"P\"); }", assembly: "SuperMetroid.ArchitectureAudit");
        Console.WriteLine("  SME6489: every form of reflection is reported in the game's code, none in the analysis tooling.");
    }

    private static void Expect(string source, int count = 0, string? message = null, string assembly = "SuperMetroid.Core")
    {
        var tree = CSharpSyntaxTree.ParseText(Usings + Owned + source, new CSharpParseOptions(LanguageVersion.Preview), path: "Case.cs");
        var compilation = CSharpCompilation.Create(assembly, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable,
                allowUnsafe: true));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))} in {source}");
        Diagnostic[] diagnostics = [.. compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new GameReflectionAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult()];
        if (diagnostics.Length != count || diagnostics.Any(d => d.Id != GameReflectionAnalyzer.ReflectionId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.Any(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {GameReflectionAnalyzer.ReflectionId} diagnostic(s) containing '{message}', got " +
                $"[{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in {source}");
        }
    }
}
