using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// The independent diagnostic reads the pinned cartridge instruction stream.
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

            var installedBus = new SporeSpawnInstructionReadGuard(rom)
            {
                DenyPresentationReads = true,
            };
            var nativeTrace = ReadReferenceSporeSpawnProgram(rom, pointer, frames, allNativeSelectors);
            var installedTrace = new List<SporeSpawnProgramFrame>(frames);
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

            framesCompared += frames;
        }
        AssertEqual(SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordCount,
            allNativeSelectors.Count,
            "all Spore Spawn selector operands execute in the five native programs");
        Console.WriteLine($"  Installed Spore Spawn visual selectors: {allNativeSelectors.Count} compiled operands and {framesCompared} native-parity program ticks, with presentation ROM reads forbidden.");
    }

    // Test-only bank-$A5 stream reader. Opcode lengths and effects are taken
    // from pinned sm_a0.c / sm_a5.c, independently of the compiled catalogs.
    // Palette, sound and particle side effects have their own focused fixtures.
    /// <summary>Interprets one Spore Spawn bank-$A5 program from cartridge bytes to build an independent per-tick reference trace.</summary>
    /// <param name="rom">Retail cartridge source for the native instruction stream and operands.</param>
    /// <param name="initial">Address of the selected program's first instruction.</param>
    /// <param name="frames">Number of update ticks to record.</param>
    /// <param name="selectors">Set receiving each spritemap operand address encountered in a timed frame.</param>
    /// <returns>Expected instruction, timer, visual selector, and gameplay-state values for each tick.</returns>
    private static List<SporeSpawnProgramFrame> ReadReferenceSporeSpawnProgram(
        SuperMetroidAddressSpace rom, ushort initial, int frames, HashSet<ushort> selectors)
    {
        ushort Read(int address) => ReadSporeSpawnProgramWord(rom, unchecked((ushort)address));
        ushort instruction = initial, timer = 1, loop = 0, visual = 0;
        ushort properties = 0, extra = 0, radius = 48;
        var function = SporeSpawnFunction.Idle;
        bool drops = false;
        var trace = new List<SporeSpawnProgramFrame>(frames);
        for (int frame = 0; frame < frames; frame++)
        {
            ushort oldTimer = timer;
            timer = unchecked((ushort)(timer - 1));
            if (oldTimer != 1)
                extra &= 0x7fff;
            else
            {
                int cursor = instruction;
                bool stopped = false;
                for (int commands = 0; commands < 64 && !stopped; commands++)
                {
                    ushort opcode = Read(cursor);
                    if (opcode < 0x8000)
                    {
                        timer = opcode;
                        visual = Read(cursor + 2);
                        selectors.Add((ushort)(cursor + 2));
                        instruction = (ushort)(cursor + 4);
                        extra |= 0x8000;
                        stopped = true;
                        continue;
                    }
                    switch (opcode)
                    {
                        case 0x80ed: cursor = Read(cursor + 2); break;
                        case 0x8110:
                            ushort oldLoop = loop;
                            loop = unchecked((ushort)(loop - 1));
                            cursor = oldLoop == 1 ? cursor + 4 : Read(cursor + 2);
                            break;
                        case 0x8123: loop = Read(cursor + 2); cursor += 4; break;
                        case 0x812f: instruction = (ushort)cursor; stopped = true; break;
                        case 0x813a:
                            timer = Read(cursor + 2);
                            instruction = (ushort)(cursor + 4);
                            stopped = true;
                            break;
                        case 0xe75f:
                            if ((short)(radius - 40) < 0) radius += 8;
                            cursor += 2;
                            break;
                        case 0xe82d: radius = Read(cursor + 2); cursor += 6; break;
                        case 0xe87c: properties = (ushort)((properties & 0x5bff) | 0xa000); cursor += 2; break;
                        case 0xe8b1: drops = true; cursor += 2; break;
                        case 0xe8ba: function = (SporeSpawnFunction)Read(cursor + 2); cursor += 4; break;
                        case 0xe872: case 0xe895: case 0xe8ca: case 0xe91c: cursor += 4; break;
                        case 0xe771: case 0xe96e: case 0xe9b1: cursor += 2; break;
                        default: throw new InvalidDataException($"Unexpected native Spore Spawn opcode {opcode:X4} at {cursor:X4}.");
                    }
                }
                AssertTrue(stopped, "native Spore Spawn instruction tick terminates");
            }
            trace.Add(new SporeSpawnProgramFrame(instruction, timer, visual,
                128, 624, 960, properties, extra, function, 0, radius, false, drops));
        }
        return trace;
    }
    /// <summary>Snapshots installed runtime fields that are compared with the independently interpreted reference trace.</summary>
    /// <param name="system">Enemy system containing the active Spore Spawn state.</param>
    /// <param name="slot">Room slot carrying the current instruction, timer, visual frame, and actor properties.</param>
    /// <returns>A value record of the selected frame's mechanics and presentation state.</returns>
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

    /// <summary>Per-update comparison row for native and installed Spore Spawn instruction execution.</summary>
    /// <param name="Instruction">Current instruction pointer after processing this update.</param>
    /// <param name="Timer">Instruction countdown remaining at the end of the update.</param>
    /// <param name="VisualFrame">Selected spritemap pointer used for the actor's current appearance.</param>
    /// <param name="X">Actor center X coordinate.</param>
    /// <param name="Y">Actor center Y coordinate.</param>
    /// <param name="Health">Current enemy health.</param>
    /// <param name="Properties">Native enemy property word affecting collision and display behavior.</param>
    /// <param name="ExtraProperties">Native extended property word, including instruction-frame state.</param>
    /// <param name="Function">Current Spore Spawn AI function.</param>
    /// <param name="Angle">Current movement or attack angle.</param>
    /// <param name="MaximumXRadius">Maximum horizontal attack radius selected by the program.</param>
    /// <param name="DeathStarted">Whether the boss has entered its death sequence.</param>
    /// <param name="DeathDropRequested">Whether the death program has requested its item drop.</param>
    private readonly record struct SporeSpawnProgramFrame(
        ushort Instruction, ushort Timer, ushort VisualFrame,
        ushort X, ushort Y, ushort Health,
        ushort Properties, ushort ExtraProperties,
        SporeSpawnFunction Function, ushort Angle, ushort MaximumXRadius,
        bool DeathStarted, bool DeathDropRequested);

    /// <summary>Confirms that replacing Spore Spawn artwork leaves all five installed programs' per-tick mechanics trace unchanged.</summary>
    /// <param name="rom">Retail cartridge used by the independent reference and read guard.</param>
    /// <param name="stock">Catalog with the stock Spore Spawn art family.</param>
    /// <param name="remapped">Catalog with replacement visuals whose mechanics must match the stock run.</param>
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
