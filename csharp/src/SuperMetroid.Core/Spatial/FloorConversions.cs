namespace SuperMetroid.Core.Spatial;

/// <summary>Conversions from room positions and views to the signed floor offsets a floor transform takes.</summary>
public static class FloorConversions
{
    /// <summary>
    /// A room point as a signed floor offset when the turning floor is the foreground, as in the Ceres elevator shaft.
    /// The coordinates are unchanged; the floor transform reads them as signed.
    /// </summary>
    public static Offset16<FloorPixels> AsFloorOffset(this Position16<RoomPixels> cameraPosition) =>
        new(CoordinateAxis.AsSigned(cameraPosition.X), CoordinateAxis.AsSigned(cameraPosition.Y));

    /// <summary>
    /// A signed foreground view (#2384) as the signed floor offset a floor transform takes when the turning floor is the
    /// foreground; the coordinates are unchanged.
    /// </summary>
    public static Offset16<FloorPixels> AsFloorOffset(this Offset16<RoomPixels> foregroundView) =>
        new(foregroundView.X, foregroundView.Y);
}
