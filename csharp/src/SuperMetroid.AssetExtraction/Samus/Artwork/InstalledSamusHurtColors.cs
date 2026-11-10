using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>The installed hurt-flash and cinematic-restoration colours, both sixteen-colour sets kept as installed.</summary>
public sealed class InstalledSamusHurtColors
{
    /// <summary>The installed hurt-flash colours.</summary>
    private readonly RgbColor[] hurt;
    /// <summary>The installed cinematic-restoration colours.</summary>
    private readonly RgbColor[] intro;

    /// <summary>Keeps both complete installed colour sets.</summary>
    /// <param name="hurt">The hurt flash's <see cref="SamusHurtColorFormat.ColorsPerPalette"/> colours.</param>
    /// <param name="intro">The cinematic restoration's <see cref="SamusHurtColorFormat.ColorsPerPalette"/> colours.</param>
    /// <exception cref="InvalidDataException">Either set is missing a colour.</exception>
    internal InstalledSamusHurtColors(RgbColor[] hurt, RgbColor[] intro)
    {
        this.hurt = Complete(hurt, "hurt");
        this.intro = Complete(intro, "intro");
    }

    /// <summary>Returns one installed display colour; counter, phase, and sound rules remain in code.</summary>
    /// <param name="variant">Flash or cinematic restoration palette; this method does not select the palette from gameplay state.</param>
    /// <param name="index">Zero-based colour index, from zero through 15.</param>
    /// <returns>The installed colour.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="variant"/> is undefined or <paramref name="index"/> is outside the sixteen-color palette.</exception>
    public RgbColor Resolve(SamusHurtColorVariant variant, int index)
    {
        RgbColor[] colours = variant switch
        {
            SamusHurtColorVariant.Hurt => hurt,
            SamusHurtColorVariant.Intro => intro,
            _ => throw new ArgumentOutOfRangeException(nameof(variant)),
        };
        if ((uint)index >= colours.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colours[index];
    }

    /// <summary>Copies <paramref name="colours"/>, rejecting a set without every colour.</summary>
    private static RgbColor[] Complete(RgbColor[] colours, string name)
    {
        if (colours.Length != SamusHurtColorFormat.ColorsPerPalette)
            throw new InvalidDataException($"Samus {name} palette requires {SamusHurtColorFormat.ColorsPerPalette} colors.");
        return (RgbColor[])colours.Clone();
    }
}
