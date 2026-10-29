using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The Ceres Baby's installed colours: fifteen colours for each shade it glows with.</summary>
internal sealed record CeresBabyShadeColours
{
    /// <summary>The resting colours.</summary>
    public required RgbColor[] Resting { get; init; }

    /// <summary>The colours shown with the horizontal squish.</summary>
    public required RgbColor[] HorizontalSquish { get; init; }

    /// <summary>The colours shown with the round pose.</summary>
    public required RgbColor[] Round { get; init; }

    /// <summary>The colours shown with the vertical squish.</summary>
    public required RgbColor[] VerticalSquish { get; init; }

    /// <summary>The colours of <paramref name="shade"/>.</summary>
    internal RgbColor[] Of(CeresBabyShade shade) => shade switch
    {
        CeresBabyShade.Resting => Resting,
        CeresBabyShade.HorizontalSquish => HorizontalSquish,
        CeresBabyShade.Round => Round,
        CeresBabyShade.VerticalSquish => VerticalSquish,
        _ => throw new ArgumentOutOfRangeException(nameof(shade), shade, "Undefined Ceres Baby shade."),
    };
}
