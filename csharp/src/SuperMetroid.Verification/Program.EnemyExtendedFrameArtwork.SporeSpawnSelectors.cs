using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// The unbound diagnostic deliberately reads cartridge presentation words.
    /// Installed play must select the same frame on the same tick from compiled
    /// operands, even through Spore Spawn's long fight and death programs.
    /// </summary>
    private static void VerifyInstalledSporeSpawnSelectorPrograms(
        SuperMetroidAddressSpace rom,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.Spritemaps is not null && stock.ExtendedFrames is not null,
            "installed Spore Spawn comparison has complete enemy art");

        (ushort Pointer, int Frames)[] programs =
        [
            (SporeSpawnInstructionProgramDefinitions.InitialDead, 4),
            (SporeSpawnInstructionProgramDefinitions.InitialAlive, 260),
            (SporeSpawnInstructionProgramDefinitions.FightStarted, 2200),
            (SporeSpawnInstructionProgramDefinitions.CloseAndMove, 1600),
            (SporeSpawnInstructionProgramDefinitions.Death, 500),
        ];
        int framesCompared = 0;
        var allNativeSelectors = new HashSet<ushort>();
        foreach ((ushort pointer, int frames) in programs)
        {
            var nativeBus = new SporeSpawnInstructionReadGuard(rom);
            var installedBus = new SporeSpawnInstructionReadGuard(rom)
            {
                DenyPresentationReads = true,
            };
            var nativeTrace = new List<SporeSpawnProgramFrame>(frames);
            var installedTrace = new List<SporeSpawnProgramFrame>(frames);
            RunSporeSpawnProgram(nativeBus, pointer, frames,
                afterFrame: (system, slot) =>
                    nativeTrace.Add(CaptureSporeSpawnProgramFrame(system, slot)));
            RunSporeSpawnProgram(installedBus, pointer, frames, stock,
                afterFrame: (system, slot) =>
                    installedTrace.Add(CaptureSporeSpawnProgramFrame(system, slot)));
            AssertEqual(nativeTrace.Count, installedTrace.Count,
                $"Spore Spawn program {pointer:X4} installed frame count");
            for (int frame = 0; frame < frames; frame++)
                AssertEqual(nativeTrace[frame], installedTrace[frame],
                    $"Spore Spawn program {pointer:X4} installed mechanics/frame at tick {frame}");
            AssertEqual(0, installedBus.ObservedPresentationWords.Count,
                $"Spore Spawn program {pointer:X4} installs every frame selector");
            AssertEqual(0, installedBus.ForbiddenReadAttempts,
                $"Spore Spawn program {pointer:X4} avoids ROM selector reads");
            allNativeSelectors.UnionWith(nativeBus.ObservedPresentationWords);
            framesCompared += frames;
        }
        AssertEqual(SporeSpawnInstructionProgramDefinitions.PresentationWordCount,
            allNativeSelectors.Count,
            "all Spore Spawn selector operands execute in the five native programs");
        Console.WriteLine($"  Installed Spore Spawn visual selectors: {allNativeSelectors.Count} compiled operands and {framesCompared} native-parity program ticks, with presentation ROM reads forbidden.");
    }

    private static SporeSpawnProgramFrame CaptureSporeSpawnProgramFrame(
        RoomEnemySystem system, RoomEnemySlot slot)
    {
        SporeSpawnEnemyState state = system.SporeSpawn!;
        return new SporeSpawnProgramFrame(
            slot.CurrentInstruction, slot.InstructionTimer,
            slot.SpritemapPointer, slot.XPosition, slot.YPosition,
            slot.Health, slot.Properties, slot.ExtraProperties,
            state.Function, state.Angle, state.MaximumXRadius,
            state.DeathStarted, state.DeathDropRequested);
    }

    private readonly record struct SporeSpawnProgramFrame(
        ushort Instruction, ushort Timer, ushort VisualFrame,
        ushort X, ushort Y, ushort Health,
        ushort Properties, ushort ExtraProperties,
        SporeSpawnFunction Function, ushort Angle, ushort MaximumXRadius,
        bool DeathStarted, bool DeathDropRequested);

    private static void VerifySporeSpawnVisualRemapKeepsMechanics(
        SuperMetroidAddressSpace rom,
        EnemyTileArtworkCatalog stock,
        EnemyTileArtworkCatalog remapped)
    {
        (ushort Pointer, int Frames)[] programs =
        [
            (SporeSpawnInstructionProgramDefinitions.InitialDead, 4),
            (SporeSpawnInstructionProgramDefinitions.InitialAlive, 260),
            (SporeSpawnInstructionProgramDefinitions.FightStarted, 2200),
            (SporeSpawnInstructionProgramDefinitions.CloseAndMove, 1600),
            (SporeSpawnInstructionProgramDefinitions.Death, 500),
        ];
        foreach ((ushort pointer, int frames) in programs)
        {
            var stockBus = new SporeSpawnInstructionReadGuard(rom)
            { DenyPresentationReads = true };
            var remappedBus = new SporeSpawnInstructionReadGuard(rom)
            { DenyPresentationReads = true };
            var stockTrace = new List<SporeSpawnProgramFrame>(frames);
            var remappedTrace = new List<SporeSpawnProgramFrame>(frames);
            RunSporeSpawnProgram(stockBus, pointer, frames, stock,
                afterFrame: (system, slot) =>
                    stockTrace.Add(CaptureSporeSpawnProgramFrame(system, slot)));
            RunSporeSpawnProgram(remappedBus, pointer, frames, remapped,
                afterFrame: (system, slot) =>
                    remappedTrace.Add(CaptureSporeSpawnProgramFrame(system, slot)));
            AssertEqual(stockTrace.Count, remappedTrace.Count,
                $"Spore Spawn visual remap program {pointer:X4} tick count");
            for (int frame = 0; frame < frames; frame++)
                AssertEqual(stockTrace[frame], remappedTrace[frame],
                    $"Spore Spawn visual remap leaves program {pointer:X4} " +
                    $"timing, hit frame and movement unchanged at tick {frame}");
            AssertEqual(0, stockBus.ForbiddenReadAttempts,
                $"stock Spore Spawn program {pointer:X4} avoids ROM selectors");
            AssertEqual(0, remappedBus.ForbiddenReadAttempts,
                $"remapped Spore Spawn program {pointer:X4} avoids ROM selectors");
        }
        Console.WriteLine("  Spore Spawn animation override: a same-family visual remap changes live OAM while all five programs retain their native timing, pose, movement and hit-frame sequence.");
    }
}
