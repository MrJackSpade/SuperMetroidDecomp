using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening narration's full, editable screen colours (#1464): its eight background sets and eight sprite sets of
/// sixteen colours, kept as installed.
/// </summary>
public sealed class IntroCinematicPalette
{
    /// <summary>Retains the installed colours: the background sets, then the sprite sets, in equal halves.</summary>
    /// <param name="image">Every installed colour, background sets first.</param>
    internal IntroCinematicPalette(RgbColor[] image)
    {
        int ownerColours = image.Length / 2;
        BackgroundColours = ImmutableArray.Create(image, 0, ownerColours);
        SpriteColours = ImmutableArray.Create(image, ownerColours, ownerColours);
    }

    /// <summary>The installed background colours, eight sets of sixteen in order (#1512).</summary>
    public ImmutableArray<RgbColor> BackgroundColours { get; }

    /// <summary>The installed sprite colours, eight sets of sixteen in order (#1512).</summary>
    public ImmutableArray<RgbColor> SpriteColours { get; }

    /// <summary>The colour the opening shows where nothing is drawn: its first background colour (#1512).</summary>
    public RgbColor Backdrop => BackgroundColours[0];
}
