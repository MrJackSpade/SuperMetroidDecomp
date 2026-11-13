using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.TestSupport.MovieReplay.NativeEnemyListDefinitions;

namespace SuperMetroid.TestSupport.MovieReplay.Verification;

/// <summary>
/// A native enemy list in one bank, stepped with the enemy interpreter's timer gate. The loop counter is the
/// actor's timer word; owner callbacks are recorded with as many operand words as <paramref name="operandWords"/>
/// gives them.
/// </summary>
internal sealed class NativeEnemyListReader(int bank, ushort cursor, ushort timer, Func<ushort, int> operandWords)
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
        Timer = WrappingCounter.Decrement(Timer).Value;
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
                    Counter = WrappingCountdown.Start(Counter).Decrement().Remaining.RawValue;
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
