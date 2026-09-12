using static SuperMetroid.Core.Game.RidleyAiFunction;

namespace SuperMetroid.Core.Game;

/// <summary>Immutable eight-way action distributions selected by Ridley_Func_4.</summary>
public static class RidleyAttackChoices
{
    /// <summary>$A6:B38C, belowHalfHealth: four pogo approaches followed by four swoops.</summary>
    public static ReadOnlySpan<RidleyAiFunction> BelowHalfHealth =>
        [NorfairFireballMoveToSide, NorfairFireballMoveToSide, NorfairFireballMoveToSide, NorfairFireballMoveToSide,
         NorfairSwoopSetup, NorfairSwoopSetup, NorfairSwoopSetup, NorfairSwoopSetup];
    /// <summary>$A6:B39C, aboveHalfHealth: reversed distribution, retained even though the ordinary health branch cannot reach it.</summary>
    public static ReadOnlySpan<RidleyAiFunction> AboveHalfHealth =>
        [NorfairSwoopSetup, NorfairSwoopSetup, NorfairSwoopSetup, NorfairSwoopSetup,
         NorfairFireballMoveToSide, NorfairFireballMoveToSide, NorfairFireballMoveToSide, NorfairFireballMoveToSide];
    /// <summary>$A6:B3AC, damageBoosting: six lunges followed by two pogo approaches.</summary>
    public static ReadOnlySpan<RidleyAiFunction> DamageBoosting =>
        [NorfairGrabApproach, NorfairGrabApproach, NorfairGrabApproach, NorfairGrabApproach,
         NorfairGrabApproach, NorfairGrabApproach, NorfairFireballMoveToSide, NorfairFireballMoveToSide];
    /// <summary>$A6:B3BC, notSpinJumping: eight pogo approaches when Samus is in the pogo zone.</summary>
    public static ReadOnlySpan<RidleyAiFunction> PogoZone =>
        [NorfairFireballMoveToSide, NorfairFireballMoveToSide, NorfairFireballMoveToSide, NorfairFireballMoveToSide,
         NorfairFireballMoveToSide, NorfairFireballMoveToSide, NorfairFireballMoveToSide, NorfairFireballMoveToSide];
    /// <summary>$A6:B3CC, spinJumping: eight hover actions (named NorfairPogoSetup by the existing translated dispatcher).</summary>
    public static ReadOnlySpan<RidleyAiFunction> SpinJumping =>
        [NorfairPogoSetup, NorfairPogoSetup, NorfairPogoSetup, NorfairPogoSetup,
         NorfairPogoSetup, NorfairPogoSetup, NorfairPogoSetup, NorfairPogoSetup];
    /// <summary>$A6:B3DC, zeroHealth: eight lunges for the final grab/death sequence.</summary>
    public static ReadOnlySpan<RidleyAiFunction> ZeroHealth =>
        [NorfairGrabApproach, NorfairGrabApproach, NorfairGrabApproach, NorfairGrabApproach,
         NorfairGrabApproach, NorfairGrabApproach, NorfairGrabApproach, NorfairGrabApproach];
}
