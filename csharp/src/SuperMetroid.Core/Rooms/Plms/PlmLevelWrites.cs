using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The level-word writes PLM families share: drawing a block's word and replacing its collision type and BTS.</summary>
internal static class PlmLevelWrites
{
    /// <summary>Updates level collision and BG1 appearance; BG1 shows the new block because it renders from level data.</summary>
    internal static void DrawLevelWord(RoomLevelData level, int blockIndex, ushort levelWord, ushort? visualWord = null)
    {
        ushort renderedWord = visualWord ?? new RoomLevelWord(levelWord).VisualWord;
        level.SetPlmForegroundEntry(blockIndex, levelWord, renderedWord);
    }

    /// <summary>
    /// Shared Write_Level_Data_Block_Type_and_BTS operation: preserve visual index/flips,
    /// replace the collision nibble, and publish the independent low-byte behavior.
    /// </summary>
    internal static void WritePlmCollisionTypeAndBts(RoomLevelData level, int blockIndex, ushort typeAndBts) =>
        WritePlmCollisionTypeAndBts(level, blockIndex, new RoomLevelWord(typeAndBts).CollisionType, unchecked((byte)typeAndBts));

    /// <summary>Replaces a block's collision type and BTS, keeping its visual payload.</summary>
    internal static void WritePlmCollisionTypeAndBts(RoomLevelData level, int blockIndex, RoomCollisionType collisionType, int bts)
    {
        var original = level.GetPlmCollisionBlockByIndex(blockIndex).PackedWord;
        level.SetPlmForegroundEntry(blockIndex, RoomLevelWord.Create(original.VisualBlockIndex,
            original.VisualFlipFlags, collisionType).Raw);
        level.SetPlmBehavior(blockIndex, new RoomBlockBehavior(bts));
    }
}
