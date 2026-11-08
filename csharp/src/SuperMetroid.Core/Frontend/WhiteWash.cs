using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The white wash the frontend adds over a whole screen and fades out a step at a time (#1544): the rear-view flash of the
/// opening's Ceres flight, the ending's flyaway sky and the white flash into the post-credits logo each start at
/// <see cref="Steps"/> steps, full white, and lose one step an update or an interval; the wash shows white scaled to the
/// steps left.
/// </summary>
internal static class WhiteWash
{
    /// <summary>The steps of a white wash from full white to none.</summary>
    internal const int Steps = 31;

    /// <summary>The colour of a wash with <paramref name="stepsLeft"/> of its <see cref="Steps"/> steps left.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stepsLeft"/> is outside 0-<see cref="Steps"/>.</exception>
    internal static RgbColor Colour(int stepsLeft) => RgbColor.White.Scaled(stepsLeft, Steps);

    /// <summary>The screen tint of a wash with <paramref name="stepsLeft"/> of its <see cref="Steps"/> steps left.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stepsLeft"/> is outside 0-<see cref="Steps"/>.</exception>
    internal static ScreenTintLayer Layer(int stepsLeft) => new(Colour(stepsLeft));
}
