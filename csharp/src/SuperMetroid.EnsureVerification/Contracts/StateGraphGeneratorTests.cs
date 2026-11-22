using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SuperMetroid.EnsureAnalyzer.Generators;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// #2554: the members the state-graph generator writes (#2602) leave a get-only lazy cache (<c>X => field ??= …</c>) out of
/// a debugger state, as they leave out a <see cref="NonSerializedAttribute"/> field. Its value is derived from the object's
/// other state and rebuilt on first use; reading it to write or check it would build it, and a derived primary
/// constructor's restore of a non-sealed base would compare the stored value with one it had just built, which never
/// matches.
/// </summary>
internal static class StateGraphGeneratorTests
{
    /// <summary>
    /// A non-sealed base with a primary constructor and lazy caches, and a sealed primary-constructor type derived from it,
    /// in an assembly whose partial types get the members.
    /// </summary>
    private const string Fixture = """
        [assembly: SuperMetroid.Core.StateGraphAssembly(PartialTypesOnly = true)]
        namespace Fixture;
        internal partial class Reacting(int strength)
        {
            internal int Strength => strength;
            internal int Hits { get; set; }
            internal int Level { get; } = 2;
            internal System.Func<int> Reaction => field ??= () => strength + Hits;
            internal System.Func<int> Recoil { get => field ??= () => Hits; }
            internal System.Func<int> Settable { get => field ??= () => Level; set; }
        }
        internal sealed partial class Shutter(int strength, int height) : Reacting(strength)
        {
            internal int Height => height;
            internal System.Func<int> Rise { get { return field ??= () => height + Hits; } }
        }
        """;

    /// <summary>
    /// The generated members of the fixture compile; neither level names a lazy cache in any member, while the base still
    /// writes, checks and restores its other members and the derived type its own.
    /// </summary>
    internal static void VerifyStateGraphGeneratorLeavesOutLazyCaches()
    {
        var compilation = CSharpCompilation.Create("StateGraphGeneratorCase",
            [CSharpSyntaxTree.ParseText(Fixture, new CSharpParseOptions(LanguageVersion.Preview))],
            AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new StateGraphGenerator().AsSourceGenerator()],
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview));
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out var generatorDiagnostics);
        if (generatorDiagnostics.Length != 0)
            throw new InvalidOperationException($"The generator reported: {string.Join("; ", generatorDiagnostics)}");
        Diagnostic[] errors = [.. output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        if (errors.Length != 0)
            throw new InvalidOperationException($"The generated members do not compile: {string.Join("; ", errors.Select(error => error.ToString()))}");

        GeneratorRunResult run = driver.GetRunResult().Results.Single();
        string reacting = Generated(run, "StateGraph/Fixture_Reacting.g.cs");
        string shutter = Generated(run, "StateGraph/Fixture_Shutter.g.cs");
        foreach (string cache in (string[])["Reaction", "Recoil"])
            Absent(reacting, cache, "the base");
        Absent(shutter, "Rise", "the derived type");
        // What is no cache stays: a settable lazy property is set back, and a get-only auto-property is checked.
        Present(reacting, "this.@Settable = ", "the base restores its settable lazy property");
        Present(reacting, "__reader.Fixed(this.@Level);", "the base checks its get-only auto-property against its construction");
        Present(reacting, "this.@Hits = ", "the base restores its settable property");
        Present(shutter, "\"height\"", "the derived type keeps its captured parameter");
    }

    /// <summary>The text of the generated file <paramref name="hintName"/>.</summary>
    private static string Generated(GeneratorRunResult run, string hintName) =>
        run.GeneratedSources.SingleOrDefault(source => source.HintName == hintName).SourceText?.ToString()
            ?? throw new InvalidOperationException($"The generator wrote no {hintName}: {string.Join(", ", run.GeneratedSources.Select(source => source.HintName))}");

    /// <summary>Requires <paramref name="generated"/> to name the lazy cache <paramref name="cache"/> in no member.</summary>
    private static void Absent(string generated, string cache, string level)
    {
        if (generated.Contains("\"" + cache + "\"", StringComparison.Ordinal) || generated.Contains("this.@" + cache, StringComparison.Ordinal))
            throw new InvalidOperationException($"The members of {level} write, visit or restore its lazy cache {cache}:\n{generated}");
    }

    /// <summary>Requires <paramref name="generated"/> to contain <paramref name="text"/>.</summary>
    private static void Present(string generated, string text, string meaning)
    {
        if (!generated.Contains(text, StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected {meaning} ('{text}'):\n{generated}");
    }
}
