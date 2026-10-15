using SuperMetroid.Core.Enemies.Projectiles;

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
    private static readonly (short X, short Y)[] DecorativeAnchors =
    [
        (0x0024, -0x0025), (-0x0013, -0x000f), (-0x0004, 0x000d), (0x001d, 0x0019),
        (0x0011, -0x0037), (0x001e, -0x0016), (-0x0003, -0x0005), (0x0000, 0x0028),
        (0x0034, -0x0022), (-0x0003, -0x000f), (0x000c, 0x0013), (0x0019, 0x002c),
        (0x0004, -0x002b), (-0x000c, -0x0016), (0x000d, -0x0002), (-0x0008, 0x0034),
        (-0x0002, -0x0021), (0x000a, -0x000a), (-0x000e, 0x0010), (0x0006, 0x003b),
        (0x0014, -0x0029), (0x0004, -0x0016), (-0x0014, 0x0003), (-0x001b, 0x0039),
        (0x000a, -0x001f), (-0x0014, -0x0008), (0x0000, 0x0017), (0x001e, 0x003d),
    ];

    /// <summary>Reads one of the 28 body-relative decorative offsets in native group order.</summary>
    internal static (short X, short Y) Anchor(int index) => DecorativeAnchors[index];

    /// <summary>
    /// The three programs the native table at <c>$86:C929-$86:C92E</c> selects by projectile parameter zero through two:
    /// the shared misc-dust small explosion, smoke and big explosion.
    /// </summary>
    internal static MiscDustProjectileProgram Program(ushort parameter) => parameter switch
    {
        0 => MiscDustProjectileProgram.SmallExplosion,
        1 => MiscDustProjectileProgram.Smoke,
        2 => MiscDustProjectileProgram.BigExplosion,
        _ => throw new ArgumentOutOfRangeException(
            nameof(parameter), parameter, "Mother Brain death explosion parameter must be zero through two."),
    };
}
