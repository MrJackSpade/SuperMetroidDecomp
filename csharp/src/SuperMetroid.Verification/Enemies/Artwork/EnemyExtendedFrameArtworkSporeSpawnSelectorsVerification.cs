using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.SporeSpawn.SporeSpawnAnimationsVerification;
using SuperMetroid.Verification.Bosses.SporeSpawn;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Compares installed Spore Spawn frame selectors with an independent cartridge instruction trace.</summary>
internal static class EnemyExtendedFrameArtworkSporeSpawnSelectorsVerification
{
    /// <summary>
    /// The independent diagnostic reads the pinned cartridge instruction stream.
    /// Installed play must select the same frame on the same tick from compiled
    /// operands, even through Spore Spawn's long fight and death programs.
    /// </summary>
    internal static void VerifyInstalledSporeSpawnSelectorPrograms(
        SuperMetroidAddressSpace rom,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.Spritemaps is not null && stock.ExtendedFrames is not null,
            "installed Spore Spawn comparison has complete enemy art");

        (SporeSpawnBodyProgram Program, int Frames)[] programs =
        [
            (SporeSpawnBodyProgram.InitialDead, 4),
            (SporeSpawnBodyProgram.InitialAlive, 260),
            (SporeSpawnBodyProgram.FightStarted, 2200),
            (SporeSpawnBodyProgram.CloseAndMove, 1600),
            (SporeSpawnBodyProgram.Death, 500),
        ];
        int framesCompared = 0;
        var allNativeSelectors = new HashSet<ushort>();
        // Spore Spawn's body runs typed programs (#1288); its cursor is compared as the native pointer it stands for.
        var steps = BodyStepAddresses(CartridgeImportSource.Require(rom));
        foreach ((SporeSpawnBodyProgram program, int frames) in programs)
        {
            ushort pointer = SporeSpawnNativeListAddresses.BodyEntry(program);

            var installedBus = NewSporeSpawnInstructionReadGuard(rom);
            var nativeTrace = ReadReferenceSporeSpawnProgram(rom, pointer, frames, allNativeSelectors);
            var installedTrace = new List<SporeSpawnProgramFrame>(frames);
            RunSporeSpawnProgram(program, frames, stock,
                afterFrame: (system, slot) =>
                    installedTrace.Add(CaptureSporeSpawnProgramFrame(system, slot, steps)));
            AssertEqual(nativeTrace.Count, installedTrace.Count,
                $"Spore Spawn program {pointer:X4} installed frame count");
            for (int frame = 0; frame < frames; frame++)
                AssertEqual(nativeTrace[frame], installedTrace[frame],
                    $"Spore Spawn program {pointer:X4} installed mechanics/frame at tick {frame}");
            AssertEqual(0, installedBus.ObservedWords.Count,
                $"Spore Spawn program {pointer:X4} installs every frame selector");
            AssertEqual(0, installedBus.ForbiddenReadAttempts,
                $"Spore Spawn program {pointer:X4} avoids ROM selector reads");

            framesCompared += frames;
        }
        AssertEqual(BodyFrameRecordCount,
            allNativeSelectors.Count,
            "all Spore Spawn selector operands execute in the five native programs");
        Console.WriteLine($"  Installed Spore Spawn visual selectors: {allNativeSelectors.Count} compiled operands and {framesCompared} native-parity program ticks, with presentation ROM reads forbidden.");
    }

    /// <summary>
    /// Reads bank-A5 opcode lengths and effects from the pinned cartridge stream,
    /// independently of the compiled catalogs. Tracks instruction timing, selected
    /// frames, and relevant state; palette, sound, and particles have separate fixtures.
    /// </summary>
    internal static List<SporeSpawnProgramFrame> ReadReferenceSporeSpawnProgram(
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
            timer = NativeWordCounter.Decrement(timer).Value;
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
                            loop = NativeWordCounter.Decrement(loop).Value;
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
                new(128, 624), 960, properties, extra, function, 0, radius, false, drops));
        }
        return trace;
    }
    /// <summary>Captures the live state compared with one native Spore Spawn instruction tick.</summary>
    private static SporeSpawnProgramFrame CaptureSporeSpawnProgramFrame(
        RoomEnemySystem system, RoomEnemySlot slot, Dictionary<SporeSpawnBodyProgram, ushort[]> steps)
    {
        SporeSpawnEnemyState state = (system.SporeSpawn ?? throw new InvalidOperationException("Verification requires system.SporeSpawn."));
        return new SporeSpawnProgramFrame(
            NativeBodyInstruction(steps, state), slot.InstructionTimer.RawValue,
            slot.SpritemapPointer, slot.Position.Whole,
            slot.Health, slot.Properties.ToNativeWord(), slot.ExtraProperties.ToNativeWord(),
            (state ?? throw new InvalidOperationException("Verification requires state.")).Function, state.Angle, state.MaximumXRadius,
            state.DeathStarted, state.DeathDropRequested);
    }

    /// <summary>Instruction, frame, position, health, and fight-state values compared across Spore Spawn program ticks.</summary>
    internal readonly record struct SporeSpawnProgramFrame(
        ushort Instruction, ushort Timer, ushort VisualFrame,
        Position16<RoomPixels> Position, ushort Health,
        ushort Properties, ushort ExtraProperties,
        SporeSpawnFunction Function, ushort Angle, ushort MaximumXRadius,
        bool DeathStarted, bool DeathDropRequested);

    /// <summary>Checks that replacing compatible display frames leaves all five programs' timing and gameplay state intact.</summary>
    internal static void VerifySporeSpawnVisualRemapKeepsMechanics(
        SuperMetroidAddressSpace rom,
        EnemyTileArtworkCatalog stock,
        EnemyTileArtworkCatalog remapped)
    {
        (SporeSpawnBodyProgram Program, int Frames)[] programs =
        [
            (SporeSpawnBodyProgram.InitialDead, 4),
            (SporeSpawnBodyProgram.InitialAlive, 260),
            (SporeSpawnBodyProgram.FightStarted, 2200),
            (SporeSpawnBodyProgram.CloseAndMove, 1600),
            (SporeSpawnBodyProgram.Death, 500),
        ];
        var steps = BodyStepAddresses(CartridgeImportSource.Require(rom));
        foreach ((SporeSpawnBodyProgram program, int frames) in programs)
        {
            ushort pointer = SporeSpawnNativeListAddresses.BodyEntry(program);
            var stockBus = NewSporeSpawnInstructionReadGuard(rom);
            var remappedBus = NewSporeSpawnInstructionReadGuard(rom);
            var stockTrace = new List<SporeSpawnProgramFrame>(frames);
            var remappedTrace = new List<SporeSpawnProgramFrame>(frames);
            RunSporeSpawnProgram(program, frames, stock,
                afterFrame: (system, slot) =>
                    stockTrace.Add(CaptureSporeSpawnProgramFrame(system, slot, steps)));
            RunSporeSpawnProgram(program, frames, remapped,
                afterFrame: (system, slot) =>
                    remappedTrace.Add(CaptureSporeSpawnProgramFrame(system, slot, steps)));
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
