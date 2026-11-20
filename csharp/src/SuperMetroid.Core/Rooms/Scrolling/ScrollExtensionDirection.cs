namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The way from a scroll extension's block back to the trigger it extends, one block away.</summary>
internal enum ScrollExtensionDirection
{
    /// <summary>The trigger is the block to the left.</summary>
    Left,
    /// <summary>The trigger is the block to the right.</summary>
    Right,
    /// <summary>The trigger is the block above.</summary>
    Up,
    /// <summary>The trigger is the block below.</summary>
    Down,
}
