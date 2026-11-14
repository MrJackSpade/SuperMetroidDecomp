namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Where Phantoon's body's area lies against him (#1858): the room pixel the top-left corner of the area his body's frames
/// are placed on lies this far left of and above his position.
/// </summary>
internal static class PhantoonBodyAreaDefinitions
{
    /// <summary>The pixels his body's area's left edge lies left of him.</summary>
    internal const int LeftOfBody = 40;

    /// <summary>The pixels his body's area's top edge lies above him.</summary>
    internal const int AboveBody = 40;
}
