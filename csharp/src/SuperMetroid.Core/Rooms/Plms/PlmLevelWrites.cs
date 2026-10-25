using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The level-word writes PLM families share: drawing a block's word and replacing its collision type and BTS.</summary>
internal static class PlmLevelWrites
{
    /// <summary>Updates level collision and BG1 appearance; BG1 shows the new block because it renders from level data.</summary>
    internal static void DrawLevelWord(RoomLevelData level, int blockIndex, int levelWord, int? visualWord = null) =>
        level.SetPlmForegroundEntry(blockIndex, levelWord, visualWord ?? RoomLevelWord.FromRaw(levelWord).VisualWord);

    /// <summary>
    /// Shared Write_Level_Data_Block_Type_and_BTS operation: replaces a block's collision type and BTS, keeping its
    /// visual index and flips.
    /// </summary>
    internal static void WritePlmCollisionTypeAndBts(RoomLevelData level, int blockIndex, RoomCollisionType collisionType, int bts)
    {
        var original = level.GetPlmCollisionBlockByIndex(blockIndex).PackedWord;
        level.SetPlmForegroundEntry(blockIndex, RoomLevelWord.Create(original.VisualBlockIndex,
            original.VisualFlipFlags, collisionType).Raw);
        level.SetPlmBehavior(blockIndex, new RoomBlockBehavior(bts));
    }
}
