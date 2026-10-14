using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer;

namespace SuperMetroid.EnsureVerification;

internal static class OwnedReflectionAnalyzerTests
{
    private const string Usings = "using System; using System.Collections.Generic; using System.Reflection; " +
        "using System.Runtime.CompilerServices; using SuperMetroid.Core; ";

    /// <summary>The fixture's own types: anything this compilation declares is owned.</summary>
    private const string Owned = "sealed class C { private int x; public int P { get; set; } private void M() { } } ";

    /// <summary>#622 owned-reflection rules: positive and negative cases for each diagnostic.</summary>
    internal static void VerifyOwnedReflectionAnalyzer()
    {
        const string reflection = OwnedReflectionAnalyzer.ReflectionId;

        // SME6370: any lookup through typeof(owned) or owned.GetType(), public or not.
        Expect("static class T { static object? R() => typeof(C).GetField(\"x\", BindingFlags.Instance | BindingFlags.NonPublic); }",
            reflection, 1, "C.GetField");
        Expect("static class T { static object? R() => typeof(C).GetProperty(\"P\"); }", reflection, 1, "C.GetProperty");
        Expect("static class T { static object? R(C c) => c.GetType().GetMethod(\"M\", BindingFlags.Instance | BindingFlags.NonPublic); }",
            reflection, 1, "C.GetMethod");
        Expect("static class T { static object R() => typeof(C).GetMethods(); }", reflection, 1);
        Expect("static class T { static object R() => typeof(C).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic); }", reflection, 1);
        Expect("static class T { static object R() => typeof(C).GetNestedTypes(); }", reflection, 1);
        Expect("static class T { static object R() => typeof(C[]).GetMembers(); }", reflection, 1);
        Expect("static class T { static object R() => typeof(C).GetTypeInfo().DeclaredFields; }", reflection, 1, "declared members of C");
        Expect("static class T { static object? R() => typeof(C).GetRuntimeProperty(\"P\"); }", reflection, 1);
        // A SuperMetroid assembly is owned too.
        Expect("static class T { static object R() => typeof(Ensure).GetMethods(); }", reflection, 1, "Ensure.GetMethods");
        // A receiver held in a Type variable may be owned: non-public lookups and unprovable masks are reported.
        Expect("static class T { static object? R(Type t) => t.GetField(\"x\", BindingFlags.Instance | BindingFlags.NonPublic); }",
            reflection, 1, "non-public members via GetField");
        Expect("static class T { static object R(Type t, BindingFlags f) => t.GetFields(f); }", reflection, 1);
        Expect("static class T { const BindingFlags Hidden = BindingFlags.Static | BindingFlags.NonPublic; static object R(Type t) => t.GetMethods(Hidden); }",
            reflection, 1);
        // Constructor bypasses and string type lookups.
        Expect("static class T { static object R() => RuntimeHelpers.GetUninitializedObject(typeof(C)); }", reflection, 1, "unconstructed C");
        Expect("static class T { static object R(Type t) => RuntimeHelpers.GetUninitializedObject(t); }", reflection, 1);
        Expect("static class T { static object? R(Type t) => Activator.CreateInstance(t, nonPublic: true); }", reflection, 1);
        Expect("static class T { static object? R(Type t) => Activator.CreateInstance(t, BindingFlags.Instance | BindingFlags.NonPublic, null, null, null); }",
            reflection, 1);
        Expect("static class T { static object? R() => typeof(C).Assembly.GetType(\"SuperMetroid.Core.Hidden\"); }", reflection, 1, "SuperMetroid.Core.Hidden");
        Expect("static class T { static object? R() => Type.GetType(\"SuperMetroid.Core.Hidden\"); }", reflection, 1);
        // Lambdas and local functions inherit their member's (missing) exemption; each site is reported.
        Expect("static class T { static void R() { Func<object?> f = () => typeof(C).GetField(\"x\", BindingFlags.Instance | BindingFlags.NonPublic); " +
            "object? L() => typeof(C).GetProperty(\"P\"); } }", reflection, 2);
        // An audit elsewhere in the file does not cover this member.
        Expect("static class T { [ReflectionAudit(\"metadata\")] static object A() => typeof(C).GetMethods(); static object R() => typeof(C).GetFields(); }",
            reflection, 1);

        // SME6371: UnsafeAccessor into an owned type, by instance parameter or return type.
        Expect("static class T { [UnsafeAccessor(UnsafeAccessorKind.Field, Name = \"x\")] static extern ref int X(C c); }",
            OwnedReflectionAnalyzer.UnsafeAccessorId, 1, "owned type C");
        Expect("static class T { [UnsafeAccessor(UnsafeAccessorKind.Constructor)] static extern C Make(); }",
            OwnedReflectionAnalyzer.UnsafeAccessorId, 1);

        // SME6372: an exemption must say why reflection is the subject; an unexplained one exempts nothing.
        Expect("static class T { [ReflectionAudit(\"\")] static void R() { } }", OwnedReflectionAnalyzer.UnexplainedAuditId, 1, "T.R()");
        Expect("[ReflectionAudit(\"  \")] static class T { }", OwnedReflectionAnalyzer.UnexplainedAuditId, 1);
        ExpectIds("static class T { [ReflectionAudit(\" \")] static object R() => typeof(C).GetMethods(); }",
            [OwnedReflectionAnalyzer.UnexplainedAuditId, reflection]);

        // Allowed: public lookups on types the project does not own.
        Expect("static class T { static object? R() => typeof(string).GetMethod(\"Trim\", Type.EmptyTypes); }");
        Expect("static class T { static object? R() => typeof(List<int>).GetProperty(\"Count\"); }");
        Expect("static class T { static object R(Type t) => t.GetProperties(BindingFlags.Instance | BindingFlags.Public); }");
        Expect("static class T { static object R(Type t) => t.GetMethods(); }");
        Expect("static class T { static object R(object value) => value.GetType().GetFields(); }");
        Expect("static class T { static object R<TValue>() => typeof(TValue).GetProperties(); }");
        Expect("static class T { static object? R() => typeof(C).Assembly.GetType(\"System.String\"); }");
        Expect("static class T { static object? R(Type t) => Activator.CreateInstance(t); }");
        Expect("static class T { static object R() => RuntimeHelpers.GetUninitializedObject(typeof(string)); }");
        Expect("static class T { [UnsafeAccessor(UnsafeAccessorKind.Field, Name = \"_items\")] static extern ref int[] Items(List<int> list); }");
        // Allowed: typed access, which is what #622 asks for.
        Expect("static class T { static int R(C c) => c.P; }");
        // Allowed: an explained audit on the member, an enclosing type, a property, or a constructor covers nested code.
        Expect("static class T { [ReflectionAudit(\"metadata shape is the subject\")] static object R() => typeof(C).GetFields(BindingFlags.Instance | BindingFlags.NonPublic); }");
        Expect("[ReflectionAudit(\"serializer\")] static class T { static object R() { Func<object> f = () => RuntimeHelpers.GetUninitializedObject(typeof(C)); return f(); } }");
        Expect("static class T { [ReflectionAudit(\"coverage\")] static object P => typeof(C).GetProperties(); }");
        Expect("sealed class T { [ReflectionAudit(\"discovery\")] T() { object L() => typeof(C).GetNestedTypes(); L(); } }");
        Expect("static class T { [ReflectionAudit(\"package\")] [UnsafeAccessor(UnsafeAccessorKind.Field, Name = \"x\")] static extern ref int X(C c); }");
        // The exemption is matched by name, so an assembly may declare its own attribute.
        Expect("namespace Local { sealed class ReflectionAuditAttribute(string reason) : Attribute { public string Reason { get; } = reason; } " +
            "static class T { [ReflectionAudit(\"local\")] static object R() => typeof(C).GetMethods(); } }");
    }

    private static void Expect(string source, string? id = null, int count = 0, string? message = null)
    {
        Diagnostic[] diagnostics = Analyze(source);
        if (id is null)
        {
            if (diagnostics.Length != 0)
                throw new InvalidOperationException($"Unexpected analyzer diagnostic: {diagnostics[0]} in {source}");
            return;
        }
        if (diagnostics.Length != count || diagnostics.Any(d => d.Id != id || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {id} diagnostic(s) containing '{message}', got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in {source}");
        }
    }

    private static void ExpectIds(string source, string[] ids)
    {
        Diagnostic[] diagnostics = Analyze(source);
        if (!diagnostics.Select(d => d.Id).Order(StringComparer.Ordinal).SequenceEqual(ids.Order(StringComparer.Ordinal)))
            throw new InvalidOperationException(
                $"Expected [{string.Join(", ", ids)}], got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in {source}");
    }

    private static Diagnostic[] Analyze(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(Usings + Owned + source, new CSharpParseOptions(LanguageVersion.Preview), path: "Case.cs");
        var compilation = CSharpCompilation.Create("OwnedReflectionCase", [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable,
                allowUnsafe: true));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))} in {source}");
        return [.. compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new OwnedReflectionAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult()];
    }
}
