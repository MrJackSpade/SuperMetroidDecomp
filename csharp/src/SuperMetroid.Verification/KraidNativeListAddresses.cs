using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

/// <summary>
/// The native bank-$A7 and bank-$86 lists the Kraid ROM oracle (#1284) walks: the part and rock list entry points,
/// their callbacks and the cue each must become, and a minimal rock-list reader. Only verification reads them; the
/// port runs typed programs.
/// </summary>
internal static class KraidNativeListAddresses
{
    /// <summary>Kraid's instruction bank, $A7.</summary>
    internal const byte PartBankNumber = 0xa7;
    /// <summary>The bank-$86 projectile instruction lists, as a long-address base.</summary>
    internal const int ProjectileBank = 0x860000;

    /// <summary>Each typed part program's native entry point in bank $A7, in authored order.</summary>
    internal static readonly (KraidPartProgram Program, ushort Entry)[] PartEntries =
    [
        (KraidPartProgram.FootInitial, 0x86e7),
        (KraidPartProgram.FootNeutral, 0x86ed),
        (KraidPartProgram.FootWalkingForward, 0x86f3),
        (KraidPartProgram.FootWalkingForwardFinished, 0x87bb),
        (KraidPartProgram.FootLungeForward, 0x87bd),
        (KraidPartProgram.FootLungeForwardFinished, 0x8885),
        (KraidPartProgram.FootWalkingBackward, 0x8887),
        (KraidPartProgram.FootWalkingBackwardLoop, 0x8939),
        (KraidPartProgram.ArmNormal, 0x89f3),
        (KraidPartProgram.ArmNormalPause, 0x8a37),
        (KraidPartProgram.ArmSlow, 0x8a41),
        (KraidPartProgram.ArmRisingOrSinking, 0x8aa4),
        (KraidPartProgram.ArmDyingOrPreparingToLunge, 0x8af0),
        (KraidPartProgram.LintInitial, 0x8afe),
        (KraidPartProgram.LintPostGrowth, 0x8b04),
        (KraidPartProgram.NailLoop, 0x8b0a),
    ];

    /// <summary>The bank-$A7 part callbacks and the cue each must become; the no-operation callback has none.</summary>
    internal static readonly Dictionary<ushort, KraidPartCue?> PartCallbacks = new()
    {
        [0xb633] = null,
        [0xb636] = KraidPartCue.RaiseBody,
        [0xb63c] = KraidPartCue.LowerBodyAndShake,
        [0xb64e] = KraidPartCue.StepSound,
        [0xb65a] = KraidPartCue.StepLeft,
        [0xb667] = KraidPartCue.StepLeft,
        [0xb674] = KraidPartCue.StepRight,
        [0x8a8f] = KraidPartCue.SlowArmBelowHalfHealth,
    };

    /// <summary>The part program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static KraidPartProgram? PartProgramAt(ushort address) =>
        PartEntries.Where(item => item.Entry == address).Select(item => (KraidPartProgram?)item.Program).SingleOrDefault();

    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort PartEntry(KraidPartProgram program) => PartEntries.Single(item => item.Program == program).Entry;

    /// <summary>Whether the native part entries ascend, so the typed declaration order is the authored order.</summary>
    internal static bool PartEntriesAscend => PartEntries.Zip(PartEntries.Skip(1)).All(pair => pair.First.Entry < pair.Second.Entry);

    /// <summary>Each typed rock program's native entry point in bank $86.</summary>
    internal static readonly (KraidRockProgram Program, ushort Entry)[] RockEntries =
    [
        (KraidRockProgram.SharedPose, 0x9c7d),
        (KraidRockProgram.RisingRightPose, 0x9c83),
        (KraidRockProgram.SpitRockShot, 0x9c89),
    ];

    /// <summary>What a native bank-$86 Kraid rock list word does.</summary>
    internal enum RockWordKind
    {
        /// <summary>A frame: a duration and a spritemap operand.</summary>
        Frame,
        /// <summary>The pre-instruction install and its routine operand.</summary>
        PreInstruction,
        /// <summary>The sleep.</summary>
        Sleep,
        /// <summary>The deletion.</summary>
        Delete,
    }

    /// <summary>The bank-$86 opcodes the rock lists use besides frames.</summary>
    private static readonly Dictionary<ushort, RockWordKind> RockWords = new()
    {
        [(ushort)EnemyProjectileInstruction.PreInstructionInY] = RockWordKind.PreInstruction,
        [(ushort)EnemyProjectileInstruction.Sleep] = RockWordKind.Sleep,
        [(ushort)EnemyProjectileInstruction.Delete] = RockWordKind.Delete,
    };

    /// <summary>Decodes a native rock list word, rejecting words outside the compared set.</summary>
    internal static RockWordKind ClassifyRockWord(ushort word) =>
        (word & NativeEnemyListDefinitions.InstructionBit) == 0 ? RockWordKind.Frame
        : RockWords.TryGetValue(word, out RockWordKind kind) ? kind
        : throw new InvalidDataException($"Kraid rock list word ${word:X4} is outside the compared set.");

    /// <summary>A native bank-$86 Kraid rock list with the projectile interpreter's timer gate.</summary>
    internal sealed class NativeRockList(ushort cursor, ushort timer)
    {
        /// <summary>The next native instruction.</summary>
        private ushort cursor = cursor;
        /// <summary>Updates remaining before the next native instruction is consumed.</summary>
        internal ushort Timer { get; private set; } = timer;
        /// <summary>The spritemap operand of the latest native frame.</summary>
        internal ushort Operand { get; private set; }
        /// <summary>The pre-instruction the list installed, if any.</summary>
        internal ushort? PreInstruction { get; private set; }
        /// <summary>Whether the list has deleted its rock.</summary>
        internal bool Deleted { get; private set; }

        /// <summary>Runs one update.</summary>
        internal void Step(IImportCartridgeSource cartridge)
        {
            ushort old = Timer;
            Timer = NativeWordCounter.Decrement(Timer).Value;
            if (old != 1)
                return;
            for (int command = 0; command < 8; command++)
            {
                ushort word = cartridge.ReadCartridgeWord(ProjectileBank | cursor);
                switch (ClassifyRockWord(word))
                {
                    case RockWordKind.Frame:
                        Timer = word;
                        Operand = unchecked((ushort)(cursor + 2));
                        cursor = unchecked((ushort)(cursor + 4));
                        return;
                    case RockWordKind.PreInstruction:
                        PreInstruction = cartridge.ReadCartridgeWord(ProjectileBank | (cursor + 2));
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    case RockWordKind.Sleep:
                        return;
                    case RockWordKind.Delete:
                        Deleted = true;
                        return;
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(RockWordKind)}.");
                }
            }
            throw new InvalidDataException("A native Kraid rock list ran eight commands without a frame.");
        }
    }
}
