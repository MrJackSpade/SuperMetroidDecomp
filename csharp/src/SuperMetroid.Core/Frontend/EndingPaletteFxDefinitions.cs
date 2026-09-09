namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$8D palette objects spawned by the ending's bank-$8B callbacks.</summary>
internal static class EndingPaletteFxDefinitions
{
    /// <summary>$8D:E1C4, fade zoomed-out exploding Zebes, spawned by $8B:F284.</summary>
    public const ushort FadePlanet = 0xe1c4;
    /// <summary>$8D:E1C8, initial supernova color program, spawned by $8B:F2B7.</summary>
    public const ushort Supernova = 0xe1c8;
    /// <summary>$8D:E1CC, final supernova color program, spawned by $8B:F2FA.</summary>
    public const ushort SupernovaFinale = 0xe1cc;
    /// <summary>$8D:E1D0, explosion color program, spawned by $8B:F2B7.</summary>
    public const ushort Explosion = 0xe1d0;
    /// <summary>$8D:E1D8, exploding lava, spawned by $8B:D6D7.</summary>
    public const ushort Lava = 0xe1d8;
    /// <summary>$8D:E1DC, fading crust, spawned by $8B:D6D7.</summary>
    public const ushort Crust = 0xe1dc;
    /// <summary>$8D:E1E0, fading grey clouds, spawned by $8B:D731.</summary>
    public const ushort GreyClouds = 0xe1e0;
    /// <summary>$8D:E1E8, wide supernova background, spawned by $8B:F2B7.</summary>
    public const ushort WideExplosion = 0xe1e8;
    /// <summary>$8D:E1D4, planet afterglow, spawned by $8B:DBC4.</summary>
    public const ushort PlanetAfterglow = 0xe1d4;
    /// <summary>$8D:E1E4, gunship emerging from the explosion, spawned by $8B:DBC4.</summary>
    public const ushort GunshipEmergence = 0xe1e4;
}
