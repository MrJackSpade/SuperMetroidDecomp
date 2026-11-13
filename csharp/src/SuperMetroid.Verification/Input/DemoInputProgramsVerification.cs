using SuperMetroid.Diagnostics.Input;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Cartridge;
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
    internal static void VerifyDemoInputPrograms(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(DemoInputListWords.ListBank | address);

        AssertEqual(Enum.GetValues<DemoInputRecordingId>().Length, DemoInputNativeListAddresses.Entries.Length, "every demo recording has a native entry");
        foreach ((ushort header, ushort frameBehavior, DemoInputRecordingId program) in DemoInputNativeListAddresses.Objects)
        {
            AssertEqual(DemoInputRomData.Routines.NoOp, Word(header), $"demo object $91:{header:X4} initializer");
            AssertEqual(Word(header + 2), frameBehavior, $"demo object $91:{header:X4} pre-instruction is the owner's first watch");
            AssertEqual(Word(header + 4), DemoInputNativeListAddresses.Entry(program), $"demo object $91:{header:X4} list");
        }

        int records = 0;
        foreach ((DemoInputRecordingId id, ushort entry) in DemoInputNativeListAddresses.Entries)
        {
            DemoRecording recording = DemoInputRecordings.Of(id);
            DemoInputNativeLoopEntry? loop = DemoInputNativeListAddresses.LoopEntries.Where(candidate => candidate.Recording == id)
                .Cast<DemoInputNativeLoopEntry?>().SingleOrDefault();
            ushort cursor = entry;
            int index = 0;
            while (true)
            {
                string at = $"demo {id} input {index} at $91:{cursor:X4}";
                if (loop is { } authored && cursor == authored.Entry)
                    AssertEqual(authored.FirstRecord, index, $"{at}: the recording runs on into its loop's list at the loop's first input");
                AssertEqual(cursor, DemoInputNativeListAddresses.Address(new DemoPlaybackPosition(id, index)), $"{at}: native address");
                ushort word = Word(cursor);
                if (word < DemoInputListWords.RoutineWord)
                {
                    AssertTrue(index < recording.Records.Count, $"{at}: the recording holds this input");
                    DemoInputRecord input = recording.Records[index];
                    AssertEqual(word, input.Duration, $"{at}: duration");
                    AssertEqual(Word(cursor + 2), ControllerWordCodec.ToJoypadWord(input.Held), $"{at}: held");
                    AssertEqual(Word(cursor + 4), ControllerWordCodec.ToJoypadWord(input.NewlyPressed), $"{at}: newly pressed");
                    cursor = unchecked((ushort)(cursor + DemoInputListWords.InputRecordBytes));
                    index++;
                    records++;
                    continue;
                }
                AssertEqual(recording.Records.Count, index, $"{at}: the recording's inputs end where its list's do");
                if (Routines.TryGetValue(word, out DemoInputCue cue))
                {
                    AssertEqual((ushort)DemoInputInstruction.Delete, Word(cursor + 2), $"{at}: the routine is followed by the delete");
                    AssertEqual(DemoRecordingEnd.SignalsThenStops(cue), recording.End, $"{at}: the recording signals, then ends");
                    break;
                }
                switch ((DemoInputInstruction)word)
                {
                    case DemoInputInstruction.Goto:
                        ushort target = Word(cursor + 2);
                        int loopsFrom = target == entry ? 0
                            : loop is { } own && target == own.Entry ? own.FirstRecord
                            : throw new InvalidDataException($"{at}: goto ${target:X4} leaves the recording.");
                        AssertEqual(DemoRecordingEnd.LoopsFrom(loopsFrom), recording.End, $"{at}: the recording loops");
                        break;
                    case DemoInputInstruction.Delete:
                        AssertEqual(DemoRecordingEnd.Stops, recording.End, $"{at}: the recording ends");
                        break;
                    case DemoInputInstruction.SetFrameBehavior:
                    case DemoInputInstruction.ClearFrameBehavior:
                    case DemoInputInstruction.DecrementTimerAndGoto:
                    case DemoInputInstruction.SetTimer:
                    default:
                        throw new InvalidDataException($"{at}: native word ${word:X4} has no typed form.");
                }
                break;
            }
        }
        Console.WriteLine($"  Demo-input recordings: {DemoInputNativeListAddresses.Objects.Length} objects and {DemoInputNativeListAddresses.Entries.Length} recordings ({records} input records) match bank $91.");
    }

    /// <summary>Runs the demo-input program oracle.</summary>
    [VerificationCommand("--demo-input-programs")]
    internal static int DemoInputProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyDemoInputPrograms), () => VerifyDemoInputPrograms(rom));
        return 0;
    }
}
