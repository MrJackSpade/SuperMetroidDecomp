namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>How each kind of Samus's particles moves and is placed (<see cref="SamusParticles"/>).</summary>
internal static partial class SamusParticleKindDefinitions
{
    /// <summary>
    /// Each kind: the diving splash is held on the water's surface; splashes, spray and dust are centred on the particle,
    /// while the diving splash and bubbles are placed from it.
    /// </summary>
    internal static readonly IdentityTable<SamusParticleKind, SamusParticleKindDefinition> Kinds =
        new(new Dictionary<SamusParticleKind, SamusParticleKindDefinition>
        {
            [SamusParticleKind.SurfaceSplash] = new(FollowsWaterSurface: false, Centred: true),
            [SamusParticleKind.DivingSplash] = new(FollowsWaterSurface: true, Centred: false),
            [SamusParticleKind.LavaSpray] = new(FollowsWaterSurface: false, Centred: true),
            [SamusParticleKind.Bubbles] = new(FollowsWaterSurface: false, Centred: false),
            [SamusParticleKind.Dust] = new(FollowsWaterSurface: false, Centred: true),
        });

    /// <summary>A splash on wet ground or shallow water, which stays put.</summary>
    internal static readonly SamusSteadyParticle SurfaceSplash = new(SamusParticleKind.SurfaceSplash, Offset16<RoomPixels>.Zero);

    /// <summary>The splash of a dive into water, which stays put on the water's surface.</summary>
    internal static readonly SamusSteadyParticle DivingSplash = new(SamusParticleKind.DivingSplash, Offset16<RoomPixels>.Zero);

    /// <summary>Breath bubbles, which stay put.</summary>
    internal static readonly SamusSteadyParticle Bubbles = new(SamusParticleKind.Bubbles, Offset16<RoomPixels>.Zero);

    /// <summary>Dust, which rises as it shows.</summary>
    internal static readonly SamusSteadyParticle Dust = new(SamusParticleKind.Dust, SamusAtmosphericAnimationDefinitions.DustDrift);
}
