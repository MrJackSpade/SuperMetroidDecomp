namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>The flare offset and beam start of one saved.</summary>
/// <param name="FlareOffset">The flare offset.</param>
/// <param name="BeamStart">The beam start.</param>
internal readonly record struct Saved(Offset16<RoomPixels> FlareOffset, Position16<RoomPixels> BeamStart);
