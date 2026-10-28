using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Input;

/// <summary>
/// #1300: the demo-input objects and their typed programs, each matched step by step to bank $91: every object's header,
/// every input record's duration and controller words, every routine, goto, fall-through and delete, and the native
/// address each typed step stands for.
/// </summary>
internal static class DemoInputProgramsVerification
{
    /// <summary>The bank-$91 routines a list may call, and the cue each runs.</summary>
    private static readonly Dictionary<ushort, DemoInputCue> Routines = new()
    {
        [DemoInputListWords.EndIntroBabyDiscoveryRoutine] = DemoInputCue.EndIntroBabyDiscovery,
        [DemoInputListWords.EndIntroMotherBrainFlashbackRoutine] = DemoInputCue.EndIntroMotherBrainFlashback,
    };

    /// <summary>Every object and program matches bank $91.</summary>
    internal static void VerifyDemoInputPrograms(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(DemoInputListWords.ListBank | address);

        AssertEqual(Enum.GetValues<DemoInputProgram>().Length, DemoInputNativeListAddresses.Entries.Length, "every typed demo-input program has a native entry");
        foreach ((ushort header, DemoInputObjectDefinition definition) in DemoInputNativeListAddresses.Objects)
        {
            AssertEqual(DemoInputRomData.Routines.NoOp, Word(header), $"demo object $91:{header:X4} initializer");
            AssertEqual(Word(header + 2), DemoInputNativeListAddresses.PreInstructionAddress(definition.PreInstruction), $"demo object $91:{header:X4} pre-instruction");
            AssertEqual(Word(header + 4), DemoInputNativeListAddresses.Entry(definition.Program), $"demo object $91:{header:X4} list");
        }

        Dictionary<ushort, DemoInputProgram> byEntry = DemoInputNativeListAddresses.Entries.ToDictionary(entry => entry.Entry, entry => entry.Program);
        int records = 0;
        foreach ((DemoInputProgram program, ushort entry) in DemoInputNativeListAddresses.Entries)
        {
            var steps = DemoInputAnimations.Programs.Steps(program);
            ushort cursor = entry;
            for (int index = 0; index < steps.Count; index++)
            {
                string at = $"demo {program} step {index} at $91:{cursor:X4}";
                var step = steps[index];
                AssertEqual(cursor, DemoInputNativeListAddresses.Address(new AnimationProgramCursor<DemoInputProgram>(program, index)), $"{at}: native address");
                if (cursor != entry && byEntry.TryGetValue(cursor, out DemoInputProgram next))
                {
                    var fallInto = AssertIs<AnimationProgramStep<DemoInputProgram, DemoInputFrame, DemoInputCue>.FallIntoStep>(step, $"{at}: runs on into the next authored list");
                    AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                    AssertEqual(steps.Count, index + 1, $"{at}: the fall-through ends the program");
                    break;
                }
                ushort word = Word(cursor);
                if (word < DemoInputListWords.RoutineWord)
                {
                    var show = AssertIs<AnimationProgramStep<DemoInputProgram, DemoInputFrame, DemoInputCue>.ShowStep>(step, $"{at}: input record");
                    AssertEqual(word, show.Duration, $"{at}: duration");
                    AssertEqual(Word(cursor + 2), (ushort)show.Frame.Held, $"{at}: held");
                    AssertEqual(Word(cursor + 4), (ushort)show.Frame.NewlyPressed, $"{at}: newly pressed");
                    cursor = unchecked((ushort)(cursor + DemoInputListWords.InputRecordBytes));
                    records++;
                    continue;
                }
                if (Routines.TryGetValue(word, out DemoInputCue cue))
                {
                    var cueStep = AssertIs<AnimationProgramStep<DemoInputProgram, DemoInputFrame, DemoInputCue>.CueStep>(step, $"{at}: routine");
                    AssertEqual(cue, cueStep.Cue, $"{at}: routine cue");
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                }
                switch ((DemoInputInstruction)word)
                {
                    case DemoInputInstruction.Goto:
                        var goTo = AssertIs<AnimationProgramStep<DemoInputProgram, DemoInputFrame, DemoInputCue>.GoToStep>(step, $"{at}: goto");
                        AssertEqual(byEntry[Word(cursor + 2)], goTo.Target, $"{at}: goto target");
                        AssertEqual(steps.Count, index + 1, $"{at}: the goto ends the program");
                        break;
                    case DemoInputInstruction.Delete:
                        AssertTrue(step is AnimationProgramStep<DemoInputProgram, DemoInputFrame, DemoInputCue>.StopStep, $"{at}: delete");
                        AssertEqual(steps.Count, index + 1, $"{at}: the delete ends the program");
                        break;
                    case DemoInputInstruction.SetPreInstruction:
                    case DemoInputInstruction.ClearPreInstruction:
                    case DemoInputInstruction.DecrementTimerAndGoto:
                    case DemoInputInstruction.SetTimer:
                    default:
                        throw new InvalidDataException($"{at}: native word ${word:X4} has no typed form.");
                }
            }
        }
        Console.WriteLine($"  Demo-input programs: {DemoInputNativeListAddresses.Objects.Length} objects and {DemoInputNativeListAddresses.Entries.Length} programs ({records} input records) match bank $91.");
    }

    /// <summary>Runs the demo-input program oracle.</summary>
    [VerificationCommand("--demo-input-programs")]
    internal static int DemoInputProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyDemoInputPrograms), () => VerifyDemoInputPrograms(rom));
        return 0;
    }
}
