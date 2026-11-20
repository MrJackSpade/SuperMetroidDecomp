using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's position, to a fraction of a pixel, her radius and vertical speed, which room collision
/// reads.
/// </summary>
/// <remarks>
/// Positions keep their whole pixels and fractions of a pixel apart because collision intentionally
/// sets each on its own (for example, a right wall puts Samus at the largest fraction of her pixel).
/// Collapsing this state to floats would erase those observable edge semantics.
/// </remarks>
public sealed class SamusKinematicsState
{
    /// <summary>
    /// The solid enemy Samus met moving in each direction, holding no entry for a direction where she met none, kept until
    /// the end of the enemy pass clears them; the corpses' and idle shutters' contact tests observe this delayed result.
    /// They name enemies of the room they were made in, which forgets them when it unloads (#1702).
    /// </summary>
    private readonly Dictionary<SamusCollisionDirection, ISolidEnemy> _solidEnemyContacts = [];

    /// <summary>
    /// The solid enemy Samus's latest wall jump launched from, which the next enemy pass shakes. Each wall-jump check
    /// withdraws a waiting launch before it looks for a new one; it names an enemy of the room it was made in, which
    /// withdraws it when it unloads.
    /// </summary>
    internal WallJumpLaunchShake<ISolidEnemy> WallJumpLaunch { get; } = new();

    /// <summary>
    /// Contact-damage snapshot for ownerless geometry probes. Sand still reads this
    /// collision input even when a probe deliberately cannot mutate the full owner.
    /// </summary>
    internal SamusBodyAttack? ProbeBodyAttack { get; init; }

    /// <summary>The captured contact mode of an ownerless probe, otherwise <paramref name="owner"/>'s live mode.</summary>
    internal SamusBodyAttack CollisionBodyAttack(SamusState owner) =>
        ProbeBodyAttack ?? owner.HorizontalSpeed.BodyAttack;

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
        var probe = new SamusKinematicsState();
        probe.Position = Position;
        probe.Radius = Radius;
        probe.VerticalVelocity = VerticalVelocity;
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
    /// The enemies Samus's movement collides with, solid or frozen, which every ordinary movement meets before room
    /// blocks; a room whose enemies have not been placed has none.
    /// </remarks>
    public IReadOnlyList<SolidEnemyCollisionBody> InteractiveEnemies { get; set; } =
        Array.Empty<SolidEnemyCollisionBody>();

    /// <summary>The solid enemies Samus met moving in any direction since the enemy pass last cleared them.</summary>
    public IEnumerable<ISolidEnemy> SolidEnemyContacts => _solidEnemyContacts.Values;

    /// <summary>
    /// Room position, to a fraction of a pixel.
    /// Collision deliberately overwrites the whole pixels or the fraction on their own (a right wall puts Samus at the
    /// largest fraction of her pixel), which the operations on each part preserve.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; set; }

    /// <summary>
    /// The displacement things outside Samus give her.
    /// Enemy/room-object producers write it independently of Samus's own speeds;
    /// it persists until its producer clears it, and Samus's movement neither consumes nor zeroes it.
    /// </summary>
    public FixedVector<RoomPixels> ExtraDisplacement { get; set; }

    /// <summary>
    /// Collision half-extents: X is always every pose's horizontal radius
    /// (<see cref="SamusPoseCollisionDefinitions.HorizontalRadius"/>); Y is the pose-defined radius.
    /// </summary>
    public Extent16<RoomPixels> Radius { get; set; } = new(SamusPoseCollisionDefinitions.HorizontalRadius, 0);

    /// <summary>
    /// Her vertical velocity (#2569): one value holding how fast she moves vertically and which way, whose rest and apex
    /// readings the vertical moves and the jump-release test depend on (<see cref="SamusVerticalVelocity"/>).
    /// </summary>
    public SamusVerticalVelocity VerticalVelocity { get; set; }

    /// <summary>The gravity applied to vertical speed.</summary>
    public UnsignedSixteenSixteen YAcceleration { get; set; }

    /// <summary>
    /// Whether a slope supports her: a downward square-slope clip or a horizontal move's slope alignment leaves her supported,
    /// which the next vertical move without speed reads for its one-pixel grounding probe, and the next terrain Y move of
    /// some distance ends.
    /// </summary>
    public bool PositionAdjustedBySlope { get; private set; }

    /// <summary>
    /// Exclusive bottom collision boundary. Unlike the occupied bottom pixel returned by
    /// the bottom-boundary check, this does not subtract one. It wraps in room space.
    /// </summary>
    public int BottomBoundary => TerrainScanEdges.Edge(Position.Y.Whole + Radius.Y);

    /// <summary>
    /// Bottom-centre occupied point: Samus's whole X and the last pixel row her collision box
    /// covers, one above the exclusive bottom boundary, wrapping in room space.
    /// </summary>
    public Position16<RoomPixels> BottomPixel =>
        new(Position.X.Whole, TerrainScanEdges.Edge(BottomBoundary - 1));

    /// <summary>A downward square-slope clip, or a horizontal move's alignment that adjusted her height, leaves her supported.</summary>
    internal void RestOnSlope() => PositionAdjustedBySlope = true;

    /// <summary>A terrain Y move of some distance ends the slope's support.</summary>
    internal void LeaveSlope() => PositionAdjustedBySlope = false;

    /// <summary>Publishes the result of one directional solid-enemy probe: the enemy met, or null.</summary>
    internal void RecordSolidEnemyCollision(
        SamusCollisionDirection direction,
        ISolidEnemy? solidEnemy)
    {
        if (solidEnemy is null)
            _solidEnemyContacts.Remove(RequireContactDirection(direction));
        else
            _solidEnemyContacts[RequireContactDirection(direction)] = solidEnemy;
    }

    /// <summary><paramref name="direction"/>, which must be one Samus can meet an enemy moving in.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not left, right, up or down.</exception>
    private static SamusCollisionDirection RequireContactDirection(SamusCollisionDirection direction) => direction switch
    {
        SamusCollisionDirection.Left or SamusCollisionDirection.Right or SamusCollisionDirection.Up or SamusCollisionDirection.Down => direction,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };

    /// <summary>Whether Samus met <paramref name="enemy"/> in any direction, as the corpses' wait for her contact tests.</summary>
    internal bool DidCollideWithSolidEnemy(ISolidEnemy enemy)
    {
        Ensure.NotNull(enemy);
        return _solidEnemyContacts.Values.Any(contact => ReferenceEquals(contact, enemy));
    }

    /// <summary>Forgets every direction's contact, as the end of the enemy pass does.</summary>
    internal void ClearSolidEnemyCollisions() => _solidEnemyContacts.Clear();

    /// <summary>
    /// Forgets the enemies of a room that unloads (#1697, #1702): a waiting launch shake and every direction's contact
    /// name enemies that leave with it. A door's source room runs its enemy pass, which clears the contacts, before the
    /// destination loads; a game, demo or Ceres-to-Zebes load starts in a room none of whose enemies reads them.
    /// </summary>
    internal void ForgetUnloadedRoomEnemies()
    {
        WallJumpLaunch.Withdraw();
        ClearSolidEnemyCollisions();
    }
}
