using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
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
