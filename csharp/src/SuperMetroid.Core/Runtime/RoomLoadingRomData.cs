using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

/// <summary>Cartridge addresses and fixed destinations consumed by shared room loading.</summary>
internal static class RoomLoadingRomData
{
    /// <summary>Common gameplay OBJ palette at $9A:FC00.</summary>
    public const int CommonGameplaySpritePalette = GameplayBasePaletteFormat.CommonSpriteSourceAddress;

    /// <summary>CGRAM color index receiving the common gameplay OBJ palette.</summary>
    public const int CommonGameplaySpritePaletteCgramIndex = 128;

    /// <summary>Initial enemy-projectile OBJ palette at $9A:81A0.</summary>
    public const int InitialEnemyProjectilePalette = GameplayBasePaletteFormat.InitialSourceAddress +
        GameplayBasePaletteFormat.EnemyProjectileInitialColor * sizeof(ushort);

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
