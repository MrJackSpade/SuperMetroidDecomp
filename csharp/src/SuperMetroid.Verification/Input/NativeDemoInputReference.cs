using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>
/// #1300: the native bank-$91 demo-input interpreter (<c>$91:834E..8463</c>), reading an object and its list words from a
/// cartridge or constructed fixture. Verification runs it beside a typed <see cref="DemoInputState"/> as the independent
/// reference for timing, input words and list position.
/// </summary>
internal sealed class NativeDemoInputReference(Func<ushort, ushort> readWord)
{
    /// <summary>High-bit enable flag installed by <c>$91:834E</c>.</summary>
    internal bool Enabled { get; private set; }

    /// <summary>Object-specific routine called before the shared instruction timer.</summary>
    internal ushort PreInstructionPointer { get; private set; }

    /// <summary>Address of the next bank-$91 input record or control instruction; zero once deleted.</summary>
    internal ushort InstructionPointer { get; private set; }

    /// <summary>Frames remaining in the active input record, with native word wrapping.</summary>
    internal NativeCountdown InstructionTimer { get; private set; }

    /// <summary>General loop timer manipulated by instructions <c>$844F/$8459</c>.</summary>
    internal NativeCountdown Timer { get; private set; }

    /// <summary>Initializer parameter stored at WRAM <c>$0A80</c>.</summary>
    internal ushort InitializationParameter { get; private set; }

    /// <summary>Controller word currently replacing player-held input.</summary>
    internal ushort Held { get; private set; }

    /// <summary>Controller edge word currently replacing newly pressed input.</summary>
    internal ushort NewlyPressed { get; private set; }

    /// <summary>Previous demo-held word retained by <c>$91:83D3</c>.</summary>
    internal ushort PreviousHeld { get; private set; }

    /// <summary>Previous demo-edge word retained by <c>$91:83D9</c>.</summary>
    internal ushort PreviousNewlyPressed { get; private set; }

    /// <summary>Prior held word published for this handler call.</summary>
    internal ushort PublishedPreviousHeld { get; private set; }

    /// <summary>Prior edge word published for this handler call.</summary>
    internal ushort PublishedPreviousNewlyPressed { get; private set; }

    /// <summary>Models <c>$91:834E</c>.</summary>
    internal void Enable() => Enabled = true;

    /// <summary>Models <c>$91:835F</c>.</summary>
    internal void Disable() => Enabled = false;

    /// <summary>Loads the six-byte object at <paramref name="objectPointer"/> as <c>$91:8395</c> does; its initializer must return at once.</summary>
    internal void Load(ushort objectPointer, ushort initializationParameter)
    {
        InitializationParameter = initializationParameter;
        AssertEqual(DemoInputRomData.Routines.NoOp, readWord(objectPointer), $"demo object $91:{objectPointer:X4} initializer");
        PreInstructionPointer = readWord(unchecked((ushort)(objectPointer + 2)));
        InstructionPointer = readWord(unchecked((ushort)(objectPointer + 4)));
        InstructionTimer = NativeCountdown.Start(1);
        Timer = NativeCountdown.Start(0);
    }

    /// <summary>A pre-instruction's redirect of all three object words.</summary>
    internal void Redirect(ushort preInstructionPointer, ushort instructionPointer, ushort instructionTimer = 1)
    {
        PreInstructionPointer = preInstructionPointer;
        InstructionPointer = instructionPointer;
        InstructionTimer = NativeCountdown.FromRaw(instructionTimer);
    }

    /// <summary>
    /// Executes <c>$91:83C0/$83F2</c> once. <paramref name="preInstruction"/> runs the object's pre-instruction and
    /// <paramref name="routine"/> each no-operand private routine of its list.
    /// </summary>
    internal void Step(Action<NativeDemoInputReference, ushort> preInstruction, Action<NativeDemoInputReference, ushort> routine)
    {
        if (!Enabled || InstructionPointer == 0)
            return;
        preInstruction(this, PreInstructionPointer);
        NativeWordCounterStep instructionTimer = InstructionTimer.Decrement();
        InstructionTimer = instructionTimer.Remaining;
        if (instructionTimer.IsZero)
            ProcessInstructionList(routine);
        PublishedPreviousHeld = PreviousHeld;
        PublishedPreviousNewlyPressed = PreviousNewlyPressed;
        PreviousHeld = Held;
        PreviousNewlyPressed = NewlyPressed;
    }

    /// <summary>Runs list words until an input record or a deletion yields.</summary>
    private void ProcessInstructionList(Action<NativeDemoInputReference, ushort> routine)
    {
        ushort cursor = InstructionPointer;
        while (true)
        {
            ushort word = readWord(cursor);
            if (word < DemoInputListWords.RoutineWord)
            {
                InstructionTimer = NativeCountdown.FromRaw(word);
                Held = readWord(unchecked((ushort)(cursor + 2)));
                NewlyPressed = readWord(unchecked((ushort)(cursor + 4)));
                InstructionPointer = unchecked((ushort)(cursor + DemoInputListWords.InputRecordBytes));
                return;
            }
            cursor = unchecked((ushort)(cursor + 2));
            if (!Enum.IsDefined((DemoInputInstruction)word))
            {
                routine(this, word);
                continue;
            }
            switch ((DemoInputInstruction)word)
            {
                case DemoInputInstruction.Delete:
                    InstructionPointer = 0;
                    Held = 0;
                    NewlyPressed = 0;
                    return;
                case DemoInputInstruction.SetPreInstruction:
                    PreInstructionPointer = readWord(cursor);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case DemoInputInstruction.ClearPreInstruction:
                    PreInstructionPointer = DemoInputRomData.Routines.ClearedPreInstruction;
                    break;
                case DemoInputInstruction.Goto:
                    cursor = readWord(cursor);
                    break;
                case DemoInputInstruction.DecrementTimerAndGoto:
                    NativeWordCounterStep timer = Timer.Decrement();
                    Timer = timer.Remaining;
                    cursor = !timer.IsZero ? readWord(cursor) : unchecked((ushort)(cursor + 2));
                    break;
                case DemoInputInstruction.SetTimer:
                    Timer = NativeCountdown.FromRaw(readWord(cursor));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(DemoInputInstruction)} {word:X4}.");
            }
        }
    }

    /// <summary>The object's words, in the order <see cref="Of(DemoInputState)"/> gives a typed object's.</summary>
    internal string Snapshot() =>
        $"{Enabled}/{InitializationParameter}/{PreInstructionPointer}/{InstructionPointer}/{InstructionTimer.RawValue}/" +
        $"{Timer.RawValue}/{Held}/{NewlyPressed}/{PreviousHeld}/{PreviousNewlyPressed}/{PublishedPreviousHeld}/{PublishedPreviousNewlyPressed}";

    /// <summary>A typed object's words: its cursor as the native instruction pointer and its buttons as joypad words.</summary>
    internal static string Of(DemoInputState state) =>
        $"{state.Enabled}/{state.InitializationParameter}/{DemoInputNativeListAddresses.PreInstructionAddress(state.PreInstruction)}/{DemoInputNativeListAddresses.Address(state.Cursor)}/" +
        $"{state.InstructionTimer.RawValue}/{state.Timer.RawValue}/{(ushort)state.Held}/{(ushort)state.NewlyPressed}/{(ushort)state.PreviousHeld}/" +
        $"{(ushort)state.PreviousNewlyPressed}/{(ushort)state.PublishedPreviousHeld}/{(ushort)state.PublishedPreviousNewlyPressed}";

    /// <summary>A pre-instruction for an object whose owner runs none: only the return-only routines may be installed.</summary>
    internal static void ReturnOnly(NativeDemoInputReference _, ushort preInstruction)
    {
        if (!Enum.IsDefined((ReturnOnlyPreInstruction)preInstruction))
            throw new InvalidDataException($"Demo object pre-instruction $91:{preInstruction:X4} needs its owner.");
    }

    /// <summary>The return-only pre-instructions an object without its own may hold.</summary>
    private enum ReturnOnlyPreInstruction : ushort
    {
        /// <summary>$91:83BF, installed by the demo object definition.</summary>
        NoOp = DemoInputRomData.Routines.NoOp,
        /// <summary>$91:8447, installed when the pre-instruction is cleared.</summary>
        Cleared = DemoInputRomData.Routines.ClearedPreInstruction,
    }

    /// <summary>A list routine for an object whose list calls none.</summary>
    internal static void NoRoutines(NativeDemoInputReference _, ushort routine) =>
        throw new InvalidDataException($"Demo list routine $91:{routine:X4} needs its owner.");
}
