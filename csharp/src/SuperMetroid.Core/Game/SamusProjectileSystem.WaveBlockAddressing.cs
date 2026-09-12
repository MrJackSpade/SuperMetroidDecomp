using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    /// <summary>Preserves bank $94:A352's byte indexing and chained-ADC carry.</summary>
    private static void ScanHorizontalWaveShotReactions(
        RoomLevelData level, SamusProjectileSlot slot, RoomPlmSystem? roomPlms)
    {
        ushort top = unchecked((ushort)(slot.YPosition - slot.YRadius));
        ushort bottom = unchecked((ushort)(slot.YPosition + slot.YRadius - 1));
        int remaining = unchecked((ushort)(bottom - (top & 0xfff0))) >> 4;
        ushort targetX = unchecked((ushort)(slot.XVelocity < 0
            ? slot.XPosition - slot.XRadius : slot.XPosition + slot.XRadius - 1));
        // The native XBA/BMI tests the original low byte's sign, before masking
        // the exchanged word down to the center's screen coordinate.
        if (remaining >= 16 || (slot.YPosition & 0x80) != 0 ||
            (slot.YPosition >> 8) >= (level.HeightInBlocks + 15) / 16 ||
            (targetX >> 8) >= (level.WidthInBlocks + 15) / 16)
            return;

        int product = unchecked((byte)(top >> 4)) * unchecked((byte)level.WidthInBlocks);
        ushort byteOffset = unchecked((ushort)((product + (targetX >> 4)) * 2));
        do
        {
            int index = byteOffset >> 1;
            if (index < level.ForegroundEntries.Length)
            {
                RoomCollisionBlock block = level.GetCollisionBlockByIndex(index);
                if ((byteOffset & 1) != 0)
                {
                    if (index + 1 >= level.ForegroundEntries.Length)
                        throw new NotSupportedException("Odd Wave collision read crosses the modeled level-data allocation.");
                    ushort next = level.GetCollisionBlockByIndex(index + 1).LevelWord;
                    // Dispatch consumes the unaligned word; BTS and PLM setup
                    // still use floor(byteOffset / 2), including aligned writes.
                    block = block with { LevelWord = unchecked((ushort)((block.LevelWord >> 8) | (next << 8))) };
                }
                RunShotReaction(level, slot, block, roomPlms);
            }

            // There is only one CLC before the two ADCs. Do not simplify this
            // to offset += width * 2: the first carry causes ceiling odd reads.
            int first = byteOffset + level.WidthInBlocks;
            byteOffset = unchecked((ushort)((ushort)first + level.WidthInBlocks + (first > ushort.MaxValue ? 1 : 0)));
        } while (--remaining >= 0);
    }

    /// <summary>
    /// Preserves bank $94:A3E4's linear tile addressing across the room's side edges.
    /// </summary>
    private static void ScanVerticalWaveShotReactions(
        RoomLevelData level, SamusProjectileSlot slot, RoomPlmSystem? roomPlms)
    {
        ushort left = unchecked((ushort)(slot.XPosition - slot.XRadius));
        ushort right = unchecked((ushort)(slot.XPosition + slot.XRadius - 1));
        int remaining = unchecked((ushort)(right - (left & 0xfff0))) >> 4;
        if (remaining >= 16) return;
        ushort targetY = unchecked((ushort)(slot.YVelocity < 0
            ? slot.YPosition - slot.YRadius
            : slot.YPosition + slot.YRadius - 1));
        int centerScreenX = slot.XPosition >> 8;
        if (centerScreenX >= (level.WidthInBlocks + 15) / 16 ||
            (targetY >> 8) >= (level.HeightInBlocks + 15) / 16)
            return;

        // The hardware multiplier consumes bytes. The following addition and
        // byte-offset shift consume words. X is deliberately not clipped to the
        // room width: right-edge spans reach the following row; left underflow
        // starts 4095 tiles forward while the center remains inside the room.
        // The native block reaction dispatcher bounds the resulting allocation
        // offset, not the horizontal extent of this individual tile span.
        int rowOffset = unchecked((byte)(targetY >> 4)) * level.WidthInBlocks;
        ushort byteOffset = unchecked((ushort)((rowOffset + (left >> 4)) * 2));
        do
        {
            int index = byteOffset >> 1;
            if (index < level.WidthInBlocks * level.HeightInBlocks)
                RunShotReaction(level, slot, level.GetCollisionBlock(index % level.WidthInBlocks, index / level.WidthInBlocks), roomPlms);
            byteOffset = unchecked((ushort)(byteOffset + 2));
        } while (--remaining >= 0);
    }
}
