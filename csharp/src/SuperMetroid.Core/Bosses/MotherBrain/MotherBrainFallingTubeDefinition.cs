namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A falling Mother Brain tube piece's collision radii and the floor it lands on.</summary>
/// <param name="Radius">The piece's collision radii.</param>
/// <param name="Floor">The landing floor's Y position.</param>
/// <param name="FallWaitUpdates">
/// The updates the piece waits before it falls, the last being the update it starts to fall on; none for a piece that falls
/// from its first update.
/// </param>
/// <param name="KeepsUpdatingOffScreen">Whether the piece keeps updating while it is off screen.</param>
[IndependentFlag("whether a falling tube piece keeps updating off screen is a fact of that piece, independent of its other values")]
internal readonly partial record struct MotherBrainFallingTubeDefinition(Extent16<RoomPixels> Radius, int Floor, int? FallWaitUpdates = null,
    bool KeepsUpdatingOffScreen = false);
