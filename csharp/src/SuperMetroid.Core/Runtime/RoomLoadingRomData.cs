using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

/// <summary>Cartridge addresses and fixed destinations consumed by shared room loading.</summary>
internal static class RoomLoadingRomData
{

    /// <summary>CGRAM color index receiving the common gameplay OBJ palette.</summary>
    public const int CommonGameplaySpritePaletteCgramIndex = 128;

    /// <summary>CGRAM color index receiving the initial enemy-projectile palette.</summary>
    public const int InitialEnemyProjectilePaletteCgramIndex =
        GameplayBasePaletteFormat.EnemyProjectileInitialColor;

    /// <summary>Power Suit load-appearance palette-FX definition at $8D:E1F4.</summary>
    public const ushort PowerSuitLoadPaletteFx = 0xe1f4;

    /// <summary>Varia Suit load-appearance palette-FX definition at $8D:E1F8.</summary>
    public const ushort VariaSuitLoadPaletteFx = 0xe1f8;

    /// <summary>Gravity Suit load-appearance palette-FX definition at $8D:E1FC.</summary>
    public const ushort GravitySuitLoadPaletteFx = 0xe1fc;

    /// <summary>Native duration of Samus's saved-game appearance handler.</summary>
    public const ushort SavedGameAppearanceFrameCount = 0x0168;
}
