using static SuperMetroid.Core.Game.RidleyAiFunction;

namespace SuperMetroid.Core.Game;

/// <summary>Direct action selection from Ridley_Func_4's eight equally likely choices.</summary>
public static class RidleyAttackChoices
{
    /// <summary>
    /// $A6:B38C..B3EB: health distributions split at four; damage boosting splits
    /// at six. Pogo-zone, spin and zero-health situations select a single action.
    /// The reversed above-half-health distribution remains independently defined,
    /// even though the ordinary health branch cannot reach it.
    /// </summary>
    /// <param name="situation">The mutually exclusive distribution already selected from Ridley's health and Samus's movement and position.</param>
    /// <param name="choice">An equally likely index from 0 through 7, normally the low three bits of the caller's RNG result.</param>
    /// <returns>The translated AI entry; this lookup does not draw RNG or execute the selected action.</returns>
    /// <exception cref="IndexOutOfRangeException">The choice is outside 0..7, including for a distribution whose entries are identical.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The situation is not a defined distribution.</exception>
    public static RidleyAiFunction Resolve(RidleyAttackSituation situation, int choice)
    {
        if ((uint)choice >= 8) throw new IndexOutOfRangeException();
        return situation switch
        {
            RidleyAttackSituation.BelowHalfHealth => choice < 4 ? NorfairFireballMoveToSide : NorfairSwoopSetup,
            RidleyAttackSituation.AboveHalfHealth => choice < 4 ? NorfairSwoopSetup : NorfairFireballMoveToSide,
            RidleyAttackSituation.DamageBoosting => choice < 6 ? NorfairGrabApproach : NorfairFireballMoveToSide,
            RidleyAttackSituation.PogoZone => NorfairFireballMoveToSide,
            RidleyAttackSituation.SpinJumping => NorfairPogoSetup,
            RidleyAttackSituation.ZeroHealth => NorfairGrabApproach,
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
    }
}
