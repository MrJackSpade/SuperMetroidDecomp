using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Reads and sets where the Speed Booster's and Screw Attack's shared glow stands as the step the original counts it by
/// (#2387): the Screw Attack shade it shows next, counted from the first of its six; and the updates the Speed Booster's
/// shown shade still holds (#2447).
/// </summary>
internal static class SamusFullBodyCycleAccess
{
    /// <summary>The Screw Attack glow, whose six shades number the shared steps.</summary>
    private static Animation<SamusGlowSegment> ScrewAttack =>
        SamusGlowAnimationDefinitions.Animations[SamusGlowSegment.ScrewAttackPower];

    /// <summary>Reads and sets the shared glow.</summary>
    extension(SamusHorizontalSpeedState self)
    {
        /// <summary>
        /// The step the shared glow shows next, zero through five: a Speed Booster glow resting on its brightest stands at
        /// three, and a glow before its first shade or past its last at zero.
        /// </summary>
        internal int CycleShadeStep
        {
            get => self.FullBodyGlow.Playback.GoingOnIn(ScrewAttack).Place.Beat;
            set => self.FullBodyGlow.GoOnFrom(new AnimationPlayback<SamusGlowSegment>(
                ScrewAttack, new AnimationPlace(0, Ensure.BetweenInclusive(0, ScrewAttack.Phases[0].BeatCount - 1, value))));
        }

        /// <summary>
        /// The updates the Speed Booster glow's shown shade still holds: its next boosted update moves the glow on once this
        /// is one or less. Starting the glow again leaves one, so the next boosted update moves it on.
        /// </summary>
        internal int CycleShadeTimer
        {
            get => self.FullBodyGlow.UpdatesLeft;
            set => PrivateState.Set(self.FullBodyGlow,
                nameof(AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue>.UpdatesLeft), value);
        }
    }
}
