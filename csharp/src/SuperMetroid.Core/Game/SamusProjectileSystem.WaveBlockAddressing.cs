using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    /// <summary>Preserves bank $94:A352's byte indexing and chained-ADC carry.</summary>
    private static void ScanHorizontalWaveShotReactions(
        RoomLevelData level, SamusProjectileSlot slot, RoomPlmSystem? roomPlms)
    {
        ushort top = unchecked((ushort)(slot.Position.Y.Whole - slot.Radius.Y));
        ushort bottom = unchecked((ushort)(slot.Position.Y.Whole + slot.Radius.Y - 1));
        int remaining = unchecked((ushort)(bottom - (top & 0xfff0))) >> 4;
        int span = remaining;
        ushort targetX = unchecked((ushort)(slot.Velocity.X.SignedRawValue < 0
            ? slot.Position.X.Whole - slot.Radius.X : slot.Position.X.Whole + slot.Radius.X - 1));
        // XBA sets N/Z from its resulting LOW BYTE even with a 16-bit accumulator.
        // BMI therefore tests the original coordinate's high-byte sign, not bit 7
        // of its pixel offset. Testing the latter disables Wave reactions throughout
        // the lower half of every screen, including ordinary blue-door caps.
        if (remaining >= 16 || unchecked((short)slot.Position.Y.Whole) < 0 ||
            (slot.Position.Y.Whole >> 8) >= (level.Size.Y + 15) / 16 ||
            (targetX >> 8) >= (level.Size.X + 15) / 16)
            return;

        int product = unchecked((byte)(top >> 4)) * unchecked((byte)level.Size.X);
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
                RunShotReaction(level, slot, block, roomPlms, new ShotSpan(span, remaining, targetX), out bool endSpan);
                // Wave discards collision carry, but still honors PLM scan termination.
                if (endSpan) return;
            }

            // There is only one CLC before the two ADCs. Do not simplify this
            // to offset += width * 2: the first carry causes ceiling odd reads.
            int first = byteOffset + level.Size.X;
            byteOffset = unchecked((ushort)((ushort)first + level.Size.X + (first > ushort.MaxValue ? 1 : 0)));
        } while (--remaining >= 0);
    }

    /// <summary>
    /// Preserves bank $94:A3E4's linear tile addressing across the room's side edges.
    /// </summary>
    private static void ScanVerticalWaveShotReactions(
        RoomLevelData level, SamusProjectileSlot slot, RoomPlmSystem? roomPlms)
    {
        ushort left = unchecked((ushort)(slot.Position.X.Whole - slot.Radius.X));
        ushort right = unchecked((ushort)(slot.Position.X.Whole + slot.Radius.X - 1));
        int remaining = unchecked((ushort)(right - (left & 0xfff0))) >> 4;
        if (remaining >= 16) return;
        int span = remaining;
        ushort targetY = unchecked((ushort)(slot.Velocity.Y.SignedRawValue < 0
            ? slot.Position.Y.Whole - slot.Radius.Y
            : slot.Position.Y.Whole + slot.Radius.Y - 1));
        int centerScreenX = slot.Position.X.Whole >> 8;
        if (centerScreenX >= (level.Size.X + 15) / 16 ||
            (targetY >> 8) >= (level.Size.Y + 15) / 16)
            return;

        // The hardware multiplier consumes bytes. The following addition and
        // byte-offset shift consume words. X is deliberately not clipped to the
        // room width: right-edge spans reach the following row; left underflow
        // starts 4095 tiles forward while the center remains inside the room.
        // The native block reaction dispatcher bounds the resulting allocation
        // offset, not the horizontal extent of this individual tile span.
        int rowOffset = unchecked((byte)(targetY >> 4)) * level.Size.X;
        ushort byteOffset = unchecked((ushort)((rowOffset + (left >> 4)) * 2));
        do
        {
            int index = byteOffset >> 1;
            if (index < level.Size.X * level.Size.Y)
            {
                RunShotReaction(level, slot, level.GetCollisionBlock(new(unchecked((ushort)(index % level.Size.X)), unchecked((ushort)(index / level.Size.X)))), roomPlms,
                    new ShotSpan(span, remaining, targetY), out bool endSpan, horizontalMovement: false);
                if (endSpan) return;
            }
            byteOffset = unchecked((ushort)(byteOffset + 2));
        } while (--remaining >= 0);
    }
}
