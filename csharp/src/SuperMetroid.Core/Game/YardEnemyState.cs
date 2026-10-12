using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A3 movement pointer stored in Yard's native enemy variable F.</summary>
public enum YardMovementFunction : ushort
{
    /// <summary>$A3:CF5F, RTL stub: main movement remains idle until an animation instruction installs a movement pointer.</summary>
    InstructionPending = 0xcf5f,
    /// <summary>$A3:CF60, Function_Yard_Movement_Hiding: probes the current attachment surface while hidden and drops Yard if support disappears.</summary>
    Hiding = 0xcf60,
    /// <summary>$A3:CFA6, Function_Yard_Movement_Crawling_UpsideUp_MovingLeft: floor-attached leftward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideUpMovingLeft = 0xcfa6,
    /// <summary>$A3:CFB7, Function_Yard_Movement_Crawling_UpsideLeft_MovingDown: downward crawl with Yard's top facing left, using vertical surface and corner probes.</summary>
    CrawlingUpsideLeftMovingDown = 0xcfb7,
    /// <summary>$A3:CFBD, Function_Yard_Movement_Crawling_UpsideDown_MovingRight: ceiling-attached rightward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideDownMovingRight = 0xcfbd,
    /// <summary>$A3:CFCE, Function_Yard_Movement_Crawling_UpsideRight_MovingUp: upward crawl with Yard's top facing right, using vertical surface and corner probes.</summary>
    CrawlingUpsideRightMovingUp = 0xcfce,
    /// <summary>$A3:CFD4, Function_Yard_Movement_Crawling_UpsideUp_MovingRight: floor-attached rightward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideUpMovingRight = 0xcfd4,
    /// <summary>$A3:CFE5, Function_Yard_Movement_Crawling_UpsideRight_MovingDown: downward crawl with Yard's top facing right, using vertical surface and corner probes.</summary>
    CrawlingUpsideRightMovingDown = 0xcfe5,
    /// <summary>$A3:CFEB, Function_Yard_Movement_Crawling_UpsideDown_MovingLeft: ceiling-attached leftward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideDownMovingLeft = 0xcfeb,
    /// <summary>$A3:CFFC, Function_Yard_Movement_Crawling_UpsideLeft_MovingUp: upward crawl with Yard's top facing left, using vertical surface and corner probes.</summary>
    CrawlingUpsideLeftMovingUp = 0xcffc,
    /// <summary>$A3:D1B3, Function_Yard_Movement_Airborne: applies 16.16 shell motion, horizontal decay, gravity, native per-word collision bounces, and low-speed floor landing.</summary>
    Airborne = 0xd1b3,
}

/// <summary>
/// Typed debugger view of Yard's split slot/parallel-table state. Names follow what each
/// word does rather than the original generic $7800 indexes, while every value remains an
/// exact unsigned SNES word.
/// </summary>
public sealed class YardEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new yard enemy state instance.</summary>
    internal YardEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Native variables A/B: signed 8.8 horizontal and vertical crawl velocity, each axis used
    /// either tangentially or as a normal-axis surface probe according to the movement pointer.
    /// </summary>
    public EightEightVector<RoomPixels> CrawlingVelocity
    {
        get => new(new SnesSignedEightEight(_slot.VariableA), new SnesSignedEightEight(_slot.VariableB));
        internal set
        {
            _slot.VariableA = value.X.RawValue;
            _slot.VariableB = value.Y.RawValue;
        }
    }

    /// <summary>Native variable C: airborne animation facing, 0 for left or 1 for right, selected from the eight surface-direction records.</summary>
    public ushort AirborneFacingDirection
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native variable D: bank-$A3 hidden-shell instruction-list pointer for the current orientation; $CF5F suppresses the look-at hiding transition.</summary>
    public ushort HidingInstructionList
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native variable E: consecutive missing-surface probe count, reset on surface contact; the fourth failure drops Yard instead of installing another outside-corner turn.</summary>
    public ushort ConsecutiveTurnCounter
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native variable F: bank-$A3 movement dispatcher pointer, which may lag a direction instruction by one update and therefore owns dispatch independently of <see cref="Direction"/>.</summary>
    public YardMovementFunction MovementFunction
    {
        get => (YardMovementFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>
    /// Native extension offsets $00-$06: signed 16.16 airborne velocity in pixels per AI update
    /// (Y fraction/whole at $00/$02, X fraction/whole at $04/$06). Gravity adds $2000 to Y,
    /// horizontal speed decays by $1000 toward zero when not colliding, and collisions negate
    /// each word separately; downward collision lands when the Y whole word is 0..2.
    /// </summary>
    public FixedVector<RoomPixels> AirborneVelocity { get; internal set; }
    /// <summary>Native extension offset $08: slope/turn cooldown counter reset by adjustment or turning; an unadjusted crawl call whose increment reaches 16 reenables transitions without storing that increment.</summary>
    public ushort TurnTransitionDisableCounter { get; internal set; }
    /// <summary>Native extension offset $0A flag selecting the turn-disabled corner-probe definitions, set after slope alignment, a turn, or landing and cleared by the crawl cooldown.</summary>
    public bool TurnTransitionDisabled { get; internal set; }
    /// <summary>Native extension offset $0C: saved population crawl-speed index, restored after ordinary touch in place of the aggressive post-kick speed index 8.</summary>
    public ushort IdleCrawlingSpeedIndex { get; internal set; }
    /// <summary>Native extension offset $0E: live surface orientation index 0..7, ordered right/up, right/down, left/up, left/down, upside-down/left, upside-down/right, upright/left, upright/right; only direction instructions or turns publish it.</summary>
    public ushort Direction { get; internal set; }
    /// <summary>Native extension offset $10: mutually exclusive behavior 0 normal crawling, 1 aggressive crawling, 2 hiding, 3 dropped, 4 kicked, or 5 beam-launched, controlling look-at, solidity, touch, and airborne decisions.</summary>
    public YardBehavior Behavior { get; internal set; }
    /// <summary>Native parallel $7E:8000 flag: set by a horizontal airborne collision and cleared by a vertical bounce; a record of bounce state rather than a per-frame event.</summary>
    public bool BouncedHorizontally { get; internal set; }
}
