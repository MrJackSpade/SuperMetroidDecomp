namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// What each scroll extension writes at its block (#2481): the way back toward the trigger it extends, which its
/// extension collision and behaviour follow. A placement's setup carries the extension it places.
/// </summary>
internal static class ScrollExtensionDefinitions
{
    /// <summary>An extension reaching right from its trigger: its trigger is the block to its left.</summary>
    internal static ScrollExtension Rightwards { get; } = new(ScrollExtensionDirection.Left);

    /// <summary>An extension reaching left from its trigger: its trigger is the block to its right.</summary>
    internal static ScrollExtension Leftwards { get; } = new(ScrollExtensionDirection.Right);

    /// <summary>An extension reaching down from its trigger: its trigger is the block above.</summary>
    internal static ScrollExtension Downwards { get; } = new(ScrollExtensionDirection.Up);

    /// <summary>An extension reaching up from its trigger: its trigger is the block below.</summary>
    internal static ScrollExtension Upwards { get; } = new(ScrollExtensionDirection.Down);
}
