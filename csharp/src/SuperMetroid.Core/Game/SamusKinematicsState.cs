namespace SuperMetroid.Core.Game;

/// <summary>
/// Samus position, subposition, radius, and vertical-speed WRAM words consumed by bank-$94
/// room collision.
/// </summary>
/// <remarks>
/// Positions are split high/low words because collision intentionally overwrites individual
/// halves (for example, a right wall sets X subposition to $FFFF). Collapsing this state to
/// floats would erase those observable edge semantics.
/// </remarks>
public sealed class SamusKinematicsState
{
    // `$7E:182C-$1833` remembers the solid enemy hit in each movement direction until the
    // next EnemyMain tail clears all four words. Dead-monster actors inspect this delayed
    // producer state; a one-call return value alone cannot reproduce that ordering.
    private readonly ushort[] _solidEnemyCollisionIndexes =
        [ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue];

    /// <summary>
    /// Full Samus owner for bank-$94 collision side effects that cannot be represented by
    /// position words alone, notably spike damage and hurt timers. Standalone synthetic
    /// kinematics remain valid and intentionally have no damage owner.
    /// </summary>
    internal SamusState? SamusOwner { get; }

    /// <summary>Constructs an ownerless kinematics fixture for block-only probes.</summary>
    public SamusKinematicsState()
    {
    }

    /// <summary>Constructs the live collision child owned by one <see cref="SamusState"/>.</summary>
    internal SamusKinematicsState(SamusState samusOwner)
    {
        SamusOwner = samusOwner ?? throw new ArgumentNullException(nameof(samusOwner));
    }

    /// <summary>
    /// Current pose byte sampled by bank-$94 collision. Door collision is mostly geometric,
    /// but the elevator pseudo-door handlers admit only poses below $09 before publishing
    /// <c>elevator_flags</c>. Keeping that byte beside the geometry lets every shared block
    /// mover preserve the native test without guessing which higher-level movement caller
    /// happened to reach it.
    /// </summary>
    public byte CollisionPose { get; internal set; } = byte.MaxValue;

    /// <summary>
    /// Current native-order snapshot of WRAM <c>InteractiveEnemyIndices</c> and the collision
    /// words of the referenced enemy slots.
    /// </summary>
    /// <remarks>
    /// Movement owns this snapshot because every ordinary bank-$90 movement wrapper probes
    /// solid/frozen enemies before dispatching to bank-$94 room blocks. Empty remains the
    /// correct default for translated rooms whose enemy actor loader has not run. Keeping the
    /// list explicit prevents decorative render sprites from accidentally becoming physics.
    /// </remarks>
    public IReadOnlyList<SolidEnemyCollisionBody> InteractiveEnemies { get; set; } =
        Array.Empty<SolidEnemyCollisionBody>();

    /// <summary>
    /// Native left/right/up/down collision-index words retained for debugger inspection.
    /// <c>$FFFF</c> means that direction did not encounter a solid enemy.
    /// </summary>
    public IReadOnlyList<ushort> SolidEnemyCollisionIndexes => _solidEnemyCollisionIndexes;

    /// <summary>Whole-pixel world X at WRAM <c>$0AF6</c>.</summary>
    public ushort XPosition { get; set; }

    /// <summary>Fractional world X at WRAM <c>$0AF8</c>.</summary>
    public ushort XSubposition { get; set; }

    /// <summary>Whole-pixel world Y at WRAM <c>$0AFA</c>.</summary>
    public ushort YPosition { get; set; }

    /// <summary>Fractional world Y at WRAM <c>$0AFC</c>.</summary>
    public ushort YSubposition { get; set; }

    /// <summary>
    /// Whole signed X displacement at WRAM <c>$0B58</c>. Enemy/PLM producers write this
    /// independently of Samus's own base and extra-run speeds.
    /// </summary>
    public ushort ExtraXDisplacement { get; set; }

    /// <summary>Fractional X displacement at WRAM <c>$0B56</c>.</summary>
    public ushort ExtraXSubdisplacement { get; set; }

    /// <summary>
    /// Whole signed Y displacement at WRAM <c>$0B5C</c>. The word persists until its
    /// producer clears it; bank `$90` does not consume or automatically zero it.
    /// </summary>
    public ushort ExtraYDisplacement { get; set; }

    /// <summary>Fractional Y displacement at WRAM <c>$0B5A</c>.</summary>
    public ushort ExtraYSubdisplacement { get; set; }

    /// <summary>Horizontal collision radius; <c>Samus_SetRadius</c> always writes five.</summary>
    public ushort XRadius { get; set; } = 5;

    /// <summary>Pose-defined vertical collision radius at WRAM <c>$0B00</c>.</summary>
    public ushort YRadius { get; set; }

    /// <summary>Whole vertical speed used to suppress grounded horizontal slope scaling.</summary>
    public ushort YSpeed { get; set; }

    /// <summary>Fractional vertical speed used to suppress grounded horizontal slope scaling.</summary>
    public ushort YSubspeed { get; set; }

    /// <summary>
    /// Vertical direction at WRAM <c>$0B36</c>: zero is none, one is upward, and two is
    /// downward. The native game stores speed as a magnitude and uses this separate word;
    /// treating upward velocity as a negative host number would lose the underflow quirks
    /// in <c>$90:90E2</c> and the jump-release test in <c>$90:8FB3</c>.
    /// </summary>
    public ushort YDirection { get; set; }

    /// <summary>Whole gravity word at WRAM <c>$0B34</c>.</summary>
    public ushort YAcceleration { get; set; }

    /// <summary>Fractional gravity word at WRAM <c>$0B32</c>.</summary>
    public ushort YSubacceleration { get; set; }

    /// <summary>Native <c>enable_horiz_slope_coll</c>; bit 1 enables post-X Y alignment.</summary>
    public ushort HorizontalSlopeCollisionEnable { get; set; } = 3;

    /// <summary>Native flag set when square collision or non-square alignment changes Y.</summary>
    public bool PositionAdjustedBySlope { get; set; }

    /// <summary>Current position as an unsigned native 16.16 pair.</summary>
    public uint XFixed => ((uint)XPosition << 16) | XSubposition;

    /// <summary>Current position as an unsigned native 16.16 pair.</summary>
    public uint YFixed => ((uint)YPosition << 16) | YSubposition;

    /// <summary>Current vertical speed as an unsigned native high/low pair.</summary>
    public uint VerticalSpeedFixed => ((uint)YSpeed << 16) | YSubspeed;

    /// <summary>
    /// Current extra X pair interpreted exactly as signed two's-complement 16.16. Keeping
    /// the component words public preserves producer-level WRAM semantics while this view
    /// prevents every movement consumer from reimplementing the cast/wrap operation.
    /// </summary>
    public int ExtraXFixed => unchecked((int)(((uint)ExtraXDisplacement << 16) |
        ExtraXSubdisplacement));

    /// <summary>Current extra Y pair interpreted as signed two's-complement 16.16.</summary>
    public int ExtraYFixed => unchecked((int)(((uint)ExtraYDisplacement << 16) |
        ExtraYSubdisplacement));

    /// <summary>
    /// Whole-pixel top collision boundary returned by <c>Get_Samus_Top_Boundary</c>.
    /// Liquid and wall-jump code deliberately samples this separately from Samus's feet.
    /// </summary>
    public ushort TopBoundary => unchecked((ushort)(YPosition - YRadius));

    /// <summary>
    /// Whole-pixel bottom collision boundary returned by <c>Get_Samus_Bottom_Boundary</c>.
    /// The native addition is 16-bit and therefore wraps at the room-coordinate boundary.
    /// </summary>
    public ushort BottomBoundary => unchecked((ushort)(YPosition + YRadius));

    /// <summary>Publishes the result of one directional bank-$A0 solid-enemy probe.</summary>
    internal void RecordSolidEnemyCollision(
        SamusCollisionDirection direction,
        ushort? nativeEnemyIndex)
    {
        if ((uint)direction > (uint)SamusCollisionDirection.Down)
            throw new ArgumentOutOfRangeException(nameof(direction));
        _solidEnemyCollisionIndexes[(int)direction] = nativeEnemyIndex ?? ushort.MaxValue;
    }

    /// <summary>Tests the four collision words exactly as dead-monster wait AI does.</summary>
    internal bool DidCollideWithSolidEnemy(ushort nativeEnemyIndex)
    {
        foreach (ushort index in _solidEnemyCollisionIndexes)
        {
            if (index == nativeEnemyIndex)
                return true;
        }
        return false;
    }

    /// <summary>Ports EnemyMain's end-of-frame clear of <c>$182C-$1833</c>.</summary>
    internal void ClearSolidEnemyCollisionIndexes() =>
        Array.Fill(_solidEnemyCollisionIndexes, ushort.MaxValue);

    internal void SetXFixed(uint value)
    {
        XPosition = unchecked((ushort)(value >> 16));
        XSubposition = unchecked((ushort)value);
    }

    internal void SetYFixed(uint value)
    {
        YPosition = unchecked((ushort)(value >> 16));
        YSubposition = unchecked((ushort)value);
    }
}
