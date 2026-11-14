namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Where Crocomire's body's area lies against him (#1802): the room pixel the top-left corner of the area his body's frames
/// are placed on lies this far left of and above his position. Each frame's own placement on the area, including the rows
/// some poses sit lower, is the frame's, as the import placed it.
/// </summary>
internal static class CrocomireBodyAreaDefinitions
{
    /// <summary>The pixels his body's area's left edge lies left of him.</summary>
    internal const int LeftOfBody = 51;

    /// <summary>The pixels his body's area's top edge lies above him.</summary>
    internal const int AboveBody = 67;
}
