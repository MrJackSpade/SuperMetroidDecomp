using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Calculated Fake Kraid installed-art selections and the fingernail's initial frame.</summary>
internal static class KraidVisualDefinitions
{
    /// <summary>Initial fingernail frame at $A7:A617, selected by $A7:8B0C.</summary>
    internal const ushort InitialNailFrame = 0xa617;
    /// <summary>First left-facing MiniKraid walk frame operand at $A6:99B0.</summary>
    private const ushort FirstFakeOperand = 0x99b0;
    /// <summary>Matching right-facing operand at $A6:99FE, 78 bytes after the left program.</summary>
    private const int FacingProgramStride = 0x4e;
    /// <summary>First MiniKraid spritemap at $A6:9C64; fourteen maps contain sixteen five-byte OAM entries.</summary>
    private const ushort FirstFakeFrame = 0x9c64;

    /// <summary>
    /// Nail frames alternate two/four OAM entries, giving12/22-byte strides.
    /// MiniKraid has seven82-byte frames per facing; walking reverses four poses,
    /// and spitting advances three mouth poses then returns to the middle pose.
    /// Only native visual operands are accepted; unsupported selectors throw.
    /// </summary>
    internal static ushort FrameAt(EnemyDefinitionId enemyDefinition, ushort address)
    {
        if (enemyDefinition == EnemyDefinitionId.MiniKraid)
        {
            int facing = address >= FirstFakeOperand + FacingProgramStride ? 1 : 0;
            int offset = address - FirstFakeOperand - facing * FacingProgramStride;
            int pose = offset switch
            {
                >= 0 and <= 12 when offset % 4 == 0 => offset / 4,
                0x18 => 0,
                >= 0x1e and <= 0x26 when (offset - 0x1e) % 4 == 0 => 3 - (offset - 0x1e) / 4,
                >= 0x2e and <= 0x3a when (offset - 0x2e) % 6 == 0 => 4 + (offset - 0x2e) / 6,
                0x3e => 5,
                _ => -1,
            };
            if (pose >= 0) return (ushort)(FirstFakeFrame + 82 * (7 * facing + pose));
        }
        else
        {
            throw new InvalidDataException($"Enemy ${(int)enemyDefinition:X4} has no compiled Kraid-family visuals.");
        }
        throw new InvalidDataException(
            $"Kraid-family ${(int)enemyDefinition:X4} visual operand ${address:X4} is not compiled.");
    }
}
