namespace SuperMetroid.Core.Rooms;

/// <summary>The bank-$84 draw lists Botwoon's wall programs draw.</summary>
internal enum BotwoonWallDraw : ushort
{
    /// <summary><c>$84:A345</c>: the first frame of a single block's crumble, shared with the respawning shot block.</summary>
    CrumbleFirst = RoomPlmShotBlockDrawDefinitions.SingleFrame0,
    /// <summary><c>$84:A34B</c>: the second crumble frame.</summary>
    CrumbleSecond = RoomPlmShotBlockDrawDefinitions.SingleFrame0 + 6,
    /// <summary><c>$84:A351</c>: the third crumble frame.</summary>
    CrumbleThird = RoomPlmShotBlockDrawDefinitions.SingleFrame0 + 12,
    /// <summary><c>$84:A357</c>: the last crumble frame, the cleared block.</summary>
    CrumbleFourth = RoomPlmShotBlockDrawDefinitions.SingleFrame0 + 18,
    /// <summary><c>$84:930F</c>: the nine cleared wall blocks of an already-defeated room.</summary>
    Clear = BotwoonWallPlmDrawDefinitions.ClearPointer,
}
