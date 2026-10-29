using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>The Ceres Baby's installed colours, fifteen RGB5 colours for each shade it glows with, excluding the transparent colour.</summary>
public sealed record CeresBabyShadeColorsDocument
{
    /// <summary>The resting colours, restored when the expressive loop ends.</summary>
    public required PaletteRgb5[] Resting { get; init; }

    /// <summary>The colours shown with the horizontal squish.</summary>
    public required PaletteRgb5[] HorizontalSquish { get; init; }

    /// <summary>The colours shown with the round pose.</summary>
    public required PaletteRgb5[] Round { get; init; }

    /// <summary>The colours shown with the vertical squish, the brightest.</summary>
    public required PaletteRgb5[] VerticalSquish { get; init; }

    /// <summary>The colours of <paramref name="shade"/>.</summary>
    public PaletteRgb5[] Of(CeresBabyShade shade) => shade switch
    {
        CeresBabyShade.Resting => Resting,
        CeresBabyShade.HorizontalSquish => HorizontalSquish,
        CeresBabyShade.Round => Round,
        CeresBabyShade.VerticalSquish => VerticalSquish,
        _ => throw new ArgumentOutOfRangeException(nameof(shade), shade, "Undefined Ceres Baby shade."),
    };
}
