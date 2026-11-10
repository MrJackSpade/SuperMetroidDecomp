namespace SuperMetroid.Core.Fx;

/// <summary>
/// The cartridge's earthquake type numbers and displacement tables, which only checks read: they compare the
/// displacement of each <see cref="EarthquakeShake"/> with the table row its type number indexes, and name the shakes the
/// cartridge's producers write by number.
/// </summary>
internal static class NativeEarthquakeTypes
{
    /// <summary>The earthquake type numbers the cartridge shows: 0 through 35.</summary>
    public const int ShownTypeCount = 36;

    /// <summary>$A0:872D BGShakeDisplacements.BG1X/BG1Y, at eight-byte record stride.</summary>
    public const int Bg1ReferenceAddress = 0xa0872d;

    /// <summary>$A0:8731 BGShakeDisplacements.BG2X/BG2Y, at eight-byte record stride.</summary>
    public const int Bg2ReferenceAddress = 0xa08731;

    /// <summary>$86:846B Get_Values_for_Screen_Shaking.horizontalX/Y, at four-byte stride.</summary>
    public const int ProjectileReferenceAddress = 0x86846b;

    /// <summary>
    /// The shake the cartridge's earthquake type <paramref name="type"/> shows. The type numbers run through every
    /// reach, then strength, then direction: reach = type / 9, strength = type / 3 % 3 and direction = type % 3.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not a shown type.</exception>
    public static EarthquakeShake Shake(int type)
    {
        if (type is < 0 or >= ShownTypeCount)
            throw new ArgumentOutOfRangeException(nameof(type), type, "The cartridge shows earthquake types 0 through 35.");
        return new(Strengths[type / 3 % 3], Directions[type % 3], Reaches[type / 9]);
    }

    /// <summary>The cartridge's earthquake type number for <paramref name="shake"/>, the inverse of <see cref="Shake"/>.</summary>
    public static int TypeOf(EarthquakeShake shake) =>
        Array.IndexOf(Reaches, shake.Reach) * 9 + Array.IndexOf(Strengths, shake.Strength) * 3 +
        Array.IndexOf(Directions, shake.Direction);

    /// <summary>The reaches in the order the type numbers run through them.</summary>
    private static readonly EarthquakeReach[] Reaches =
    [
        EarthquakeReach.Foreground, EarthquakeReach.ForegroundAndBackground,
        EarthquakeReach.ForegroundBackgroundAndEnemies, EarthquakeReach.BackgroundAndEnemies,
    ];

    /// <summary>The strengths in the order the type numbers run through them.</summary>
    private static readonly EarthquakeStrength[] Strengths = [EarthquakeStrength.Light, EarthquakeStrength.Medium, EarthquakeStrength.Strong];

    /// <summary>The directions in the order the type numbers run through them.</summary>
    private static readonly EarthquakeDirection[] Directions =
        [EarthquakeDirection.Horizontal, EarthquakeDirection.Vertical, EarthquakeDirection.Diagonal];
}
