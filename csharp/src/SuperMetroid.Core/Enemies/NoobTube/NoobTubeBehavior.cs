using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>The Noob tube species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class NoobTubeBehavior : IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.NoobTubeCrack];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup) => new NoobTubeCrackEntity(place, common);

    /// <summary>The pictures and shading every tube piece draws with: the shared sheet in the tube's crack colours.</summary>
    private static readonly SpriteGraphics TubePieceGraphics =
        SpriteGraphics.SharedIn(PictureShading.Shaded(ChozoAndTubeShades.TubeCracks));

    /// <summary>Provides room-load state and the RNG for shard and bubble launch angles.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates and advances crack, glass shard, and released-air-bubble projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds projectile initialization to the loaded room and its projectiles.</summary>
    internal NoobTubeBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Spawns the crack, glass shard or released air bubble the request names, at the tube's block the request
    /// carries.
    /// </summary>
    public void SpawnNoobTubeProjectile(
        NoobTubeProjectileRequest request)
    {

        NoobTubePiece piece = request.Piece;
        RoomEnemyProjectileKind kind = piece.Definition switch
        {
            NoobTubeProjectileDefinition.Crack => RoomEnemyProjectileKind.NoobTubeCrack,
            NoobTubeProjectileDefinition.Shard => RoomEnemyProjectileKind.NoobTubeShard,
            NoobTubeProjectileDefinition.ReleasedAirBubble =>
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble,
            _ => throw new InvalidOperationException(
                $"Undefined n00b-tube projectile definition {piece.Definition}."),
        };

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        // The tube's pieces draw from the shared sheet in the crack colours the room's crack setup installs (#1512).
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, TubePieces, graphics: TubePieceGraphics);

        Position16<RoomPixels> place = request.RoomObjectBlock.ToRoomPixels() + piece.Offset;
        projectile.StartAnimation(new NoobTubeProjectileAnimation(piece.Start switch
        {
            NoobTubePieceStart.AtSegment start => NoobTubeProjectileAnimations.StartingWith(start.Segment),
            NoobTubePieceStart.ShardFlight flight => NoobTubeProjectileAnimations.ShardAnimations[flight.Shard],
            _ => throw new InvalidOperationException($"A n00b tube piece starts by {piece.Start}, which no case answers."),
        }));
        switch (piece.Definition)
        {
            case NoobTubeProjectileDefinition.Crack:
                projectile.State = new NoobTubeCrackState();
                projectile.Position = projectile.Position.WithWhole(place);
                return;

            case NoobTubeProjectileDefinition.Shard:
                projectile.State = new NoobTubeDebrisState(place.X);
                projectile.Position = projectile.Position.WithWholeY(place.Y);
                projectile.Velocity = piece.Velocity;
                return;

            case NoobTubeProjectileDefinition.ReleasedAirBubble:
                projectile.State = new NoobTubeDebrisState(place.X);
                projectile.Position = projectile.Position.WithWholeY(place.Y);
                // The bubble sets only its rise: it sways by its own arc, and the sideways speed its place's previous
                // projectile left passes on to the next projectile spawned there (EnemyProjectileHandOver).
                projectile.Velocity = projectile.Velocity.WithY(piece.Velocity.Y);
                return;

            default:
                throw new InvalidDataException(
                    $"N00b-tube setup lost projectile definition {piece.Definition}.");
        }
    }

    /// <summary>Drops the detached crack at its fixed fall speed.</summary>
    private static void DropNoobTubeCrack(EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.AdvanceY(NoobTubeDebrisDefinitions.CrackFallVelocity.ToSixteenSixteenDelta());

    /// <summary>Flies a newly emitted shard by its velocity and deletes it once it leaves the screen vertically.</summary>
    private static void FlyNoobTubeShard(EnemyProjectile projectile, int cameraY)
    {
        AdvanceNoobTubeShardX(projectile, projectile.Velocity.X);
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <summary>Sways a falling shard along its arc, drops it, and deletes it once it leaves the screen vertically.</summary>
    private static void DropNoobTubeShard(EnemyProjectile projectile, int cameraY)
    {
        MoveNoobTubeProjectileHorizontallyAlongArc(projectile, NoobTubeDebrisMotionDefinitions.ShardSwaySteps);
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <summary>Sways a falling bubble along its arc, then moves it vertically once as a flying bubble does.</summary>
    private static void DropNoobTubeBubble(EnemyProjectile projectile)
    {
        MoveNoobTubeProjectileHorizontallyAlongArc(projectile, NoobTubeDebrisMotionDefinitions.BubbleSwaySteps);
        MoveNoobTubeBubbleVertically(projectile);
    }

    /// <summary>Advances the debris's own horizontal position by one update's <paramref name="velocity"/>.</summary>
    /// <remarks>
    /// The position keeps the room's axis, as <see cref="FixedCoordinate.MovedBy(SignedEightEight)"/> keeps it, because a
    /// shard can pass the room's left edge (#2537): in the Glass Tunnel the tube's block lies two blocks from that edge,
    /// so the shard that flies up and out starts 64 pixels from it and flies 48 pixels toward it, and a swaying fall whose
    /// sway starts late in the range it is drawn from drifts up to about 21 pixels further, carrying the
    /// shard past the edge unless its fall has already taken it far enough below the camera to delete it. Its column then
    /// lies at the end of the axis, and its mirror image across the tube just past the room's right edge, as the reflected
    /// flicker places it.
    /// </remarks>
    private static void AdvanceNoobTubeShardX(EnemyProjectile projectile, SignedEightEight velocity)
    {
        NoobTubeDebrisState debris = projectile.StateOf<NoobTubeDebrisState>();
        debris.X = debris.X.MovedBy(velocity);
    }

    /// <summary>Uses the sine of the debris's sway to advance its own horizontal position, then steps the sway.</summary>
    internal static void MoveNoobTubeProjectileHorizontallyAlongArc(
        EnemyProjectile projectile,
        int swaySteps)
    {
        NoobTubeDebrisState debris = projectile.StateOf<NoobTubeDebrisState>();
        AdvanceNoobTubeShardX(projectile, SwayVelocity(debris.SwayAngle));
        debris.AdvanceSway(swaySteps);
    }

    /// <summary>
    /// The debris's sideways speed at sway angle <paramref name="angle"/>: a quarter of the one-pixel sine velocity,
    /// rounded toward decreasing coordinates.
    /// </summary>
    internal static SignedEightEight SwayVelocity(TrigonometryAngle angle) => EnemyMath.SineVelocity(angle, 1).DividedRoundingDown(4);

    /// <summary>Advances the bubble by its vertical velocity and publishes its own horizontal position.</summary>
    private static void MoveNoobTubeBubbleVertically(EnemyProjectile projectile)
    {
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta()).WithWholeX(projectile.StateOf<NoobTubeDebrisState>().X.Whole);
    }

    /// <summary>The tube's crack, shards and released-air bubbles (#1760): their spawner starts their animations.</summary>
    internal EnemyProjectileBehavior TubePieces => field ??= new NoobTubeProjectileBehavior(_room);

    /// <summary>A crack, shard or bubble: the crack's, shards' and bubbles' movers, and their animations' cues.</summary>
    /// <param name="room">The loaded room, whose random numbers set a falling piece's sway.</param>
    private sealed class NoobTubeProjectileBehavior(EnemyRoomBinding room)
        : EnemyProjectileMotionBehavior<NoobTubePieceMotion>(StartingMotions), IEnemyProjectileCues<NoobTubeProjectileCue>
    {
        /// <summary>A shard flies from its spawn; the crack and the bubbles hold still until their animations start them.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, NoobTubePieceMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.NoobTubeShard] = NoobTubePieceMotion.ShardFlying,
            [RoomEnemyProjectileKind.NoobTubeCrack] = null,
            [RoomEnemyProjectileKind.NoobTubeReleasedAirBubble] = null,
        };

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, NoobTubePieceMotion motion)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (motion)
            {
                case NoobTubePieceMotion.CrackFlickering:
                    FlickerNoobTubeCrack(projectile, context.ShortCounter);
                    break;
                case NoobTubePieceMotion.CrackFalling:
                    DropNoobTubeCrack(projectile);
                    break;
                case NoobTubePieceMotion.ShardFlying:
                    FlyNoobTubeShard(projectile, context.Camera.Y);
                    break;
                case NoobTubePieceMotion.ShardFalling:
                    DropNoobTubeShard(projectile, context.Camera.Y);
                    break;
                case NoobTubePieceMotion.BubbleFalling:
                    DropNoobTubeBubble(projectile);
                    break;
                case NoobTubePieceMotion.BubbleFlying:
                    MoveNoobTubeBubbleVertically(projectile);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NoobTubePieceMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, NoobTubeProjectileCue cue) =>
            ApplyNoobTubeProjectileCue(context.Projectile, context.ShortCounter, cue);

        /// <summary>
        /// Flickers the crack: alternate values of <paramref name="shortCounter"/> hide it and show it. The crack keeps
        /// whichever its flicker last chose once it falls.
        /// </summary>
        private static void FlickerNoobTubeCrack(EnemyProjectile projectile, int shortCounter) =>
            projectile.StateOf<NoobTubeCrackState>().IsHidden = NoobTubeCadenceDefinitions.CrackHidden.IsSetIn(shortCounter);

        /// <summary>Carries out one cue of the crack, shard and bubble animations; <paramref name="shortCounter"/> times the flicker.</summary>
        private void ApplyNoobTubeProjectileCue(EnemyProjectile projectile, int shortCounter, NoobTubeProjectileCue cue)
        {
            bool atOwnColumn = NoobTubeCadenceDefinitions.DebrisAtOwnColumn.IsSetIn(shortCounter);
            switch (cue.Kind)
            {
                case NoobTubeProjectileCueKind.FlickerReflected:
                    // The shard shows itself at its column, or its mirror image mirrored across the tube. A column past the
                    // room's left edge lies at the end of the room's axis (AdvanceNoobTubeShardX), and its mirror image
                    // lies as far past the room's right edge, on the axis as OffRoomPlaces keeps it (#2537).
                    int column = projectile.StateOf<NoobTubeDebrisState>().X.Whole;
                    projectile.Position = projectile.Position.WithWholeX(atOwnColumn
                        ? column
                        : OffRoomPlaces.Place(NoobTubeDebrisDefinitions.ShardMirrorSpan - column));
                    ShardAnimation(projectile).ShowsMirroredFrame = !atOwnColumn;
                    break;
                case NoobTubeProjectileCueKind.FlickerHidden:
                    // The shard shows at its column on odd updates and is hidden on even ones.
                    projectile.Position = projectile.Position.WithWholeX(projectile.StateOf<NoobTubeDebrisState>().X.Whole);
                    projectile.StateOf<NoobTubeDebrisState>().IsHidden = !atOwnColumn;
                    break;
                case NoobTubeProjectileCueKind.AssignShardFallingAngle:
                    projectile.StateOf<NoobTubeDebrisState>().StartSway(NoobTubeDebrisDefinitions.FallingSwayStart.Of(room.Random.Generate()));
                    projectile.Velocity = projectile.Velocity.WithY(NoobTubeDebrisDefinitions.ShardFallYVelocity);
                    break;
                case NoobTubeProjectileCueKind.AssignBubbleFallingAngle:
                    projectile.StateOf<NoobTubeDebrisState>().StartSway(NoobTubeDebrisDefinitions.FallingSwayStart.Of(room.Random.Generate()));
                    break;
                case NoobTubeProjectileCueKind.StartCrackFlicker:
                    projectile.StartMotion(NoobTubePieceMotion.CrackFlickering);
                    break;
                case NoobTubeProjectileCueKind.StartCrackFall:
                    projectile.StartMotion(NoobTubePieceMotion.CrackFalling);
                    break;
                case NoobTubeProjectileCueKind.StartShardFall:
                    projectile.StartMotion(NoobTubePieceMotion.ShardFalling);
                    break;
                case NoobTubeProjectileCueKind.StartBubbleRise:
                    projectile.StartMotion(NoobTubePieceMotion.BubbleFlying);
                    break;
                case NoobTubeProjectileCueKind.StartBubbleFall:
                    projectile.StartMotion(NoobTubePieceMotion.BubbleFalling);
                    break;
                case NoobTubeProjectileCueKind.Delete:
                    projectile.Delete();
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NoobTubeProjectileCueKind)} {cue.Kind}.");
            }
        }
    }

    /// <summary>The typed animation a shard's flicker cue runs in.</summary>
    private static NoobTubeProjectileAnimation ShardAnimation(EnemyProjectile projectile) =>
        projectile.Animation as NoobTubeProjectileAnimation
        ?? throw new InvalidOperationException($"A n00b-tube flicker cue ran outside its typed animation; the projectile runs {projectile.Animation}.");
}
