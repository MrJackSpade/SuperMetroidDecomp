using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1315 species-reach rule, SME6407.</summary>
internal static class SpeciesReachAnalyzerTests
{
    /// <summary>The enemy system with one species, one encounter and one capability.</summary>
    private const string EnemySystem = """
        namespace SuperMetroid.Core.Enemies
        {
            public sealed class KraidBehavior { public int Kraid => 0; }
            public sealed class RidleyEncounter { public int CeresStatus => 0; }
            public interface IElevatorJourney { int ElevatorStatus { get; } }
        }
        namespace SuperMetroid.Core.World
        {
            using SuperMetroid.Core.Enemies;
            public sealed class RoomEnemySystem
            {
                public KraidBehavior KraidBehavior { get; } = new();
                public RidleyEncounter RidleyEncounter { get; } = new();
                public IElevatorJourney Elevator => null!;
            }
        }
        """;

    /// <summary>Positive and negative cases for SME6407.</summary>
    internal static void VerifySpeciesReachAnalyzer()
    {
        const string speciesRead = "namespace SuperMetroid.Core.Runtime { class R { int M(SuperMetroid.Core.World.RoomEnemySystem e) => e.KraidBehavior.Kraid; } }";
        const string encounterWrite = "namespace SuperMetroid.Core.Rooms { class R { object M(SuperMetroid.Core.World.RoomEnemySystem e) => e.RidleyEncounter; } }";
        const string capabilityRead = "namespace SuperMetroid.Core.Runtime { class R { int M(SuperMetroid.Core.World.RoomEnemySystem e) => e.Elevator.ElevatorStatus; } }";

        Expect("Runtime/R.cs", speciesRead, "'KraidBehavior' reaches a species from module 'Runtime'");
        Expect("Rooms/R.cs", encounterWrite, "'RidleyEncounter' reaches a species from module 'Rooms'");
        Expect("R.cs", speciesRead, "from module ''");
        Expect("Runtime/R.cs", capabilityRead);
        Expect("Enemies/Kraid/R.cs", speciesRead);
        Expect("Bosses/Ridley/R.cs", encounterWrite);
        // Verification and other assemblies are not the runtime's consumers of species.
        Expect("Runtime/R.cs", speciesRead, assembly: "SuperMetroid.Verification");
    }

    /// <summary>Compiles the enemy system and one consumer at <paramref name="path"/> under Core and checks the result.</summary>
    private static void Expect(string path, string consumer, string? message = null, string assembly = CoreLayerAnalyzer.CoreAssembly)
    {
        const string root = "C:/fixture-repo/csharp/src/SuperMetroid.Core/";
        SyntaxTree[] trees =
        [
            CSharpSyntaxTree.ParseText(EnemySystem, path: root + "Enemies/RoomEnemySystem.cs"),
            CSharpSyntaxTree.ParseText(consumer, path: root + path),
        ];
        var compilation = CSharpCompilation.Create(assembly, trees, AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        Diagnostic[] errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid species-reach fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        Diagnostic[] diagnostics = compilation.WithAnalyzers([new SpeciesReachAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToArray();
        string context = $"{assembly} {path}: {consumer}";
        if (message is null)
        {
            if (diagnostics.Length != 0)
                throw new InvalidOperationException($"Unexpected {diagnostics[0].Id}: {diagnostics[0].GetMessage()} for {context}");
            return;
        }
        if (diagnostics.Length != 1 || diagnostics[0].Id != SpeciesReachAnalyzer.Id ||
            !diagnostics[0].GetMessage().Contains(message, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Expected one {SpeciesReachAnalyzer.Id} containing '{message}' for {context}; got " +
                string.Join("; ", diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}")));
        }
    }
}
