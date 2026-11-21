using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Reads and sets where the charge pulse stands as the step the original counts it by (#2351): the shade it shows next,
/// counted from its first.
/// </summary>
internal static class BeamChargeStateAccess
{
    /// <summary>The pulse's shades: every pulse runs as many.</summary>
    private static int PulseShades =>
        SamusGlowAnimationDefinitions.Animations[SamusGlowSegment.In(SamusGlow.ChargedBeamPulse, SamusSuit.Power)].Phases[0].BeatCount;

    /// <summary>Reads and sets the charge pulse.</summary>
    extension(BeamChargeState self)
    {
        /// <summary>The shade the charge pulse shows next, counted from its first: zero at its start and after its last.</summary>
        internal int ChargeGlowStep
        {
            get => self.ChargePulse.HasNotEntered ? 0 : self.ChargePulse.Beat % PulseShades;
            set => self.ChargePulse = new AnimationPlace(0, Ensure.BetweenInclusive(0, PulseShades - 1, value));
        }

        /// <summary>Places the charge at <paramref name="count"/> updates held, as a recorded state or a check's setup holds it.</summary>
        internal void SetFlareCount(int count) =>
            self.Level = BeamChargeLevel.Charged(count);
    }
}
