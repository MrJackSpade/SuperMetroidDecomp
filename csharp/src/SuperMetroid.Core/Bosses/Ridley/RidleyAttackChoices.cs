using static SuperMetroid.Core.Bosses.Ridley.NorfairRidleyPhase;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Direct action selection from Ridley's eight equally likely attack choices.</summary>
public static class RidleyAttackChoices
{
    /// <summary>
    /// The ordinary distribution splits at four and damage boosting at six. The pogo-zone, spin-jumping and zero-health
    /// situations select a single action.
    /// </summary>
    /// <param name="situation">The mutually exclusive distribution already selected from Ridley's health and Samus's movement and position.</param>
    /// <param name="choice">An equally likely index from 0 through 7, normally Ridley's random draw modulo 8.</param>
    /// <returns>The AI entry; this lookup does not draw RNG or execute the selected action.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The choice is outside 0..7, including for a distribution whose entries are identical.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The situation is not a defined distribution.</exception>
    public static NorfairRidleyPhase Resolve(RidleyAttackSituation situation, int choice)
    {
        if ((uint)choice >= 8) throw new ArgumentOutOfRangeException(nameof(choice));
        return situation switch
        {
            RidleyAttackSituation.Ordinary => choice < 4 ? PogoMoveToSide : SwoopSetup,
            RidleyAttackSituation.DamageBoosting => choice < 6 ? GrabApproach : PogoMoveToSide,
            RidleyAttackSituation.PogoZone => PogoMoveToSide,
            RidleyAttackSituation.SpinJumping => WallSweepSetup,
            RidleyAttackSituation.ZeroHealth => GrabApproach,
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
    }
}
