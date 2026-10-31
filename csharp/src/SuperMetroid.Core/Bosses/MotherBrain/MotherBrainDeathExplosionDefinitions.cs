using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Compiled instruction selectors for Mother Brain's body-relative death explosions.</summary>
internal static class MotherBrainDeathExplosionDefinitions
{
    /// <summary>Seven four-anchor groups selected in reverse order by $A9:B03E.</summary>
    internal const int GroupCount = 7;

    /// <summary>Four interleaved X/Y pairs per group at $A9:B099-$B108.</summary>
    internal const int AnchorsPerGroup = 4;

    /// <summary>
    /// Twenty-eight authored decorative positions read at $A9:B03E before RNG selects
    /// explosion type. $86:C8F5/$C914 adds these offsets to the moving body without
    /// velocity integration; the $86:CB13 projectile has zero hitbox radii and cannot
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

    /// <summary>Reads one of the 28 body-relative decorative offsets in native group order.</summary>
    internal static Offset16<RoomPixels> Anchor(int index) => DecorativeAnchors[index];

    /// <summary>
    /// The three programs the native table at <c>$86:C929-$86:C92E</c> selects by projectile parameter zero through two:
    /// the shared misc-dust small explosion, smoke and big explosion.
    /// </summary>
    internal static readonly MiscDustProjectileProgram[] Programs =
        [MiscDustProjectileProgram.SmallExplosion, MiscDustProjectileProgram.Smoke, MiscDustProjectileProgram.BigExplosion];
}
