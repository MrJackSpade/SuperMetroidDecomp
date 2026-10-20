namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A falling Mother Brain tube piece's collision radii and the floor it lands on.</summary>
/// <param name="Radius">The piece's collision radii.</param>
/// <param name="Floor">The landing floor's Y position.</param>
internal readonly record struct MotherBrainFallingTubeDefinition(Extent16<RoomPixels> Radius, ushort Floor);
