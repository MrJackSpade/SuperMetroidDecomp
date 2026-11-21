using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2567 declaration and enumeration order rule, SME6469.</summary>
internal static class EnumerationOrderAnalyzerTests
{
    /// <summary>The advice every SME6469 message ends with.</summary>
    private const string Advice = "; state the order as data (an explicit list), or use it only as a set, a lookup or a count";

    /// <summary>One positive per way an enum's or a dictionary's order feeds an order, and the order-free uses the rule leaves alone.</summary>
    internal static void VerifyEnumerationOrderAnalyzer()
    {
        // An enum's members listed, exposed or built into a list in their declaration order.
        Expect(Member("internal static IReadOnlyList<Frame> Frames { get; } = Enum.GetValues<Frame>();"),
            "'Enum.GetValues<Frame>()' takes its order from the declaration of 'Frame'" + Advice);
        Expect(Body("return [.. Enum.GetValues<Frame>().Select(frame => frame.ToString())];"), "the declaration of 'Frame'" + Advice);
        Expect(Body("return Enum.GetValues<Frame>().Where(frame => frame != Frame.Closed).Select(Name).ToList();"), "the declaration of 'Frame'");
        Expect(Body("Frame[] frames = Enum.GetValues<Frame>();\n        return [Name(frames[0])];"), "the declaration of 'Frame'");
        Expect(Body("return [.. Enum.GetValues(typeof(Frame)).Cast<Frame>().Select(Name)];"), "an enum's declaration");
        Expect(Body("return [.. Kinds.SelectMany(kind => Enum.GetValues<Frame>().Select(Name))];"), "the declaration of 'Frame'");
        // A loop over the members with an ordered effect: a yield, an accumulation, a first match, a running offset.
        Expect(Iterator("foreach (Frame frame in Enum.GetValues<Frame>())\n            yield return Name(frame);"), "the declaration of 'Frame'");
        Expect(Body("var names = new List<string>();\n        foreach (Frame frame in Enum.GetValues<Frame>())\n            names.Add(Name(frame));\n        return names;"),
            "the declaration of 'Frame'");
        Expect(Body("var text = new System.Text.StringBuilder();\n        foreach (Frame frame in Enum.GetValues<Frame>())\n            text.Append(Name(frame));\n        return [text.ToString()];"),
            "the declaration of 'Frame'");
        Expect(Body("foreach (Frame frame in Enum.GetValues<Frame>())\n            if (Name(frame).Length > 3)\n                return [Name(frame)];\n        return [];"),
            "the declaration of 'Frame'");
        Expect(Body("var offsets = new Dictionary<Frame, int>();\n        int offset = 0;\n        foreach (Frame frame in Enum.GetValues<Frame>())\n        {\n            offsets[frame] = offset;\n            offset += Name(frame).Length;\n        }\n        return [offsets.Count.ToString()];"),
            "the declaration of 'Frame'");
        // A sort by an enum's values, and a list sorted by them.
        Expect(Body("return [.. Placed.Order().Select(Name)];"), "'Placed.Order()' takes its order from the values of 'Frame'" + Advice);
        Expect(Body("return [.. Pairs.OrderBy(pair => pair.Frame).ThenBy(pair => pair.Size).Select(pair => Name(pair.Frame))];"), "the values of 'Frame'");
        Expect(Body("var frames = new List<Frame>(Placed);\n        frames.Sort();\n        return [.. frames.Select(Name)];"), "the values of 'Frame'");
        // A dictionary enumerated into an order.
        Expect(Body("return [.. Names.Keys.Select(Name)];"), "'Names.Keys' takes its order from a dictionary's enumeration" + Advice);
        Expect(Body("var names = new List<string>();\n        foreach (KeyValuePair<Frame, string> pair in Names)\n            names.Add(pair.Value);\n        return names;"),
            "a dictionary's enumeration");
        Expect(Body("return [.. Names.Values];"), "a dictionary's enumeration");

        // Order-free uses: a set, a lookup, a count, a membership, a resort, a bitwise union, a loop without ordered effects.
        Expect(Body("var byName = Enum.GetValues<Frame>().ToDictionary(Name);\n        return [byName.Count.ToString()];"), count: 0);
        Expect(Body("HashSet<Frame> all = [.. Enum.GetValues<Frame>()];\n        return [all.Count.ToString()];"), count: 0);
        Expect(Body("return [Enum.GetValues<Frame>().Length.ToString(), Enum.GetValues<Frame>().All(Names.ContainsKey).ToString()];"), count: 0);
        Expect(Body("return [.. Enum.GetValues<Frame>().OrderBy(Name, StringComparer.Ordinal).Select(Name)];"), count: 0);
        Expect(Body("Frame all = Enum.GetValues<Frame>().Aggregate((left, right) => left | right);\n        return [Name(all)];"), count: 0);
        Expect(Body("var names = new Dictionary<Frame, string>();\n        int count = 0;\n        bool any = false;\n        foreach (Frame frame in Enum.GetValues<Frame>())\n        {\n            if (!Names.ContainsKey(frame))\n                throw new InvalidOperationException(Name(frame));\n            names[frame] = Name(frame);\n            count++;\n            any = true;\n        }\n        return [count.ToString(), any.ToString()];"),
            count: 0);
        Expect(Body("return [Names.Values.Any(name => name.Length == 0).ToString(), Names.Keys.ToHashSet().Count.ToString()];"), count: 0);
        Expect(Body("return [.. Names.ToDictionary(pair => pair.Value, pair => pair.Key).Keys.Order(StringComparer.Ordinal)];"), count: 0);
        // One match, a thrown message, a dictionary keyed by names, and a list only read by a call are order-free too.
        Expect(Body("return [Name(Enum.GetValues<Frame>().Single(frame => Name(frame) == \"Open\"))];"), count: 0);
        Expect(Body("Frame[] missing = [.. Enum.GetValues<Frame>().Where(frame => !Names.ContainsKey(frame))];\n        if (missing.Length != 0)\n            throw new InvalidOperationException(string.Join(\", \", missing));\n        return [];"),
            count: 0);
        Expect(Body("var byName = new Dictionary<string, Frame> { [\"open\"] = Frame.Open };\n        return [.. byName.Keys];"), count: 0);
        Expect(Body("var names = new List<string> { \"open\" };\n        int longest = 0;\n        foreach (Frame frame in Enum.GetValues<Frame>())\n            longest = Math.Max(longest, names.Count(name => name == Name(frame)));\n        return [longest.ToString()];"),
            "the declaration of 'Frame'");
        Expect(Body("var names = new List<string> { \"open\" };\n        foreach (Frame frame in Enum.GetValues<Frame>())\n            if (names.Contains(Name(frame)))\n                throw new InvalidOperationException(Name(frame));\n        return names;"),
            count: 0);
        // Followed across members: entries a function of the assembly only files into a lookup, a result its caller only
        // unions into a sorted set, and a registry's list read only to file each entry by key feed no order; the same
        // sequences listed in order by the callee, the caller or the registry's reader do.
        Expect(Member(Filing("if (!filed.TryAdd(Name(frame), frame))\n                throw new InvalidOperationException(Name(frame));")), count: 0);
        Expect(Member(Filing("listed.Add(frame);")), "a dictionary's enumeration");
        Expect(Member(Unioned("all.Add(name);")), count: 0);
        Expect(Member(Unioned("listed.Add(name);")), "the declaration of 'Frame'");
        Expect(Member(Registered("filed.TryAdd(frame, species);")), count: 0);
        Expect(Member(Registered("listed.Add(frame);")), "a dictionary's enumeration");
        // #2602: a member's sequence a source generator's output lists in order (a debugger state's writer) feeds no order;
        // the same listing authored, header and all, does.
        const string listing = "// <auto-generated/>\nusing System.Collections.Generic;\nusing System.Linq;\nnamespace SuperMetroid.Core.Fixture\n{\n    internal static partial class Fixture\n    {\n        internal static IReadOnlyList<string> Writes() => [.. Keys.Select(Name)];\n    }\n}\n";
        string keys = Member("internal static IReadOnlyList<Frame> Keys { get; } = [.. Names.Keys];");
        ExpectWith(keys, listing, "C:/fixture-repo/csharp/src/SuperMetroid.Core/obj/Release/net10.0/generated/SuperMetroid.EnsureAnalyzer/SuperMetroid.EnsureAnalyzer.Generators.StateGraphGenerator/StateGraph/Fixture.g.cs", count: 0);
        ExpectWith(keys, listing, "C:/fixture-repo/csharp/src/SuperMetroid.Core/Fixture/Listing.cs", "a dictionary's enumeration");
        // A written-out list is data, and outside Core and the import the rule does not apply.
        Expect(Body("return [.. Written.Select(Name)];"), count: 0);
        Expect(Member("internal static IReadOnlyList<Frame> Frames { get; } = Enum.GetValues<Frame>();"), count: 0, assemblyName: "SuperMetroid.Desktop");
        Expect(Member("internal static IReadOnlyList<Frame> Frames { get; } = Enum.GetValues<Frame>();"), "the declaration of 'Frame'",
            assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>The fixture's declarations: a flags enum, a pair record, and the lists and dictionary the methods read.</summary>
    private const string Declarations = """
        using System;
        using System.Collections.Generic;
        using System.Linq;

        namespace SuperMetroid.Core.Fixture
        {
            [Flags]
            internal enum Frame { Closed = 1, Middle = 2, Open = 4 }
            internal sealed record Pair(Frame Frame, int Size);
            internal static partial class Fixture
            {
                private static readonly Frame[] Written = [Frame.Open, Frame.Closed, Frame.Middle];
                private static readonly HashSet<Frame> Placed = [Frame.Open, Frame.Closed];
                private static readonly Pair[] Pairs = [new(Frame.Open, 1), new(Frame.Closed, 2)];
                private static readonly int[] Kinds = [1, 2];
                private static readonly Dictionary<Frame, string> Names = new() { [Frame.Open] = "open", [Frame.Closed] = "closed" };
                private static string Name(Frame frame) => frame.ToString();
            }
        }
        """;

    /// <summary>The fixture with <paramref name="member"/> declared in its class.</summary>
    private static string Member(string member) => Declarations + $$"""

        namespace SuperMetroid.Core.Fixture
        {
            using System;
            using System.Collections.Generic;
            using System.Linq;
            internal static partial class Fixture
            {
                {{member}}
            }
        }
        """;

    /// <summary>
    /// Members handing a dictionary's keys to a private function that runs <paramref name="filing"/> on each, with a lookup
    /// <c>filed</c> and a list <c>listed</c> to put them in.
    /// </summary>
    private static string Filing(string filing) => $$"""
        internal static IReadOnlyDictionary<string, Frame> Filed { get; } = File(Names.Select(pair => pair.Key));
                private static Dictionary<string, Frame> File(IEnumerable<Frame> frames)
                {
                    var filed = new Dictionary<string, Frame>();
                    var listed = new List<Frame>();
                    foreach (Frame frame in frames)
                        {{filing}}
                    filed.Add(listed.Count.ToString(), Frame.Open);
                    return filed;
                }
        """;

    /// <summary>
    /// Members returning an enum's members' names from a private method whose one caller runs <paramref name="adding"/> on
    /// each, with a sorted set <c>all</c> and a list <c>listed</c> to put them in.
    /// </summary>
    private static string Unioned(string adding) => $$"""
        private static IEnumerable<string> Spelled() => Enum.GetValues<Frame>().Select(Name);
                internal static SortedSet<string> Union()
                {
                    var all = new SortedSet<string>(StringComparer.Ordinal);
                    var listed = new List<string>();
                    foreach (string name in Spelled())
                        {{adding}}
                    all.Add(listed.Count.ToString());
                    return all;
                }
        """;

    /// <summary>
    /// A registry whose one species lists a dictionary's keys, read through the species' interface by a function that runs
    /// <paramref name="registering"/> on each, with a lookup <c>filed</c> and a list <c>listed</c> to put them in.
    /// </summary>
    private static string Registered(string registering) => $$"""
        internal interface ISpecies { IReadOnlyList<Frame> Frames { get; } }
                internal sealed class Species : ISpecies { public IReadOnlyList<Frame> Frames { get; } = [.. Names.Keys]; }
                internal static Dictionary<Frame, ISpecies> Register(ISpecies species)
                {
                    var filed = new Dictionary<Frame, ISpecies>();
                    var listed = new List<Frame>();
                    foreach (Frame frame in species.Frames)
                        {{registering}}
                    filed.Add(listed.Count == 0 ? Frame.Open : Frame.Closed, species);
                    return filed;
                }
        """;

    /// <summary>The fixture with a method returning names that runs <paramref name="statements"/>.</summary>
    private static string Body(string statements) => Member($$"""
        internal static List<string> Listing()
                {
                    {{statements}}
                }
        """);

    /// <summary>The fixture with an iterator method that runs <paramref name="statements"/>.</summary>
    private static string Iterator(string statements) => Member($$"""
        internal static IEnumerable<string> Listed()
                {
                    {{statements}}
                }
        """);

    /// <summary>Compiles a fixture with a second file at <paramref name="path"/> and compares the SME6469 count and message.</summary>
    private static void ExpectWith(string source, string second, string path, string? message = null, int count = 1)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Fixture/Fixture.cs");
        var other = CSharpSyntaxTree.ParseText(second, new CSharpParseOptions(LanguageVersion.Preview), path: path);
        var compilation = CSharpCompilation.Create(CoreLayerAnalyzer.CoreAssembly, [tree, other], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new EnumerationOrderAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count || (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {EnumerationOrderAnalyzer.EnumerationOrderId} diagnostic(s) containing '{message}' with {path}, " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}].");
        }
    }

    /// <summary>Compiles one source and compares the SME6469 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Fixture/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new EnumerationOrderAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != EnumerationOrderAnalyzer.EnumerationOrderId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {EnumerationOrderAnalyzer.EnumerationOrderId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
