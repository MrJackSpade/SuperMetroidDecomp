namespace SuperMetroid.Core.Rooms;

/// <summary>Native bank-$7F room-level allocation geometry established by bank $82.</summary>
internal static class RoomLevelMemoryLayout
{
    /// <summary>
    /// Number of words cleared to $8000 by <c>LoadLevelDataAndOtherThings</c> at
    /// <c>$82:E7D3</c>; the loop writes byte offsets $63FE through $0000 inclusively.
    /// </summary>
    public const int PrefilledStreamingWordCount = 0x3200;

    /// <summary>Solid visual word used to prefill the complete native streaming allocation.</summary>
    public const ushort PrefilledLevelWord = 0x8000;
}
