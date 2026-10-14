using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static IntroCinematicArtworkVerification;
using static IntroMotherBrainInputSourceVerification;
using static RetailGameFixtureVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Checks the compiled Mother Brain flashback demo against its bank-$91 input program.</summary>
internal static class IntroMotherBrainInputDefinitionsVerification
{
    /// <summary>Compares every demo input edge and timer while forbidding program ROM reads in the live intro owner.</summary>
    internal static void VerifyIntroMotherBrainDemoInput(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock)
    {
        Suite(nameof(VerifyIntroMotherBrainInputSource), () => VerifyIntroMotherBrainInputSource(bus));
        ushort ReferenceWord(ushort pointer) => (ushort)(SnesLittleEndian.Word(IntroMotherBrainDemoInputRomByte(bus, 0x910000 | pointer), IntroMotherBrainDemoInputRomByte(bus, 0x910000 | unchecked((ushort)(pointer + 1)))));
        AssertThrows<InvalidDataException>(() =>
            IntroMotherBrainInputDefinitions.ReadWord(
                IntroMotherBrainInputDefinitions.ListEnd),
            "intro Mother Brain demo rejects adjacent bank-$91 code");

        var guarded = NewIntroArtworkSourceReadGuard(bus);
        var compiled = new DemoInputState();
        var reference = new DemoInputState();
        compiled.Clear();
        reference.Clear();
        compiled.Enable();
        reference.Enable();
        compiled.LoadObject(guarded, IntroMotherBrainInputDefinitions.HeaderStart,
            definitionWord: IntroMotherBrainInputDefinitions.ReadWord);
        reference.LoadObject(bus, IntroMotherBrainInputDefinitions.HeaderStart, definitionWord: ReferenceWord);
        int endFrame = -1;
        for (int frame = 0; frame < 1024; frame++)
        {
            compiled.Step(guarded, new FlashbackEndDemoProgram(IntroMotherBrainInputDefinitions.ReadWord));
            reference.Step(bus, new FlashbackEndDemoProgram(ReferenceWord));
            AssertEqual(reference.InstructionPointer, compiled.InstructionPointer,
                $"intro Mother Brain demo cursor frame {frame}");
            AssertEqual(reference.InstructionTimer.RawValue, compiled.InstructionTimer.RawValue,
                $"intro Mother Brain demo countdown frame {frame}");
            AssertEqual(reference.Held, compiled.Held,
                $"intro Mother Brain demo held frame {frame}");
            AssertEqual(reference.NewlyPressed, compiled.NewlyPressed,
                $"intro Mother Brain demo input edge frame {frame}");
            AssertEqual(reference.Enabled, compiled.Enabled,
                $"intro Mother Brain demo enable state frame {frame}");
            if (compiled.InstructionPointer == 0)
            {
                endFrame = frame;
                break;
            }
        }
        AssertTrue(endFrame >= 0 && !compiled.Enabled,
            "intro Mother Brain demo reaches its native disable/delete instruction");

        // Exercise the actual cinematic owner under the same source guard: both its
        // constructor-time object load and frame-time interpreter must use installed data.
        var intro = CreateRetailIntroFixture(guarded, characterArtwork: stock);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        intro.SetupFirstIllustratedPage();
        intro.SetupMotherBrainFlashback();
        intro.StepMotherBrainDemo();
        var live = intro.flashbackDemoInput;
        var nativeFirst = new DemoInputState();
        nativeFirst.Clear();
        nativeFirst.Enable();
        nativeFirst.LoadObject(bus, IntroMotherBrainInputDefinitions.HeaderStart, definitionWord: ReferenceWord);
        nativeFirst.Step(bus, new DemoInputListProgram(ReferenceWord));
        AssertEqual(nativeFirst.PreInstructionPointer, (live ?? throw new InvalidOperationException("Verification requires live.")).PreInstructionPointer,
            "production flashback owns the compiled demo header");
        AssertEqual(nativeFirst.InstructionPointer, live.InstructionPointer,
            "production flashback selects the native first record");
        AssertEqual(nativeFirst.InstructionTimer.RawValue, live.InstructionTimer.RawValue,
            "production flashback uses the native first-record timer");
        AssertEqual(nativeFirst.Held, live.Held,
            "production flashback publishes the native first held input");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production flashback never rereads the Mother Brain demo program");

    }

    /// <summary>Reads one native demo-program byte for the independent comparison source.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro Mother Brain demo input")]
    private static byte IntroMotherBrainDemoInputRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>The flashback demo's end routine over either instruction source (#633).</summary>
    internal sealed class FlashbackEndDemoProgram(Func<ushort, ushort> readWord) : IDemoInputProgram
    {
        /// <summary>Obtains a word from the selected compiled or cartridge instruction source.</summary>
        public ushort ReadInstructionWord(ushort pointer) => readWord(pointer);

        /// <summary>The flashback demo has no separate pre-instruction action.</summary>
        public bool TryRunPreInstruction(DemoInputState demo, ushort pointer) => false;

        /// <summary>Disables the demo when the flashback's expected end opcode is reached.</summary>
        public DemoInputInstructionResult RunSpecialInstruction(DemoInputState demo, ushort opcode, ushort cursor)
        {
            if (opcode != IntroCinematicRomData.Flashback.ExpectedEndInstruction)
                return DemoInputInstructionResult.NotHandled(cursor);
            demo.Disable();
            return DemoInputInstructionResult.ContinueAt(cursor);
        }
    }
}
