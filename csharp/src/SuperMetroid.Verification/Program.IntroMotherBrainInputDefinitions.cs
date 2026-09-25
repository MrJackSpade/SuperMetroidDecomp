using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyIntroMotherBrainDemoInput(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock)
    {
        foreach ((int start, int end) in new[]
        {
            (IntroMotherBrainInputDefinitions.ListStart,
                IntroMotherBrainInputDefinitions.ListEnd),
            (IntroMotherBrainInputDefinitions.HeaderStart,
                IntroMotherBrainInputDefinitions.HeaderEnd),
        })
            for (int pointer = start; pointer < end; pointer++)
                AssertEqual(bus.ReadByte(DemoInputRomData.BankBase | pointer),
                    IntroMotherBrainInputDefinitions.ReadByte((ushort)pointer),
                    $"intro Mother Brain demo byte $91:{pointer:X4}");
        AssertThrows<InvalidDataException>(() =>
            IntroMotherBrainInputDefinitions.ReadWord(
                IntroMotherBrainInputDefinitions.ListEnd),
            "intro Mother Brain demo rejects adjacent bank-$91 code");

        var guarded = new IntroArtworkSourceReadGuard(bus);
        var compiled = new DemoInputState();
        var reference = new DemoInputState();
        compiled.Clear();
        reference.Clear();
        compiled.Enable();
        reference.Enable();
        compiled.LoadObject(guarded, IntroMotherBrainInputDefinitions.HeaderStart,
            definitionWord: IntroMotherBrainInputDefinitions.ReadWord);
        reference.LoadObject(bus, IntroMotherBrainInputDefinitions.HeaderStart);
        int endFrame = -1;
        for (int frame = 0; frame < 1024; frame++)
        {
            compiled.Step(guarded, specialInstruction: HandleEnd,
                instructionWord: IntroMotherBrainInputDefinitions.ReadWord);
            reference.Step(bus, specialInstruction: HandleEnd);
            AssertEqual(reference.InstructionPointer, compiled.InstructionPointer,
                $"intro Mother Brain demo cursor frame {frame}");
            AssertEqual(reference.InstructionTimer, compiled.InstructionTimer,
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
        var intro = new IntroCinematicState(guarded, characterArtwork: stock);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
            .Invoke(intro, null);
        typeof(IntroCinematicState).GetMethod("SetupMotherBrainFlashback", flags)!
            .Invoke(intro, null);
        typeof(IntroCinematicState).GetMethod("StepMotherBrainDemo", flags)!
            .Invoke(intro, null);
        var live = (DemoInputState)typeof(IntroCinematicState)
            .GetField("flashbackDemoInput", flags)!.GetValue(intro)!;
        var nativeFirst = new DemoInputState();
        nativeFirst.Clear();
        nativeFirst.Enable();
        nativeFirst.LoadObject(bus, IntroMotherBrainInputDefinitions.HeaderStart);
        nativeFirst.Step(bus);
        AssertEqual(nativeFirst.PreInstructionPointer, live.PreInstructionPointer,
            "production flashback owns the compiled demo header");
        AssertEqual(nativeFirst.InstructionPointer, live.InstructionPointer,
            "production flashback selects the native first record");
        AssertEqual(nativeFirst.InstructionTimer, live.InstructionTimer,
            "production flashback uses the native first-record timer");
        AssertEqual(nativeFirst.Held, live.Held,
            "production flashback publishes the native first held input");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production flashback never rereads the Mother Brain demo program");

        static DemoInputInstructionResult HandleEnd(DemoInputState state,
            ushort instruction, ushort argumentPointer)
        {
            if (instruction != IntroCinematicRomData.Flashback.ExpectedEndInstruction)
                return DemoInputInstructionResult.NotHandled(argumentPointer);
            state.Disable();
            return DemoInputInstructionResult.ContinueAt(argumentPointer);
        }
    }
}
