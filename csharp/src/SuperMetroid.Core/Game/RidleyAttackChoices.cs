using static SuperMetroid.Core.Game.RidleyAiFunction;

namespace SuperMetroid.Core.Game;

/// <summary>Mutually exclusive attack distributions selected by the native Ridley AI.</summary>
public enum RidleyAttackSituation
{
    /// <summary>$A6:B38C table: choices 0..3 select the translated ground-attack side phase and 4..7 the swoop; used below 14400 health and by the default branch.</summary>
    BelowHalfHealth,
    /// <summary>$A6:B39C table: choices 0..3 select the swoop and 4..7 the ground-attack side phase; the reversed distribution is unreachable through the ordinary selector.</summary>
    AboveHalfHealth,
    /// <summary>$A6:B3AC table: grab-eligible Samus movement outside the pogo zone selects grab approach for choices 0..5 and the ground-attack side phase for 6..7.</summary>
    DamageBoosting,
    /// <summary>$A6:B3BC table: Samus at room Y >= 352 pixels with Ridley at least 14400 health selects the ground-attack side phase for every choice.</summary>
    PogoZone,
    /// <summary>$A6:B3CC native hover table: Samus's spin-jump movement takes priority and selects the translated pogo-setup entry for every choice.</summary>
    SpinJumping,
    /// <summary>$A6:B3DC table: zero health with Samus not spin jumping selects grab approach for every choice; the caller counts these death lunges.</summary>
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
