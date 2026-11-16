namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's fall through his bridge (#2171): how his tongue waits to hide and where his sinking body disappears.</summary>
internal static class CrocomireBridgeCollapseDefinitions
{
    /// <summary>
    /// How long the collapsing bridge's tongue holds its last frame, in updates: the longest wait a timer is given, so it
    /// holds until its hidden animation takes over. It is kept exact (#1627): the tongue keeps counting it down, and its
    /// frame and new-frame flag follow that count until the hidden animation takes over.
    /// </summary>
    internal const int TongueHoldsUntilHidden = 32767;

    /// <summary>How far below the top of Crocomire's view, in pixels, the row from which his sinking hides his body lies.</summary>
    internal const int SinkHiddenRowDepth = 288;
}
