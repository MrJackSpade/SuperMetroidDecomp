using static SuperMetroid.Core.Bosses.Ridley.NorfairRidleyPhase;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Which attack Ridley chooses in each situation, by the named chances of the random sample his choice draws.</summary>
public static partial class RidleyAttackChoices
{
    /// <summary>
    /// Half his ordinary choices pogo and half swoop; three in four of his choices at a damage-boosting Samus lunge at her
    /// and the rest pogo. The pogo-zone, spin-jumping and zero-health situations always take one attack.
    /// </summary>
    /// <param name="situation">The situation already chosen from Ridley's health and Samus's movement and position.</param>
    /// <param name="sample">The random number his choice draws.</param>
    /// <returns>The phase his attack starts in; choosing it draws no random number and starts nothing.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The situation is not a defined situation.</exception>
    public static NorfairRidleyPhase Resolve(RidleyAttackSituation situation, int sample)
    {
        return situation switch
        {
            RidleyAttackSituation.Ordinary => RidleyRandomDefinitions.OrdinaryAttackPogos.IsTakenBy(sample) ? PogoMoveToSide : SwoopSetup,
            RidleyAttackSituation.DamageBoosting => RidleyRandomDefinitions.DamageBoostingAttackLunges.IsTakenBy(sample) ? GrabApproach : PogoMoveToSide,
            RidleyAttackSituation.PogoZone => PogoMoveToSide,
            RidleyAttackSituation.SpinJumping => WallSweepSetup,
            RidleyAttackSituation.ZeroHealth => GrabApproach,
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
    }
}
