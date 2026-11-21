namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// How one kind of Samus's particles is placed (<see cref="SamusParticleKindDefinitions.Kinds"/>); how it drifts is the
/// steady particle's (<see cref="SamusSteadyParticle"/>), or lava spray's own (<see cref="SamusParticles.SpawnLavaSpray"/>).
/// </summary>
/// <param name="FollowsWaterSurface">Whether the particle is held on the water's surface line on each update it shows.</param>
/// <param name="Centred">Whether its eight-pixel picture is centred on the particle rather than placed from it.</param>
[IndependentFlag("independent facts of one particle kind: whether it follows the water surface and whether it is centred")]
internal sealed record SamusParticleKindDefinition(
    bool FollowsWaterSurface,
    bool Centred);
