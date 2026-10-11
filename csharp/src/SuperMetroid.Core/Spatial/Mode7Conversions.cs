namespace SuperMetroid.Core.Spatial;

/// <summary>Names the Mode 7 register pairs programmed from a layer-one room position.</summary>
public static class Mode7Conversions
{
    /// <summary>
    /// The M7X/M7Y (or M7HOFS/M7VOFS) words of a room point when the Mode 7 plane is layer one,
    /// as in the Ceres elevator shaft. The words are unchanged; the registers read them as signed.
    /// </summary>
    public static Offset16<Mode7Pixels> AsMode7Register(this Position16<RoomPixels> layer1) =>
        new(unchecked((short)layer1.X), unchecked((short)layer1.Y));
}
