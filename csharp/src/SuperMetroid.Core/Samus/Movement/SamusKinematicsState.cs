using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus position, subposition, radius, and vertical-speed WRAM words consumed by bank-$94
/// room collision.
/// </summary>
/// <remarks>
/// Positions keep their native whole and subpixel words because collision intentionally
/// overwrites individual halves (for example, a right wall sets X subposition to $FFFF).
/// Collapsing this state to floats would erase those observable edge semantics.
/// </remarks>
public sealed class SamusKinematicsState
{
    /// <summary>
    /// WRAM $7E:182C-$1833 retains each direction's solid-enemy hit until EnemyMain's
    /// tail clears all four words; dead-monster AI observes this delayed result.
    /// </summary>
    internal readonly ushort[] _solidEnemyCollisionIndexes =
        [ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue];

    /// <summary>
    /// Full Samus owner for bank-$94 collision side effects that cannot be represented by
    /// position words alone, notably spike damage and hurt timers. Standalone synthetic
    /// kinematics remain valid and intentionally have no damage owner.
    /// </summary>
    internal SamusState? SamusOwner { get; }

    /// <summary>
    /// Contact-damage snapshot for ownerless geometry probes. Sand still reads this
    /// native collision input even when a probe deliberately cannot mutate the full owner.
    /// </summary>
    internal SamusContactDamageIndex? ProbeContactDamageIndex { get; init; }

    /// <summary>Live contact mode, or the captured mode while testing a prospective pose.</summary>
    internal SamusContactDamageIndex CollisionContactDamageIndex =>
        ProbeContactDamageIndex ?? SamusOwner?.HorizontalSpeed.ContactDamageIndex ?? SamusContactDamageIndex.None;

    /// <summary>Constructs an ownerless kinematics fixture for block-only probes.</summary>
    public SamusKinematicsState()
    {
    }

    /// <summary>Constructs the live collision child owned by one <see cref="SamusState"/>.</summary>
    internal SamusKinematicsState(SamusState samusOwner)
    {
        SamusOwner = Ensure.NotNull(samusOwner);
    }

    /// <summary>
    /// A copy of this body's collision geometry for a bank-$94 probe that must not move
    /// Samus. The probe keeps the Samus owner, because native block reactions reached by
    /// such probes (spike damage, speed-block setup) act on the real Samus.
    /// </summary>
    internal SamusKinematicsState CreateCollisionProbe()
    {
        SamusKinematicsState probe = SamusOwner is null
            ? new SamusKinematicsState { ProbeContactDamageIndex = CollisionContactDamageIndex }
            : new SamusKinematicsState(SamusOwner);
        probe.CollisionPose = CollisionPose;
        probe.Position = Position;
        probe.Radius = Radius;
        probe.YSpeed = YSpeed;
        probe.YSubspeed = YSubspeed;
        probe.YDirection = YDirection;
        probe.SandCollisionArea = SandCollisionArea;
        probe.YAcceleration = YAcceleration;
        probe.YSubacceleration = YSubacceleration;
        probe.HorizontalSlopeCollisionEnable = HorizontalSlopeCollisionEnable;
        probe.PositionAdjustedBySlope = PositionAdjustedBySlope;
        // Probes still call the native solid-enemy detector before blocks. The
        // per-frame actor snapshots are immutable, so the probe shares them.
        probe.InteractiveEnemies = InteractiveEnemies;
        return probe;
    }

    /// <summary>
    /// Current pose byte sampled by bank-$94 collision. Door collision is mostly geometric,
    /// but the elevator pseudo-door handlers admit only poses below $09 before publishing
    /// <c>elevator_flags</c>. Keeping that byte beside the geometry lets every shared block
    /// mover preserve the native test without guessing which higher-level movement caller
    /// happened to reach it.
    /// </summary>
    // Unsampled state reads as $FF, above every pose the elevator pseudo-doors admit.
    public SamusPoseId CollisionPose { get; internal set; } = (SamusPoseId)byte.MaxValue;

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
    /// Native left/right/up/down collision-index words. <c>$FFFF</c> means that direction did
    /// not encounter a solid enemy.
    /// </summary>
    public IReadOnlyList<ushort> SolidEnemyCollisionIndexes => _solidEnemyCollisionIndexes;

    /// <summary>
    /// Room-pixel 16.16 position: X at WRAM <c>$0AF6.$0AF8</c>, Y at <c>$0AFA.$0AFC</c>.
    /// Collision deliberately overwrites individual halves (a right wall sets the X
    /// subposition to <c>$FFFF</c>), which the half-word operations preserve.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; set; }

    /// <summary>
    /// External signed 16.16 displacement: X at WRAM <c>$0B58.$0B56</c>, Y at
    /// <c>$0B5C.$0B5A</c>. Enemy/PLM producers write it independently of Samus's own speeds;
    /// it persists until its producer clears it, and bank `$90` neither consumes nor zeroes it.
    /// </summary>
    public FixedVector<RoomPixels> ExtraDisplacement { get; set; }

    /// <summary>
    /// Collision half-extents: X is always five (<c>Samus_SetRadius</c>); Y is the
    /// pose-defined radius at WRAM <c>$0B00</c>.
    /// </summary>
    public Extent16<RoomPixels> Radius { get; set; } = new(5, 0);

    /// <summary>Whole vertical speed used to suppress grounded horizontal slope scaling.</summary>
    public int YSpeed
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Fractional vertical speed used to suppress grounded horizontal slope scaling.</summary>
    public int YSubspeed
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// Vertical direction at WRAM <c>$0B36</c>: zero is none, one is upward, and two is
    /// downward. The native game stores speed as a magnitude and uses this separate word;
    /// treating upward velocity as a negative host number would lose the underflow quirks
    /// in <c>$90:90E2</c> and the jump-release test in <c>$90:8FB3</c>.
    /// </summary>
    public SamusVerticalDirection YDirection { get; set; }

    /// <summary>Area-table selector used by bank-$94 sand reactions; refreshed before movement.</summary>
    public AreaId SandCollisionArea { get; set; }

    /// <summary>Whole gravity word at WRAM <c>$0B34</c>.</summary>
    public int YAcceleration
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Fractional gravity word at WRAM <c>$0B32</c>.</summary>
    public int YSubacceleration
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Native <c>enable_horiz_slope_coll</c>; bit 1 enables post-X Y alignment.</summary>
    public int HorizontalSlopeCollisionEnable
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = 3;

    /// <summary>Native flag set when square collision or non-square alignment changes Y.</summary>
    public bool PositionAdjustedBySlope { get; set; }

    /// <summary>Current vertical speed as an unsigned native high/low pair.</summary>
    public uint VerticalSpeedFixed => SnesSixteenSixteen.Compose(YSpeed, YSubspeed);

    /// <summary>
    /// Top-centre point of the collision box: Samus's whole X and her whole-pixel top
    /// boundary, wrapping in room space. Liquid code deliberately samples this separately
    /// from Samus's feet.
    /// </summary>
    public Position16<RoomPixels> TopBoundary =>
        new(Position.X.Whole, unchecked((ushort)(Position.Y.Whole - Radius.Y)));

    /// <summary>
    /// Exclusive bottom collision boundary. Unlike the occupied bottom pixel returned by
    /// <c>$90:EC3E</c>, this does not subtract one. The native addition wraps at 16 bits.
    /// </summary>
    public int BottomBoundary => unchecked((ushort)(Position.Y.Whole + Radius.Y));

    /// <summary>
    /// Bottom-centre occupied point: Samus's whole X and the last pixel row her collision box
    /// covers, one above the exclusive bottom boundary, wrapping in room space.
    /// </summary>
    public Position16<RoomPixels> BottomPixel =>
        new(Position.X.Whole, unchecked((ushort)(BottomBoundary - 1)));

    /// <summary>Publishes the result of one directional bank-$A0 solid-enemy probe.</summary>
    internal void RecordSolidEnemyCollision(
        SamusCollisionDirection direction,
        int? nativeEnemyIndex)
    {
        if ((uint)direction > (uint)SamusCollisionDirection.Down)
            throw new ArgumentOutOfRangeException(nameof(direction));
        _solidEnemyCollisionIndexes[(int)direction] = checked((ushort)(nativeEnemyIndex ?? ushort.MaxValue));
    }

    /// <summary>Tests the four collision words exactly as dead-monster wait AI does.</summary>
    internal bool DidCollideWithSolidEnemy(int nativeEnemyIndex)
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
}
