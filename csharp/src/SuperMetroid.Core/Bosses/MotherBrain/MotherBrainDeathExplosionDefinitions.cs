using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Compiled animation selectors for Mother Brain's body-relative death explosions.</summary>
internal static class MotherBrainDeathExplosionDefinitions
{
    /// <summary>Seven four-anchor groups selected in reverse order by the death explosions.</summary>
    internal const int GroupCount = 7;

    /// <summary>Four interleaved X/Y pairs per group.</summary>
    internal const int AnchorsPerGroup = 4;

    /// <summary>
    /// Twenty-eight authored decorative positions read before RNG selects
    /// explosion type. The explosion's initializer adds these offsets to the moving body without
    /// velocity integration; the explosion projectile has zero hitbox radii and cannot
    /// damage Samus. Only these coordinates are chosen visual scatter, not the group
    /// indexing, cadence, types, or other offsets.
    /// </summary>
    private static readonly Offset16<RoomPixels>[] DecorativeAnchors =
    [
        new Offset16<RoomPixels>(36, -37), new Offset16<RoomPixels>(-19, -15), new Offset16<RoomPixels>(-4, 13), new Offset16<RoomPixels>(29, 25),
        new Offset16<RoomPixels>(17, -55), new Offset16<RoomPixels>(30, -22), new Offset16<RoomPixels>(-3, -5), new Offset16<RoomPixels>(0, 40),
        new Offset16<RoomPixels>(52, -34), new Offset16<RoomPixels>(-3, -15), new Offset16<RoomPixels>(12, 19), new Offset16<RoomPixels>(25, 44),
        new Offset16<RoomPixels>(4, -43), new Offset16<RoomPixels>(-12, -22), new Offset16<RoomPixels>(13, -2), new Offset16<RoomPixels>(-8, 52),
        new Offset16<RoomPixels>(-2, -33), new Offset16<RoomPixels>(10, -10), new Offset16<RoomPixels>(-14, 16), new Offset16<RoomPixels>(6, 59),
        new Offset16<RoomPixels>(20, -41), new Offset16<RoomPixels>(4, -22), new Offset16<RoomPixels>(-20, 3), new Offset16<RoomPixels>(-27, 57),
        new Offset16<RoomPixels>(10, -31), new Offset16<RoomPixels>(-20, -8), new Offset16<RoomPixels>(0, 23), new Offset16<RoomPixels>(30, 61),
    ];

    /// <summary>Reads one of the 28 body-relative decorative offsets in group order.</summary>
    internal static Offset16<RoomPixels> Anchor(int index) => DecorativeAnchors[index];

    /// <summary>
    /// The shared misc-dust animation each of Mother Brain's death explosions shows: the small explosion, smoke and the big
    /// explosion.
    /// </summary>
    internal static readonly IReadOnlyDictionary<MotherBrainDeathExplosionAnimation, MiscDustProjectileSegment> Animations =
        new Dictionary<MotherBrainDeathExplosionAnimation, MiscDustProjectileSegment>
        {
            [MotherBrainDeathExplosionAnimation.SmallExplosion] = MiscDustProjectileSegment.SmallExplosion,
            [MotherBrainDeathExplosionAnimation.Smoke] = MiscDustProjectileSegment.Smoke,
            [MotherBrainDeathExplosionAnimation.BigExplosion] = MiscDustProjectileSegment.BigExplosion,
        };
}
