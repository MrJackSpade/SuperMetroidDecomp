namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>How one kind of Samus's particles moves and is placed (<see cref="SamusParticleKindDefinitions.Kinds"/>).</summary>
/// <param name="Drift">The pixels the particle moves by on each update it shows, for a kind that does not drift to a side.</param>
/// <param name="DriftsToOneSide">Whether the particle drifts to one side, a drift its spawn gives it, rather than by <paramref name="Drift"/>.</param>
/// <param name="FollowsWaterSurface">Whether the particle is held on the water's surface line on each update it shows.</param>
/// <param name="Centred">Whether its eight-pixel picture is centred on the particle rather than placed from it.</param>
[IndependentFlag("independent facts of one particle kind: whether it drifts to a side, follows the water surface and is centred")]
internal sealed record SamusParticleKindDefinition(
    Offset16<RoomPixels> Drift,
    bool DriftsToOneSide,
    bool FollowsWaterSurface,
    bool Centred);
