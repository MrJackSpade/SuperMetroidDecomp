namespace SuperMetroid.Core.Game;

/// <summary>Compiled instruction selectors for Mother Brain's body-relative death explosions.</summary>
internal static class MotherBrainDeathExplosionDefinitions
{
    /// <summary>Seven four-anchor groups selected in reverse order by $A9:B03E.</summary>
    internal const int GroupCount = 7;

    /// <summary>Four interleaved X/Y pairs per group at $A9:B099-$B108.</summary>
    internal const int AnchorsPerGroup = 4;

    // Permitted nonsense exception: these 28 positions are the decorative scatter itself.
    // $A9:B03E reads each pair before RNG selects only the explosion type. $86:C8F5/$C914
    // stores and adds the offsets to the moving body without velocity integration. The
    // $86:CB13 projectile has zero hitbox radii and damage-disabled properties. Inventing
    // a geometric distribution would replace the chosen visual layout. This exception
    // covers only these coordinates, not group indexing, cadence, types or other offsets.
    /// <summary>Four decorative body-relative X/Y offsets for each of the seven explosion groups.</summary>
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

    /// <summary>Returns the stored decorative offset at the supplied position in the flattened anchor table.</summary>
    /// <param name="index">Zero-based anchor position, from 0 through 27.</param>
    internal static (short X, short Y) Anchor(int index) => DecorativeAnchors[index];

    /// <summary>
    /// Three instruction-list identities at <c>$86:C929-$86:C92E</c>: small explosion,
    /// smoke, and big explosion, selected by projectile parameter zero through two.
    /// </summary>
    internal static ushort InstructionList(ushort parameter) => parameter switch
    {
        0 => EnemyProjectileInstructionMechanicsDefinitions.MotherBrainSmallDeathExplosionInitial,
        1 => EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDeathSmokeInitial,
        2 => EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBigDeathExplosionInitial,
        _ => throw new ArgumentOutOfRangeException(
            nameof(parameter), parameter, "Mother Brain death explosion parameter must be zero through two."),
    };
}
