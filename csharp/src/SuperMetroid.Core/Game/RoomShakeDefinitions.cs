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
