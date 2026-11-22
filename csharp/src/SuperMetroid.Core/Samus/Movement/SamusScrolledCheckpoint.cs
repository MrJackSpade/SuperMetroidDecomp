namespace SuperMetroid.Core.Samus.Movement;

/// <summary>A scrolling pass has run since the camera was placed, ending with Samus at <paramref name="Point"/>.</summary>
/// <param name="Point">Samus's point as the last scrolling pass ended.</param>
public sealed partial record SamusScrolledCheckpoint(FixedPosition<RoomPixels> Point) : SamusScrollCheckpoint;
