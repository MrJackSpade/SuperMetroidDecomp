using SuperMetroid.Core.Animation;
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
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.NoobTubeCrack], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.PlacedOnly(new NoobTubeCrackEntity(occupant, place, common));

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

    /// <summary>What <see cref="SpawnNoobTubeProjectile"/> does for each <see cref="NoobTubeProjectileDefinition"/> member, each member's own arm.</summary>
    private IdentityTable<NoobTubeProjectileDefinition, Action<NoobTubePiece, EnemyProjectile, Position16<RoomPixels>>> SpawnNoobTubeProjectileForNoobTubeProjectileDefinition => field ??= new(new Dictionary<NoobTubeProjectileDefinition, Action<NoobTubePiece, EnemyProjectile, Position16<RoomPixels>>>
    {
        [NoobTubeProjectileDefinition.Crack] = (piece, projectile, place) =>
        {
            TubePieces.Pieces.Start(projectile, new NoobTubeDebrisState(place.X));
            projectile.Position = projectile.Position.WithWhole(place);
        },
        [NoobTubeProjectileDefinition.Shard] = (piece, projectile, place) =>
        {
            TubePieces.Pieces.Start(projectile, new NoobTubeDebrisState(place.X));
            projectile.Position = projectile.Position.WithWholeY(place.Y);
            projectile.Velocity = piece.Velocity;
        },
        [NoobTubeProjectileDefinition.ReleasedAirBubble] = (piece, projectile, place) =>
        {
            TubePieces.Pieces.Start(projectile, new NoobTubeDebrisState(place.X));
            projectile.Position = projectile.Position.WithWholeY(place.Y);
            // The bubble sets only its rise: it sways by its own arc, and the sideways speed its place's previous
            // projectile left passes on to the next projectile spawned there (EnemyProjectileHandOver).
            projectile.Velocity = projectile.Velocity.WithY(piece.Velocity.Y);
        },
    });

    /// <summary>
    /// Spawns the crack, glass shard or released air bubble the request names, at the tube's block the request
    /// carries.
    /// </summary>
    public void SpawnNoobTubeProjectile(
        NoobTubeProjectileRequest request)
    {
        NoobTubePiece piece = request.Piece;
        RoomEnemyProjectileKind kind = PieceKinds[piece.Definition];

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(kind, TubePieces, graphics: TubePieceGraphics, new NoobTubeProjectileAnimation(TubePieces, piece.Start.Match(PieceStartAnimations, 0)));
        if (projectile is null)
            return;
        // The tube's pieces draw from the shared sheet in the tube's crack colours (#1512).

        Position16<RoomPixels> place = request.RoomObjectBlock.ToRoomPixels() + piece.Offset;
        SpawnNoobTubeProjectileForNoobTubeProjectileDefinition[piece.Definition](piece, projectile, place);
    }

    /// <summary>Drops the detached crack at its fixed fall speed.</summary>
    private static void DropNoobTubeCrack(EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.AdvanceY(NoobTubeDebrisDefinitions.CrackFallVelocity.ToSixteenSixteenDelta());

    /// <summary>Flies a newly emitted shard by its velocity and deletes it once it leaves the screen vertically.</summary>
    private static void FlyNoobTubeShard(NoobTubeProjectileBehavior owner, EnemyProjectile projectile, int cameraY)
    {
        AdvanceNoobTubeShardX(owner, projectile, projectile.Velocity.X);
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <summary>Sways a falling shard along its arc, drops it, and deletes it once it leaves the screen vertically.</summary>
    private static void DropNoobTubeShard(NoobTubeProjectileBehavior owner, EnemyProjectile projectile, int cameraY)
    {
        MoveNoobTubeProjectileHorizontallyAlongArc(owner, projectile, NoobTubeDebrisMotionDefinitions.ShardSwaySteps);
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <summary>Sways a falling bubble along its arc, then moves it vertically once as a flying bubble does.</summary>
    private static void DropNoobTubeBubble(NoobTubeProjectileBehavior owner, EnemyProjectile projectile)
    {
        MoveNoobTubeProjectileHorizontallyAlongArc(owner, projectile, NoobTubeDebrisMotionDefinitions.BubbleSwaySteps);
        MoveNoobTubeBubbleVertically(owner, projectile);
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
    private static void AdvanceNoobTubeShardX(NoobTubeProjectileBehavior owner, EnemyProjectile projectile, SignedEightEight velocity)
    {
        NoobTubeDebrisState debris = owner.Pieces.Of(projectile);
        debris.X = debris.X.MovedBy(velocity);
    }

    /// <summary>Uses the sine of the debris's sway to advance its own horizontal position, then steps the sway.</summary>
    private static void MoveNoobTubeProjectileHorizontallyAlongArc(
        NoobTubeProjectileBehavior owner,
        EnemyProjectile projectile,
        int swaySteps)
    {
        NoobTubeDebrisState debris = owner.Pieces.Of(projectile);
        AdvanceNoobTubeShardX(owner, projectile, SwayVelocity(debris.SwayAngle));
        debris.AdvanceSway(swaySteps);
    }

    /// <summary>
    /// The debris's sideways speed at sway angle <paramref name="angle"/>: a quarter of the one-pixel sine velocity,
    /// rounded toward decreasing coordinates.
    /// </summary>
    internal static SignedEightEight SwayVelocity(TrigonometryAngle angle) => EnemyMath.SineVelocity(angle, 1).DividedRoundingDown(4);

    /// <summary>Advances the bubble by its vertical velocity and publishes its own horizontal position.</summary>
    private static void MoveNoobTubeBubbleVertically(NoobTubeProjectileBehavior owner, EnemyProjectile projectile)
    {
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta()).WithWholeX(owner.Pieces.Of(projectile).X.Whole);
    }

    /// <summary>The tube's crack, shards and released-air bubbles (#1760): their spawner starts their animations.</summary>
    private NoobTubeProjectileBehavior TubePieces => field ??= new NoobTubeProjectileBehavior(_room);

    /// <summary>The enemy projectile each n00b-tube piece spawns as (#2553): its crack, a glass shard or a released bubble.</summary>
    private static IdentityTable<NoobTubeProjectileDefinition, RoomEnemyProjectileKind> PieceKinds { get; } = new(new Dictionary<NoobTubeProjectileDefinition, RoomEnemyProjectileKind>
    {
        [NoobTubeProjectileDefinition.Crack] = RoomEnemyProjectileKind.NoobTubeCrack,
        [NoobTubeProjectileDefinition.Shard] = RoomEnemyProjectileKind.NoobTubeShard,
        [NoobTubeProjectileDefinition.ReleasedAirBubble] = RoomEnemyProjectileKind.NoobTubeReleasedAirBubble,
    });

    /// <summary>The animation each start of a piece plays (#2553): the one beginning at its segment, or its shard's flight.</summary>
    private static NoobTubePieceStartCases<int, Animation<NoobTubeProjectileSegment>> PieceStartAnimations { get; } = new(
        AtSegment: static (start, _) => NoobTubeProjectileAnimations.StartingWith(start.Segment),
        ShardFlight: static (flight, _) => NoobTubeProjectileAnimations.ShardAnimations[flight.Shard]);

    /// <summary>A crack, shard or bubble: the crack's, shards' and bubbles' movers, and their animations' cues.</summary>
    /// <param name="room">The loaded room, whose random numbers set a falling piece's sway.</param>
    internal sealed class NoobTubeProjectileBehavior(EnemyRoomBinding room)
        : EnemyProjectileMotionBehavior<NoobTubePieceMotion>(StartingMotions)
    {
        /// <summary>What each of the tube's pieces keeps: its column, its flicker and its sway, given as the tube breaks.</summary>
        internal EnemyProjectileStates<NoobTubeDebrisState> Pieces { get; } = new();

        /// <inheritdoc/>
        internal override bool IsHidden(EnemyProjectile projectile) => Pieces.Of(projectile).IsHidden;

        /// <summary>A shard flies from its spawn; the crack and the bubbles hold still until their animations start them.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, NoobTubePieceMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.NoobTubeShard] = NoobTubePieceMotion.ShardFlying,
            [RoomEnemyProjectileKind.NoobTubeCrack] = null,
            [RoomEnemyProjectileKind.NoobTubeReleasedAirBubble] = null,
        };

        /// <summary>What <c>Move</c> does for each <see cref="NoobTubePieceMotion"/> member, each member's own arm.</summary>
        private IdentityTable<NoobTubePieceMotion, Action<EnemyProjectileMotionContext, EnemyProjectile>> MoveForNoobTubePieceMotion => field ??= new(new Dictionary<NoobTubePieceMotion, Action<EnemyProjectileMotionContext, EnemyProjectile>>
        {
            [NoobTubePieceMotion.CrackFlickering] = (context, projectile) => FlickerNoobTubeCrack(projectile, context.ShortCounter),
            [NoobTubePieceMotion.CrackFalling] = (context, projectile) => DropNoobTubeCrack(projectile),
            [NoobTubePieceMotion.ShardFlying] = (context, projectile) => FlyNoobTubeShard(this, projectile, context.Camera.Y),
            [NoobTubePieceMotion.ShardFalling] = (context, projectile) => DropNoobTubeShard(this, projectile, context.Camera.Y),
            [NoobTubePieceMotion.BubbleFalling] = (context, projectile) => DropNoobTubeBubble(this, projectile),
            [NoobTubePieceMotion.BubbleFlying] = (context, projectile) => MoveNoobTubeBubbleVertically(this, projectile),
        });

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, NoobTubePieceMotion motion)
        {
            EnemyProjectile projectile = context.Projectile;
            MoveForNoobTubePieceMotion[motion](context, projectile);
        }

        /// <inheritdoc/>
        /// <summary>
        /// Carries out one event of a piece's animation, which hands the behaviour the mirroring of the frames it shows: a
        /// shard's reflected flicker shows its mirrored frames on the updates it draws at its mirror image.
        /// </summary>
        /// <param name="context">The piece and the pass it runs in.</param>
        /// <param name="cue">The event the piece's animation raised.</param>
        /// <param name="mirroring">Whether the piece's animation shows its mirrored frames.</param>
        internal void RunCue(EnemyProjectileCueContext context, NoobTubeProjectileCue cue, NoobTubeShardMirroring mirroring) =>
            ApplyNoobTubeProjectileCue(context.Projectile, context.ShortCounter, cue, Ensure.NotNull(mirroring));

        /// <summary>
        /// Flickers the crack: alternate values of <paramref name="shortCounter"/> hide it and show it. The crack keeps
        /// whichever its flicker last chose once it falls.
        /// </summary>
        private void FlickerNoobTubeCrack(EnemyProjectile projectile, Compatibility.ShortFrameCount shortCounter) =>
            Pieces.Of(projectile).IsHidden = NoobTubeCadenceDefinitions.CrackHidden.IsSetIn(shortCounter);

        /// <summary>What <see cref="ApplyNoobTubeProjectileCue"/> does for each <see cref="NoobTubeProjectileCueKind"/> member, each member's own arm.</summary>
        private IdentityTable<NoobTubeProjectileCueKind, Action<EnemyProjectile, NoobTubeShardMirroring, bool>> ApplyNoobTubeProjectileCueForNoobTubeProjectileCueKind => field ??= new(new Dictionary<NoobTubeProjectileCueKind, Action<EnemyProjectile, NoobTubeShardMirroring, bool>>
        {
            [NoobTubeProjectileCueKind.FlickerReflected] = (projectile, mirroring, atOwnColumn) =>
            {
                // The shard shows itself at its column, or its mirror image mirrored across the tube. A column past the
                // room's left edge lies at the end of the room's axis (AdvanceNoobTubeShardX), and its mirror image
                // lies as far past the room's right edge, on the axis as OffRoomPlaces keeps it (#2537).
                int column = Pieces.Of(projectile).X.Whole;
                projectile.Position = projectile.Position.WithWholeX(atOwnColumn
                    ? column
                    : OffRoomPlaces.Place(NoobTubeDebrisDefinitions.ShardMirrorSpan - column));
                mirroring.ShowsMirroredFrame = !atOwnColumn;
            },
            [NoobTubeProjectileCueKind.FlickerHidden] = (projectile, mirroring, atOwnColumn) =>
            {
                // The shard shows at its column on odd updates and is hidden on even ones.
                projectile.Position = projectile.Position.WithWholeX(Pieces.Of(projectile).X.Whole);
                Pieces.Of(projectile).IsHidden = !atOwnColumn;
            },
            [NoobTubeProjectileCueKind.AssignShardFallingAngle] = (projectile, mirroring, atOwnColumn) =>
            {
                Pieces.Of(projectile).StartSway(NoobTubeDebrisDefinitions.FallingSwayStart.Of(room.Random.Generate()));
                projectile.Velocity = projectile.Velocity.WithY(NoobTubeDebrisDefinitions.ShardFallYVelocity);
            },
            [NoobTubeProjectileCueKind.AssignBubbleFallingAngle] = (projectile, mirroring, atOwnColumn) => Pieces.Of(projectile).StartSway(NoobTubeDebrisDefinitions.FallingSwayStart.Of(room.Random.Generate())),
            [NoobTubeProjectileCueKind.StartCrackFlicker] = (projectile, mirroring, atOwnColumn) => projectile.StartMotion(this, NoobTubePieceMotion.CrackFlickering),
            [NoobTubeProjectileCueKind.StartCrackFall] = (projectile, mirroring, atOwnColumn) => projectile.StartMotion(this, NoobTubePieceMotion.CrackFalling),
            [NoobTubeProjectileCueKind.StartShardFall] = (projectile, mirroring, atOwnColumn) => projectile.StartMotion(this, NoobTubePieceMotion.ShardFalling),
            [NoobTubeProjectileCueKind.StartBubbleRise] = (projectile, mirroring, atOwnColumn) => projectile.StartMotion(this, NoobTubePieceMotion.BubbleFlying),
            [NoobTubeProjectileCueKind.StartBubbleFall] = (projectile, mirroring, atOwnColumn) => projectile.StartMotion(this, NoobTubePieceMotion.BubbleFalling),
            [NoobTubeProjectileCueKind.Delete] = (projectile, mirroring, atOwnColumn) => projectile.Delete(),
        });

        /// <summary>Carries out one cue of the crack, shard and bubble animations; <paramref name="shortCounter"/> times the flicker.</summary>
        private void ApplyNoobTubeProjectileCue(EnemyProjectile projectile, Compatibility.ShortFrameCount shortCounter, NoobTubeProjectileCue cue, NoobTubeShardMirroring mirroring)
        {
            bool atOwnColumn = NoobTubeCadenceDefinitions.DebrisAtOwnColumn.IsSetIn(shortCounter);
            ApplyNoobTubeProjectileCueForNoobTubeProjectileCueKind[cue.Kind](projectile, mirroring, atOwnColumn);
        }
    }
}
