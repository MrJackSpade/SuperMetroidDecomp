using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's position, subposition, radius and vertical speed, which room collision
/// reads.
/// </summary>
/// <remarks>
/// Positions keep their whole pixels and subpixels apart because collision intentionally
/// sets each on its own (for example, a right wall puts Samus on the last subpixel of her pixel).
/// Collapsing this state to floats would erase those observable edge semantics.
/// </remarks>
public sealed class SamusKinematicsState
{
    /// <summary>
    /// The solid enemy Samus met moving in each direction, holding no entry for a direction where she met none, kept until
    /// the end of the enemy pass clears them; the corpses' and idle shutters' contact tests observe this delayed result.
    /// They name enemies of the room they were made in, which forgets them when it unloads (#1702).
    /// </summary>
    internal readonly Dictionary<SamusCollisionDirection, ISolidEnemy> solidEnemyContacts = [];

    /// <summary>
    /// The solid enemy Samus's latest wall jump launched from, waiting for the next enemy pass to shake it; null when no
    /// launch from an enemy waits. It names an enemy of the room it was made in, which withdraws it when it unloads.
    /// </summary>
    private ISolidEnemy? _wallJumpLaunchEnemy;

    /// <summary>
    /// Contact-damage snapshot for ownerless geometry probes. Sand still reads this
    /// collision input even when a probe deliberately cannot mutate the full owner.
    /// </summary>
    internal SamusBodyAttack? ProbeBodyAttack { get; init; }

    /// <summary>The captured contact mode of an ownerless probe, otherwise <paramref name="owner"/>'s live mode.</summary>
    internal SamusBodyAttack CollisionBodyAttack(SamusState? owner) =>
        ProbeBodyAttack ?? owner?.HorizontalSpeed.BodyAttack ?? SamusBodyAttack.None;

    /// <summary>Constructs kinematics with no owner, for block collision that moves no Samus.</summary>
    public SamusKinematicsState()
    {
    }

    /// <summary>
    /// A copy of this body's collision geometry for a probe that must not move
    /// Samus. The probe keeps the Samus owner, because block reactions reached by
    /// such probes (spike damage, speed-block setup) act on the real Samus.
    /// </summary>
    internal SamusKinematicsState CreateCollisionProbe()
    {
        var probe = new SamusKinematicsState { ProbeBodyAttack = ProbeBodyAttack };
        probe.Position = Position;
        probe.Radius = Radius;
        probe.YSpeed = YSpeed;
        probe.YDirection = YDirection;
        probe.YAcceleration = YAcceleration;
        probe.PositionAdjustedBySlope = PositionAdjustedBySlope;
        // Probes still call the solid-enemy detector before blocks. The
        // per-frame actor snapshots are immutable, so the probe shares them.
        probe.InteractiveEnemies = InteractiveEnemies;
        return probe;
    }

    /// <summary>
    /// Current snapshot, in the game's order, of the interactive enemies and the collision
    /// values of the enemies it lists.
    /// </summary>
    /// <remarks>
    /// Movement owns this snapshot because every ordinary movement probes
    /// solid/frozen enemies before reacting to room blocks. Empty remains the
    /// correct default for rooms whose enemy actor loader has not run. Keeping the
    /// list explicit prevents decorative render sprites from accidentally becoming physics.
    /// </remarks>
    public IReadOnlyList<SolidEnemyCollisionBody> InteractiveEnemies { get; set; } =
        Array.Empty<SolidEnemyCollisionBody>();

    /// <summary>The solid enemies Samus met moving in any direction since the enemy pass last cleared them.</summary>
    public IEnumerable<ISolidEnemy> SolidEnemyContacts => solidEnemyContacts.Values;

    /// <summary>
    /// Room position in 1/65536 pixels.
    /// Collision deliberately overwrites the whole pixels or the subpixels on their own (a right wall puts Samus on the
    /// last subpixel of her pixel), which the operations on each part preserve.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; set; }

    /// <summary>
    /// External signed 1/65536-pixel displacement.
    /// Enemy/room-object producers write it independently of Samus's own speeds;
    /// it persists until its producer clears it, and Samus's movement neither consumes nor zeroes it.
    /// </summary>
    public FixedVector<RoomPixels> ExtraDisplacement { get; set; }

    /// <summary>
    /// Collision half-extents: X is always five; Y is the
    /// pose-defined radius.
    /// </summary>
    public Extent16<RoomPixels> Radius { get; set; } = new(5, 0);

    /// <summary>Vertical speed magnitude; its direction is <see cref="YDirection"/>.</summary>
    public UnsignedSixteenSixteen YSpeed { get; set; }

    /// <summary>
    /// Vertical direction: <see cref="SamusVerticalDirection.None"/>, <see cref="SamusVerticalDirection.Up"/> or
    /// <see cref="SamusVerticalDirection.Down"/>. The game stores speed as a magnitude and uses this separate direction;
    /// treating upward velocity as a negative number would lose the quirks of a speed passing zero
    /// in the vertical move and the jump-release test.
    /// </summary>
    public SamusVerticalDirection YDirection { get; set; }

    /// <summary>The gravity applied to vertical speed.</summary>
    public UnsignedSixteenSixteen YAcceleration { get; set; }

    /// <summary>Flag set when square collision or non-square alignment changes Y.</summary>
    public bool PositionAdjustedBySlope { get; set; }

    /// <summary>
    /// Top-centre point of the collision box: Samus's whole X and her whole-pixel top
    /// boundary, wrapping in room space. Liquid code deliberately samples this separately
    /// from Samus's feet.
    /// </summary>
    public Position16<RoomPixels> TopBoundary =>
        new(Position.X.Whole, CoordinateAxis.Wrap(Position.Y.Whole - Radius.Y));

    /// <summary>
    /// Exclusive bottom collision boundary. Unlike the occupied bottom pixel returned by
    /// the bottom-boundary check, this does not subtract one. The addition wraps at 65536.
    /// </summary>
    public int BottomBoundary => CoordinateAxis.Wrap(Position.Y.Whole + Radius.Y);

    /// <summary>
    /// Bottom-centre occupied point: Samus's whole X and the last pixel row her collision box
    /// covers, one above the exclusive bottom boundary, wrapping in room space.
    /// </summary>
    public Position16<RoomPixels> BottomPixel =>
        new(Position.X.Whole, CoordinateAxis.Wrap(BottomBoundary - 1));

    /// <summary>Publishes the result of one directional solid-enemy probe: the enemy met, or null.</summary>
    internal void RecordSolidEnemyCollision(
        SamusCollisionDirection direction,
        ISolidEnemy? solidEnemy)
    {
        if (solidEnemy is null)
            solidEnemyContacts.Remove(RequireContactDirection(direction));
        else
            solidEnemyContacts[RequireContactDirection(direction)] = solidEnemy;
    }

    /// <summary><paramref name="direction"/>, which must be one Samus can meet an enemy moving in.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not left, right, up or down.</exception>
    internal static SamusCollisionDirection RequireContactDirection(SamusCollisionDirection direction) => direction switch
    {
        SamusCollisionDirection.Left or SamusCollisionDirection.Right or SamusCollisionDirection.Up or SamusCollisionDirection.Down => direction,
        SamusCollisionDirection.NonDirectionalProbe => throw new ArgumentOutOfRangeException(nameof(direction), direction,
            "A non-directional probe meets no solid enemy."),
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };

    /// <summary>Whether Samus met <paramref name="enemy"/> in any direction, as the corpses' wait for her contact tests.</summary>
    internal bool DidCollideWithSolidEnemy(ISolidEnemy enemy)
    {
        Ensure.NotNull(enemy);
        foreach (ISolidEnemy contact in solidEnemyContacts.Values)
        {
            if (ReferenceEquals(contact, enemy))
                return true;
        }
        return false;
    }

    /// <summary>Forgets every direction's contact, as the end of the enemy pass does.</summary>
    internal void ClearSolidEnemyCollisions() => solidEnemyContacts.Clear();

    /// <summary>
    /// Forgets the enemies of a room that unloads (#1697, #1702): a waiting launch shake and every direction's contact
    /// name enemies that leave with it. A door's source room runs its enemy pass, which clears the contacts, before the
    /// destination loads; a game, demo or Ceres-to-Zebes load starts in a room none of whose enemies reads them.
    /// </summary>
    internal void ForgetUnloadedRoomEnemies()
    {
        WithdrawWallJumpLaunchShake();
        ClearSolidEnemyCollisions();
    }

    /// <summary>Asks the next enemy pass to shake <paramref name="launchedFrom"/>, which a wall jump has just launched from.</summary>
    internal void RequestWallJumpLaunchShake(ISolidEnemy launchedFrom) => _wallJumpLaunchEnemy = Ensure.NotNull(launchedFrom);

    /// <summary>
    /// Withdraws a waiting launch's shake, as each wall-jump check does before it looks for a new launch, and as a room
    /// does when it unloads with the enemy the launch named.
    /// </summary>
    internal void WithdrawWallJumpLaunchShake() => _wallJumpLaunchEnemy = null;

    /// <summary>Takes the enemy a wall jump launched from for the enemy pass to shake, leaving none waiting.</summary>
    internal ISolidEnemy? TakeWallJumpLaunchShake()
    {
        ISolidEnemy? launchedFrom = _wallJumpLaunchEnemy;
        _wallJumpLaunchEnemy = null;
        return launchedFrom;
    }
}
