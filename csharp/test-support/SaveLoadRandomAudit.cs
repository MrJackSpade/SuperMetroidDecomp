using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Desktop;

/// <summary>#441: real frontend loading from identical in-memory SRAM, with varied file-map waits.</summary>
internal static class SaveLoadRandomAudit
{
    public static int Run(string rom, Action<SuperMetroidGame> bindPresentation)
    {
        foreach (int wait in new[] { 0, 1, 2 })
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(rom);
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
            void Step(ushort input)
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
                Step(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0);
            }
            var runtime = game.RuntimeForVerification;
            if (!insertedWait || runtime?.Samus is null)
                throw new InvalidDataException("Save-load RNG audit did not reach the loaded runtime through file map.");
            Console.WriteLine($"SAVE RNG wait={wait} frontendFrames={sequence} dispatches={dispatches} rng={runtime.System.RandomNumber} room={runtime.ActiveRoom?.Pointer:X4} energy={runtime.Samus.Health} PB={runtime.Samus.PowerBombs}/{runtime.Samus.MaxPowerBombs}");
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
            Step(0);
            expected.NextRandom();
            if (runtime.System.RandomNumber != expected.RandomNumber)
                throw new InvalidDataException("The first loaded gameplay frame must advance RNG exactly once.");

            // Isolate the continue-menu handoff without a route-length death setup.
            // Retain the actual loaded runtime/RNG/SRAM and enter its file-map state.
            typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!
                .SetValue(game, SuperMetroidGameState.FileSelectMap);
            for (int tick = 0; tick < 1000 && (game.RuntimeForVerification is null || ReferenceEquals(runtime, game.RuntimeForVerification)); tick++)
            {
                game.StepCaptured(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0, ++sequence, 1);
                expected.NextRandom();
                if (game.DispatcherRandomNumber != expected.RandomNumber)
                    throw new InvalidDataException("Continued save lost the live runtime RNG at the file-map/load handoff.");
            }
            if (game.RuntimeForVerification is null || ReferenceEquals(runtime, game.RuntimeForVerification))
                throw new InvalidDataException("Continue-menu audit did not replace the loaded runtime.");
            Console.WriteLine($"CONTINUE RNG wait={wait} rng={expected.RandomNumber}");
            typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!
                .SetValue(game, SuperMetroidGameState.Reset);
            game.StepCaptured(0, ++sequence, 1);
            if (game.DispatcherRandomNumber != 758)
                throw new InvalidDataException("A true reset must reseed before the state-zero main-loop advance.");
        }
        return 0;
    }
}
