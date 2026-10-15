using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;

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
    internal delegate bool CueReader<TCue>(IImportCartridgeSource cartridge, ushort cursor, out TCue cue, out int length);

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
        IReadOnlyList<(TProgram Program, ushort Entry)> entries,
        Func<TFrame, ushort> drawList,
        CueReader<TCue> cueAt,
        string family,
        ShowReader<TFrame>? showAt = null)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        AssertEqual(Enum.GetValues<TProgram>().Length, entries.Count, $"every typed {family} program has a native entry");
        var oracle = new ProgramSetOracle<TProgram, TFrame, TCue>(cartridge, programs,
            entries.ToDictionary(entry => entry.Entry, entry => entry.Program), drawList, cueAt, family, showAt);
        return entries.Sum(entry => oracle.Compare(entry.Program, entry.Entry));
    }

    /// <summary>
    /// The native instruction address of every typed step, from the same walk <see cref="CompareProgramSet"/> makes: a
    /// typed cursor's step stands where the native instruction pointer would, for comparisons with native traces.
    /// </summary>
    internal static IReadOnlyDictionary<(TProgram Program, int Position), ushort> StepAddresses<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        IReadOnlyList<(TProgram Program, ushort Entry)> entries,
        Func<TFrame, ushort> drawList,
        CueReader<TCue> cueAt,
        string family,
        ShowReader<TFrame>? showAt = null)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        var oracle = new ProgramSetOracle<TProgram, TFrame, TCue>(cartridge, programs,
            entries.ToDictionary(entry => entry.Entry, entry => entry.Program), drawList, cueAt, family, showAt);
        foreach ((TProgram program, ushort entry) in entries)
            oracle.Compare(program, entry);
        return oracle.Addresses;
    }

    /// <summary>One family's typed programs beside its native lists.</summary>
    private sealed class ProgramSetOracle<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        Dictionary<ushort, TProgram> byEntry,
        Func<TFrame, ushort> drawList,
        CueReader<TCue> cueAt,
        string family,
        ShowReader<TFrame>? showAt)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        /// <summary>The native address of each typed step the walks have reached.</summary>
        internal Dictionary<(TProgram Program, int Position), ushort> Addresses { get; } = [];

        /// <summary>Compares one typed program with its native list and returns its typed step count.</summary>
        internal int Compare(TProgram program, ushort entry)
        {
            IReadOnlyList<AnimationProgramStep<TProgram, TFrame, TCue>> steps = programs.Steps(program);
            ushort cursor = entry;
            int index = 0;
            for (int command = 0; command < 512; command++)
            {
                Addresses[(program, index)] = cursor;
                string at = $"{family} {program} step {index} at $84:{cursor:X4}";
                AssertTrue(index < steps.Count, $"{at}: the typed program ends before the native list");
                AnimationProgramStep<TProgram, TFrame, TCue> step = steps[index];
                if (cursor != entry && byEntry.TryGetValue(cursor, out TProgram next))
                {
                    AssertEqual(AnimationProgramStepKind.FallInto, step.Kind, $"{at}: runs on into the next authored list");
                    AssertEqual(next, step.Target, $"{at}: fall-through target");
                    return Ends(index);
                }
                ushort word = Word(cursor);
                if ((word & InstructionBit) == 0)
                {
                    AssertEqual(AnimationProgramStepKind.Show, step.Kind, $"{at}: timed draw");
                    AssertEqual((int)word, step.Duration, $"{at}: duration");
                    AssertEqual(Word(cursor + 2), drawList(step.Frame), $"{at}: draw list");
                    cursor = unchecked((ushort)(cursor + 4));
                    index++;
                    continue;
                }
                switch ((RoomPlmInstruction)word)
                {
                    case RoomPlmInstruction.Sleep:
                        AssertEqual(AnimationProgramStepKind.Hold, step.Kind, $"{at}: sleep");
                        return Ends(index);
                    case RoomPlmInstruction.Delete:
                        AssertEqual(AnimationProgramStepKind.Stop, step.Kind, $"{at}: delete");
                        return Ends(index);
                    case RoomPlmInstruction.Goto:
                        AssertEqual(AnimationProgramStepKind.GoTo, step.Kind, $"{at}: goto");
                        AssertEqual(Target(cursor + 2), step.Target, $"{at}: goto target");
                        return Ends(index);
                    case RoomPlmInstruction.SetEightBitTimer:
                        AssertEqual(AnimationProgramStepKind.SetCounter, step.Kind, $"{at}: loop timer");
                        AssertEqual((int)ReadOperandByte(cartridge, cursor + 2), step.Duration, $"{at}: loop count");
                        cursor = unchecked((ushort)(cursor + 3));
                        index++;
                        continue;
                    case RoomPlmInstruction.DecrementTimerAndGoto:
                        AssertEqual(AnimationProgramStepKind.RepeatWhileCounting, step.Kind, $"{at}: counted repeat");
                        AssertEqual(Target(cursor + 2), step.Target, $"{at}: repeat target");
                        cursor = unchecked((ushort)(cursor + 4));
                        index++;
                        continue;
                    default:
                        if (showAt is not null && showAt(cartridge, cursor, out TFrame shown, out int shownFor, out int showLength))
                        {
                            AssertEqual(AnimationProgramStepKind.Show, step.Kind, $"{at}: instruction ${word:X4} draws");
                            AssertEqual(shown, step.Frame, $"{at}: drawn frame");
                            AssertEqual(shownFor, step.Duration, $"{at}: draw duration");
                            cursor = unchecked((ushort)(cursor + showLength));
                            index++;
                            continue;
                        }
                        AssertTrue(cueAt(cartridge, cursor, out TCue cue, out int length), $"{at}: native instruction ${word:X4} is known");
                        AssertEqual(AnimationProgramStepKind.Cue, step.Kind, $"{at}: instruction ${word:X4}");
                        AssertEqual(cue, step.PublishedCue, $"{at}: cue");
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
