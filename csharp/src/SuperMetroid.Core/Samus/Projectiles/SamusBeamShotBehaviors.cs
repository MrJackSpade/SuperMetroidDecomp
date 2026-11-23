using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which behaviour each beam shot runs.</summary>
internal static partial class SamusBeamShotBehaviors
{
    /// <summary>
    /// What firing a shot of <paramref name="combination"/> does, charged when <paramref name="charged"/>: the behaviour it
    /// runs, or, for a Spazer and Plasma combination, what follows the authored behaviours
    /// (<see cref="SamusBeamShotBehaviorDefinitions.Unauthored"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination is not one Samus's beams can make.</exception>
    /// <remarks>
    /// A combination without Wave runs the beam's behaviour. One with Wave trails its shot: Wave with Spazer or Plasma
    /// always the charged trail, Wave without them the charged trail only when charged.
    /// </remarks>
    public static SamusShotFiring Resolve(bool charged, SamusBeamCombination combination)
    {
        if (combination.PairsSpazerWithPlasma)
            return SamusBeamShotBehaviorDefinitions.Unauthored.ShotBehavior(combination, charged);
        if (!combination.HasWave)
            return SamusShotBehaviors.Beam;
        return charged || combination.HasSpazer || combination.HasPlasma
            ? SamusShotBehaviors.WaveBeamTrail
            : SamusShotBehaviors.UnchargedWaveBeamTrail;
    }
}
