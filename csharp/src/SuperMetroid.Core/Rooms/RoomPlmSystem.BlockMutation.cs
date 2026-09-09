namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>
    /// Shared Write_Level_Data_Block_Type_and_BTS operation: preserve visual index/flips,
    /// replace the collision nibble, and publish the independent low-byte behavior.
    /// </summary>
    private static void WritePlmCollisionTypeAndBts(RoomLevelData level, int blockIndex, ushort typeAndBts)
    {
        var original = level.GetPlmCollisionBlockByIndex(blockIndex).PackedWord;
        var replacement = new RoomLevelWord(typeAndBts);
        level.SetPlmForegroundEntry(blockIndex, RoomLevelWord.Create(original.VisualBlockIndex,
            original.VisualFlipFlags, replacement.CollisionType).Raw);
        level.SetPlmBehavior(blockIndex, unchecked((byte)typeAndBts));
    }
}
