namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The draw lists Botwoon's wall programs draw.</summary>
internal enum BotwoonWallDraw
{
    /// <summary>The first frame of a single block's crumble, shared with the respawning shot block.</summary>
    CrumbleFirst,
    /// <summary>The second crumble frame.</summary>
    CrumbleSecond,
    /// <summary>The third crumble frame.</summary>
    CrumbleThird,
    /// <summary>The last crumble frame, the cleared block.</summary>
    CrumbleFourth,
    /// <summary>The nine cleared wall blocks of an already-defeated room.</summary>
    Clear,
}
