namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed debugger view of Yard's split slot/parallel-table state. Names follow what each
/// word does rather than the original generic $7800 indexes, while every value remains an
/// exact unsigned SNES word.
/// </summary>
public sealed class YardEnemyState
{
    /// <summary>Physical enemy slot whose variables A-F hold Yard's current movement state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects one Yard enemy slot into typed debugger-visible state.</summary>
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
