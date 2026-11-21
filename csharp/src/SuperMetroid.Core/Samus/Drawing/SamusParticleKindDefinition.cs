namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>How one kind of Samus's particles moves and is placed (<see cref="SamusParticleKindDefinitions.Kinds"/>).</summary>
/// <param name="Drift">
/// The pixels the particle moves by on each update it shows, or none for a kind that drifts to one side, a drift its spawn
/// gives it.
/// </param>
/// <param name="FollowsWaterSurface">Whether the particle is held on the water's surface line on each update it shows.</param>
/// <param name="Centred">Whether its eight-pixel picture is centred on the particle rather than placed from it.</param>
[IndependentFlag("independent facts of one particle kind: whether it follows the water surface and whether it is centred")]
internal sealed record SamusParticleKindDefinition(
    Offset16<RoomPixels>? Drift,
    bool FollowsWaterSurface,
    bool Centred);
