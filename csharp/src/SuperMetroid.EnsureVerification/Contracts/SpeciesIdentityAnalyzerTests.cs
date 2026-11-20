using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the species-identity rule, SME6392, including ownership declared through a part catalog (#1381).</summary>
internal static class SpeciesIdentityAnalyzerTests
{
    /// <summary>The definition identities and the factory interface the rule reads.</summary>
    private const string EnemyTypes = """
        namespace SuperMetroid.Core.Enemies
        {
            public enum EnemyDefinitionId { None, FooBody, FooArm, Bar }
            public interface IEnemyEntityFactory { System.Collections.Generic.IReadOnlyList<EnemyDefinitionId> Definitions { get; } }
        }
        """;

    /// <summary>A species whose factory lists its definitions directly.</summary>
    private const string ListedFactory = """
        namespace SuperMetroid.Core.Enemies.Bar
        {
            using System.Collections.Generic;
            sealed class BarBehavior : IEnemyEntityFactory { public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Bar]; }
        }
        """;

    /// <summary>A species whose factory takes its definitions from its part catalog.</summary>
    private const string CatalogFactory = """
        namespace SuperMetroid.Core.Enemies.Foo
        {
            using System.Collections.Generic;
            enum FooPart { Body, Arm }
            static class FooPartDefinitions
            {
                internal static IReadOnlyDictionary<EnemyDefinitionId, FooPart> Parts { get; } = new Dictionary<EnemyDefinitionId, FooPart>
                {
                    [EnemyDefinitionId.FooBody] = FooPart.Body,
                    [EnemyDefinitionId.FooArm] = FooPart.Arm,
                };
            }
            sealed class FooBehavior : IEnemyEntityFactory { public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. FooPartDefinitions.Parts.Keys]; }
        }
        """;

    /// <summary>Positive and negative cases for SME6392.</summary>
    internal static void VerifySpeciesIdentityAnalyzer()
    {
        const string testsFooArm = "namespace SuperMetroid.Core.Enemies { class T { bool M(EnemyDefinitionId id) => id == EnemyDefinitionId.FooArm; } }";
        const string testsBar = "namespace SuperMetroid.Core.Enemies { class T { bool M(EnemyDefinitionId id) => id == EnemyDefinitionId.Bar; } }";
        Expect("Enemies/Foo/T.cs", testsFooArm, reported: false);
        Expect("Enemies/Bar/T.cs", testsFooArm, reported: true);
        Expect("Enemies/Bar/T.cs", testsBar, reported: false);
        Expect("Enemies/Foo/T.cs", testsBar, reported: true);
    }

    /// <summary>Compiles both species and one tester at <paramref name="path"/> under Core and checks the result.</summary>
    private static void Expect(string path, string tester, bool reported)
    {
        const string root = "C:/fixture-repo/csharp/src/SuperMetroid.Core/";
        SyntaxTree[] trees =
        [
            CSharpSyntaxTree.ParseText(EnemyTypes, new CSharpParseOptions(LanguageVersion.Preview), path: root + "Enemies/EnemyDefinitionId.cs"),
            CSharpSyntaxTree.ParseText(ListedFactory, new CSharpParseOptions(LanguageVersion.Preview), path: root + "Enemies/Bar/BarBehavior.cs"),
            CSharpSyntaxTree.ParseText(CatalogFactory, new CSharpParseOptions(LanguageVersion.Preview), path: root + "Enemies/Foo/FooBehavior.cs"),
            CSharpSyntaxTree.ParseText(tester, new CSharpParseOptions(LanguageVersion.Preview), path: root + path),
        ];
        var compilation = CSharpCompilation.Create("SuperMetroid.Core", trees, AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        Diagnostic[] errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid species-identity fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        Diagnostic[] diagnostics = compilation.WithAnalyzers([new SpeciesIdentityAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult()
            .Where(d => d.Id == SpeciesIdentityAnalyzer.ForeignIdentityId).ToArray();
        if (diagnostics.Length != (reported ? 1 : 0))
            throw new InvalidOperationException(
                $"Expected {(reported ? "one" : "no")} {SpeciesIdentityAnalyzer.ForeignIdentityId} for {path}: {tester}; got " +
                string.Join("; ", diagnostics.Select(d => d.GetMessage())));
    }
}
