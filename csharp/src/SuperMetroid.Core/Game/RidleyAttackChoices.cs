using static SuperMetroid.Core.Game.RidleyAiFunction;

namespace SuperMetroid.Core.Game;

/// <summary>Mutually exclusive attack distributions selected by the native Ridley AI.</summary>
public enum RidleyAttackSituation
{
    BelowHalfHealth,
    AboveHalfHealth,
    DamageBoosting,
    PogoZone,
    SpinJumping,
    ZeroHealth,
}

/// <summary>Direct action selection from Ridley_Func_4's eight equally likely choices.</summary>
public static class RidleyAttackChoices
{
    /// <summary>
    /// $A6:B38C..B3EB: health distributions split at four; damage boosting splits
    /// at six. Pogo-zone, spin and zero-health situations select a single action.
    /// The reversed above-half-health distribution remains independently defined,
    /// even though the ordinary health branch cannot reach it.
    /// </summary>
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
