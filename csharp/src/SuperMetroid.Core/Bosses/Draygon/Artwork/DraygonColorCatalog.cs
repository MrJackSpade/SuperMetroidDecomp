using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Draygon's editable RGB5 opening, normal, flash, and health-band colors. Health
/// thresholds, hit reactions, and flash timing remain cartridge-mechanics code.
/// </summary>
public sealed class DraygonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => DraygonPalette.ContentIdentity(this);

    /// <summary><c>Palette_Draygon_WhiteFlash</c> at $A5:A297: every visible flash ink is white.</summary>
    internal static readonly RgbColor FlashInk = new(31, 31, 31);

    /// <summary>The selected opening, material, flash and health-band colours.</summary>
    internal readonly DraygonColorSelection selection;

    /// <summary>Retains validated colors and stores only flash entries that differ from stock white.</summary>
    internal DraygonColorCatalog(RgbColor[] intro, RgbColor[] background, RgbColor[] sprite,
        RgbColor[] whiteFlash, RgbColor[][] healthBands) =>
        selection = new(new DraygonIntroPaint(intro), new DraygonMaterialPaint(background), new DraygonMaterialPaint(sprite),
            DraygonPalette.WhiteFlashEdits(whiteFlash), new DraygonHealthPaint(healthBands));

    /// <summary>Gets one of the 25 selected RGB5 colors used by Draygon's opening palette.</summary>
    public RgbColor ResolveIntro(int color) => selection.Intro.Color(color);
    /// <summary>Gets one of the 16 selected normal background colors.</summary>
    public RgbColor ResolveBackground(int color) => selection.Background.Color(color);
    /// <summary>Gets one of the 16 selected normal sprite colors.</summary>
    public RgbColor ResolveSprite(int color) => selection.Sprite.Color(color);
    /// <summary>Gets one of the 16 selected white-flash colors, preserving transparent entry zero.</summary>
    public RgbColor ResolveWhiteFlash(int color) => DraygonPalette.WhiteFlash(selection.WhiteFlash, color);

    /// <summary>Gets one of the four selected colors in an eight-row health palette table.</summary>
    public RgbColor ResolveHealthBand(int band, int color) => DraygonPalette.HealthBand(selection.HealthBands, band, color);

    /// <summary>Installs the selected opening colors at their native CGRAM destination.</summary>
    public void ApplyIntro(GameplayColours colours) => DraygonPalette.ApplyIntro(this, colours);

    /// <summary>Installs Draygon's normal or white-flash material colors and the active health band.</summary>
    public void ApplyHurt(GameplayColours colours, bool whiteFrame, int healthBand) =>
        DraygonPalette.ApplyHurt(this, colours, whiteFrame, healthBand);

    /// <summary>Installs the four-color health band 0-7.</summary>
    public void ApplyHealthBand(GameplayColours colours, int band) => DraygonPalette.ApplyHealthBand(this, colours, band);
}
