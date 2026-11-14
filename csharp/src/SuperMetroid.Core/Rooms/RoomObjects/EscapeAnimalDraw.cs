namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The rescue wall's pictures as it breaks open for the escaping animals: the three solid blocks of its column show the
/// breakup a shot block shows, then blank.
/// </summary>
internal enum EscapeAnimalDraw
{
    /// <summary>The wall starts to break apart.</summary>
    Cracking,
    /// <summary>The wall breaks into pieces.</summary>
    Breaking,
    /// <summary>The last of the wall crumbles away.</summary>
    Crumbling,
    /// <summary>The wall is gone from view; its blocks stay solid until the change finishes.</summary>
    Blank,
}
