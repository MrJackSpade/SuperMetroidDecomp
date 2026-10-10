namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Cacatac's upright and inverted idle/attack programs.
/// Interleaved visual selectors are compiled separately in
/// <see cref="Assets.EnemySpritemapDefinitions"/>.
/// </summary>
internal abstract class CacatacInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_Cacatac_UpsideUp_Idling</c> at $A2:9E8A-$A2:9EAF.
    /// Its $A095 moving-left/right instruction precedes eight animation durations:
    /// the word at $9E8C + 4*i is exactly $0008 for i = 0..7. Each duration is
    /// followed by a fixed visual selector. The $80ED goto at $9EAC targets
    /// $9E8A, so every idle cycle re-executes the moving-left/right instruction.
    /// </summary>
    internal const ushort UpsideUpIdle = 0x9e8a;
    /// <summary>
    /// <c>InstList_Cacatac_UpsideUp_Attacking</c> at $A2:9EB0-$A2:9ED9.
    /// Four opening duration words at base + 4*i are $0015, $0005, $0015,
    /// $0005 for i = 0..3, each followed by a fixed visual selector.
    /// At base + $10 the $9F2A sound command precedes five $A0A7 spike
    /// commands at base + $12 + 4*i for i = 0..4. Their authored direction
    /// selectors are left-facing-up, up-left, up, up-right, right-facing-up.
    /// The $80ED goto at base + $26 returns to <see cref="UpsideUpIdle"/>.
    /// </summary>
    internal const ushort UpsideUpAttack = 0x9eb0;
    /// <summary>
    /// <c>InstList_Cacatac_UpsideDown_Idling_0</c> at $A2:9EDA-$A2:9EFF.
    /// Its initial $A095 moving-left/right instruction is executed once before
    /// the eight-pose loop. The duration word at $9EDC + 4*i is exactly $0008
    /// for i = 0..7, with a fixed visual selector after each duration.
    /// The $80ED goto at $9EFC targets <see cref="UpsideDownIdleLoop"/>.
    /// </summary>
    internal const ushort UpsideDownIdle = 0x9eda;
    /// <summary>
    /// <c>InstList_Cacatac_UpsideDown_Idling_1</c> at $A2:9EDC starts the
    /// inverted idle loop after its one-time moving-left/right instruction.
    /// This differs from the upright loop target $9E8A.
    /// </summary>
    internal const ushort UpsideDownIdleLoop = 0x9edc;
    /// <summary>
    /// <c>InstList_Cacatac_UpsideDown_Attacking</c> at $A2:9F00-$A2:9F29.
    /// It has the same four-duration, sound, and five-spike command layout as
    /// the upright list. Its authored direction selectors are left-facing-down,
    /// down-left, down, down-right, right-facing-down. The $80ED goto at
    /// base + $26 returns to <see cref="UpsideDownIdle"/>.
    /// </summary>
    internal const ushort UpsideDownAttack = 0x9f00;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value)) return value;
        throw new InvalidDataException(
            $"Cacatac instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool TryRead(int address, out ushort value)
    {
        int offset = address - UpsideUpIdle;
        value = 0;
        if (offset < 0 || offset >= 160 || (offset & 1) != 0) return false;
        bool inverted = offset >= 80;
        offset %= 80;
        ushort idle = inverted ? UpsideDownIdle : UpsideUpIdle;
        if (offset < 38)
        {
            if (offset == 0) value = (ushort)CacatacInstruction.SetFunctionMovingLeftRight;
            else if (offset < 34 && offset % 4 == 2) value = 8;
            else if (offset == 34) value = (ushort)CommonEnemyInstruction.Goto;
            else if (offset == 36) value = inverted ? UpsideDownIdleLoop : UpsideUpIdle;
            else return false;
            return true;
        }
        offset -= 38;
        if (offset < 16)
        {
            if (offset % 4 != 0) return false;
            value = (ushort)(((offset / 4) & 1) == 0 ? 21 : 5);
        }
        else if (offset == 16) value = (ushort)CacatacInstruction.PlaySpikesSFX;
        else if (offset < 38)
        {
            if ((offset - 18) % 4 == 0)
                value = (ushort)CacatacInstruction.SpawnSpikeProjectileWithParameterInY;
            else
            {
                int shot = (offset - 20) / 4;
                // Sweep from left through vertical to right, interleaving diagonals.
                value = (ushort)((shot & 1) == 0 ? (int)(inverted ? CacatacSpikeDirection.LeftFacingDown : CacatacSpikeDirection.LeftFacingUp) + shot
                    : (int)(inverted ? CacatacSpikeDirection.DownLeft : CacatacSpikeDirection.UpLeft) + shot - 1);
            }
        }
        else value = offset == 38 ? (ushort)CommonEnemyInstruction.Goto : idle;
        return true;
    }
}