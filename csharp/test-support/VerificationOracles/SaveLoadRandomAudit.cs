using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.SaveMapFixtures;

/// <summary>#441: real frontend loading from identical in-memory SRAM, with varied file-map waits.</summary>
internal static class SaveLoadRandomAudit
{
    /// <summary>Tracks dispatcher RNG across file-map waits, debugger restore, continued saves, and true reset.</summary>
    public static int Run(string rom, Action<SuperMetroidGame> bindPresentation)
    {
        foreach (int wait in new[] { 0, 1, 2 })
        {
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(rom);
            var save = new SuperMetroidSaveSnapshot
            {
                Area = (ushort)AreaId.Tourian, SaveStation = 0,
                Health = 399, MaxHealth = 399,
                EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.GravitySuit),
                CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.GravitySuit),
                PowerBombs = 5, MaxPowerBombs = 5,
                Missiles = 10, MaxMissiles = 10, SuperMissiles = 10, MaxSuperMissiles = 10,
            };
            new SuperMetroidSaveRam(bus, SaveMapPresentationFixture.Create(bus)).SaveSlot(0, save);
            var game = new SuperMetroidGame(bus);
            bindPresentation(game);
            bool insertedWait = false;
            long sequence = 0;
            long dispatches = 0;
            // Each main-loop dispatch calls GenerateRandomNumber once; an update that only
            // resumes a menu or loading dispatch after its NMI wait does not.
            void Step(SnesButton input)
            {
                if (!game.NextUpdateResumesNmiWait) dispatches++;
                game.StepCaptured(input, ++sequence, 1);
            }
            for (int tick = 0; tick < 2000 && game.RuntimeForVerification is null; tick++)
            {
                if (!insertedWait && game.GameState == SuperMetroidGameState.FileSelectMap)
                {
                    insertedWait = true;
                    for (int extra = 0; extra < wait; extra++) Step(0);
                    if (wait == 1)
                    {
                        // A debugger capture in the menu must retain its RNG history,
                        // not lazily recreate the reset seed when gameplay is loaded.
                        using var graph = new MemoryStream();
                        DebuggerObjectGraphSerializer.Serialize(graph, game);
                        graph.Position = 0;
                        game = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(graph);
                        bindPresentation(game);
                    }
                }
                Step(tick % 47 == 0 ? SnesButton.Start : SnesButton.None);
            }
            var runtime = game.RuntimeForVerification;
            if (!insertedWait || runtime?.Player.Samus is null)
                throw new InvalidDataException("Save-load RNG audit did not reach the loaded runtime through file map.");
            Console.WriteLine($"SAVE RNG wait={wait} frontendFrames={sequence} dispatches={dispatches} rng={runtime.System.RandomNumber} room={runtime.Room.ActiveRoom?.Pointer:X4} energy={runtime.Player.Samus.Health} PB={runtime.Player.Samus.PowerBombs}/{runtime.Player.Samus.MaxPowerBombs}");
            // The reset vector seeds before the first main-loop/state-zero pass.
            // This empty save room has no random-consuming population initializer.
            var expected = new Bank80SystemState();
            for (long dispatch = 0; dispatch < dispatches; dispatch++) expected.NextRandom();
            if (runtime.System.RandomNumber != expected.RandomNumber)
                throw new InvalidDataException($"Menu/load RNG: expected {expected.RandomNumber}, got {runtime.System.RandomNumber} after {dispatches} main-loop dispatches.");
            // Original-CPU probe: GenerateRandomNumber called 426/427/428 times from the
            // reset seed. It validates the generator, not the menu's dispatch count.
            ushort[] nativeLoaded = [51088, 59105, 33654];
            var probe = new Bank80SystemState();
            for (int call = 0; call < 426 + wait; call++) probe.NextRandom();
            if (probe.RandomNumber != nativeLoaded[wait])
                throw new InvalidDataException("RNG generator no longer matches its recorded original-CPU words.");
            // State six built the runtime in its loading dispatch, then waits for its NMIs
            // ($82:8000); those continuations make no main-loop RNG call.
            while (game.NextUpdateResumesNmiWait)
                Step(0);
            if (runtime.System.RandomNumber != expected.RandomNumber)
                throw new InvalidDataException("State six's loading waits must not advance RNG.");
            Step(0);
            expected.NextRandom();
            if (runtime.System.RandomNumber != expected.RandomNumber)
                throw new InvalidDataException("The first loaded gameplay frame must advance RNG exactly once.");

            // Isolate the continue-menu handoff without a route-length death setup.
            // Retain the actual loaded runtime/RNG/SRAM and enter its file-map state.
            game.GameState = SuperMetroidGameState.FileSelectMap;
            for (int tick = 0; tick < 1000 && (game.RuntimeForVerification is null || ReferenceEquals(runtime, game.RuntimeForVerification)); tick++)
            {
                // The map's final dispatch resumes after its forced-blank NMI wait without a main-loop call.
                bool resumesNmiWait = game.NextUpdateResumesNmiWait;
                game.StepCaptured(tick % 47 == 0 ? SnesButton.Start : (ushort)0, ++sequence, 1);
                if (!resumesNmiWait)
                    expected.NextRandom();
                if (game.DispatcherRandomNumber != expected.RandomNumber)
                    throw new InvalidDataException("Continued save lost the live runtime RNG at the file-map/load handoff.");
            }
            if (game.RuntimeForVerification is null || ReferenceEquals(runtime, game.RuntimeForVerification))
                throw new InvalidDataException("Continue-menu audit did not replace the loaded runtime.");
            Console.WriteLine($"CONTINUE RNG wait={wait} rng={expected.RandomNumber}");
            game.GameState = SuperMetroidGameState.Reset;
            game.StepCaptured(0, ++sequence, 1);
            if (game.DispatcherRandomNumber != 758)
                throw new InvalidDataException("A true reset must reseed before the state-zero main-loop advance.");
        }
        return 0;
    }

    /// <summary>Runs the save-load RNG audit with the repository cartridge and installed presentation.</summary>
    [VerificationCommand("--save-load-rng-fixture")]
    internal static int SaveLoadRngFixtureCommand()
    {
     return SaveLoadRandomAudit.Run(Path.GetFullPath("Super Metroid.smc"), InstalledFixtures.BindGame); 
    }
}
