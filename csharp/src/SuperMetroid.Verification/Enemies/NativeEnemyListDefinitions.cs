using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// A minimal reader of native enemy instruction lists for the #641 ROM oracles: it decodes frames, the common
/// bank-$A0 commands and owner callbacks, and steps a list with the enemy interpreter's timer gate so a typed
/// program can be compared with it update by update. Only verification reads cartridge lists; the port runs
/// typed programs.
/// </summary>
internal static class NativeEnemyListDefinitions
{
    /// <summary>Bit 15 marks an instruction word; a clear bit makes the word a frame duration.</summary>
    internal const ushort InstructionBit = 0x8000;

    /// <summary>What a native enemy list word does.</summary>
    internal enum WordKind
    {
        /// <summary>A frame: a duration and a spritemap operand.</summary>
        Frame,
        /// <summary>The common goto and its target operand.</summary>
        GoTo,
        /// <summary>The common sleep.</summary>
        Sleep,
        /// <summary>The common stop, which deletes the actor.</summary>
        Stop,
        /// <summary>The common timer load and its count operand.</summary>
        SetCounter,
        /// <summary>The common timer countdown and its target operand.</summary>
        RepeatWhileCounting,
        /// <summary>The common wait and its update-count operand.</summary>
        Wait,
        /// <summary>An owner callback.</summary>
        Callback,
    }

    /// <summary>The common commands the reader decodes.</summary>
    private static readonly Dictionary<ushort, WordKind> CommonWords = new()
    {
        [(ushort)CommonEnemyInstruction.Goto] = WordKind.GoTo,
        [(ushort)CommonEnemyInstruction.Sleep] = WordKind.Sleep,
        [(ushort)CommonEnemyInstruction.StopScript] = WordKind.Stop,
        [(ushort)CommonEnemyInstruction.SetTimer] = WordKind.SetCounter,
        [(ushort)CommonEnemyInstruction.DecrementTimerAndGoto] = WordKind.RepeatWhileCounting,
        [(ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate] = WordKind.RepeatWhileCounting,
        [(ushort)CommonEnemyInstruction.WaitFrames] = WordKind.Wait,
    };

    /// <summary>Decodes a native enemy list word.</summary>
    internal static WordKind Classify(ushort word) =>
        (word & InstructionBit) == 0 ? WordKind.Frame
        : CommonWords.TryGetValue(word, out WordKind kind) ? kind
        : WordKind.Callback;

    /// <summary>One owner callback a native update executed, with its operand words.</summary>
    internal readonly record struct Callback(ushort Word, ushort[] Operands);

    /// <summary>
    /// A native enemy list in one bank, stepped with the enemy interpreter's timer gate. The loop counter is the
    /// actor's timer word; owner callbacks are recorded with as many operand words as <paramref name="operandWords"/>
    /// gives them.
    /// </summary>
    internal sealed class Reader(int bank, ushort cursor, ushort timer, Func<ushort, int> operandWords)
    {
        /// <summary>The next native instruction.</summary>
        internal ushort Cursor { get; private set; } = cursor;
        /// <summary>Updates remaining before the next native instruction is consumed.</summary>
        internal ushort Timer { get; private set; } = timer;
        /// <summary>The actor's loop-counter word.</summary>
        internal ushort Counter { get; set; }
        /// <summary>Spritemap of the latest native frame.</summary>
        internal ushort Spritemap { get; private set; }
        /// <summary>Whether the latest update showed a new frame; a wait or sleep keeps the previous answer.</summary>
        internal bool NewFrame { get; private set; }
        /// <summary>Whether the list has stopped and deleted its actor.</summary>
        internal bool Stopped { get; private set; }

        /// <summary>A long address in the reader's bank.</summary>
        private int At(int offset) => (bank << 16) | (ushort)offset;

        /// <summary>Runs one update and returns the callbacks it executed.</summary>
        internal List<Callback> Step(IImportCartridgeSource cartridge)
        {
            var callbacks = new List<Callback>();
            if (Stopped)
                return callbacks;
            ushort old = Timer;
            Timer = NativeWordCounter.Decrement(Timer).Value;
            if (old != 1)
            {
                NewFrame = false;
                return callbacks;
            }
            for (int command = 0; command < 64; command++)
            {
                ushort word = cartridge.ReadCartridgeWord(At(Cursor));
                ushort operand = cartridge.ReadCartridgeWord(At(Cursor + 2));
                switch (Classify(word))
                {
                    case WordKind.Frame:
                        Timer = word;
                        Spritemap = operand;
                        Cursor = unchecked((ushort)(Cursor + 4));
                        NewFrame = true;
                        return callbacks;
                    case WordKind.GoTo:
                        Cursor = operand;
                        continue;
                    case WordKind.Sleep:
                        return callbacks;
                    case WordKind.Stop:
                        Stopped = true;
                        return callbacks;
                    case WordKind.SetCounter:
                        Counter = operand;
                        Cursor = unchecked((ushort)(Cursor + 4));
                        continue;
                    case WordKind.RepeatWhileCounting:
                        Counter = NativeCountdown.FromRaw(Counter).Decrement().Remaining.RawValue;
                        Cursor = Counter != 0 ? operand : unchecked((ushort)(Cursor + 4));
                        continue;
                    case WordKind.Wait:
                        Timer = operand;
                        Cursor = unchecked((ushort)(Cursor + 4));
                        return callbacks;
                    case WordKind.Callback:
                    {
                        int count = operandWords(word);
                        var operands = new ushort[count];
                        for (int index = 0; index < count; index++)
                            operands[index] = cartridge.ReadCartridgeWord(At(Cursor + 2 + 2 * index));
                        callbacks.Add(new(word, operands));
                        Cursor = unchecked((ushort)(Cursor + 2 + 2 * count));
                        continue;
                    }
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(WordKind)}.");
                }
            }
            throw new InvalidDataException("A native enemy list ran 64 commands without a frame.");
        }

        /// <summary>Moves the reader to another list, as a native list install does.</summary>
        internal void Install(ushort list)
        {
            Cursor = list;
            Timer = 1;
        }
    }
}
