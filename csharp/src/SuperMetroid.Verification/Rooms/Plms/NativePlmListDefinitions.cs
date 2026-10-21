using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Enemies;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// A structural reader of native bank-$84 PLM instruction lists for the #1296 ROM oracles: it walks a list word by
/// word beside the typed program that replaced it. Timed draws, sleeps, deletions, gotos, the loop timer and
/// fall-through into the next authored list are compared here; each family names what its other instructions mean.
/// Only verification reads cartridge lists; the port runs typed programs.
/// </summary>
internal static class NativePlmListDefinitions
{
    /// <summary>The PLM program bank, as a long-address base.</summary>
    internal const int ProgramBank = 0x840000;
    /// <summary>Bit 15 marks an instruction word; a clear bit makes the word a draw's duration.</summary>
    private const ushort InstructionBit = 0x8000;

    /// <summary>
    /// Decodes a family instruction at <paramref name="cursor"/>: the typed cue it became and its native length in
    /// bytes, or false when the family has no such instruction.
    /// </summary>
    internal delegate bool CueReader<TCue>(IImportCartridgeSource cartridge, ushort cursor, [MaybeNullWhen(false)] out TCue cue, out int length);

    /// <summary>
    /// Decodes an instruction at <paramref name="cursor"/> that draws and ends the pass, such as the restore of a
    /// respawning block: the typed frame it became, its duration and its native length, or false for any other word.
    /// </summary>
    internal delegate bool ShowReader<TFrame>(IImportCartridgeSource cartridge, ushort cursor, out TFrame frame, out int duration, out int length);

    /// <summary>The cue reader of families whose lists hold only draws, loops and deletions.</summary>
    internal static CueReader<TCue> NoCues<TCue>() where TCue : struct =>
        (_, _, out cue, out length) =>
        {
            cue = default;
            length = 0;
            return false;
        };

    /// <summary>The native instruction word at <paramref name="cursor"/>, as the routine catalog <typeparamref name="TRoutine"/>.</summary>
    internal static TRoutine InstructionAt<TRoutine>(IImportCartridgeSource cartridge, int cursor) where TRoutine : struct, Enum =>
        (TRoutine)Enum.ToObject(typeof(TRoutine), cartridge.ReadCartridgeWord(ProgramBank | cursor));

    /// <summary>The byte operand at <paramref name="address"/>: the low byte of the word read there.</summary>
    internal static byte ReadOperandByte(IImportCartridgeSource cartridge, int address) =>
        unchecked((byte)cartridge.ReadCartridgeWord(ProgramBank | address));

    /// <summary>
    /// Compares every program of <paramref name="programs"/> with its native list at its entry; returns the typed
    /// steps compared.
    /// </summary>
    internal static int CompareProgramSet<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        IReadOnlyList<NativeProgramEntry<TProgram>> entries,
        Func<TFrame, PlmDraw> drawList,
        CueReader<TCue> cueAt,
        string family,
        ShowReader<TFrame>? showAt = null)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull =>
        CompareProgramSet(cartridge, programs, entries, (TProgram _, TFrame frame) => drawList(frame), cueAt, family, showAt);

    /// <summary>
    /// <see cref="CompareProgramSet{TProgram, TFrame, TCue}(IImportCartridgeSource, AnimationProgramSet{TProgram, TFrame, TCue}, IReadOnlyList{NativeProgramEntry{TProgram}}, Func{TFrame, PlmDraw}, CueReader{TCue}, string, ShowReader{TFrame})"/>
    /// for a family whose draw lists depend on the program a frame is shown in.
    /// </summary>
    internal static int CompareProgramSet<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        IReadOnlyList<NativeProgramEntry<TProgram>> entries,
        Func<TProgram, TFrame, PlmDraw> drawList,
        CueReader<TCue> cueAt,
        string family,
        ShowReader<TFrame>? showAt = null)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        AssertEqual(Enum.GetValues<TProgram>().Length, entries.Count, $"every typed {family} program has a native entry");
        return CompareProgramSubset(cartridge, programs, entries, drawList, cueAt, family, showAt);
    }

    /// <summary>
    /// Compares the programs of <paramref name="entries"/> with their native lists, for a family whose variants each
    /// run only some of its programs; the caller confirms that the variants together cover every program.
    /// </summary>
    internal static int CompareProgramSubset<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        IReadOnlyList<NativeProgramEntry<TProgram>> entries,
        Func<TProgram, TFrame, PlmDraw> drawList,
        CueReader<TCue> cueAt,
        string family,
        ShowReader<TFrame>? showAt = null)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        var oracle = new ProgramSetOracle<TProgram, TFrame, TCue>(cartridge, programs,
            entries.ToDictionary(entry => entry.Entry, entry => entry.Program), drawList, cueAt, family, showAt);
        return entries.Sum(entry => oracle.Compare(entry.Program, entry.Entry));
    }

    /// <summary>
    /// The native instruction address of every typed step, from the same walk <see cref="CompareProgramSet{TProgram, TFrame, TCue}(IImportCartridgeSource, AnimationProgramSet{TProgram, TFrame, TCue}, IReadOnlyList{NativeProgramEntry{TProgram}}, Func{TFrame, PlmDraw}, CueReader{TCue}, string, ShowReader{TFrame})"/> makes: a
    /// typed cursor's step stands where the native instruction pointer would, for comparisons with native traces.
    /// </summary>
    internal static IReadOnlyDictionary<ProgramStep<TProgram>, ushort> StepAddresses<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        IReadOnlyList<NativeProgramEntry<TProgram>> entries,
        Func<TFrame, PlmDraw> drawList,
        CueReader<TCue> cueAt,
        string family,
        ShowReader<TFrame>? showAt = null)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        var oracle = new ProgramSetOracle<TProgram, TFrame, TCue>(cartridge, programs,
            entries.ToDictionary(entry => entry.Entry, entry => entry.Program), (TProgram _, TFrame frame) => drawList(frame), cueAt, family, showAt);
        foreach ((TProgram program, ushort entry) in entries)
            oracle.Compare(program, entry);
        return oracle.Addresses;
    }

    /// <summary>One family's typed programs beside its native lists.</summary>
    private sealed class ProgramSetOracle<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        Dictionary<ushort, TProgram> byEntry,
        Func<TProgram, TFrame, PlmDraw> drawList,
        CueReader<TCue> cueAt,
        string family,
        ShowReader<TFrame>? showAt)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        /// <summary>The native address of each typed step the walks have reached.</summary>
        internal Dictionary<ProgramStep<TProgram>, ushort> Addresses { get; } = [];

        /// <summary>Compares one typed program with its native list and returns its typed step count.</summary>
        internal int Compare(TProgram program, ushort entry)
        {
            IReadOnlyList<AnimationProgramStep<TProgram, TFrame, TCue>> steps = programs.Steps(program);
            ushort cursor = entry;
            int index = 0;
            for (int command = 0; command < 512; command++)
            {
                Addresses[new ProgramStep<TProgram>(program, index)] = cursor;
                string at = $"{family} {program} step {index} at $84:{cursor:X4}";
                AssertTrue(index < steps.Count, $"{at}: the typed program ends before the native list");
                AnimationProgramStep<TProgram, TFrame, TCue> step = steps[index];
                if (cursor != entry && byEntry.TryGetValue(cursor, out TProgram next))
                {
                    var fallInto = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.FallIntoStep>(step, $"{at}: runs on into the next authored list");
                    AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                    return Ends(index);
                }
                ushort word = Word(cursor);
                if ((word & InstructionBit) == 0)
                {
                    var show = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.ShowStep>(step, $"{at}: timed draw");
                    AssertEqual(word, show.Duration, $"{at}: duration");
                    AssertEqual(Word(cursor + 2), PlmDrawAddressCodec.Of(drawList(program, show.Frame)), $"{at}: draw list");
                    cursor = unchecked((ushort)(cursor + 4));
                    index++;
                    continue;
                }
                switch ((RoomPlmInstruction)word)
                {
                    case RoomPlmInstruction.Sleep:
                        AssertTrue(step is AnimationProgramStep<TProgram, TFrame, TCue>.HoldStep, $"{at}: sleep");
                        return Ends(index);
                    case RoomPlmInstruction.Delete:
                        AssertTrue(step is AnimationProgramStep<TProgram, TFrame, TCue>.StopStep, $"{at}: delete");
                        return Ends(index);
                    case RoomPlmInstruction.Goto when !byEntry.ContainsKey(Word(cursor + 2)):
                        // A goto out of the family hands the PLM to another family's program: the typed program
                        // publishes the handoff, then closes with the hold its set requires and never reaches.
                        if (!cueAt(cartridge, cursor, out TCue? handoff, out _) || handoff is null)
                            throw new InvalidOperationException($"Verification failed: {at}: goto out of the family is a known handoff.");
                        var handoffStep = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.CueStep>(step, $"{at}: handoff");
                        AssertEqual(handoff, handoffStep.Cue, $"{at}: handoff cue");
                        AssertTrue(index + 1 < steps.Count && steps[index + 1] is AnimationProgramStep<TProgram, TFrame, TCue>.HoldStep,
                            $"{at}: the handoff closes with a hold");
                        Addresses[new ProgramStep<TProgram>(program, index + 1)] = cursor;
                        return Ends(index + 1);
                    case RoomPlmInstruction.Goto:
                        var goTo = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.GoToStep>(step, $"{at}: goto");
                        AssertEqual(Target(cursor + 2), goTo.Target, $"{at}: goto target");
                        return Ends(index);
                    case RoomPlmInstruction.SetEightBitTimer:
                        var setCounter = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.SetCounterStep>(step, $"{at}: loop timer");
                        AssertEqual(ReadOperandByte(cartridge, cursor + 2), setCounter.Count, $"{at}: loop count");
                        cursor = unchecked((ushort)(cursor + 3));
                        index++;
                        continue;
                    case RoomPlmInstruction.DecrementTimerAndGoto:
                        var repeat = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.RepeatWhileCountingStep>(step, $"{at}: counted repeat");
                        AssertEqual(Target(cursor + 2), repeat.Target, $"{at}: repeat target");
                        cursor = unchecked((ushort)(cursor + 4));
                        index++;
                        continue;
                    case RoomPlmInstruction.SetGreyDoorPreInstruction:
                    case RoomPlmInstruction.InstallPreInstruction:
                    case RoomPlmInstruction.ClearPreInstruction:
                    case RoomPlmInstruction.LinkInstruction:
                    case RoomPlmInstruction.GotoIfDoorBitSet:
                    case RoomPlmInstruction.IncrementDoorHitCounterAndGoto:
                    case RoomPlmInstruction.IncrementArgumentAndGotoIfGreaterOrEqual:
                    case RoomPlmInstruction.GotoIfSamusNear:
                    case RoomPlmInstruction.CopyFromRamToVram:
                    case RoomPlmInstruction.GotoIfAreaBossBitSet:
                    case RoomPlmInstruction.GotoIfEventSet:
                    case RoomPlmInstruction.SetEvent:
                    case RoomPlmInstruction.SetPlmBtsFromByte:
                    case RoomPlmInstruction.DrawPlmBlock:
                    case RoomPlmInstruction.DrawPlmBlockClone:
                    case RoomPlmInstruction.QueueSoundLibrary2Maximum6:
                    case RoomPlmInstruction.QueueSoundLibrary3Maximum6:
                    case RoomPlmInstruction.ClearDownwardGateTrigger:
                    case RoomPlmInstruction.SpawnDownwardGateProjectile:
                    case RoomPlmInstruction.WakeDownwardGateProjectile:
                    case RoomPlmInstruction.ShootEyeDoorProjectile:
                    case RoomPlmInstruction.SpawnEyeDoorSweat:
                    case RoomPlmInstruction.SpawnTwoEyeDoorSmoke:
                    case RoomPlmInstruction.SpawnEyeDoorSmoke:
                    case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight:
                    case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft:
                    case RoomPlmInstruction.DamageDraygonCannonFacingRight:
                    case RoomPlmInstruction.DamageDraygonCannonFacingLeft:
                    case RoomPlmInstruction.QueueSoundLibrary2Maximum3:
                    case RoomPlmInstruction.QueueSoundLibrary2Maximum1:
                    case RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct:
                    case RoomPlmInstruction.GotoIfSamusHasNoBombs:
                    case RoomPlmInstruction.SetPlmBtsToOne:
                    case RoomPlmInstruction.GotoIfRoomArgumentLess:
                    case RoomPlmInstruction.SpawnFourMotherBrainGlassShards:
                    case RoomPlmInstruction.SpawnTorizoStatueBreaking:
                    case RoomPlmInstruction.QueueSongOneMusicTrack:
                    case RoomPlmInstruction.EnableNoobTubeWaterPhysics:
                    case RoomPlmInstruction.SpawnNoobTubeCrack:
                    case RoomPlmInstruction.TriggerNoobTubeEarthquake:
                    case RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles:
                    case RoomPlmInstruction.LockSamus:
                    case RoomPlmInstruction.UnlockSamus:
                    case RoomPlmInstruction.MoveRightFourBlocks:
                    case RoomPlmInstruction.SetAnimalsEscapedEvent:
                    case RoomPlmInstruction.SamusEaterDamage:
                    case RoomPlmInstruction.SamusEaterReleaseImmunity:
                    case RoomPlmInstruction.MoveTourianAccessDown:
                    case RoomPlmInstruction.TransformSpikesToSlopes:
                    case RoomPlmInstruction.RevertSlopesToSpikes:
                    case RoomPlmInstruction.SetLoweredAcidHeight:
                    default:
                        if (showAt is not null && showAt(cartridge, cursor, out TFrame shown, out int shownFor, out int showLength))
                        {
                            var show = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.ShowStep>(step, $"{at}: instruction ${word:X4} draws");
                            AssertEqual(shown, show.Frame, $"{at}: drawn frame");
                            AssertEqual(shownFor, show.Duration, $"{at}: draw duration");
                            cursor = unchecked((ushort)(cursor + showLength));
                            index++;
                            continue;
                        }
                        if (!cueAt(cartridge, cursor, out TCue? cue, out int length) || cue is null)
                            throw new InvalidOperationException($"Verification failed: {at}: native instruction ${word:X4} is known.");
                        var cueStep = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.CueStep>(step, $"{at}: instruction ${word:X4}");
                        AssertEqual(cue, cueStep.Cue, $"{at}: cue");
                        cursor = unchecked((ushort)(cursor + length));
                        index++;
                        continue;
                }
            }
            throw new InvalidDataException($"{family} {program} ran past its native list.");

            int Ends(int last)
            {
                AssertEqual(steps.Count, last + 1, $"{family} {program}: the native list ends with typed step {last}");
                return steps.Count;
            }
        }

        /// <summary>The native word at a bank-$84 offset.</summary>
        private ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);

        /// <summary>The typed program a native branch operand at <paramref name="address"/> targets.</summary>
        private TProgram Target(int address)
        {
            ushort target = Word(address);
            AssertTrue(byEntry.TryGetValue(target, out TProgram found), $"{family}: native target $84:{target:X4} is a typed program entry");
            return found;
        }
    }
}
