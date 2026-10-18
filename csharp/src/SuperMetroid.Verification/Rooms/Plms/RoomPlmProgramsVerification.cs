using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using static SuperMetroid.Verification.Rooms.Plms.NativePlmListDefinitions;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmNativeListAddresses;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// #1296: the room PLM programs typed in place of their bank-$84 lists. Each typed program must equal its native list
/// step by step: draws and durations, deletion, sleeps, and the cue of every other instruction with its operands. The
/// families' runtime suites check what those cues do.
/// </summary>
internal static class RoomPlmProgramsVerification
{
    /// <summary>The n00b tube's programs equal their native lists.</summary>
    internal static void VerifyNoobTubePrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), NoobTubePlmProgramDefinitions.Programs, NoobTubeEntries,
            draw => (ushort)draw, NoobTubeCueAt, "n00b tube");
        Console.WriteLine($"  N00b tube programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The shared delete and sleep programs equal their one-instruction native lists.</summary>
    internal static void VerifySharedPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), RoomPlmSharedProgramDefinitions.Programs, SharedEntries,
            draw => throw new InvalidOperationException($"The shared programs never draw; got {draw}."), NoCues<PlmNoCue>(), "shared PLM");
        Console.WriteLine($"  Shared PLM programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The lavaquake program equals its native list; each install names its phase's pre-instruction.</summary>
    internal static void VerifySpeedBoosterEscapePrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), SpeedBoosterEscapePlmProgramDefinitions.Programs,
            SpeedBoosterEscapeEntries, draw => throw new InvalidOperationException($"The lavaquake program never draws; got {draw}."),
            (IImportCartridgeSource cartridge, ushort cursor, out SpeedBoosterEscapePreInstruction cue, out int length) =>
            {
                (cue, length) = ((SpeedBoosterEscapePreInstruction)cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), 4);
                return InstructionAt<RoomPlmInstruction>(cartridge, cursor) == RoomPlmInstruction.InstallPreInstruction;
            }, "Speed Booster lavaquake");
        // A wake steps over the sleep that closes each phase, onto the next phase's list.
        for (int phase = 0; phase < SpeedBoosterEscapeEntries.Count; phase++)
        {
            (SpeedBoosterEscapeProgram program, ushort entry) = SpeedBoosterEscapeEntries[phase];
            SpeedBoosterEscapeProgram? expected = phase + 1 < SpeedBoosterEscapeEntries.Count ? SpeedBoosterEscapeEntries[phase + 1].Program : null;
            if (expected is { } next)
                AssertEqual(unchecked((ushort)(entry + 6)), SpeedBoosterEscapeEntries[phase + 1].Entry, $"lavaquake {next} follows {program}'s sleep");
            AssertEqual(expected, SpeedBoosterEscapePlmProgramDefinitions.PhaseAfter(program), $"lavaquake phase after {program}");
        }
        Console.WriteLine($"  Speed Booster lavaquake program: {steps} typed steps match bank $84.");
    }

    /// <summary>The block-reaction programs equal their native lists; the restore instruction draws the saved block.</summary>
    internal static void VerifyBlockReactionPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), BlockReactionPlmProgramDefinitions.Programs, BlockReactionEntries,
            frame => BlockReactionPlmDrawDefinitions.DrawLists[frame], BlockReactionCueAt, "block reaction", BlockReactionRestoreAt);
        Console.WriteLine($"  Block-reaction programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The block reactions' sound queues and grapple BTS write.</summary>
    private static bool BlockReactionCueAt(IImportCartridgeSource cartridge, ushort cursor, out BlockReactionPlmCue cue, out int length)
    {
        (cue, length) = (default, 0);
        switch (InstructionAt<RoomPlmInstruction>(cartridge, cursor))
        {
            case RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct:
                (cue, length) = (new(BlockReactionPlmCueKind.QueueSoundLibrary2Maximum1, ReadOperandByte(cartridge, cursor + 2)), 3);
                return true;
            case RoomPlmInstruction.QueueSoundLibrary2Maximum3:
                (cue, length) = (new(BlockReactionPlmCueKind.QueueSoundLibrary2Maximum3, ReadOperandByte(cartridge, cursor + 2)), 3);
                return true;
            case RoomPlmInstruction.QueueSoundLibrary2Maximum6:
                (cue, length) = (new(BlockReactionPlmCueKind.QueueSoundLibrary2Maximum6, ReadOperandByte(cartridge, cursor + 2)), 3);
                return true;
            case RoomPlmInstruction.SetPlmBtsToOne:
                (cue, length) = (new(BlockReactionPlmCueKind.SetBtsToOne, 0), 2);
                return true;
            default:
                return false;
        }
    }

    /// <summary>The restore instruction, which draws the saved block for one update.</summary>
    private static bool BlockReactionRestoreAt(IImportCartridgeSource cartridge, ushort cursor, out BlockReactionPlmFrame frame, out int duration, out int length)
    {
        (frame, duration, length) = (BlockReactionPlmFrame.RestoredBlock, 1, 2);
        return InstructionAt<RoomPlmInstruction>(cartridge, cursor) == RoomPlmInstruction.DrawPlmBlock;
    }

    /// <summary>The Speed Booster blocks' programs equal their native lists; both restore instructions draw the saved block.</summary>
    internal static void VerifySpeedBoosterBlockPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), SpeedBoosterBlockPlmProgramDefinitions.Programs,
            SpeedBoosterBlockEntries, SpeedBoosterBlockDrawList, SpeedBoosterBlockCueAt, "Speed Booster block", SpeedBoosterBlockRestoreAt);
        Console.WriteLine($"  Speed Booster block programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The blocks' break-sound instruction.</summary>
    private static bool SpeedBoosterBlockCueAt(IImportCartridgeSource cartridge, ushort cursor, out SpeedBoosterBlockCue cue, out int length)
    {
        (cue, length) = (SpeedBoosterBlockCue.QueueBreakSound, 3);
        if (InstructionAt<RoomPlmInstruction>(cartridge, cursor) != RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct)
            return false;
        AssertEqual(SpeedBoosterBlockPlmProgramDefinitions.BreakSoundId, ReadOperandByte(cartridge, cursor + 2), "speed block break sound");
        AssertEqual((byte)1, SpeedBoosterBlockPlmProgramDefinitions.BreakSoundMaximumQueued, "speed block break sound queue cap");
        return true;
    }

    /// <summary>The two restore instructions, which draw the saved block for one update.</summary>
    private static bool SpeedBoosterBlockRestoreAt(IImportCartridgeSource cartridge, ushort cursor, out SpeedBoosterBlockFrame frame, out int duration, out int length)
    {
        (frame, duration, length) = (SpeedBoosterBlockFrame.RestoredBlock, 1, 2);
        return InstructionAt<RoomPlmInstruction>(cartridge, cursor) is RoomPlmInstruction.DrawPlmBlock or RoomPlmInstruction.DrawPlmBlockClone;
    }

    /// <summary>The bank-$84 draw list a Speed Booster block frame draws.</summary>
    private static ushort SpeedBoosterBlockDrawList(SpeedBoosterBlockFrame frame) => frame switch
    {
        SpeedBoosterBlockFrame.Crumble0 => SpeedBoosterBlockPlmDrawDefinitions.CrumbleDrawList(0),
        SpeedBoosterBlockFrame.Crumble1 => SpeedBoosterBlockPlmDrawDefinitions.CrumbleDrawList(1),
        SpeedBoosterBlockFrame.Crumble2 => SpeedBoosterBlockPlmDrawDefinitions.CrumbleDrawList(2),
        SpeedBoosterBlockFrame.Crumble3 => SpeedBoosterBlockPlmDrawDefinitions.CrumbleDrawList(3),
        SpeedBoosterBlockFrame.BombReveal => SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw,
        SpeedBoosterBlockFrame.RestoredBlock => throw new InvalidOperationException("The restored block is drawn by its native restore instruction."),
        _ => throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterBlockFrame)} {frame}."),
    };

    /// <summary>The downward gate's programs equal their native lists, and each wake continues after the sleep it ends.</summary>
    internal static void VerifyDownwardGatePrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), DownwardGatePlmProgramDefinitions.Programs, DownwardGateEntries,
            draw => (ushort)draw, DownwardGateCueAt, "downward gate");
        foreach ((DownwardGateProgram program, ushort entry) in DownwardGateEntries)
        {
            if (DownwardGatePlmProgramDefinitions.ProgramAfterSleep(program) is not { } next)
                continue;
            AssertEqual(unchecked((ushort)(entry + DownwardGateSleepContinuationOffset)), DownwardGateEntries.Single(e => e.Program == next).Entry,
                $"downward gate {next} continues {program} past its sleep");
        }
        ushort firstTrigger = DownwardGateEntries.Single(e => e.Program == DownwardGateProgram.TriggerBlueLeft).Entry;
        AssertTrue(DownwardGatePlmProgramDefinitions.TriggerPrograms.SequenceEqual(
                DownwardGateEntries.Where(e => e.Entry >= firstTrigger).OrderBy(e => e.Entry).Select(e => e.Program)),
            "downward gate triggers are selected in native list order");
        Console.WriteLine($"  Downward gate programs: {steps} typed steps match bank $84.");
    }

    /// <summary>
    /// The native instruction pointer a downward gate's typed program stands for: the address of the native instruction
    /// its cursor rests on, for comparisons with native gate traces.
    /// </summary>
    internal static ushort DownwardGateNativeInstruction(ISnesAddressSpace rom, DownwardGatePlmAnimation gate)
    {
        downwardGateAddresses ??= StepAddresses(CartridgeImportSource.Require(rom), DownwardGatePlmProgramDefinitions.Programs,
            DownwardGateEntries, draw => (ushort)draw, DownwardGateCueAt, "downward gate");
        return downwardGateAddresses[(gate.Cursor.Program, gate.Cursor.Position)];
    }

    /// <summary>The downward gate's step addresses, walked once.</summary>
    private static IReadOnlyDictionary<(DownwardGateProgram Program, int Position), ushort>? downwardGateAddresses;

    /// <summary>The gate's trigger, pre-instruction, projectile and sound instructions with their operands.</summary>
    private static bool DownwardGateCueAt(IImportCartridgeSource cartridge, ushort cursor, out DownwardGateCue cue, out int length)
    {
        ushort operand = cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2));
        (DownwardGateCue Cue, int Length)? decoded = InstructionAt<RoomPlmInstruction>(cartridge, cursor) switch
        {
            RoomPlmInstruction.ClearDownwardGateTrigger => (DownwardGateCue.ClearTrigger, 2),
            RoomPlmInstruction.InstallPreInstruction => ((DownwardGatePreInstruction)operand == DownwardGatePreInstruction.WakeIfTriggered
                ? DownwardGateCue.ArmWakeIfTriggered : DownwardGateCue.ArmWakeIfTriggeredOrSamusBelow, 4),
            RoomPlmInstruction.SpawnDownwardGateProjectile => (DownwardGateCue.SpawnMovingGate, 4),
            RoomPlmInstruction.WakeDownwardGateProjectile => (DownwardGateCue.WakeGateProjectile, 4),
            RoomPlmInstruction.QueueSoundLibrary3Maximum6 => (DownwardGateCue.QueueMovementSound, 3),
            _ => null,
        };
        (cue, length) = decoded ?? default;
        if (decoded is null)
            return false;
        if (cue == DownwardGateCue.ArmWakeIfTriggeredOrSamusBelow)
            AssertEqual((ushort)DownwardGatePreInstruction.WakeIfTriggeredOrSamusBelow, operand, "downward gate closed wake");
        if (cue == DownwardGateCue.SpawnMovingGate)
            AssertEqual((ushort)RoomEnemyProjectileKind.DownwardGateMoving, operand, "downward gate moving projectile");
        if (cue == DownwardGateCue.QueueMovementSound)
        {
            AssertEqual(DownwardGatePlmRomData.MovementSound, ReadOperandByte(cartridge, cursor + 2), "downward gate movement sound");
            AssertEqual((byte)6, DownwardGatePlmProgramDefinitions.MovementSoundMaximumQueued, "downward gate movement sound queue cap");
        }
        return true;
    }

    /// <summary>The Tourian elevator access programs equal their native lists.</summary>
    internal static void VerifyTourianAccessPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), TourianAccessPlmProgramDefinitions.Programs, TourianAccessEntries,
            draw => (ushort)draw,
            (IImportCartridgeSource cartridge, ushort cursor, out TourianAccessCue cue, out int length) =>
            {
                (cue, length) = (TourianAccessCue.MoveDown, 2);
                return InstructionAt<RoomPlmInstruction>(cartridge, cursor) == RoomPlmInstruction.MoveTourianAccessDown;
            }, "Tourian access");
        Console.WriteLine($"  Tourian access programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The Wrecked Ship attic's program equals its native list.</summary>
    internal static void VerifyWreckedShipAtticPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), WreckedShipAtticPlmProgramDefinitions.Programs, WreckedShipAtticEntries,
            draw => throw new InvalidOperationException($"The attic program never draws; got {draw}."),
            (IImportCartridgeSource cartridge, ushort cursor, out WreckedShipAtticCue cue, out int length) =>
            {
                (cue, length) = (WreckedShipAtticCue.InstallInertPreInstruction, 4);
                if (InstructionAt<RoomPlmInstruction>(cartridge, cursor) != RoomPlmInstruction.InstallPreInstruction)
                    return false;
                AssertEqual(WreckedShipAtticPlmRomData.NoOpCallback, cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), "attic pre-instruction");
                return true;
            }, "Wrecked Ship attic");
        Console.WriteLine($"  Wrecked Ship attic program: {steps} typed steps match bank $84.");
    }

    /// <summary>The blue caps' programs equal their native lists.</summary>
    internal static void VerifyBlueDoorPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), BlueDoorPlmProgramDefinitions.Programs, DoorPlmNativeListAddresses.BlueEntries,
            BlueDoorDrawList, BlueDoorCueAt, "blue door");
        Console.WriteLine($"  Blue door programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The draw list a blue program's frame draws: the program names the facing.</summary>
    private static ushort BlueDoorDrawList(BlueDoorProgram program, BlueDoorFrame frame)
    {
        ColoredDoorOrientation orientation = BlueDoorPlmProgramDefinitions.Orientation(program);
        return frame switch
        {
            BlueDoorFrame.Clear => DoorPlmDrawListDefinitions.Clear(orientation),
            BlueDoorFrame.Blue0 => DoorPlmDrawListDefinitions.Blue(orientation, 0),
            BlueDoorFrame.Blue1 => DoorPlmDrawListDefinitions.Blue(orientation, 1),
            BlueDoorFrame.Blue2 => DoorPlmDrawListDefinitions.Blue(orientation, 2),
            BlueDoorFrame.Blue3 => DoorPlmDrawListDefinitions.Blue(orientation, 3),
            _ => throw new InvalidOperationException($"Undefined {nameof(BlueDoorFrame)} {frame}."),
        };
    }

    /// <summary>The blue caps' sound and BTS-restore instructions.</summary>
    private static bool BlueDoorCueAt(IImportCartridgeSource cartridge, ushort cursor, out BlueDoorCue cue, out int length)
    {
        (BlueDoorCue Cue, int Length)? decoded = InstructionAt<RoomPlmInstruction>(cartridge, cursor) switch
        {
            RoomPlmInstruction.QueueSoundLibrary3Maximum6 => (DoorSoundCue(Library3Sound(cartridge, cursor), BlueDoorCue.QueueOpenSound, BlueDoorCue.QueueCloseSound), 3),
            RoomPlmInstruction.SetPlmBtsFromByte => (BlueDoorCue.RestoreBlueBts, 3),
            _ => null,
        };
        (cue, length) = decoded ?? default;
        return decoded is not null;
    }

    /// <summary>The library-3 sound a queue-sound instruction at <paramref name="cursor"/> names.</summary>
    private static SoundEffectId Library3Sound(IImportCartridgeSource cartridge, int cursor) =>
        new(SoundEffectLibrary.Library3, ReadOperandByte(cartridge, cursor + 2));

    /// <summary>A door sound as its cue: the open or the close sound.</summary>
    private static TCue DoorSoundCue<TCue>(SoundEffectId sound, TCue open, TCue close) =>
        sound == BlueDoorPlmProgramDefinitions.OpenSound ? open :
        sound == BlueDoorPlmProgramDefinitions.CloseSound ? close :
        throw new InvalidOperationException($"Door sound {sound} is neither the open nor the close sound.");

    /// <summary>Every coloured cap's programs equal their native lists.</summary>
    internal static void VerifyColoredDoorPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int steps = 0;
        foreach (ColoredDoorColor color in Enum.GetValues<ColoredDoorColor>())
        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        {
            var entries = DoorPlmNativeListAddresses.ColoredEntries(color, orientation);
            steps += CompareProgramSet(cartridge, ColoredDoorPlmProgramDefinitions.ProgramsFor(color, orientation), entries,
                frame => ColoredCapDraw(color, orientation, frame),
                (IImportCartridgeSource source, ushort cursor, out ColoredDoorCue cue, out int length) =>
                    ColoredDoorCueAt(source, cursor, color, orientation, entries, out cue, out length),
                $"{color} {orientation} door");
        }
        Console.WriteLine($"  Coloured door programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The draw list a coloured cap's frame draws.</summary>
    private static ushort ColoredCapDraw(ColoredDoorColor color, ColoredDoorOrientation orientation, DoorCapFrame frame) => frame switch
    {
        DoorCapFrame.Clear => DoorPlmDrawListDefinitions.Clear(orientation),
        DoorCapFrame.Closed0 => DoorPlmDrawListDefinitions.Colored(color, orientation, 0),
        DoorCapFrame.Closed1 => DoorPlmDrawListDefinitions.Colored(color, orientation, 1),
        DoorCapFrame.Closed2 => DoorPlmDrawListDefinitions.Colored(color, orientation, 2),
        DoorCapFrame.Closed3 => DoorPlmDrawListDefinitions.Colored(color, orientation, 3),
        DoorCapFrame.BlueFlash => DoorPlmDrawListDefinitions.Blue(orientation, 0),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapFrame)} {frame}."),
    };

    /// <summary>A coloured cap's branch, link, wake, hit-count and sound instructions with their operands.</summary>
    private static bool ColoredDoorCueAt(IImportCartridgeSource cartridge, ushort cursor, ColoredDoorColor color, ColoredDoorOrientation orientation,
        IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> entries, out ColoredDoorCue cue, out int length)
    {
        ushort Word(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        ushort EntryOf(ColoredDoorProgram program) => entries.Single(entry => entry.Program == program).Entry;
        switch (InstructionAt<RoomPlmInstruction>(cartridge, cursor))
        {
            case RoomPlmInstruction.GotoIfDoorBitSet:
                AssertEqual(DoorPlmNativeListAddresses.BlueEntries.Single(e => e.Program == BlueDoorPlmProgramDefinitions.Closed(orientation)).Entry,
                    Word(2), $"{color} {orientation} door becomes its blue cap");
                (cue, length) = (ColoredDoorCue.BranchIfOpened, 4);
                return true;
            case RoomPlmInstruction.LinkInstruction:
                AssertEqual(EntryOf(ColoredDoorProgram.Hit), Word(2), $"{color} {orientation} door links its hit");
                (cue, length) = (ColoredDoorCue.LinkHit, 4);
                return true;
            case RoomPlmInstruction.InstallPreInstruction:
                AssertEqual((ushort)ColoredDoorPlmProgramDefinitions.WakeOf(color), Word(2), $"{color} {orientation} door shot wake");
                (cue, length) = (ColoredDoorCue.ArmWake, 4);
                return true;
            case RoomPlmInstruction.IncrementDoorHitCounterAndGoto:
                AssertEqual(ColoredDoorPlmProgramDefinitions.HitThresholdOf(color), ReadOperandByte(cartridge, cursor + 2), $"{color} door hit threshold");
                AssertEqual(EntryOf(ColoredDoorProgram.Open), Word(3), $"{color} {orientation} door opens at its threshold");
                (cue, length) = (ColoredDoorCue.CountHit, 5);
                return true;
            case RoomPlmInstruction.QueueSoundLibrary3Maximum6:
                SoundEffectId sound = Library3Sound(cartridge, cursor);
                (cue, length) = (sound == ColoredDoorPlmProgramDefinitions.RedHitSound
                    ? ColoredDoorCue.QueueHitSound
                    : DoorSoundCue(sound, ColoredDoorCue.QueueOpenSound, ColoredDoorCue.QueueCloseSound), 3);
                return true;
            default:
                (cue, length) = (default, 0);
                return false;
        }
    }

    /// <summary>Every grey cap's programs, Bomb Torizo's included, equal their native lists.</summary>
    internal static void VerifyGreyDoorPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int steps = 0;
        foreach (GreyDoorVariant variant in DoorPlmNativeListAddresses.GreyVariants)
            steps += CompareProgramSet(cartridge, GreyDoorPlmProgramDefinitions.ProgramsFor(variant), DoorPlmNativeListAddresses.GreyEntries(variant),
                frame => GreyCapDraw(variant.Orientation, frame),
                (IImportCartridgeSource source, ushort cursor, out GreyDoorCue cue, out int length) =>
                    GreyDoorCueAt(source, cursor, variant, out cue, out length),
                variant.BombTorizo ? "Bomb Torizo door" : $"grey {variant.Orientation} door");
        Console.WriteLine($"  Grey door programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The draw list a grey cap's frame draws.</summary>
    private static ushort GreyCapDraw(ColoredDoorOrientation orientation, DoorCapFrame frame) => frame switch
    {
        DoorCapFrame.Clear => DoorPlmDrawListDefinitions.Clear(orientation),
        DoorCapFrame.Closed0 => DoorPlmDrawListDefinitions.Grey(orientation, 0),
        DoorCapFrame.Closed1 => DoorPlmDrawListDefinitions.Grey(orientation, 1),
        DoorCapFrame.Closed2 => DoorPlmDrawListDefinitions.Grey(orientation, 2),
        DoorCapFrame.Closed3 => DoorPlmDrawListDefinitions.Grey(orientation, 3),
        DoorCapFrame.BlueFlash => DoorPlmDrawListDefinitions.Blue(orientation, 0),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapFrame)} {frame}."),
    };

    /// <summary>A grey cap's branch, link, wake, hit-count, sound and Bombs-wait instructions with their operands.</summary>
    private static bool GreyDoorCueAt(IImportCartridgeSource cartridge, ushort cursor, GreyDoorVariant variant, out GreyDoorCue cue, out int length)
    {
        ushort Word(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        var entries = DoorPlmNativeListAddresses.GreyEntries(variant);
        ushort EntryOf(GreyDoorProgram program) => entries.Single(entry => entry.Program == program).Entry;
        switch (InstructionAt<RoomPlmInstruction>(cartridge, cursor))
        {
            case RoomPlmInstruction.GotoIfDoorBitSet:
                AssertEqual(DoorPlmNativeListAddresses.BlueEntries.Single(e => e.Program == BlueDoorPlmProgramDefinitions.Closed(variant.Orientation)).Entry,
                    Word(2), $"grey {variant} becomes its blue cap");
                (cue, length) = (GreyDoorCue.BranchIfOpened, 4);
                return true;
            case RoomPlmInstruction.LinkInstruction:
                ushort target = Word(2);
                AssertTrue(target == EntryOf(GreyDoorProgram.Activate) || target == EntryOf(GreyDoorProgram.Hit), $"grey {variant} link target");
                (cue, length) = (target == EntryOf(GreyDoorProgram.Activate) ? GreyDoorCue.LinkActivate : GreyDoorCue.LinkHit, 4);
                return true;
            case RoomPlmInstruction.SetGreyDoorPreInstruction:
                (cue, length) = (GreyDoorCue.ArmCondition, 2);
                return true;
            case RoomPlmInstruction.InstallPreInstruction:
                AssertEqual((ushort)GreyDoorPreInstruction.WakeWhenShot, Word(2), $"grey {variant} shot wake");
                (cue, length) = (GreyDoorCue.ArmShotWake, 4);
                return true;
            case RoomPlmInstruction.IncrementDoorHitCounterAndGoto:
                AssertEqual(GreyDoorPlmProgramDefinitions.HitThreshold, ReadOperandByte(cartridge, cursor + 2), "grey door hit threshold");
                AssertEqual(EntryOf(GreyDoorProgram.Open), Word(3), $"grey {variant} opens at its threshold");
                (cue, length) = (GreyDoorCue.CountHit, 5);
                return true;
            case RoomPlmInstruction.QueueSoundLibrary3Maximum6:
                (cue, length) = (DoorSoundCue(Library3Sound(cartridge, cursor), GreyDoorCue.QueueOpenSound, GreyDoorCue.QueueCloseSound), 3);
                return true;
            case RoomPlmInstruction.GotoIfSamusHasNoBombs:
                AssertEqual(EntryOf(GreyDoorProgram.Closing), Word(2), "Bomb Torizo door repeats its closing");
                (cue, length) = (GreyDoorCue.RepeatUntilSamusHasBombs, 4);
                return true;
            default:
                (cue, length) = (default, 0);
                return false;
        }
    }

    /// <summary>Every eye-door component's programs, both facings, equal their native lists.</summary>
    internal static void VerifyEyeDoorPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int steps = 0;
        HashSet<EyeDoorPlmProgram> covered = [];
        foreach (EyeDoorOrientation orientation in Enum.GetValues<EyeDoorOrientation>())
        foreach (EyeDoorComponent component in Enum.GetValues<EyeDoorComponent>())
        {
            var entries = DoorPlmNativeListAddresses.EyeDoorEntries(orientation, component);
            covered.UnionWith(entries.Select(entry => entry.Program));
            steps += CompareProgramSubset(cartridge, EyeDoorPlmProgramDefinitions.ProgramsFor(orientation), entries,
                (EyeDoorPlmProgram _, EyeDoorPlmFrame frame) => EyeDoorPlmDrawDefinitions.DrawList(orientation, component, frame),
                (IImportCartridgeSource source, ushort cursor, out EyeDoorPlmCue cue, out int length) =>
                    EyeDoorCueAt(source, cursor, orientation, entries, out cue, out length),
                $"{orientation} eye door {component}");
        }
        AssertEqual(Enum.GetValues<EyeDoorPlmProgram>().Length, covered.Count, "every typed eye-door program has a native entry");
        Console.WriteLine($"  Eye door programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The eye door's branch, proximity, link, wake, projectile, smoke, hit, conversion and handoff instructions.</summary>
    private static bool EyeDoorCueAt(IImportCartridgeSource cartridge, ushort cursor, EyeDoorOrientation orientation,
        IReadOnlyList<(EyeDoorPlmProgram Program, ushort Entry)> entries, out EyeDoorPlmCue cue, out int length)
    {
        ushort Word(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        EyeDoorPlmProgram ProgramAt(ushort entry) => entries.Single(e => e.Entry == entry).Program;
        string at = $"{orientation} eye door at $84:{cursor:X4}";
        switch (InstructionAt<RoomPlmInstruction>(cartridge, cursor))
        {
            case RoomPlmInstruction.GotoIfDoorBitSet:
                (cue, length) = (ProgramAt(Word(2)) switch
                {
                    EyeDoorPlmProgram.EyeOpened => EyeDoorPlmCue.ConvertIfOpened,
                    EyeDoorPlmProgram.ComponentDelete => EyeDoorPlmCue.DeleteIfOpened,
                    var target => throw new InvalidDataException($"{at}: door-bit branch to {target}."),
                }, 4);
                return true;
            case RoomPlmInstruction.GotoIfSamusNear:
                var near = new EyeDoorProximity(new(ReadOperandByte(cartridge, cursor + 2), ReadOperandByte(cartridge, cursor + 3)), ProgramAt(Word(4)));
                (cue, length) = (Enum.GetValues<EyeDoorPlmCue>().Single(candidate => candidate is EyeDoorPlmCue.ActivateIfSamusNear
                    or EyeDoorPlmCue.CloseIfSamusInFront or EyeDoorPlmCue.WatchIfSamusNear or EyeDoorPlmCue.PulseIfSamusNear
                    or EyeDoorPlmCue.KeepPulsingIfSamusNear && EyeDoorPlmProgramDefinitions.ProximityOf(candidate) == near), 6);
                return true;
            case RoomPlmInstruction.LinkInstruction:
                (cue, length) = (ProgramAt(Word(2)) switch
                {
                    EyeDoorPlmProgram.EyeHit => EyeDoorPlmCue.LinkHit,
                    EyeDoorPlmProgram.ComponentDelete => EyeDoorPlmCue.LinkDelete,
                    var target => throw new InvalidDataException($"{at}: link to {target}."),
                }, 4);
                return true;
            case RoomPlmInstruction.InstallPreInstruction:
                (cue, length) = ((EyeDoorPreInstruction)Word(2) switch
                {
                    EyeDoorPreInstruction.MissileHit => EyeDoorPlmCue.ArmMissileWake,
                    EyeDoorPreInstruction.WakeWhenDoorBitSet => EyeDoorPlmCue.ArmDoorBitWake,
                    var wake => throw new InvalidDataException($"{at}: wake ${(ushort)wake:X4}."),
                }, 4);
                return true;
            case RoomPlmInstruction.ShootEyeDoorProjectile:
                AssertEqual(EyeDoorPlmProgramDefinitions.ShotParameterOf(orientation), Word(2), $"{at}: shot operand");
                (cue, length) = (EyeDoorPlmCue.Shoot, 4);
                return true;
            case RoomPlmInstruction.SpawnEyeDoorSweat:
                AssertEqual(EyeDoorPlmProgramDefinitions.SweatParameterOf(orientation), Word(2), $"{at}: sweat operand");
                (cue, length) = (EyeDoorPlmCue.SpawnSweat, 4);
                return true;
            case RoomPlmInstruction.QueueSoundLibrary2Maximum6:
                AssertEqual(EyeDoorPlmProgramDefinitions.HitSound, ReadOperandByte(cartridge, cursor + 2), $"{at}: hit sound");
                (cue, length) = (EyeDoorPlmCue.QueueHitSound, 3);
                return true;
            case RoomPlmInstruction.SpawnTwoEyeDoorSmoke:
                (cue, length) = (EyeDoorPlmCue.SpawnTwoSmoke, 2);
                return true;
            case RoomPlmInstruction.SpawnEyeDoorSmoke:
                (cue, length) = (EyeDoorPlmCue.SpawnSmoke, 2);
                return true;
            case RoomPlmInstruction.ClearPreInstruction:
                (cue, length) = (EyeDoorPlmCue.ClearPreInstruction, 2);
                return true;
            case RoomPlmInstruction.IncrementDoorHitCounterAndGoto:
                AssertEqual(EyeDoorPlmProgramDefinitions.HitThreshold, ReadOperandByte(cartridge, cursor + 2), $"{at}: hit threshold");
                AssertEqual(EyeDoorPlmProgram.EyeDestroyed, ProgramAt(Word(3)), $"{at}: the third hit destroys the eye");
                (cue, length) = (EyeDoorPlmCue.CountHit, 5);
                return true;
            case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight:
                AssertEqual(EyeDoorOrientation.Right, orientation, $"{at}: blue conversion facing");
                (cue, length) = (EyeDoorPlmCue.MakeBlue, 2);
                return true;
            case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft:
                AssertEqual(EyeDoorOrientation.Left, orientation, $"{at}: blue conversion facing");
                (cue, length) = (EyeDoorPlmCue.MakeBlue, 2);
                return true;
            case RoomPlmInstruction.Goto:
                ColoredDoorOrientation facing = orientation == EyeDoorOrientation.Right ? ColoredDoorOrientation.Right : ColoredDoorOrientation.Left;
                AssertEqual(DoorPlmNativeListAddresses.BlueEntries.Single(e => e.Program == BlueDoorPlmProgramDefinitions.Closed(facing)).Entry,
                    Word(2), $"{at}: the eye becomes its blue cap");
                (cue, length) = (EyeDoorPlmCue.BecomeBlueDoor, 4);
                return true;
            default:
                (cue, length) = (default, 0);
                return false;
        }
    }

    /// <summary>Each grey cap's step addresses, walked once.</summary>
    private static readonly Dictionary<GreyDoorVariant, IReadOnlyDictionary<(GreyDoorProgram Program, int Position), ushort>> greyDoorAddresses = [];

    /// <summary>
    /// The native instruction pointer a grey cap's typed program stands for: the native instruction its cursor rests on,
    /// for comparisons with native movie snapshots.
    /// </summary>
    internal static ushort GreyDoorNativeInstruction(ISnesAddressSpace rom, GreyDoorPlmAnimation door) =>
        GreyDoorAddresses(rom, door.Variant)[(door.Cursor.Program, door.Cursor.Position)];

    /// <summary>The native list a grey cap's typed program begins at.</summary>
    internal static ushort GreyDoorNativeEntry(GreyDoorVariant variant, GreyDoorProgram program) =>
        DoorPlmNativeListAddresses.GreyEntries(variant).Single(entry => entry.Program == program).Entry;

    /// <summary>Grey cap <paramref name="variant"/>'s step addresses.</summary>
    private static IReadOnlyDictionary<(GreyDoorProgram Program, int Position), ushort> GreyDoorAddresses(ISnesAddressSpace rom, GreyDoorVariant variant)
    {
        if (!greyDoorAddresses.TryGetValue(variant, out var addresses))
        {
            addresses = StepAddresses(CartridgeImportSource.Require(rom), GreyDoorPlmProgramDefinitions.ProgramsFor(variant),
                DoorPlmNativeListAddresses.GreyEntries(variant), frame => GreyCapDraw(variant.Orientation, frame),
                (IImportCartridgeSource source, ushort cursor, out GreyDoorCue cue, out int length) => GreyDoorCueAt(source, cursor, variant, out cue, out length),
                $"grey {variant}");
            greyDoorAddresses[variant] = addresses;
        }
        return addresses;
    }

    /// <summary>The Brinstar plants' programs equal their native lists.</summary>
    internal static void VerifySamusEaterPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), SamusEaterPlmProgramDefinitions.Programs, SamusEaterEntries,
            draw => (ushort)draw, SamusEaterCueAt, "Brinstar plant");
        Console.WriteLine($"  Brinstar plant programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The plants' hold, chew-sound, damage, release and clear instructions.</summary>
    private static bool SamusEaterCueAt(IImportCartridgeSource cartridge, ushort cursor, out SamusEaterCue cue, out int length)
    {
        (SamusEaterCue Cue, int Length)? decoded = InstructionAt<RoomPlmInstruction>(cartridge, cursor) switch
        {
            RoomPlmInstruction.InstallPreInstruction => (SamusEaterCue.HoldSamus, 4),
            RoomPlmInstruction.QueueSoundLibrary2Maximum6 => (SamusEaterCue.QueueChewSound, 3),
            RoomPlmInstruction.SamusEaterDamage => (SamusEaterCue.DamageSamus, 2),
            RoomPlmInstruction.SamusEaterReleaseImmunity => (SamusEaterCue.ReleaseSamus, 2),
            RoomPlmInstruction.ClearPreInstruction => (SamusEaterCue.ClearPreInstruction, 2),
            _ => null,
        };
        (cue, length) = decoded ?? default;
        if (decoded is null)
            return false;
        if (cue == SamusEaterCue.HoldSamus)
            AssertEqual(SamusEaterPlmRomData.HoldPreInstruction, cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), "plant hold pre-instruction");
        if (cue == SamusEaterCue.QueueChewSound)
        {
            AssertEqual(SamusEaterPlmRomData.ChewSound, ReadOperandByte(cartridge, cursor + 2), "plant chew sound");
            AssertEqual((byte)6, SamusEaterPlmProgramDefinitions.ChewSoundMaximumQueued, "plant chew sound queue cap");
        }
        return true;
    }

    /// <summary>The n00b tube's native instructions, with their event, link, pre-instruction and sound operands.</summary>
    private static bool NoobTubeCueAt(IImportCartridgeSource cartridge, ushort cursor, out NoobTubeCue cue, out int length)
    {
        ushort Operand(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        NoobTubeProgram Entry(ushort address) => NoobTubeEntries.Single(entry => entry.Entry == address).Program;
        RoomPlmInstruction instruction = InstructionAt<RoomPlmInstruction>(cartridge, cursor);
        (NoobTubeCue Cue, int Length)? decoded = instruction switch
        {
            RoomPlmInstruction.GotoIfEventSet => (NoobTubeCue.SkipIfBroken, 6),
            RoomPlmInstruction.LinkInstruction => (Entry(Operand(2)) == NoobTubeProgram.WaitForInput ? NoobTubeCue.LinkInputWait : NoobTubeCue.LinkBreak, 4),
            RoomPlmInstruction.InstallPreInstruction =>
                ((NoobTubePlmPreInstruction)Operand(2) == NoobTubePlmPreInstruction.WakeOnPowerBomb ? NoobTubeCue.ArmPowerBombWake : NoobTubeCue.ArmInputWake, 4),
            RoomPlmInstruction.ClearPreInstruction => (NoobTubeCue.ClearPreInstruction, 2),
            RoomPlmInstruction.LockSamus => (NoobTubeCue.LockSamus, 2),
            RoomPlmInstruction.SpawnNoobTubeCrack => (NoobTubeCue.SpawnCrack, 2),
            RoomPlmInstruction.QueueSoundLibrary2Maximum6 => (NoobTubeCue.QueueBreakSound, 3),
            RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles => (NoobTubeCue.SpawnShardsAndBubbles, 2),
            RoomPlmInstruction.TriggerNoobTubeEarthquake => (NoobTubeCue.TriggerEarthquake, 2),
            RoomPlmInstruction.SetEvent => (NoobTubeCue.SetBrokenEvent, 4),
            RoomPlmInstruction.EnableNoobTubeWaterPhysics => (NoobTubeCue.EnableWaterPhysics, 2),
            RoomPlmInstruction.UnlockSamus => (NoobTubeCue.UnlockSamus, 2),
            _ => null,
        };
        (cue, length) = decoded ?? default;
        if (decoded is null)
            return false;
        switch (cue)
        {
            case NoobTubeCue.SkipIfBroken:
                AssertEqual((ushort)NoobTubePlmRomData.BrokenEvent, Operand(2), "n00b tube skip event");
                AssertEqual(NoobTubeProgram.AlreadyBroken, Entry(Operand(4)), "n00b tube skip target");
                break;
            case NoobTubeCue.LinkBreak:
                AssertEqual(NoobTubeProgram.Break, Entry(Operand(2)), "n00b tube break link");
                break;
            case NoobTubeCue.ArmInputWake:
                AssertEqual((ushort)NoobTubePlmPreInstruction.WakeOnAcceptedInput, Operand(2), "n00b tube input wake");
                break;
            case NoobTubeCue.QueueBreakSound:
                AssertEqual(NoobTubePlmRomData.BreakSound, ReadOperandByte(cartridge, cursor + 2), "n00b tube break sound");
                AssertEqual((byte)6, NoobTubePlmProgramDefinitions.BreakSoundMaximumQueued, "n00b tube break sound queue cap");
                break;
            case NoobTubeCue.SetBrokenEvent:
                AssertEqual((ushort)NoobTubePlmRomData.BrokenEvent, Operand(2), "n00b tube broken event");
                break;
        }
        return true;
    }
}
