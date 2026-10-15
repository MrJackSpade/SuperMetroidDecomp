namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The literal bank-$A8 function pointer stored in a Powamp body's variable F. Keeping the
/// cartridge addresses visible makes a debugger watch directly comparable with WRAM $0FB2.
/// The balloon half uses <see cref="BalloonNoOp"/> and never owns vertical movement.
/// </summary>
public enum PowampEnemyFunction : ushort
{
    /// <summary><c>Function_Powamp_Deflated_Resting</c> at <c>$A8:C283</c>; holds the body at its resting height until its 60-update countdown starts balloon inflation.</summary>
    DeflatedResting = 0xc283,
    /// <summary><c>Function_Powamp_Inflating</c> at <c>$A8:C2A6</c>; waits ten AI updates, then selects fast body animation and upward velocity of half a pixel per update.</summary>
    Inflating = 0xc2a6,
    /// <summary><c>Function_Powamp_Inflated_RiseToTargetHeight</c> at <c>$A8:C2CF</c>; rises with horizontal wiggle toward 64 pixels above the balloon's spawn Y, switching immediately to grappled rise if grapple AI activates.</summary>
    InflatedRiseToTargetHeight = 0xc2cf,
    /// <summary><c>Function_Powamp_Inflated_FinishWiggle</c> at <c>$A8:C36B</c>; completes the wiggle to a centered phase before deflating, continuing terrain-clipped upward movement while off center.</summary>
    InflatedFinishWiggle = 0xc36b,
    /// <summary><c>Function_Powamp_Grappled_RiseToTargetHeight</c> at <c>$A8:C3E1</c>; rises toward the population-defined grapple height while attached, switching to ordinary finish-wiggle without moving on the release update.</summary>
    GrappledRiseToTargetHeight = 0xc3e1,
    /// <summary><c>Function_Powamp_Grappled_FinishWiggle</c> at <c>$A8:C469</c>; centers the wiggle before resting at the grapple target, or hands off to ordinary finish-wiggle when released.</summary>
    GrappledFinishWiggle = 0xc469,
    /// <summary><c>Function_Powamp_Grappled_Resting</c> at <c>$A8:C4DC</c>; holds the raised body while grapple AI remains active and starts deflation on release.</summary>
    GrappledResting = 0xc4dc,
    /// <summary><c>Function_Powamp_Deflating</c> at <c>$A8:C500</c>; waits ten AI updates for balloon deflation, then selects downward velocity of one pixel per update.</summary>
    Deflating = 0xc500,
    /// <summary><c>Function_Powamp_Deflated_Sinking</c> at <c>$A8:C51D</c>; sinks with terrain collision handling until the body reaches the balloon's spawn Y, then restores slow animation and the rest countdown.</summary>
    DeflatedSinking = 0xc51d,
    /// <summary><c>RTL_A8C568</c> at <c>$A8:C568</c>; no-operation main AI for the balloon half, whose position is maintained by its following body.</summary>
    BalloonNoOp = 0xc568,
    /// <summary><c>Function_Powamp_FatalDamage</c> at <c>$A8:C569</c>; adjusts the balloon's inflation animation and initializes the 32-update death delay after a fatal shot.</summary>
    FatalDamage = 0xc569,
    /// <summary><c>Function_Powamp_DeathSequence</c> at <c>$A8:C59F</c>; maintains balloon alignment during the countdown, then deletes both halves and fires spikes in eight directions.</summary>
    DeathSequence = 0xc59f,
}
