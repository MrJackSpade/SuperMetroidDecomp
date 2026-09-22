namespace SuperMetroid.Core.Game;

/// <summary>One cartridge earthquake type's background and enemy-projectile displacement.</summary>
internal readonly record struct RoomShakeDefinition(
    short Bg1X,
    short Bg1Y,
    short Bg2X,
    short Bg2Y,
    short ProjectileX,
    short ProjectileY);

/// <summary>
/// The 36 earthquake definitions paired across <c>$A0:872D-$A0:884C</c> and
/// <c>$86:846B-$86:84FA</c>.
/// </summary>
internal static class RoomShakeDefinitions
{
    /// <summary>Background and projectile displacement pairs for all 36 rendered earthquake types.</summary>
    /// <remarks>
    /// #625 exact factorization: for type t=0..35, group=t/9, magnitude=(t/3)%3+1,
    /// direction=t%3; X is zero only for direction 1, Y only for direction 0,
    /// and each nonzero component equals magnitude. Enable this pair for BG1 when
    /// group!=3, BG2 when group!=0, and projectiles when group&gt;=2; otherwise use (0,0).
    /// All divisions are integer. This is a Cartesian product of four recipient groups,
    /// three strengths, and three directions, not 36 independently tuned shakes.
    /// LookupTableResearch checks all 216 words against both pinned assembly tables,
    /// NTSC J/U v1.0 ROM, and these compiled records. No rounding or entry patches are needed.
    /// Keep t bounded and preserve caller-controlled sign alternation; runtime migration is deferred.
    /// </remarks>
    private static readonly RoomShakeDefinition[] Definitions =
    [
        new(1, 0, 0, 0, 0, 0),
        new(0, 1, 0, 0, 0, 0),
        new(1, 1, 0, 0, 0, 0),
        new(2, 0, 0, 0, 0, 0),
        new(0, 2, 0, 0, 0, 0),
        new(2, 2, 0, 0, 0, 0),
        new(3, 0, 0, 0, 0, 0),
        new(0, 3, 0, 0, 0, 0),
        new(3, 3, 0, 0, 0, 0),

        new(1, 0, 1, 0, 0, 0),
        new(0, 1, 0, 1, 0, 0),
        new(1, 1, 1, 1, 0, 0),
        new(2, 0, 2, 0, 0, 0),
        new(0, 2, 0, 2, 0, 0),
        new(2, 2, 2, 2, 0, 0),
        new(3, 0, 3, 0, 0, 0),
        new(0, 3, 0, 3, 0, 0),
        new(3, 3, 3, 3, 0, 0),

        new(1, 0, 1, 0, 1, 0),
        new(0, 1, 0, 1, 0, 1),
        new(1, 1, 1, 1, 1, 1),
        new(2, 0, 2, 0, 2, 0),
        new(0, 2, 0, 2, 0, 2),
        new(2, 2, 2, 2, 2, 2),
        new(3, 0, 3, 0, 3, 0),
        new(0, 3, 0, 3, 0, 3),
        new(3, 3, 3, 3, 3, 3),

        new(0, 0, 1, 0, 1, 0),
        new(0, 0, 0, 1, 0, 1),
        new(0, 0, 1, 1, 1, 1),
        new(0, 0, 2, 0, 2, 0),
        new(0, 0, 0, 2, 0, 2),
        new(0, 0, 2, 2, 2, 2),
        new(0, 0, 3, 0, 3, 0),
        new(0, 0, 0, 3, 0, 3),
        new(0, 0, 3, 3, 3, 3),
    ];

    /// <summary>Returns the physical displacement record for an authored earthquake type.</summary>
    internal static RoomShakeDefinition ForType(ushort earthquakeType)
    {
        if (earthquakeType >= Definitions.Length)
        {
            throw new InvalidDataException(
                $"Earthquake type ${earthquakeType:X4} exceeds the 36 rendered definitions.");
        }

        return Definitions[earthquakeType];
    }
}
