using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The white wash the frontend adds over a whole screen and fades out a step at a time (#1544): the rear-view flash of the
/// opening's Ceres flight, the ending's flyaway sky and the white flash into the post-credits logo each start at
/// <see cref="Steps"/> steps, full white, and lose one step an update or an interval. Each step is one of the game's fade
/// levels (<see cref="FadeLevelColour"/>): the wash shows the grey whose every level is the steps left.
/// </summary>
internal static class WhiteWash
{
    /// <summary>The steps of a white wash from full white to none: one a fade level.</summary>
    internal const int Steps = FadeLevelColour.FullLevel;

    /// <summary>The colour of a wash with <paramref name="stepsLeft"/> of its <see cref="Steps"/> steps left.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stepsLeft"/> is outside 0-<see cref="Steps"/>.</exception>
    internal static RgbColor Colour(int stepsLeft) => new FadeLevelColour(stepsLeft, stepsLeft, stepsLeft).ToColour();

    /// <summary>The screen tint of a wash with <paramref name="stepsLeft"/> of its <see cref="Steps"/> steps left.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stepsLeft"/> is outside 0-<see cref="Steps"/>.</exception>
    internal static ScreenTintLayer Layer(int stepsLeft) => new(Colour(stepsLeft));
}
