namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The bounded nine-block vertical wall-clear draw.</summary>
internal static class BotwoonWallObjectDrawDefinitions
{
    /// <summary>The nine cleared wall blocks of an already-defeated room; the crumble frames are shot-block breakup draws.</summary>
    internal static RoomObjectFrameDraw<BotwoonWallDraw> Clear { get; } = RoomObjectDraw.Of(BotwoonWallDraw.Clear);

    /// <summary>Number of air blocks written by the single vertical clear-wall run.</summary>
    internal const int BlockCount = 9;
}
