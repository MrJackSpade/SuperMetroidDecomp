using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;

/// <summary>
/// Draygon's installed RGB5 opening, normal, flash, and health-band colors, each set kept as installed. Health
/// thresholds, hit reactions, and flash timing remain mechanics code.
/// </summary>
public sealed class DraygonColorCatalog
{
    /// <summary>The installed opening colours.</summary>
    private readonly InstalledColourRows intro;
    /// <summary>The installed normal background colours.</summary>
    private readonly InstalledColourRows background;
    /// <summary>The installed normal sprite colours.</summary>
    private readonly InstalledColourRows sprite;
    /// <summary>The installed white-flash colours.</summary>
    private readonly InstalledColourRows whiteFlash;
    /// <summary>The installed health bands' colours.</summary>
    private readonly InstalledColourRows healthBands;

    /// <summary>Keeps every complete installed colour set.</summary>
    /// <param name="intro">The opening's <see cref="DraygonColorDefinitions.IntroCount"/> colours.</param>
    /// <param name="background">The background's <see cref="DraygonColorDefinitions.BackgroundCount"/> colours.</param>
    /// <param name="sprite">The sprite's <see cref="DraygonColorDefinitions.SpriteCount"/> colours.</param>
    /// <param name="whiteFlash">The white flash's <see cref="DraygonColorDefinitions.WhiteFlashCount"/> colours.</param>
    /// <param name="healthBands">Each health band's <see cref="DraygonColorDefinitions.HealthBandColorCount"/> colours.</param>
    /// <exception cref="InvalidDataException">A set is missing a band or a colour.</exception>
    internal DraygonColorCatalog(RgbColor[] intro, RgbColor[] background, RgbColor[] sprite, RgbColor[] whiteFlash,
        RgbColor[][] healthBands)
    {
        this.intro = new InstalledColourRows(intro, DraygonColorDefinitions.IntroCount, "Draygon intro");
        this.background = new InstalledColourRows(background, DraygonColorDefinitions.BackgroundCount, "Draygon background");
        this.sprite = new InstalledColourRows(sprite, DraygonColorDefinitions.SpriteCount, "Draygon sprite");
        this.whiteFlash = new InstalledColourRows(whiteFlash, DraygonColorDefinitions.WhiteFlashCount, "Draygon white flash");
        this.healthBands = new InstalledColourRows(healthBands, DraygonColorDefinitions.HealthBandCount,
            DraygonColorDefinitions.HealthBandColorCount, "Draygon health bands");
    }

    /// <summary>Gets one of the 25 installed colours of Draygon's opening palette.</summary>
    public RgbColor ResolveIntro(int color) => intro.Colour(color);
    /// <summary>Gets one of the 16 installed normal background colors.</summary>
    public RgbColor ResolveBackground(int color) => background.Colour(color);
    /// <summary>Gets one of the 16 installed normal sprite colors.</summary>
    public RgbColor ResolveSprite(int color) => sprite.Colour(color);
    /// <summary>Gets one of the 16 installed white-flash colors, including transparent entry zero.</summary>
    public RgbColor ResolveWhiteFlash(int color) => whiteFlash.Colour(color);

    /// <summary>Gets one of the four installed colors of one of Draygon's eight health bands.</summary>
    public RgbColor ResolveHealthBand(int band, int color) => healthBands.Colour(band, color);
}
