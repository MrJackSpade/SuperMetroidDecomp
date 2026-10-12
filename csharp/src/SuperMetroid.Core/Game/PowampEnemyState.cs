using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

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

/// <summary>
/// Typed view of the six common enemy variables reused by both halves of a Powamp. The ROM
/// gives the words different meanings according to population parameter one: a nonzero
/// parameter is the balloon, while zero is the immediately following moving body.
/// </summary>
public sealed class PowampEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new powamp enemy state instance.</summary>
    internal PowampEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Whether population parameter one is nonzero, selecting the balloon interpretation of shared variables; zero selects the immediately following moving body.</summary>
    public bool IsBalloon => _slot.Parameter1 != 0;

    // Body meanings for variables A/B. Together these are a signed 16.16 displacement.
    /// <summary>Body variable A, the signed high word of vertical pixels-per-AI-update displacement; negative values move upward, and this word aliases <see cref="BalloonSpawn"/>'s X on balloon slots.</summary>
    public ushort YVelocity { get => _slot.VariableA; internal set => _slot.VariableA = value; }
    /// <summary>Body variable B, the low word of vertical displacement in 1/65536-pixel units, combined with <see cref="YVelocity"/> as signed 16.16; aliases <see cref="BalloonSpawn"/>'s Y on balloon slots.</summary>
    public ushort YSubvelocity { get => _slot.VariableB; internal set => _slot.VariableB = value; }

    // Balloon meanings for the same physical words.
    /// <summary>Balloon variables A/B, the original room position: X is both halves' horizontal wiggle center and Y feeds the body's resting and rise-target calculations; aliases body <see cref="YVelocity"/> and <see cref="YSubvelocity"/>.</summary>
    public Position16<RoomPixels> BalloonSpawn
    {
        get => new(_slot.VariableA, _slot.VariableB);
        internal set
        {
            _slot.VariableA = value.X;
            _slot.VariableB = value.Y;
        }
    }

    /// <summary>Body variable C, the zero-based index 0-11 into the signed three-pixel wiggle wave; phases zero and six are centered.</summary>
    public ushort WiggleIndex { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    /// <summary>Body variable D, an AI-update countdown reset to five for each horizontal wiggle offset; aliases balloon <see cref="BalloonGrappleTravelDistance"/>.</summary>
    public ushort WiggleTimer { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    /// <summary>Balloon variable D, copied from population parameter two; room-pixel distance subtracted from the balloon's spawn Y to obtain the grappled body target height.</summary>
    public ushort BalloonGrappleTravelDistance { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    /// <summary>Body variable E, decremented once per relevant main-AI invocation; initialized to 60 for rest, ten for inflation/deflation, or 32 for death, and expires at zero or signed underflow.</summary>
    public ushort FunctionTimer { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    /// <summary>Variable F at first-slot WRAM $0FB2, interpreted as the bank-$A8 main-AI function pointer; balloon slots retain the no-op entry.</summary>
    public PowampEnemyFunction Function
    {
        get => (PowampEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
