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
        Position16<RoomPixels> place = request.RoomObjectBlock.ToRoomPixels() + piece.Offset;
        var animation = new NoobTubeProjectileAnimation(piece.Start switch
        {
            NoobTubePieceStart.AtSegment start => NoobTubeProjectileAnimations.StartingWith(start.Segment),
            NoobTubePieceStart.ShardFlight flight => NoobTubeProjectileAnimations.ShardAnimations[flight.Shard],
            _ => throw new InvalidOperationException($"A n00b tube piece starts by {piece.Start}, which no case answers."),
        });
        // The tube's pieces draw from the shared sheet in the crack colours the room's crack setup installs (#1512). A shard
        // flies from its spawn; the crack and the bubbles hold still until their animations start them.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, piece.Definition switch
        {
            NoobTubeProjectileDefinition.Crack => new CrackBehavior(new NoobTubeCrackState()),
            NoobTubeProjectileDefinition.Shard => new DebrisBehavior(_room, new NoobTubeDebrisState(place.X), animation, NoobTubePieceMotion.ShardFlying),
            NoobTubeProjectileDefinition.ReleasedAirBubble => new DebrisBehavior(_room, new NoobTubeDebrisState(place.X), animation, initial: null),
            _ => throw new InvalidDataException($"N00b-tube setup lost projectile definition {piece.Definition}."),
        }, graphics: TubePieceGraphics);
        projectile.StartAnimation(animation);
        switch (piece.Definition)
        {
            case NoobTubeProjectileDefinition.Crack:
                projectile.Position = projectile.Position.WithWhole(place);
                return;

            case NoobTubeProjectileDefinition.Shard:
                projectile.Position = projectile.Position.WithWholeY(place.Y);
                projectile.Velocity = piece.Velocity;
                return;

            case NoobTubeProjectileDefinition.ReleasedAirBubble:
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
    private static void FlyNoobTubeShard(EnemyProjectile projectile, NoobTubeDebrisState debris, int cameraY)
    {
        AdvanceNoobTubeShardX(debris, projectile.Velocity.X);
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <summary>Sways a falling shard along its arc, drops it, and deletes it once it leaves the screen vertically.</summary>
    private static void DropNoobTubeShard(EnemyProjectile projectile, NoobTubeDebrisState debris, int cameraY)
    {
        MoveNoobTubeProjectileHorizontallyAlongArc(debris, NoobTubeDebrisMotionDefinitions.ShardSwaySteps);
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <summary>Sways a falling bubble along its arc, then moves it vertically once as a flying bubble does.</summary>
    private static void DropNoobTubeBubble(EnemyProjectile projectile, NoobTubeDebrisState debris)
    {
        MoveNoobTubeProjectileHorizontallyAlongArc(debris, NoobTubeDebrisMotionDefinitions.BubbleSwaySteps);
        MoveNoobTubeBubbleVertically(projectile, debris);
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
    private static void AdvanceNoobTubeShardX(NoobTubeDebrisState debris, SignedEightEight velocity) =>
        debris.X = debris.X.MovedBy(velocity);

    /// <summary>Uses the sine of the debris's sway to advance its own horizontal position, then steps the sway.</summary>
    internal static void MoveNoobTubeProjectileHorizontallyAlongArc(
        NoobTubeDebrisState debris,
        int swaySteps)
    {
        AdvanceNoobTubeShardX(debris, SwayVelocity(debris.SwayAngle));
        debris.AdvanceSway(swaySteps);
    }

    /// <summary>
    /// The debris's sideways speed at sway angle <paramref name="angle"/>: a quarter of the one-pixel sine velocity,
    /// rounded toward decreasing coordinates.
    /// </summary>
    internal static SignedEightEight SwayVelocity(TrigonometryAngle angle) => EnemyMath.SineVelocity(angle, 1).DividedRoundingDown(4);

    /// <summary>Advances the bubble by its vertical velocity and publishes its own horizontal position.</summary>
    private static void MoveNoobTubeBubbleVertically(EnemyProjectile projectile, NoobTubeDebrisState debris)
    {
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta()).WithWholeX(debris.X.Whole);
    }

    /// <summary>The tube's crack (#1760): it flickers, then falls, holding whether its flicker hides it.</summary>
    /// <param name="crack">What the crack keeps.</param>
    private sealed class CrackBehavior(NoobTubeCrackState crack)
        : EnemyProjectileMotionBehavior<NoobTubePieceMotion>(initial: null), IEnemyProjectileCues<NoobTubeProjectileCue>
    {
        /// <inheritdoc/>
        internal override bool IsHidden => crack.IsHidden;

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, NoobTubePieceMotion motion)
        {
            switch (motion)
            {
                case NoobTubePieceMotion.CrackFlickering:
                    // Alternate values of the short counter hide the crack and show it; it keeps whichever its flicker last
                    // chose once it falls.
                    crack.IsHidden = NoobTubeCadenceDefinitions.CrackHidden.IsSetIn(context.ShortCounter);
                    break;
                case NoobTubePieceMotion.CrackFalling:
                    DropNoobTubeCrack(context.Projectile);
                    break;
                case NoobTubePieceMotion.ShardFlying or NoobTubePieceMotion.ShardFalling or NoobTubePieceMotion.BubbleFlying or
                    NoobTubePieceMotion.BubbleFalling:
                    throw new InvalidOperationException($"The n00b tube's crack does not move by {nameof(NoobTubePieceMotion)} {motion}.");
                default:
                    throw new InvalidOperationException($"The n00b tube's crack does not move by {nameof(NoobTubePieceMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, NoobTubeProjectileCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue.Kind)
            {
                case NoobTubeProjectileCueKind.StartCrackFlicker:
                    projectile.StartMotion(NoobTubePieceMotion.CrackFlickering);
                    break;
                case NoobTubeProjectileCueKind.StartCrackFall:
                    projectile.StartMotion(NoobTubePieceMotion.CrackFalling);
                    break;
                case NoobTubeProjectileCueKind.Delete:
                    projectile.Delete();
                    break;
                case NoobTubeProjectileCueKind.FlickerReflected or NoobTubeProjectileCueKind.FlickerHidden or
                    NoobTubeProjectileCueKind.AssignShardFallingAngle or NoobTubeProjectileCueKind.AssignBubbleFallingAngle or
                    NoobTubeProjectileCueKind.StartShardFall or NoobTubeProjectileCueKind.StartBubbleRise or NoobTubeProjectileCueKind.StartBubbleFall:
                    throw new InvalidDataException($"The n00b tube's crack ran {nameof(NoobTubeProjectileCueKind)} {cue.Kind}, a cue of its shards and bubbles.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NoobTubeProjectileCueKind)} {cue.Kind}.");
            }
        }
    }

    /// <summary>
    /// A shard of the tube or a released-air bubble (#1760, #2044): it flies or rises, then sways down, holding its column,
    /// its sway and whether its flicker hides it, and the animation it plays, whose flicker shows its mirror image.
    /// </summary>
    /// <param name="room">The loaded room, whose random numbers set a falling piece's sway.</param>
    /// <param name="debris">What the piece keeps.</param>
    /// <param name="animation">The animation the piece plays from its spawn.</param>
    /// <param name="initial">How the piece moves from its spawn: a shard flies, a bubble holds still until its animation starts it.</param>
    private sealed class DebrisBehavior(EnemyRoomBinding room, NoobTubeDebrisState debris, NoobTubeProjectileAnimation animation,
        NoobTubePieceMotion? initial)
        : EnemyProjectileMotionBehavior<NoobTubePieceMotion>(initial), IEnemyProjectileCues<NoobTubeProjectileCue>
    {
        /// <inheritdoc/>
        internal override bool IsHidden => debris.IsHidden;

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, NoobTubePieceMotion motion)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (motion)
            {
                case NoobTubePieceMotion.ShardFlying:
                    FlyNoobTubeShard(projectile, debris, context.Camera.Y);
                    break;
                case NoobTubePieceMotion.ShardFalling:
                    DropNoobTubeShard(projectile, debris, context.Camera.Y);
                    break;
                case NoobTubePieceMotion.BubbleFalling:
                    DropNoobTubeBubble(projectile, debris);
                    break;
                case NoobTubePieceMotion.BubbleFlying:
                    MoveNoobTubeBubbleVertically(projectile, debris);
                    break;
                case NoobTubePieceMotion.CrackFlickering or NoobTubePieceMotion.CrackFalling:
                    throw new InvalidOperationException($"A n00b tube shard or bubble does not move by {nameof(NoobTubePieceMotion)} {motion}.");
                default:
                    throw new InvalidOperationException($"A n00b tube shard or bubble does not move by {nameof(NoobTubePieceMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, NoobTubeProjectileCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            bool atOwnColumn = NoobTubeCadenceDefinitions.DebrisAtOwnColumn.IsSetIn(context.ShortCounter);
            switch (cue.Kind)
            {
                case NoobTubeProjectileCueKind.FlickerReflected:
                    // The shard shows itself at its column, or its mirror image mirrored across the tube. A column past the
                    // room's left edge lies at the end of the room's axis (AdvanceNoobTubeShardX), and its mirror image
                    // lies as far past the room's right edge, on the axis as OffRoomPlaces keeps it (#2537).
                    int column = debris.X.Whole;
                    projectile.Position = projectile.Position.WithWholeX(atOwnColumn
                        ? column
                        : OffRoomPlaces.Place(NoobTubeDebrisDefinitions.ShardMirrorSpan - column));
                    animation.ShowsMirroredFrame = !atOwnColumn;
                    break;
                case NoobTubeProjectileCueKind.FlickerHidden:
                    // The shard shows at its column on odd updates and is hidden on even ones.
                    projectile.Position = projectile.Position.WithWholeX(debris.X.Whole);
                    debris.IsHidden = !atOwnColumn;
                    break;
                case NoobTubeProjectileCueKind.AssignShardFallingAngle:
                    debris.StartSway(NoobTubeDebrisDefinitions.FallingSwayStart.Of(room.Random.Generate()));
                    projectile.Velocity = projectile.Velocity.WithY(NoobTubeDebrisDefinitions.ShardFallYVelocity);
                    break;
                case NoobTubeProjectileCueKind.AssignBubbleFallingAngle:
                    debris.StartSway(NoobTubeDebrisDefinitions.FallingSwayStart.Of(room.Random.Generate()));
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
                case NoobTubeProjectileCueKind.StartCrackFlicker or NoobTubeProjectileCueKind.StartCrackFall:
                    throw new InvalidDataException($"A n00b tube shard or bubble ran {nameof(NoobTubeProjectileCueKind)} {cue.Kind}, a cue of its crack.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NoobTubeProjectileCueKind)} {cue.Kind}.");
            }
        }
    }
}
