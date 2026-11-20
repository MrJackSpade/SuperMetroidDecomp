using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2570 private-state member rule, SME6477.</summary>
internal static class PrivateStateMemberAnalyzerTests
{
    /// <summary>One positive per kind of named member and way of stating the owner, and the calls the rule leaves alone.</summary>
    internal static void VerifyPrivateStateMemberAnalyzer()
    {
        // A name the owner lacks, for each kind of call and each way the owner is stated.
        Expect(Call("PrivateState.Get<Owner, int>(owner, \"Missing\");"), "names the property 'Missing', which Owner does not have");
        Expect(Call("PrivateState.Set(owner, \"Missing\", 1);"), "names the property 'Missing', which Owner does not have");
        Expect(Call("PrivateState.Field<Owner, int>(owner, \"missing\");"), "names the field 'missing', which Owner does not have");
        Expect(Call("PrivateState.ValueField<Value, int>(value, \"missing\");"), "names the field 'missing', which Value does not have");
        Expect(Call("PrivateState.Invoke<Owner, int>(owner, \"Missing\", [], []);"), "names the method 'Missing', which Owner does not have");
        Expect(Call("PrivateState.StaticGet<int>(typeof(Owner), \"Missing\");"), "names the property 'Missing', which Owner does not have");
        Expect(Call("PrivateState.InvokeStatic<int>(typeof(Owner), \"Missing\", [], []);"), "names the method 'Missing', which Owner does not have");
        // A member of another kind does not stand in: the reflection behind each call looks up one kind.
        Expect(Call("PrivateState.Get<Owner, int>(owner, \"count\");"), "names the property 'count', which Owner does not have");
        Expect(Call("PrivateState.Field<Owner, int>(owner, \"Count\");"), "names the field 'Count', which Owner does not have");
        Expect(Call("PrivateState.Invoke<Owner, int>(owner, \"Count\", [], []);"), "names the method 'Count', which Owner does not have");
        // A base type's private member, and its static one, are not found on the derived owner.
        Expect(Call("PrivateState.Field<Derived, int>(derived, \"hidden\");"), "names the field 'hidden', which Derived does not have");
        Expect(Call("PrivateState.StaticField<int>(typeof(Derived), \"shared\");"), "names the field 'shared', which Derived does not have");
        // An accessor or an extension block's member the class does not have.
        Expect(Call("PrivateState.InvokeStatic<int>(typeof(Owner), \"get_Missing\", [], []);"), "names the method 'get_Missing', which Owner does not have");
        Expect(Call("PrivateState.InvokeStatic<int>(typeof(Helpers), \"Thrice\", [], []);"), "names the method 'Thrice', which Helpers does not have");

        // Members the owner declares, at any accessibility, and a base type's non-private instance members.
        Expect(Call("PrivateState.Get<Owner, int>(owner, \"Count\");"), count: 0);
        Expect(Call("PrivateState.Set(owner, \"Count\", 1);"), count: 0);
        Expect(Call("PrivateState.Field<Owner, int>(owner, \"count\");"), count: 0);
        Expect(Call("PrivateState.ValueField<Value, int>(value, \"total\");"), count: 0);
        Expect(Call("PrivateState.Invoke<Owner, int>(owner, \"Step\", [], []);"), count: 0);
        Expect(Call("PrivateState.StaticGet<int>(typeof(Owner), \"Limit\");"), count: 0);
        Expect(Call("PrivateState.InvokeStatic<int>(typeof(Owner), \"Make\", [], []);"), count: 0);
        Expect(Call("PrivateState.Field<Derived, int>(derived, \"shown\");"), count: 0);
        Expect(Call("PrivateState.Field<Generic<int>, int>(generic, \"held\");"), count: 0);
        // A property's accessor, a classic extension method, and the methods a static class's extension block compiles to.
        Expect(Call("PrivateState.InvokeStatic<int>(typeof(Owner), \"get_Limit\", [], []);"), count: 0);
        Expect(Call("PrivateState.Invoke<Owner, int>(owner, \"add_Changed\", [], []);"), count: 0);
        Expect(Call("PrivateState.InvokeStatic<int>(typeof(Helpers), \"Plain\", [], []);"), count: 0);
        Expect(Call("PrivateState.InvokeStatic<int>(typeof(Helpers), \"Twice\", [], []);"), count: 0);
        Expect(Call("PrivateState.InvokeStatic<int>(typeof(Helpers), \"get_Doubled\", [], []);"), count: 0);
        // An owner or a name the call does not state, and a construction, which names no member.
        Expect(Call("PrivateState.Field<Owner, int>(owner, name);"), count: 0);
        Expect(Call("PrivateState.StaticGet<int>(type, \"Missing\");"), count: 0);
        Expect(Call("Reach(owner);"), count: 0);
        Expect(Call("PrivateState.New<Owner>([], []);"), count: 0);
    }

    /// <summary>A check that runs <paramref name="statements"/> against owners with known private members.</summary>
    private static string Call(string statements) => $$"""
        namespace SuperMetroid.Core
        {
            internal static class PrivateState
            {
                internal static void Set<TOwner>(TOwner owner, string property, object? value) where TOwner : class { }
                internal static TValue Get<TOwner, TValue>(TOwner owner, string property) where TOwner : class => default!;
                internal static TValue Field<TOwner, TValue>(TOwner owner, string field) where TOwner : class => default!;
                internal static TValue ValueField<TOwner, TValue>(TOwner owner, string field) where TOwner : struct => default!;
                internal static TValue StaticField<TValue>(System.Type owner, string field) => default!;
                internal static TValue StaticGet<TValue>(System.Type owner, string property) => default!;
                internal static TResult Invoke<TOwner, TResult>(TOwner owner, string method, System.Type[] parameters, params object?[] arguments) where TOwner : class => default!;
                internal static TResult InvokeStatic<TResult>(System.Type owner, string method, System.Type[] parameters, params object?[] arguments) => default!;
                internal static TOwner New<TOwner>(System.Type[] parameters, params object?[] arguments) => default!;
            }
        }
        namespace SuperMetroid.Verification
        {
            using SuperMetroid.Core;
            internal class Base
            {
                private int hidden;
                internal int shown;
                private static int shared;
                internal int Read() => hidden + shared;
            }
            internal sealed class Derived : Base { }
            internal sealed class Owner
            {
                private int count;
                private int Count { get; set; }
                private static int Limit => 3;
                private int Step() => count + Count;
                private static int Make() => Limit;
                private event System.Action? Changed;
                internal void Raise() => Changed?.Invoke();
            }
            internal static class Helpers
            {
                extension(Owner owner)
                {
                    internal int Doubled => 2;
                    internal int Twice() => 2;
                }
                private static int Plain(this Owner owner) => 1;
                internal static int Read(Owner owner) => owner.Plain();
            }
            internal struct Value
            {
                private int total;
                internal int Read() => total;
            }
            internal sealed class Generic<T>
            {
                private T? held;
                internal T? Read() => held;
            }
            internal static class Check
            {
                internal static void Reach(Owner owner) { }
                internal static void Run(Owner owner, Value value, Derived derived, Generic<int> generic, string name, System.Type type)
                {
                    {{statements}}
                }
            }
        }
        """;

    /// <summary>Compiles one source and compares the SME6477 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Verification/Fixture.cs");
        var compilation = CSharpCompilation.Create("SuperMetroid.Verification", [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new PrivateStateMemberAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != PrivateStateMemberAnalyzer.PrivateStateMemberId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {PrivateStateMemberAnalyzer.PrivateStateMemberId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
