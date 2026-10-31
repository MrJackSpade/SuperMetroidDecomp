using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// Ki-Hunter's body or wing state. A body and a detached wing put different meanings in the same common-record word,
/// and each meaning has its own value; detachment sets each detached value before reading it. The values the cartridge
/// keeps in the enemy's common record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class KiHunterEnemyState
{
    /// <summary>Captures the slot's body or wing role before native deletion can clear its definition identity.</summary>
    internal KiHunterEnemyState(RoomEnemySlot slot)
    {
        // The definition word is mutable native storage and is cleared during deletion.
        // Role is established by the initializer and must survive that later write because
        // body/wing callbacks continue using physical +/-$40 slot aliases.
        IsWing = KiHunterBehavior.IsKiHunterWingDefinition(slot.DefinitionId);
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<KiHunterProgram> Program { get; set; }

    /// <summary>Role fixed by the initializer, retained even if deletion clears the native definition word; body and wing callbacks still use adjacent physical slots.</summary>
    public bool IsWing { get; }

    /// <summary>Main-AI dispatcher; none once a kill has emptied the record.</summary>
    public KiHunterEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// A body's swoop target: Samus's room position when the swoop begins, around which the dive's ellipse is centred.
    /// Only the X axis is stored and meaningful.
    /// </summary>
    public Position16<RoomPixels> SwoopTarget
    {
        get;
        internal set => field = new(value.X, 0);
    }

    /// <summary>
    /// A detached wing's speed index, in 1/256 records of the shared quadratic speed table: reset to <see
    /// cref="DetachedSpeedReset"/> at each arc and lowered by 384 per update, landing on 256 when the step would go
    /// below zero, so it stays from zero to 24,319.
    /// </summary>
    public int DetachedSpeedIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// A body's swoop origin: its room position when the swoop begins, from which the dive's ellipse hangs. Only the Y
    /// axis is stored and meaningful.
    /// </summary>
    public Position16<RoomPixels> SwoopOrigin
    {
        get;
        internal set => field = new(0, value.Y);
    }

    /// <summary>
    /// A detached wing's quadratic speed sum, accumulated at detachment until it reaches the detached-speed threshold.
    /// Detachment never resets it: it continues from zero or the earlier sum, and a kill empties it. Its sum wraps as
    /// the cartridge's word does, through <see cref="AccumulateDetachedSpeed"/>.
    /// </summary>
    public int DetachedSpeedAccumulator
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// A body's animation-change angle, in whole table units of the swoop angle: 240 for a leftward swoop or 144 for a
    /// rightward one. Passing it installs the swoop animation.
    /// </summary>
    public int AnimationChangeAngle
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// A body's swoop angle, in whole table units: 128 or 255 when a swoop begins, stepped by the whole angular
    /// velocity, and ending the swoop at 256 or below 128, so it stays from zero to 257.
    /// </summary>
    public int SwoopAngle
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// A detached wing's arc angle, which wraps over the whole turn: seeded below zero for each arc and advanced by an
    /// unaligned quadratic-table read. The arc changes when the angle crosses a quarter turn.
    /// </summary>
    public RotationAngle WingArcAngle { get; internal set; }

    /// <summary>A body's whole-angle velocity cap: minus two for a leftward swoop or two for a rightward one.</summary>
    public int MaximumAngularVelocity { get; internal set; }
    /// <summary>Wing $7E:7800,x bank-$A8 secondary function pointer, alternating between non-colliding and collision-bearing detached arcs.</summary>
    public KiHunterWingFunction WingFunction { get; internal set; }
    /// <summary>
    /// A body's signed 16.16 angular velocity in table units per update: zero when a swoop begins, accelerated each
    /// update with the fraction carrying into the whole part, and capped on its whole part. Its whole part steps the
    /// swoop angle.
    /// </summary>
    public SignedSixteenSixteen AngularVelocity { get; internal set; }
    /// <summary>A body's signed 16.16 angular acceleration: plus one eighth of a table unit per update squared for a rightward swoop, minus one eighth for a leftward one.</summary>
    public SignedSixteenSixteen AngularAcceleration { get; internal set; }
    /// <summary>A body's upper patrol boundary, sixteen pixels above its spawn Y. Only the Y axis is stored and meaningful.</summary>
    public Position16<RoomPixels> UpperPatrol { get; internal set; }
    /// <summary>
    /// A detached wing's speed-index reset, computed at detachment by stepping 384 at a time until the quadratic speed
    /// sum reaches its threshold, and reapplied at each arc change; from 384 to 24,319.
    /// </summary>
    public int DetachedSpeedReset { get; internal set; }
    /// <summary>
    /// A body's lower patrol boundary, sixteen pixels below its spawn Y; wing detachment reads the wing's own boundary,
    /// which its initializer never sets. Only the Y axis is stored and meaningful.
    /// </summary>
    public Position16<RoomPixels> LowerPatrol { get; internal set; }
    /// <summary>Body $7E:7818/$781A,x initial room position retained from body initialization rather than the changing patrol position; collision recovery returns to patrol after rising above its Y.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }
    /// <summary>Body $7E:781E,x gameplay-frame thinking countdown: twelve after landing or twenty-four after acid emission, consumed once the animation selects ground wait.</summary>
    public WrappingCountdown WaitTimer { get; internal set; }
    /// <summary>Body $7E:7820,x swipe-animation latch, cleared on swoop setup and set once the angle passes its facing-specific trigger.</summary>
    public bool SwoopAnimationChanged { get; internal set; }
    /// <summary>Body $7E:7822/$7824,x whole-pixel swoop ellipse radii: the absolute horizontal separation from Samus and Samus Y minus body Y when a swoop begins.</summary>
    public Extent16<RoomPixels> SwoopRadius { get; internal set; }
    /// <summary>Body $7E:780C-$7812,x signed 16.16 velocity in pixels per gameplay frame (X fraction/whole at $780C/$780E, Y at $7810/$7812); positive Y is downward and gravity accumulates through the Y fraction.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Wing $7E:7806/$7808,x signed pixel correction for the non-colliding arc, calculated at angle $E0 and radius $30.</summary>
    public Offset16<RoomPixels> OrbitOffset { get; internal set; }
    /// <summary>Wing $7E:7802/$7804,x signed pixel correction for the collision-bearing arc, calculated at angle $A0 and radius $30.</summary>
    public Offset16<RoomPixels> FallingArcOffset { get; internal set; }
    /// <summary>Wing $7E:780A/$780C,x room-pixel anchor used with an arc-specific correction; recaptured whenever the detached arc changes.</summary>
    public Position16<RoomPixels> OrbitCenter { get; internal set; }
    /// <summary>Wing $7E:7810/$780E,x original room position saved at detachment and restored when the collision-bearing arc deletes the wing.</summary>
    public Position16<RoomPixels> SavedWingPosition { get; internal set; }
    /// <summary>Body $7E:7828,x wingless latch, set by spawn parameter-one bit 15 or by health reaching the following wing record's parameter-one threshold.</summary>
    public bool HasLostWings { get; internal set; }

    /// <summary>Adds one quadratic-table word to <see cref="DetachedSpeedAccumulator"/>, wrapping as the cartridge's word does.</summary>
    internal void AccumulateDetachedSpeed(int step) => DetachedSpeedAccumulator = WordWrap.Unsigned(DetachedSpeedAccumulator + step);

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        SwoopTarget = Position16<RoomPixels>.Zero;
        DetachedSpeedIndex = 0;
        SwoopOrigin = Position16<RoomPixels>.Zero;
        DetachedSpeedAccumulator = 0;
        AnimationChangeAngle = 0;
        SwoopAngle = 0;
        WingArcAngle = RotationAngle.FromTableIndex(0);
    }
}
