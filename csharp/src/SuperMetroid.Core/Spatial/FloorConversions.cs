namespace SuperMetroid.Core.Spatial;

/// <summary>Conversions from room and floor positions to the signed floor offsets a floor transform takes.</summary>
public static class FloorConversions
{
    /// <summary>
    /// A room point as a signed floor offset when the turning floor is layer one, as in the Ceres elevator shaft.
    /// The words are unchanged; the floor transform reads them as signed.
    /// </summary>
    public static Offset16<FloorPixels> AsFloorWord(this Position16<RoomPixels> layer1) =>
        new(unchecked((short)layer1.X), unchecked((short)layer1.Y));

    /// <summary>
    /// A floor position as the signed offset a floor transform takes for its scroll or centre; the words are
    /// unchanged.
    /// </summary>
    public static Offset16<FloorPixels> AsSignedWord(this Position16<FloorPixels> position) =>
        new(unchecked((short)position.X), unchecked((short)position.Y));
}
