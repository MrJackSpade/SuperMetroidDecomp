using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of the palette-FX objects' typed programs (#1298): each program matched step by step to its native
/// bank-$8D list, and the typed cursor a recording's native instruction pointer stands for. The one native instruction
/// the programs deliberately do not reproduce is the set-pre-instruction of the deleted adjacent-slot inspection (#1526),
/// a presentation quirk: the comparison steps over exactly that instruction, wherever a list runs or branches into it, and
/// fails on any other native instruction the typed steps do not match.
/// </summary>
internal static class PaletteFxNativeProgramOracle
{
    /// <summary>Compares every program with its native list and returns each typed step's native address.</summary>
    internal static Dictionary<PaletteFxProgram, ushort[]> StepAddresses(IImportCartridgeSource cartridge)
    {
        Dictionary<ushort, PaletteFxProgram> byEntry = PaletteFxNativeListAddresses.Entries.ToDictionary(entry => entry.Entry, entry => entry.Program);
        return PaletteFxNativeListAddresses.Entries.ToDictionary(entry => entry.Program, entry => Compare(cartridge, byEntry, entry.Program, entry.Entry));
    }

    /// <summary>
    /// The typed cursor a recording's native instruction pointer <paramref name="nativePointer"/> stands for: a program
    /// entry where it is one, otherwise the single typed step at that address.
    /// </summary>
    internal static AnimationProgramCursor<PaletteFxProgram> CursorAt(IImportCartridgeSource cartridge, ushort nativePointer)
    {
        if (PaletteFxNativeListAddresses.Entries.Where(entry => entry.Entry == nativePointer).Select(entry => (PaletteFxProgram?)entry.Program).SingleOrDefault() is { } program)
            return AnimationProgramCursor<PaletteFxProgram>.Start(program);
        var matches = StepAddresses(cartridge)
            .SelectMany(item => item.Value.Select((address, position) => new PaletteFxProgramsMatch(item.Key, address, position)))
            .Where(item => item.Address == nativePointer)
            .ToList();
        return matches.Count == 1
            ? new AnimationProgramCursor<PaletteFxProgram>(matches[0].Key, matches[0].Position)
            : throw new InvalidDataException($"Native palette-FX pointer $8D:{nativePointer:X4} matches {matches.Count} typed steps.");
    }

    /// <summary>Compares one program with its native list.</summary>
    private static ushort[] Compare(IImportCartridgeSource cartridge, Dictionary<ushort, PaletteFxProgram> byEntry, PaletteFxProgram program, ushort entry)
    {
        var steps = PaletteFxAnimations.Programs.Steps(program);
        var addresses = new ushort[steps.Count];
        ushort cursor = entry;
        for (int index = 0; index < steps.Count; index++)
        {
            cursor = PastDeletedInspection(cursor);
            string at = $"palette-FX {program} step {index} at $8D:{cursor:X4}";
            addresses[index] = cursor;
            var step = steps[index];
            if (cursor != entry && byEntry.TryGetValue(cursor, out PaletteFxProgram next))
            {
                var fallInto = VerificationAssertions.AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.FallIntoStep>(step, $"{at}: runs on into the next authored list");
                VerificationAssertions.AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                VerificationAssertions.AssertEqual(steps.Count, index + 1, $"{at}: the fall-through ends the program");
                return addresses;
            }
            ushort word = Word(cursor);
            if (word < PaletteFxListWords.CommandWord)
            {
                var show = VerificationAssertions.AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.ShowStep>(step, $"{at}: timed record");
                VerificationAssertions.AssertEqual(word, show.Duration, $"{at}: duration");
                VerificationAssertions.AssertEqual(cursor, PaletteFxColorLayoutAccess.RecordAddress(show.Frame), $"{at}: record address");
                cursor = CompareLayout(cursor, PaletteFxRecordDefinitions.Layout(show.Frame), at);
                continue;
            }
            PaletteFxInstruction instruction = (PaletteFxInstruction)word;
            ushort operandWord = Word(cursor + 2);
            byte operandByte = LittleEndianBytes.LowByte(operandWord);
            switch (instruction)
            {
                case PaletteFxInstruction.Goto:
                    var goTo = VerificationAssertions.AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.GoToStep>(step, $"{at}: goto");
                    VerificationAssertions.AssertEqual(byEntry[PastDeletedInspection(operandWord)], goTo.Target, $"{at}: goto target");
                    VerificationAssertions.AssertEqual(steps.Count, index + 1, $"{at}: the goto ends the program");
                    return addresses;
                case PaletteFxInstruction.Delete:
                    VerificationAssertions.AssertTrue(step is AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.StopStep, $"{at}: delete");
                    VerificationAssertions.AssertEqual(steps.Count, index + 1, $"{at}: the delete ends the program");
                    return addresses;
                case PaletteFxInstruction.DecrementTimerAndGoto:
                    var repeat = VerificationAssertions.AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.RepeatWhileCountingStep>(step, $"{at}: counted repeat");
                    VerificationAssertions.AssertEqual(byEntry[PastDeletedInspection(operandWord)], repeat.Target, $"{at}: repeat target");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.SetTimer:
                    // The native command writes only the timer's low byte; its high byte is zero from spawn and every
                    // counted loop runs it down to zero, so the typed counter set is the same.
                    var setCounter = VerificationAssertions.AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.SetCounterStep>(step, $"{at}: timer");
                    VerificationAssertions.AssertEqual(operandByte, setCounter.Count, $"{at}: timer count");
                    cursor = unchecked((ushort)(cursor + 3));
                    continue;
                case PaletteFxInstruction.SetColorIndex:
                    // The native operand is a CGRAM byte index; the typed cue names the colour group, which must be
                    // placed where the operand starts.
                    VerificationAssertions.AssertTrue(operandWord % sizeof(ushort) == 0, $"{at}: colour byte index is word-aligned");
                    var colourCue = VerificationAssertions.AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep>(step, $"{at}: command");
                    var setColours = VerificationAssertions.AssertIs<PaletteFxCue.SetColours>(colourCue.Cue, $"{at}: colours cue");
                    VerificationAssertions.AssertEqual(operandWord / sizeof(ushort), setColours.Target.CartridgeColour,
                        $"{at}: {setColours.Target} starts at the native colour");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.SetFrameBehavior:
                    AssertCue(step, new PaletteFxCue.SetFrameBehavior(FrameBehavior: FxNativeWords.PaletteFxFrameBehaviors.Decode(operandWord, "palette-FX pre-instruction")), at);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.ClearFrameBehavior:
                    AssertCue(step, new PaletteFxCue.Command(PaletteFxCueKind.ClearFrameBehavior), at);
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case PaletteFxInstruction.SetPaletteFxIndex:
                    AssertCue(step, new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: operandByte), at);
                    cursor = unchecked((ushort)(cursor + 3));
                    continue;
                case PaletteFxInstruction.QueueSfx2:
                    AssertCue(step, new PaletteFxCue.QueueSoundLibrary2(Sound: SoundEffectCommands.Decode(SoundEffectLibrary.Library2, operandByte)), at);
                    cursor = unchecked((ushort)(cursor + 3));
                    continue;
                case PaletteFxInstruction.Wait:
                case PaletteFxInstruction.ColorPlus2:
                case PaletteFxInstruction.ColorPlus3:
                case PaletteFxInstruction.ColorPlus4:
                case PaletteFxInstruction.ColorPlus8:
                case PaletteFxInstruction.ColorPlus9:
                case PaletteFxInstruction.ColorPlus15:
                case PaletteFxInstruction.QueueMusic:
                case PaletteFxInstruction.QueueSfx1:
                case PaletteFxInstruction.QueueSfx3:
                default:
                    throw new InvalidDataException($"{at}: native command {instruction} has no typed form.");
            }
        }
        throw new InvalidDataException($"palette-FX {program} ran past its typed steps at $8D:{cursor:X4}.");

        ushort Word(int address) => cartridge.ReadCartridgeWord(PaletteFxListWords.ListBank | address);

        // The deleted adjacent-slot inspection (#1526) is a presentation quirk the port does not reproduce: where the native
        // list sets that one pre-instruction, the typed program runs straight on to what follows it.
        ushort PastDeletedInspection(ushort address) =>
            Word(address) == (ushort)PaletteFxInstruction.SetFrameBehavior &&
            Word(address + 2) == FxNativeWords.DeletedSlotInspectionPreInstruction
                ? unchecked((ushort)(address + 4))
                : address;

        // A record is its timer, its colour words and skip commands, then the closing wait.
        ushort CompareLayout(ushort record, IReadOnlyList<PaletteFxRecordPart> layout, string at)
        {
            ushort word = unchecked((ushort)(record + 2));
            foreach (PaletteFxRecordPart part in layout)
            {
                if (part.Skip)
                {
                    ushort skip = Word(word);
                    int skipped = (PaletteFxInstruction)skip switch
                    {
                        PaletteFxInstruction.ColorPlus2 => 2,
                        PaletteFxInstruction.ColorPlus3 => 3,
                        PaletteFxInstruction.ColorPlus4 => 4,
                        PaletteFxInstruction.ColorPlus8 => 8,
                        PaletteFxInstruction.ColorPlus9 => 9,
                        PaletteFxInstruction.ColorPlus15 => 15,
                        PaletteFxInstruction.Wait or
                            PaletteFxInstruction.Delete or
                            PaletteFxInstruction.SetFrameBehavior or
                            PaletteFxInstruction.ClearFrameBehavior or
                            PaletteFxInstruction.Goto or
                            PaletteFxInstruction.DecrementTimerAndGoto or
                            PaletteFxInstruction.SetTimer or
                            PaletteFxInstruction.SetColorIndex or
                            PaletteFxInstruction.QueueMusic or
                            PaletteFxInstruction.QueueSfx1 or
                            PaletteFxInstruction.QueueSfx2 or
                            PaletteFxInstruction.QueueSfx3 or
                            PaletteFxInstruction.SetPaletteFxIndex => throw new InvalidDataException($"{at}: $8D:{word:X4} ${skip:X4} is not a colour skip."),
                        _ => throw new InvalidDataException($"{at}: $8D:{word:X4} ${skip:X4} is not a colour skip."),
                    };
                    VerificationAssertions.AssertEqual(skipped, part.Count, $"{at}: skip at $8D:{word:X4}");
                    word = unchecked((ushort)(word + 2));
                    continue;
                }
                for (int color = 0; color < part.Count; color++)
                {
                    VerificationAssertions.AssertTrue(Word(word) < PaletteFxListWords.CommandWord, $"{at}: colour word at $8D:{word:X4}");
                    word = unchecked((ushort)(word + 2));
                }
            }
            VerificationAssertions.AssertEqual((ushort)PaletteFxInstruction.Wait, Word(word), $"{at}: the record closes with a wait");
            return unchecked((ushort)(word + 2));
        }
    }

    /// <summary>Checks that <paramref name="step"/> publishes <paramref name="expected"/>.</summary>
    private static void AssertCue(AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> step, PaletteFxCue expected, string at)
    {
        var cueStep = VerificationAssertions.AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep>(step, $"{at}: command");
        VerificationAssertions.AssertEqual(expected, cueStep.Cue, $"{at}: cue");
    }
}
