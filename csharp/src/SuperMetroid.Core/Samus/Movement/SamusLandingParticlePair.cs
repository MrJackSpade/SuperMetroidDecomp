namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Where a landing's two particles start: the right one's place from Samus, and how far left of it the other lies.</summary>
/// <param name="Right">The right particle's place from Samus's centre and last occupied pixel.</param>
/// <param name="Separation">The pixels left of the right particle at which the left one starts, at the same height.</param>
internal readonly partial record struct SamusLandingParticlePair(Offset16<RoomPixels> Right, int Separation);
