using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculated Fake Kraid and fingernail installed-art selections.</summary>
internal static class KraidVisualDefinitions
{
    /// <summary>Initial fingernail frame at $A7:A617, selected by $A7:8B0C.</summary>
    internal const ushort InitialNailFrame = 0xa617;
    /// <summary>First nail frame operand at $A7:8B0C; eight records have four-byte stride.</summary>
    private const ushort FirstNailOperand = 0x8b0c;
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
    internal static ushort FrameAt(ushort enemyDefinition, ushort address)
    {
        if (enemyDefinition is RoomEnemySystem.KraidGoodNailDefinition or RoomEnemySystem.KraidBadNailDefinition)
        {
            int distance = address - FirstNailOperand;
            if ((uint)distance <= 28 && distance % 4 == 0)
            {
                int frame = distance / 4;
                return (ushort)(InitialNailFrame + 34 * (frame / 2) + 12 * (frame % 2));
            }
        }
        else if (enemyDefinition == RoomEnemySystem.FakeKraidDefinition)
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
            throw new InvalidDataException($"Enemy ${enemyDefinition:X4} has no compiled Kraid-family visuals.");
        }
        throw new InvalidDataException(
            $"Kraid-family ${enemyDefinition:X4} visual operand ${address:X4} is not compiled.");
    }
}
