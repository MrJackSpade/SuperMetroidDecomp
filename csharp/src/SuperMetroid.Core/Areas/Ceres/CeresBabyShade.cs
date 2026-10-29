namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The shade the Ceres Baby Metroid glows with: resting, or brightening with each squish pose of its expressive loop. Each
/// shade has its own installed colours.
/// </summary>
public enum CeresBabyShade
{
    /// <summary>The Baby's resting colours, restored when the expressive loop ends.</summary>
    Resting,
    /// <summary>The colours shown with the horizontal squish.</summary>
    HorizontalSquish,
    /// <summary>The colours shown with the round pose.</summary>
    Round,
    /// <summary>The colours shown with the vertical squish, the brightest.</summary>
    VerticalSquish,
}
