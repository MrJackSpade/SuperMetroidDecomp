using SuperMetroid.Core;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// Where a native load-station or title-demo record places Samus (#2483): the record stores her offset from half a screen
/// right of its camera on X and from the camera's top on Y, which the import turns into her room position.
/// </summary>
internal static class NativeSamusPlacement
{
    /// <summary>The camera-relative origin of a record's Samus offset: half a screen right of the camera.</summary>
    private static readonly Offset16<RoomPixels> OffsetOrigin = new(128, 0);

    /// <summary>
    /// The room position a record's <paramref name="offset"/> from its <paramref name="camera"/> places Samus at, wrapping
    /// each axis at sixteen bits.
    /// </summary>
    internal static Position16<RoomPixels> RoomPosition(Position16<RoomPixels> camera, Offset16<RoomPixels> offset) =>
        camera + OffsetOrigin + offset;
}
