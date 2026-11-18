using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// What each scroll extension writes at its block (#2481): the extension collision and the step back toward the trigger
/// it extends. A placement's setup carries the extension it places.
/// </summary>
internal static class ScrollExtensionDefinitions
{
    /// <summary>An extension reaching right from its trigger: a horizontal extension stepping one block left, back to it.</summary>
    internal static ScrollExtension Rightwards { get; } = new(RoomCollisionType.HorizontalExtension, -1);

    /// <summary>An extension reaching left from its trigger: a horizontal extension stepping one block right, back to it.</summary>
    internal static ScrollExtension Leftwards { get; } = new(RoomCollisionType.HorizontalExtension, 1);

    /// <summary>An extension reaching down from its trigger: a vertical extension stepping one block up, back to it.</summary>
    internal static ScrollExtension Downwards { get; } = new(RoomCollisionType.VerticalExtension, -1);

    /// <summary>An extension reaching up from its trigger: a vertical extension stepping one block down, back to it.</summary>
    internal static ScrollExtension Upwards { get; } = new(RoomCollisionType.VerticalExtension, 1);
}
