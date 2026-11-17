namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How the camera following Samus meets the room's scroll zones and how far it may move an update (#2118).</summary>
internal static class SamusCameraDefinitions
{
    /// <summary>Half a 256-pixel screen: the camera's centre (#2280).</summary>
    internal const int HalfScreen = 128;

    /// <summary>
    /// The pixels below a screen's top the camera may sit in a screen not aligned to its top: the lower alignment of the
    /// 224-line view.
    /// </summary>
    internal const int LowerAlignment = 31;

    /// <summary>The pixels beyond its own speed the camera drifts each update out of a closed screen toward an open one.</summary>
    internal const int ClosedScreenDrift = 2;

    /// <summary>
    /// The most pixels Samus's previous position may lie from her position on either axis, which caps how far the camera
    /// moves after her in an update.
    /// </summary>
    internal const int PreviousPositionCap = 12;
}
