namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where Mother Brain's body's area lies against her (#1949).</summary>
internal static class MotherBrainBodyAreaDefinitions
{
    /// <summary>
    /// The top-left corner of her body's area, from her position: 38 pixels left of and 63 above her. Each of her body's
    /// pictures lies on the area where the import places its frame, so it stays attached to her limbs wherever she moves.
    /// </summary>
    internal static readonly Offset16<RoomPixels> OffsetFromBody = new(-38, -63);
}
