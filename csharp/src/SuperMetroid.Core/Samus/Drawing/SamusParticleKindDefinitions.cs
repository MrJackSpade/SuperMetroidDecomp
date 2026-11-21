namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>How each kind of Samus's particles moves and is placed (<see cref="SamusParticles"/>).</summary>
internal static class SamusParticleKindDefinitions
{
    /// <summary>
    /// Each kind: dust rises as it shows, lava spray rises and drifts to the side its spawn gives, and the other kinds stay
    /// put; the diving splash is held on the water's surface; splashes, spray and dust are centred on the particle, while
    /// the diving splash and bubbles are placed from it.
    /// </summary>
    internal static readonly IdentityTable<SamusParticleKind, SamusParticleKindDefinition> Kinds =
        new(new Dictionary<SamusParticleKind, SamusParticleKindDefinition>
        {
            [SamusParticleKind.SurfaceSplash] = new(Offset16<RoomPixels>.Zero, FollowsWaterSurface: false, Centred: true),
            [SamusParticleKind.DivingSplash] = new(Offset16<RoomPixels>.Zero, FollowsWaterSurface: true, Centred: false),
            [SamusParticleKind.LavaSpray] = new(Drift: null, FollowsWaterSurface: false, Centred: true),
            [SamusParticleKind.Bubbles] = new(Offset16<RoomPixels>.Zero, FollowsWaterSurface: false, Centred: false),
            [SamusParticleKind.Dust] = new(SamusAtmosphericAnimationDefinitions.DustDrift, FollowsWaterSurface: false, Centred: true),
        });
}
