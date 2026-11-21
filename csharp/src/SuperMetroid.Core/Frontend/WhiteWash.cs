using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The white wash the frontend adds over a whole screen and fades out a step at a time (#1544): the rear-view flash of the
/// opening's Ceres flight, the ending's flyaway sky and the white flash into the post-credits logo each start at
/// <see cref="Steps"/> steps, full white, and lose one step an update or an interval, at the wash's own cadence
/// (<see cref="PacedFadeSteps"/>, #2488); the scene that shows the wash owns it. The wash shows white at the level of its
/// fade the steps left reach (<see cref="FadeLevel"/>), which the backend shows in the game's colour steps (#2563).
/// </summary>
internal sealed class WhiteWash
{
    /// <summary>The steps of a white wash from full white to none: one a step of a colour channel.</summary>
    internal const int Steps = FadeLevelColour.FullLevel;

    /// <summary>A wash at full white whose fade is paced by <paramref name="fade"/>.</summary>
    private WhiteWash(PacedFadeSteps fade) => this.fade = fade;

    /// <summary>The wash's fade from full white: each step it takes leaves one fewer of the wash's steps.</summary>
    private readonly PacedFadeSteps fade;

    /// <summary>Whether the wash has faded away: it has no step left and adds nothing.</summary>
    internal bool IsGone => fade.IsComplete;

    /// <summary>The screen tint the wash adds now, in the colour of the steps it has left.</summary>
    internal ScreenTintLayer Layer => new(Colour(fade.Left));

    /// <summary>
    /// A wash starting at full white whose fade takes a step every <paramref name="updatesPerStep"/> updates, the first on
    /// its <paramref name="firstStepUpdate"/>th update.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Either count of updates is below one.</exception>
    internal static WhiteWash FullWhite(int updatesPerStep, int firstStepUpdate) =>
        new(new PacedFadeSteps(Steps, updatesPerStep, firstStepUpdate));

    /// <summary>One update of the wash's fade: on the update its cadence is due it takes one step off, stopping at none.</summary>
    internal void Fade() => fade.Advance();

    /// <summary>The colour of a wash with <paramref name="stepsLeft"/> of its <see cref="Steps"/> steps left.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stepsLeft"/> is outside 0-<see cref="Steps"/>.</exception>
    internal static ShownColour.Faded Colour(int stepsLeft) => new ShownColour.Faded(RgbColor.White, new FadeLevel(stepsLeft, Steps));
}
