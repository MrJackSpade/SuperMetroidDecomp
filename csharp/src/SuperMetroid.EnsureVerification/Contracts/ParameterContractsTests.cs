using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Uses compiler fixtures to check that parent service contracts expose only their intended child capabilities.</summary>
internal static class ParameterContractsTests
{
    /// <summary>
    /// #633 compile-time contracts: each parent operation contract converts to the narrow contracts its
    /// child operations accept, a narrow contract cannot reach a capability its child does not own, and
    /// required capabilities cannot be omitted. Finite compiler fixtures; no gameplay runs.
    /// </summary>
    internal static void VerifyParameterContracts()
    {
        // Room enemy load: the services contract is the union of its children's contracts.
        VerifyContract("enemy services feed the Chozo statue child",
            "IChozoStatueRoomServices asChozo = enemies; IMotherBrainRoomServices asMotherBrain = enemies;");
        VerifyContract("Chozo statue child cannot reach the RNG", "_ = chozo.Random;", "CS1061");
        VerifyContract("Mother Brain child cannot reach event bits", "_ = motherBrain.Events;", "CS1061");

        // Room PLM services: required capabilities and narrow family contracts.
        VerifyContract("enemy effects feed every PLM family",
            "INoobTubeProjectileSpawner noob = effects; IEyeDoorProjectileSpawner eye = effects; " +
            "IOldTourianWallExplosion wall = effects; IDraygonCannonControl cannon = effects;");
        VerifyContract("a PLM family cannot reach another family's effect", "noobTube.DisableDraygonCannon(0);", "CS1061");
        VerifyContract("PLM boss reads are read-only", "bossReader.SetDefeated(DefeatedBosses.AreaTorizo);", "CS1061");
        VerifyContract("area boss flags satisfy the PLM reader", "IAreaBossFlagReader reader = bosses;");
        VerifyContract("PLM services require Samus", "_ = new RoomObjectServices { Bosses = bossReader, Events = events, " +
            "TourianStatue = statue, RoomFx = fx, Earthquake = earthquake, Enemies = effects };", "CS9035");
        VerifyContract("PLM services require the room's FX (#2015)", "_ = new RoomObjectServices { Samus = samus, " +
            "Bosses = bossReader, Events = events, TourianStatue = statue, Earthquake = earthquake, Enemies = effects };", "CS9035");
        VerifyContract("PLM services accept their required capabilities",
            "_ = new RoomObjectServices { Samus = samus, Bosses = bossReader, Events = events, TourianStatue = statue, " +
            "RoomFx = fx, Earthquake = earthquake, Enemies = effects };");

        // Runtime frames: the request names the game state that owns the update, and only the owners themselves state what
        // each permits (#2542).
        VerifyContract("a runtime frame request names its update's owner",
            "_ = new RuntimeFrameRequest { Owner = GameplayUpdateOwner.PauseDarkening };");
        VerifyContract("a caller cannot compose an update owner's permissions (#2542)",
            "_ = new GameplayUpdateOwner();", "CS1729");
        VerifyContract("a runtime frame request carries no audio options beside its owner (#2499)",
            "_ = new RuntimeFrameRequest().Audio;", "CS1061");
        VerifyContract("the gameplay runtime is reachable only from Core and its friends (#1327)",
            "SuperMetroidRuntime? hidden = null;", "CS0122");

        Console.WriteLine("#633: parameter contracts compose child capabilities and reject unrelated or omitted ones.");
    }

    /// <summary>Compiles a caller statement and compares its diagnostics to the required capability boundary.</summary>
    private static void VerifyContract(string description, string statement, string? expectedError = null)
    {
        string source = $$"""
            using SuperMetroid.Core.Bosses.Draygon;
            using SuperMetroid.Core.Bosses.MotherBrain;
            using SuperMetroid.Core.Enemies;
            using SuperMetroid.Core.Enemies.Chozo;
            using SuperMetroid.Core.Input;
            using SuperMetroid.Core.Progression;
            using SuperMetroid.Core.Rooms;
            using SuperMetroid.Core.Rooms.Doors;
            using SuperMetroid.Core.Rooms.RoomObjects;
            using SuperMetroid.Core.Runtime;
            using SuperMetroid.Core.World;
            class ContractCaller
            {
                void Run(IRoomEnemyServices enemies, IChozoStatueRoomServices chozo, IMotherBrainRoomServices motherBrain,
                    IRoomObjectEnemyEffects effects, INoobTubeProjectileSpawner noobTube, IAreaBossFlags bosses,
                    IAreaBossFlagReader bossReader, IRoomSamusAccess samus, IGlobalEventFlags events,
                    ITourianStatueProgress statue, IRoomObjectFx fx, IRoomEarthquakeWriter earthquake,
                    DemoInputState demo)
                {
                    {{statement}}
                }
            }
            """;
        var compilation = CSharpCompilation.Create("ParameterContract",
            [CSharpSyntaxTree.ParseText(source)], References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        Diagnostic[] errors = compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (expectedError is null ? errors.Length != 0 : errors.Length != 1 || errors[0].Id != expectedError)
            throw new InvalidOperationException(
                $"{description}: expected {expectedError ?? "successful compilation"}; got {string.Join("; ", errors.Select(error => error.ToString()))}");
    }
}
