namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Native bank-$7F room-level allocation geometry established by bank $82.</summary>
internal static class RoomLevelMemoryLayout
{
    /// <summary>
    /// Number of words cleared to $8000 by <c>LoadLevelDataAndOtherThings</c> at
    /// <c>$82:E7D3</c>; the loop writes byte offsets $63FE through $0000 inclusively.
    /// </summary>
    public const int PrefilledStreamingWordCount = 0x3200;

    /// <summary>The level word the complete streaming allocation is prefilled with: block zero, unflipped, solid.</summary>
    public static readonly ushort PrefilledLevelWord = (ushort)RoomLevelWord.Create(0, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock);
}
