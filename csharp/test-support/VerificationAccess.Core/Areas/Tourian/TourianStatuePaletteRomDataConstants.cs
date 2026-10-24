
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Cartridge values and helpers of <see cref="TourianStatuePaletteRomData"/> that only verification reads.</summary>
internal static class TourianStatuePaletteRomDataConstants
{
    /// <summary>First CGRAM color-entry index of the base-decoration band: 240, OBJ palette 7 color zero; native initialization targets <c>TargetPalettes_SpriteP7</c>.</summary>
    public const int BaseCgramIndex = 240;
    /// <summary>First CGRAM color-entry index of the statue band: 160, OBJ palette 2 color zero; native initialization targets <c>TargetPalettes_SpriteP2</c>.</summary>
    public const int StatueCgramIndex = 160;
    /// <summary>First CGRAM color-entry index of the shared eye-glow band: 249, OBJ palette 7 color 9; the selected row replaces entries 249..252.</summary>
    public const int EyeCgramIndex = 249;
}
