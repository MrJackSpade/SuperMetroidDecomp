using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>
/// #1298: the palette-FX objects' typed programs, each matched step by step to its native bank-$8D list: every timed
/// record's duration, address and layout, every command and its operand, and every branch.
/// </summary>
internal static class PaletteFxProgramsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyPaletteFxPrograms(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<PaletteFxProgram>().Length, PaletteFxNativeListAddresses.Entries.Length, "every typed palette-FX program has a native entry");
        Dictionary<PaletteFxProgram, ushort[]> steps = StepAddresses(cartridge);
        HashSet<int> records = [];
        foreach (PaletteFxProgram program in Enum.GetValues<PaletteFxProgram>())
            foreach (var step in PaletteFxAnimations.Programs.Steps(program))
                if (step is AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.ShowStep show)
                    records.Add(show.Frame.Record);
        AssertEqual(PaletteFxRecordDefinitionsConstants.RecordCount, records.Count, "every colour record is shown by a program");
        Console.WriteLine($"  Palette-FX programs: {steps.Count} programs ({steps.Values.Sum(s => s.Length)} steps, {records.Count} colour records) match bank $8D.");
    }

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
            string at = $"palette-FX {program} step {index} at $8D:{cursor:X4}";
            addresses[index] = cursor;
            var step = steps[index];
            if (cursor != entry && byEntry.TryGetValue(cursor, out PaletteFxProgram next))
            {
                var fallInto = AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.FallIntoStep>(step, $"{at}: runs on into the next authored list");
                AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                AssertEqual(steps.Count, index + 1, $"{at}: the fall-through ends the program");
                return addresses;
            }
            ushort word = Word(cursor);
            if (word < PaletteFxListWords.CommandWord)
            {
                var show = AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.ShowStep>(step, $"{at}: timed record");
                AssertEqual(word, show.Duration, $"{at}: duration");
                AssertEqual(cursor, PaletteFxColorLayoutAccess.RecordAddress(show.Frame), $"{at}: record address");
                cursor = CompareLayout(cursor, PaletteFxRecordDefinitions.Layout(show.Frame), at);
                continue;
            }
            PaletteFxInstruction instruction = (PaletteFxInstruction)word;
            ushort operandWord = Word(cursor + 2);
            byte operandByte = SnesLittleEndian.LowByte(operandWord);
            switch (instruction)
            {
                case PaletteFxInstruction.Goto:
                    var goTo = AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.GoToStep>(step, $"{at}: goto");
                    AssertEqual(byEntry[operandWord], goTo.Target, $"{at}: goto target");
                    AssertEqual(steps.Count, index + 1, $"{at}: the goto ends the program");
                    return addresses;
                case PaletteFxInstruction.Delete:
                    AssertTrue(step is AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.StopStep, $"{at}: delete");
                    AssertEqual(steps.Count, index + 1, $"{at}: the delete ends the program");
                    return addresses;
                case PaletteFxInstruction.DecrementTimerAndGoto:
                    var repeat = AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.RepeatWhileCountingStep>(step, $"{at}: counted repeat");
                    AssertEqual(byEntry[operandWord], repeat.Target, $"{at}: repeat target");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.SetTimer:
                    // The native command writes only the timer's low byte; its high byte is zero from spawn and every
                    // counted loop runs it down to zero, so the typed counter set is the same.
                    var setCounter = AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.SetCounterStep>(step, $"{at}: timer");
                    AssertEqual(operandByte, setCounter.Count, $"{at}: timer count");
                    cursor = unchecked((ushort)(cursor + 3));
                    continue;
                case PaletteFxInstruction.SetColorIndex:
                    // The native operand is a CGRAM byte index; the typed cue names the colour it starts at.
                    AssertTrue(operandWord % sizeof(ushort) == 0, $"{at}: colour byte index is word-aligned");
                    AssertCue(step, new PaletteFxCue.SetColours(PaletteFxColoursPositions.FromCartridge(operandWord / sizeof(ushort))), at);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.SetPreInstruction:
                    AssertCue(step, new PaletteFxCue.SetPreInstruction(PreInstruction: FxNativeWords.PaletteFxPreInstructions.Decode(operandWord, "palette-FX pre-instruction")), at);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case PaletteFxInstruction.ClearPreInstruction:
                    AssertCue(step, new PaletteFxCue.Command(PaletteFxCueKind.ClearPreInstruction), at);
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
                            PaletteFxInstruction.SetPreInstruction or
                            PaletteFxInstruction.ClearPreInstruction or
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
                    AssertEqual(skipped, part.Count, $"{at}: skip at $8D:{word:X4}");
                    word = unchecked((ushort)(word + 2));
                    continue;
                }
                for (int color = 0; color < part.Count; color++)
                {
                    AssertTrue(Word(word) < PaletteFxListWords.CommandWord, $"{at}: colour word at $8D:{word:X4}");
                    word = unchecked((ushort)(word + 2));
                }
            }
            AssertEqual((ushort)PaletteFxInstruction.Wait, Word(word), $"{at}: the record closes with a wait");
            return unchecked((ushort)(word + 2));
        }
    }

    /// <summary>Checks that <paramref name="step"/> publishes <paramref name="expected"/>.</summary>
    private static void AssertCue(AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> step, PaletteFxCue expected, string at)
    {
        var cueStep = AssertIs<AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep>(step, $"{at}: command");
        AssertEqual(expected, cueStep.Cue, $"{at}: cue");
    }

    /// <summary>Runs the palette-FX program oracle.</summary>
    [VerificationCommand("--palette-fx-programs")]
    internal static int PaletteFxProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPaletteFxPrograms), () => VerifyPaletteFxPrograms(rom));
        return 0;
    }
}
