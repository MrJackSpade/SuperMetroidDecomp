using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.MovieReplay;

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
        [CommonEnemyInstructionConstants.Goto] = WordKind.GoTo,
        [(ushort)CommonEnemyInstruction.Sleep] = WordKind.Sleep,
        [CommonEnemyInstructionConstants.StopScript] = WordKind.Stop,
        [CommonEnemyInstructionConstants.SetTimer] = WordKind.SetCounter,
        [CommonEnemyInstructionConstants.DecrementTimerAndGoto] = WordKind.RepeatWhileCounting,
        [CommonEnemyInstructionConstants.DecrementTimerAndGotoDuplicate] = WordKind.RepeatWhileCounting,
        [CommonEnemyInstructionConstants.WaitFrames] = WordKind.Wait,
    };

    /// <summary>Decodes a native enemy list word.</summary>
    internal static WordKind Classify(ushort word) =>
        (word & InstructionBit) == 0 ? WordKind.Frame
        : CommonWords.TryGetValue(word, out WordKind kind) ? kind
        : WordKind.Callback;

    /// <summary>One owner callback a native update executed, with its operand words.</summary>
    internal readonly record struct Callback(ushort Word, ushort[] Operands);
}
