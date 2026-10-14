using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A minimal reader of native bank-$86 enemy projectile lists for the #641 ROM oracles: it steps a list with the
/// projectile interpreter's timer gate so a typed projectile animation can be compared with it update by update.
/// Only verification reads cartridge lists; the port runs typed animations.
/// </summary>
internal static class NativeEnemyProjectileListDefinitions
{
    /// <summary>The projectile bank, as a long-address base.</summary>
    internal const int ProjectileBank = 0x860000;
    /// <summary>Bit 15 marks an instruction word; a clear bit makes the word a frame duration.</summary>
    private const ushort InstructionBit = 0x8000;

    /// <summary>What a native projectile list word does.</summary>
    internal enum WordKind
    {
        /// <summary>A frame: a duration and a spritemap operand.</summary>
        Frame,
        /// <summary>The pre-instruction install and its routine operand.</summary>
        PreInstruction,
        /// <summary>Clears the pre-instruction.</summary>
        ClearPreInstruction,
        /// <summary>Clears the projectile's palette selection.</summary>
        UsePaletteZero,
        /// <summary>A long call to an external routine, recorded rather than run.</summary>
        CallExternal,
        /// <summary>The goto and its target operand.</summary>
        GoTo,
        /// <summary>The sleep.</summary>
        Sleep,
        /// <summary>The deletion.</summary>
        Delete,
    }

    /// <summary>The bank-$86 opcodes the reader decodes besides frames.</summary>
    private static readonly Dictionary<ushort, WordKind> Words = new()
    {
        [(ushort)EnemyProjectileInstruction.PreInstructionInY] = WordKind.PreInstruction,
        [(ushort)EnemyProjectileInstruction.ClearPreInstruction] = WordKind.ClearPreInstruction,
        [(ushort)EnemyProjectileInstruction.UsePalette0] = WordKind.UsePaletteZero,
        [(ushort)EnemyProjectileInstruction.CallExternalFunctionInY] = WordKind.CallExternal,
        [(ushort)EnemyProjectileInstruction.GotoY] = WordKind.GoTo,
        [(ushort)EnemyProjectileInstruction.Sleep] = WordKind.Sleep,
        [(ushort)EnemyProjectileInstruction.Delete] = WordKind.Delete,
    };

    /// <summary>Decodes a native projectile list word, rejecting words outside the compared set.</summary>
    internal static WordKind Classify(ushort word) =>
        (word & InstructionBit) == 0 ? WordKind.Frame
        : Words.TryGetValue(word, out WordKind kind) ? kind
        : throw new InvalidDataException($"Projectile list word ${word:X4} is outside the compared set.");

    /// <summary>A native bank-$86 projectile list with the projectile interpreter's timer gate.</summary>
    internal sealed class Reader(ushort cursor, ushort timer)
    {
        /// <summary>The next native instruction.</summary>
        private ushort cursor = cursor;
        /// <summary>Updates remaining before the next native instruction is consumed.</summary>
        internal ushort Timer { get; private set; } = timer;
        /// <summary>The spritemap operand address of the latest native frame.</summary>
        internal ushort Operand { get; private set; }
        /// <summary>The pre-instruction the list installed, if any.</summary>
        internal ushort? PreInstruction { get; private set; }
        /// <summary>Whether the list cleared its pre-instruction.</summary>
        internal bool PreInstructionCleared { get; private set; }
        /// <summary>Whether the list cleared its palette selection.</summary>
        internal bool PaletteCleared { get; private set; }
        /// <summary>The long routine of each external call the latest update made.</summary>
        internal List<int> Calls { get; } = [];
        /// <summary>Whether the list has deleted its projectile.</summary>
        internal bool Deleted { get; private set; }

        /// <summary>Runs one update.</summary>
        internal void Step(IImportCartridgeSource cartridge)
        {
            Calls.Clear();
            ushort old = Timer;
            Timer = unchecked((ushort)(Timer - 1));
            if (old != 1)
                return;
            for (int command = 0; command < 16; command++)
            {
                ushort word = cartridge.ReadCartridgeWord(ProjectileBank | cursor);
                switch (Classify(word))
                {
                    case WordKind.Frame:
                        Timer = word;
                        Operand = unchecked((ushort)(cursor + 2));
                        cursor = unchecked((ushort)(cursor + 4));
                        return;
                    case WordKind.PreInstruction:
                        PreInstruction = cartridge.ReadCartridgeWord(ProjectileBank | (cursor + 2));
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    case WordKind.ClearPreInstruction:
                        PreInstructionCleared = true;
                        cursor = unchecked((ushort)(cursor + 2));
                        continue;
                    case WordKind.UsePaletteZero:
                        PaletteCleared = true;
                        cursor = unchecked((ushort)(cursor + 2));
                        continue;
                    case WordKind.CallExternal:
                        Calls.Add(cartridge.ReadCartridgeLong(ProjectileBank | (cursor + 2)));
                        cursor = unchecked((ushort)(cursor + 5));
                        continue;
                    case WordKind.GoTo:
                        cursor = cartridge.ReadCartridgeWord(ProjectileBank | (cursor + 2));
                        continue;
                    case WordKind.Sleep:
                        return;
                    case WordKind.Delete:
                        Deleted = true;
                        return;
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(WordKind)}.");
                }
            }
            throw new InvalidDataException("A native projectile list ran sixteen commands without a frame.");
        }
    }
}
