using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Desktop;

/// <summary>#441: real frontend loading from identical in-memory SRAM, with varied file-map waits.</summary>
internal static class SaveLoadRandomAudit
{
    public static int Run(string rom)
    {
        foreach (int wait in new[] { 0, 1, 2 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var save = new SuperMetroidSaveSnapshot
            {
                Area = (ushort)AreaId.Tourian, SaveStation = 0,
                Health = 399, MaxHealth = 399,
                EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.GravitySuit),
                CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.GravitySuit),
                PowerBombs = 5, MaxPowerBombs = 5,
                Missiles = 10, MaxMissiles = 10, SuperMissiles = 10, MaxSuperMissiles = 10,
            };
            new SuperMetroidSaveRam(bus).SaveSlot(0, save);
            var game = new SuperMetroidGame(bus);
            bool insertedWait = false;
            long sequence = 0;
            for (int tick = 0; tick < 2000 && game.RuntimeForVerification is null; tick++)
            {
                if (!insertedWait && game.GameState == SuperMetroidGameState.FileSelectMap)
                {
                    insertedWait = true;
                    for (int extra = 0; extra < wait; extra++) game.StepCaptured(0, ++sequence, 1);
                    if (wait == 1)
                    {
                        // A debugger capture in the menu must retain its RNG history,
                        // not lazily recreate the reset seed when gameplay is loaded.
                        using var graph = new MemoryStream();
                        DebuggerObjectGraphSerializer.Serialize(graph, game);
                        graph.Position = 0;
                        game = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(graph);
                    }
                }
                game.StepCaptured(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0, ++sequence, 1);
            }
            var runtime = game.RuntimeForVerification;
            if (!insertedWait || runtime?.Samus is null)
                throw new InvalidDataException("Save-load RNG audit did not reach the loaded runtime through file map.");
            Console.WriteLine($"SAVE RNG wait={wait} frontendFrames={sequence} rng={runtime.System.RandomNumber} room={runtime.ActiveRoom?.Pointer:X4} energy={runtime.Samus.Health} PB={runtime.Samus.PowerBombs}/{runtime.Samus.MaxPowerBombs}");
            // The reset vector seeds before the first main-loop/state-zero pass.
            // This empty save room has no random-consuming population initializer.
            var expected = new Bank80SystemState();
            for (long frame = 0; frame < sequence; frame++) expected.NextRandom();
            if (runtime.System.RandomNumber != expected.RandomNumber)
                throw new InvalidDataException($"Menu/load RNG: expected {expected.RandomNumber}, got {runtime.System.RandomNumber} after {sequence} frontend frames.");
            // Original-CPU probe: 426/427/428 accepted calls followed by SRAM load.
            ushort[] nativeLoaded = [51088, 59105, 33654];
            if (sequence != 426 + wait || runtime.System.RandomNumber != nativeLoaded[wait])
                throw new InvalidDataException("Save-load fixture no longer matches its recorded original-CPU pass counts/RNG words.");
            game.StepCaptured(0, ++sequence, 1);
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
