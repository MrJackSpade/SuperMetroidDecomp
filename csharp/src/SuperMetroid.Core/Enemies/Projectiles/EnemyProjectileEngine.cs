using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The enemy-projectile engine: the room's enemy-projectile places (<see cref="EnemyProjectilePlaces"/>), spawning a fresh projectile into a free
/// place and its definition setup, the per-frame pass (motions, then each animation), the animation runner,
/// both draw-priority passes and the Samus collision passes. Each projectile runs through the behaviour its spawner bound
/// to it (<see cref="EnemyProjectileBehavior"/>, #1760); the engine names no species.
/// <para>
/// The places are ordered, and gameplay reads the order: the compatibility service <see cref="EnemyProjectilePlaces"/>
/// holds their number and the order spawns search them and every pass runs them.
/// </para>
/// </summary>
[RoomLifetime]
internal sealed class EnemyProjectileEngine
{
    /// <summary>What <see cref="ApplyCommonCue"/> does for each <see cref="EnemyProjectileCommonCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<EnemyProjectileCommonCueKind, Action<EnemyProjectile>> ApplyCommonCueForEnemyProjectileCommonCueKind => field ??= new(new Dictionary<EnemyProjectileCommonCueKind, Action<EnemyProjectile>>
    {
        [EnemyProjectileCommonCueKind.Delete] = static projectile => projectile.Delete(),
        [EnemyProjectileCommonCueKind.ReturnToOrigin] = static projectile => projectile.Position = projectile.Position.WithWhole(projectile.Anchor),
    });

    /// <summary>Carries out one shared cue of a typed projectile animation, for every kind whose animations publish them.</summary>
    internal void ApplyCommonCue(EnemyProjectile projectile, EnemyProjectileCommonCue cue)
    {
        cue.React(ApplyCommonCueReactions, new ApplyCommonCueContext(projectile, this));
    }

    /// <summary>What <see cref="ApplyCommonCue"/> does on each <see cref="EnemyProjectileCommonCue"/>, built once (#2553).</summary>
    private static EnemyProjectileCommonCueReactions<ApplyCommonCueContext> ApplyCommonCueReactions { get; } = new(
        Scatter: static (scatter, context) => context.Owner.Scatter(context.Projectile, scatter.Spread),
        Signal: static (signal, context) =>
        {
            ApplyCommonCueForEnemyProjectileCommonCueKind[signal.Kind](context.Projectile);
        });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Projectile">The <c>projectile</c> <see cref="ApplyCommonCue"/> was handed.</param>
    /// <param name="Owner">The <see cref="EnemyProjectileEngine"/> reacting.</param>
    private readonly record struct ApplyCommonCueContext(EnemyProjectile Projectile, EnemyProjectileEngine Owner);

    /// <summary>Supplies room RNG and artwork for projectile effects.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes projectiles from the room's enemy artwork.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Owns dust and explosion sprites separate from the projectile places.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>Publishes room quake effects requested by projectile cues.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues species projectile sounds under queue limits.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the room's enemy projectiles to their places, room sprites, earthquakes and audio.</summary>
    internal EnemyProjectileEngine(EnemyRoomBinding room, EnemyPresentation presentation, RoomEffectActors effectActors,
        RoomEarthquake earthquake, EnemyAudioQueue audio, EnemyProjectilePlaces places)
    {
        _room = room;
        _presentation = presentation;
        _effectActors = effectActors;
        _earthquake = earthquake;
        _audio = audio;
        Places = Ensure.NotNull(places);
    }

    /// <summary>The room's enemy-projectile places, the order spawns search them and passes run them, and what the latest spawn took.</summary>
    internal EnemyProjectilePlaces Places { get; }

    /// <summary>
    /// Each place's latest projectile of this room, live or gone, or null while the room has spawned none into it: the place
    /// is free while it holds none, or its projectile is gone or has freed it.
    /// </summary>
    private readonly EnemyProjectile?[] _places = new EnemyProjectile?[EnemyProjectilePlaces.Capacity];

    /// <summary>What each new projectile starts from: what its place's previous projectile left there, in this room or the room before.</summary>
    private readonly EnemyProjectilePlaceLeftovers _leftovers = new(EnemyProjectilePlaces.Capacity);

    /// <summary>
    /// The place the pre-Phantoon room's setup holds, while it holds it (#1851, #2556); none while the room holds no place.
    /// No projectile stands in it: a spawn passes it by, the projectile pass counts its hold at its turn, and it keeps what
    /// its previous projectile left there for the spawn that takes it once the hold frees it.
    /// </summary>
    private HeldPlace? _heldPlace;

    /// <summary>A place the room holds and the hold that counts it.</summary>
    /// <param name="Place">The zero-based enemy-projectile place.</param>
    /// <param name="Hold">The hold, which frees the place on the update after it runs out.</param>
    private sealed record HeldPlace(int Place, PrePhantoonRoomHeldProjectilePlace Hold);

    /// <summary>Whether <paramref name="place"/> is free for a spawn: no live projectile and no hold of the room's holds it.</summary>
    private bool IsFree(int place) => _places[place] is not { IsActive: true } && _heldPlace?.Place != place;

    /// <summary>The projectiles holding their places, in place order: the pool's occupants (#2558).</summary>
    public IEnumerable<EnemyProjectile> EnemyProjectiles => _places.OfType<EnemyProjectile>().Where(projectile => projectile.IsActive);

    /// <summary>How many places neither a live projectile nor the room's hold holds: the free places a spawn can take.</summary>
    public int FreePlaceCount => Enumerable.Range(0, EnemyProjectilePlaces.Capacity).Count(IsFree);

    /// <summary>
    /// The latest projectile of the place spawns search last, live or gone, or null while the room has spawned none into it:
    /// the one a falling statue piece's acceleration reads (<see cref="FirstPlaceFallAcceleration"/>, #2556).
    /// </summary>
    internal EnemyProjectile? LatestInFirstPlace => _places[0];

    /// <summary>
    /// Starts each place from what the room this one replaces left in it (<see cref="EnemyProjectilePlaceLeftovers"/>), which
    /// the room's first spawn there starts from. Called before the room's population loads.
    /// </summary>
    internal void TakeOverFrom(RoomHandOver handOver)
    {
        Array.Clear(_places);
        _heldPlace = null;
        _leftovers.TakeOverFrom(handOver);
    }

    /// <summary>Leaves what each place's latest projectile left there for the room that replaces this one (<see cref="EnemyProjectilePlaceLeftovers"/>).</summary>
    internal void HandOverTo(RoomHandOver handOver) =>
        _leftovers.HandOverTo(handOver, [.. _places.Select(projectile => projectile?.LeftInPlace)]);

    /// <summary>
    /// Queues the dud sound a rejected Samus shot makes, at <paramref name="priority"/>: a protected body part's dud competes at
    /// low priority, an indestructible projectile's and Ridley's tail's at normal priority.
    /// </summary>
    internal void QueueDudSound(SoundRequestPriority priority) =>
        _audio.QueueEnemySound(SamusSound.DudShot, priority);

    /// <summary>
    /// Tests enemy projectiles against Samus's projectiles and handles each hit. An indestructible dud, such as a
    /// Nuclear Waffle link, makes the dud sound and stays; a projectile that plays its shot animation, such as Kago's
    /// bug, switches to it.
    /// </summary>
    public void ResolveEnemyProjectileSamusProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem samusBombs)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(samusBombs);

        foreach (int enemyProjectileIndex in EnemyProjectilePlaces.InRunOrder)
        {
            if (_places[enemyProjectileIndex] is not { IsActive: true } enemyProjectile ||
                !enemyProjectile.BlocksSamusProjectiles ||
                enemyProjectile.CollisionOption == EnemyProjectileCollisionOption.NoCollision)
                continue;

            foreach (SamusProjectile shot in projectiles.Shots)
            {
                if (shot.Occupant is not { HasEnemyCollisionPayload: true } held)
                    continue;

                SamusProjectileType shotType = held.Type;
                // Bombs and the explosions shots leave behind strike no enemy projectile.
                if (shotType.Family.IsBombOrExplosion)
                    continue;

                if (!ShotAndProjectileSquares.Meet(shot.Position.Whole, enemyProjectile.Position.Whole))
                    continue;

                // A shot that meets the projectile is only marked spent, a plasma shot not even that: it strikes no
                // enemy, so a freshly fired Super Missile (or its invisible link) does not explode on the update it was
                // fired. The shot acts on the mark as it moves on the next projectile pass.
                if (!held.Type.HasAnyBeam(SamusBeamFlags.Plasma))
                    shot.Spend();

                if (enemyProjectile.CollisionOption == EnemyProjectileCollisionOption.IndestructibleDud)
                {
                    QueueDudSound(SoundRequestPriority.Normal);
                }
                else
                {
                    // Every shot animation without a typed reaction of its own is the shared delete animation.
                    enemyProjectile.StartAnimation(BehaviorOf(enemyProjectile).ShotAnimation());
                    enemyProjectile.HoldStill();

                    // A shot that destroys a projectile strips its behaviours and keeps only its damage: it no
                    // longer blocks shots or outlasts touching Samus, and it harms her on contact, whatever its
                    // definition said. Every destructible projectile is stripped alike, not only the Kago's.
                    enemyProjectile.BlocksSamusProjectiles = false;
                    enemyProjectile.PersistsOnSamusContact = false;
                    enemyProjectile.CanDamageSamus = true;
                }
            }
        }
    }

    /// <summary>One projectile pass: every live projectile's motion, then its animation.</summary>
    /// <param name="level">The room's blocks, which moving projectiles collide with.</param>
    /// <param name="samus">Samus, for behaviours and cues that act on her.</param>
    /// <param name="camera">The camera, for behaviours and cues that test the screen.</param>
    /// <param name="shortCounter">The enemies' short update count for this pass, which cadences sample.</param>
    /// <param name="samusBombs">Samus's bombs, for behaviours that react to them.</param>
    /// <param name="frameCounter">The game's update count for this pass, which cadences sample.</param>
    internal void RunPass(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomPixels> camera,
        Compatibility.ShortFrameCount shortCounter,
        SamusBombProjectileSystem samusBombs,
        int frameCounter)
    {
        Ensure.NotNull(level);

        // The pass runs the places from the last down, reading each place when it reaches it rather than a pass-start
        // snapshot. The distinction is observable whenever a motion or animation cue spawns another projectile:
        // spawns search from the last place down, so one spawned below the place being run is reached later in THIS
        // pass, and one spawned above it waits for the next.
        foreach (int place in EnemyProjectilePlaces.InRunOrder)
        {
            if (_heldPlace is { } held && held.Place == place)
            {
                if (held.Hold.FreesOnThisUpdate())
                    _heldPlace = null;
                continue;
            }
            if (_places[place] is not { IsActive: true } projectile)
                continue;

            BehaviorOf(projectile).RunMotion(
                new EnemyProjectileMotionContext(projectile, level, samus, camera, shortCounter, frameCounter, samusBombs));
            // The animation that runs next is that of whatever projectile the place holds after the motion, live
            // or not. A projectile that ended and spawned its replacement into its own place hands the rest of the pass
            // to the replacement, whose animation starts in this pass. One that freed its place without ending finishes
            // its update with its animation (a power-bombed Draygon goop that reaches Samus sticks to her through its
            // touch animation, EnemyProjectile.FreePlace). One that ended plays no animation, so nothing runs.
            AdvanceEnemyProjectileAnimations(_places[place] ?? throw new InvalidOperationException(
                $"Enemy-projectile place {place} lost its projectile during its own motion."), samus, camera, shortCounter, samusBombs);
        }
    }

    /// <summary>Publishes projectile hurt requests after room objects; the next Samus phase consumes them.</summary>
    public void ResolveEnemyProjectileSamusHits(SamusState samus)
    {
        // Gameplay runs every projectile's update first, then the Samus-collision pass. That pass reads Samus's
        // invincibility and contact damage once as it starts, and damage from one overlapping projectile does not stop
        // the others from meeting her; this shows on the Nuclear Waffle, whose four lasting body links begin in one
        // place.
        Ensure.NotNull(samus);
        if (!samus.InvincibilityTimer.IsZero ||
            samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None)
            return;

        KnockbackHorizontalDirection? finalKnockbackXDirection = null;
        foreach (int index in EnemyProjectilePlaces.InRunOrder)
        {
            if (_places[index] is { IsActive: true } projectile)
            {
                finalKnockbackXDirection =
                    ResolveEnemyProjectileSamusCollision(projectile, samus) ??
                    finalKnockbackXDirection;
            }
        }

        // The contact only publishes the knockback request and overwrites its horizontal
        // direction for every hit. Samus's update has already run this frame: do not initialize
        // a hurt pose or move Samus here. The next frame consumes the final request.
        if (finalKnockbackXDirection.HasValue)
        {
            samus.KnockbackXDirection = finalKnockbackXDirection.Value;
            samus.KnockbackHold = UpdateCountdown.For(EnemyCollisionDefinitions.SamusTouchKnockbackUpdates);
        }
    }

    /// <summary>
    /// Draws the room's effect actors and then the projectiles in front of Samus. The game draws them before Samus,
    /// so they are drawn over her.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="camera">Camera position in whole room pixels.</param>
    /// <param name="hitBursts">What the hit bursts among the effect actors draw: Samus's projectile sheets and her equipped beams.</param>
    /// <param name="timeIsFrozen">Whether time is frozen, which stops the earthquake displacement.</param>
    public void DrawEnemyProjectilesInFrontOfSamus(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera,
        SamusHitBurstArtwork hitBursts,
        bool timeIsFrozen)
    {
        Ensure.NotNull(sprites);

        // Effect actors are drawn once, with the projectiles in front of Samus. Drawing them again with the
        // projectiles behind her would duplicate explosions, dust, and Spark's trail in the same frame.
        _effectActors.DrawRoomEffectActors(sprites, camera, hitBursts);
        DrawEnemyProjectilesAt(
            sprites,
            camera,
            EnemyProjectileDepth.InFrontOfSamus, timeIsFrozen);
    }

    /// <summary>Draws the projectiles behind Samus. The game draws them after Samus, so she is drawn over them.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="camera">Camera position in whole room pixels.</param>
    /// <param name="timeIsFrozen">Whether time is frozen, which stops the earthquake displacement.</param>
    public void DrawEnemyProjectilesBehindSamus(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera,
        bool timeIsFrozen)
    {
        Ensure.NotNull(sprites);

        DrawEnemyProjectilesAt(
            sprites,
            camera,
            EnemyProjectileDepth.BehindSamus, timeIsFrozen);
    }

    /// <summary>
    /// Draws the projectiles at <paramref name="depth"/>, the last place first: among projectiles at one depth, one in a
    /// later place is drawn over one in an earlier place.
    /// </summary>
    private void DrawEnemyProjectilesAt(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera,
        EnemyProjectileDepth depth,
        bool timeIsFrozen)
    {
        Offset16<RoomPixels> shake = _earthquake.GetEnemyProjectileShake(timeIsFrozen);
        foreach (int place in EnemyProjectilePlaces.InRunOrder)
        {
            if (_places[place] is not { IsActive: true } projectile ||
                BehaviorOf(projectile).IsHidden(projectile) ||
                projectile.Depth != depth ||
                projectile.Frame is not { } frame)
                continue;

            // Every placement is published wherever it lies; the display clips it to the view (#1520).
            Offset16<ScreenPixels> screen = (projectile.Position.Whole + projectile.FramePlacement + shake).RelativeToCamera(camera);

            _presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.EnemyProjectile),
                EnemySheetDefinitions.ProjectileFrameName(frame), screen, projectile.ShownGraphics);
        }
    }

    /// <summary>
    /// Spawns a fresh projectile of <paramref name="kind"/> into the first free place searched from the last down, starting
    /// from what the place's previous projectile left there (<see cref="EnemyProjectilePlaceLeftovers"/>) and copying its
    /// kind's definition (<see cref="InitializeEnemyProjectileFromDefinition"/>), or returns null when every place is held.
    /// </summary>
    /// <param name="kind">What the projectile is, which names its definition.</param>
    /// <param name="behavior">What the projectile's kind does, bound to it for its life (#1760), which starts its motion (#1843).</param>
    /// <param name="graphics">The sheet and shading the projectile draws with.</param>
    /// <param name="animation">The animation the projectile plays from its first pass (#2553): every spawn starts one.</param>
    internal EnemyProjectile? AllocateEnemyProjectile(RoomEnemyProjectileKind kind, EnemyProjectileBehavior behavior, SpriteGraphics graphics,
        EnemyProjectileAnimation animation) =>
        FindVacancy() is { } vacancy ? Spawn(vacancy, kind, behavior, graphics, animation) : null;

    /// <summary>
    /// The first free place the spawn search finds, from the last down, or none when every place is held (#2553): a spawner
    /// that chooses its projectile's animation by a random draw the game makes only once a place is found looks first, and
    /// spawns into the place before any other projectile spawns.
    /// </summary>
    internal EnemyProjectileVacancy? FindVacancy()
    {
        foreach (int place in EnemyProjectilePlaces.InRunOrder)
        {
            if (!IsFree(place))
                continue;
            Places.RecordSpawn(place);
            return new EnemyProjectileVacancy(place);
        }
        Places.RecordSpawn(null);
        return null;
    }

    /// <summary>
    /// Spawns a fresh projectile of <paramref name="kind"/> into <paramref name="vacancy"/>, starting from what the place's
    /// previous projectile left there (<see cref="EnemyProjectilePlaceLeftovers"/>) and copying its kind's definition
    /// (<see cref="InitializeEnemyProjectileFromDefinition"/>), playing <paramref name="animation"/> from its first pass.
    /// </summary>
    /// <exception cref="InvalidOperationException">A projectile has spawned into the place since it was found free.</exception>
    internal EnemyProjectile Spawn(EnemyProjectileVacancy vacancy, RoomEnemyProjectileKind kind, EnemyProjectileBehavior behavior,
        SpriteGraphics graphics, EnemyProjectileAnimation animation)
    {
        int place = vacancy.Place;
        if (!IsFree(place))
            throw new InvalidOperationException($"Enemy projectile place {place} was taken after the spawn search found it free.");
        var projectile = new EnemyProjectile(place, _leftovers.StartOf(place, _places[place]?.LeftInPlace), kind, animation);
        _places[place] = projectile;
        InitializeEnemyProjectileFromDefinition(projectile, kind, behavior, graphics);
        return projectile;
    }

    /// <summary>
    /// Holds the first free place, searched from the last down as a spawn searches, for the pre-Phantoon room's setup
    /// (<see cref="PrePhantoonRoomHeldProjectilePlace"/>): the setup takes the place as a spawn does, and the place stays
    /// taken until the projectile pass counts the hold out or a room load clears the pool.
    /// </summary>
    /// <param name="hold">The hold that counts the place.</param>
    /// <exception cref="InvalidOperationException">Every place is held.</exception>
    internal void HoldFirstFreePlace(PrePhantoonRoomHeldProjectilePlace hold)
    {
        Ensure.NotNull(hold);
        foreach (int place in EnemyProjectilePlaces.InRunOrder)
        {
            if (IsFree(place))
            {
                Places.RecordSpawn(place);
                hold.Take();
                _heldPlace = new HeldPlace(place, hold);
                return;
            }
        }
        throw new InvalidOperationException("The pre-Phantoon room's held place found no free place in a freshly cleared pool.");
    }

    /// <summary>
    /// Copies the definition a projectile spawn installs; a family's setup may then replace fields as its own setup
    /// does.
    /// </summary>
    /// <param name="projectile">The freshly spawned projectile.</param>
    /// <param name="kind">What the projectile is, which names its definition.</param>
    /// <param name="behavior">What the projectile's kind does, bound to it for its life (#1760), which starts its motion (#1843).</param>
    /// <param name="graphics">The sheet and shading the projectile draws with.</param>
    private static void InitializeEnemyProjectileFromDefinition(
        EnemyProjectile projectile,
        RoomEnemyProjectileKind kind,
        EnemyProjectileBehavior behavior,
        SpriteGraphics graphics)
    {
        EnemyProjectileDefinition definition = EnemyProjectileKinds.Get(kind);
        projectile.Bind(kind, behavior);
        // The spawn starts the animation timer at one, so the animation the spawner chose shows its first frame on the
        // first pass.
        projectile.FrameHold = WrappingCountdown.OneFrame;

        // The spawn shows no frame before the first animation tick: it is hidden until its animation shows the first frame
        // on its own pass.
        projectile.Frame = null;
        projectile.Radius = definition.Radius;
        projectile.Damage = definition.Damage;
        projectile.Depth = definition.Depth;
        projectile.CanDamageSamus = definition.CanDamageSamus;
        projectile.PersistsOnSamusContact = definition.PersistsOnSamusContact;
        projectile.BlocksSamusProjectiles = definition.BlocksSamusProjectiles;
        projectile.CollisionOption = EnemyProjectileCollisionOption.PlaysShotAnimation;
        projectile.Graphics = graphics;
    }

    /// <summary>The behaviour bound to <paramref name="projectile"/> at its spawn.</summary>
    /// <exception cref="InvalidOperationException">The projectile was given a kind without a behaviour.</exception>
    private static EnemyProjectileBehavior BehaviorOf(EnemyProjectile projectile) =>
        projectile.Behavior ?? throw new InvalidOperationException(
            $"Enemy projectile {projectile.Kind} in place {projectile.Place} has no behaviour bound.");

    /// <summary>
    /// Moves one projectile axis by its own velocity on that axis through each crossed room block, colliding with the
    /// room's terrain (<see cref="EnemyProjectileTerrainCollision"/>) and clamping it flush at the first collision.
    /// </summary>
    internal static bool MoveProjectileAxis(
        EnemyProjectile projectile,
        RoomLevelData level,
        Axis axis) =>
        MoveProjectileAxis(projectile, level, axis, EnemyProjectileTerrainCollision.Rule);

    /// <summary>
    /// Moves one projectile axis by <paramref name="speed"/> instead of the projectile's own velocity on that axis, through
    /// each crossed room block, colliding with the room's terrain and clamping it flush at the first collision.
    /// </summary>
    internal static bool MoveProjectileAxis(
        EnemyProjectile projectile,
        RoomLevelData level,
        Axis axis,
        SignedEightEight speed) =>
        MoveProjectileAxis(projectile, level, axis, speed, EnemyProjectileTerrainCollision.Rule);

    /// <summary>
    /// Moves one projectile axis by its own velocity on that axis through each crossed room block, stopping where
    /// <paramref name="collision"/> says a block stops it.
    /// </summary>
    internal static bool MoveProjectileAxis(
        EnemyProjectile projectile,
        RoomLevelData level,
        Axis axis,
        EnemyProjectileBlockCollision collision) =>
        MoveProjectileAxis(projectile, level, axis, axis == Axis.Horizontal ? projectile.Velocity.X : projectile.Velocity.Y, collision);

    /// <summary>
    /// Moves one projectile axis by <paramref name="speed"/> through each crossed room block, stopping where
    /// <paramref name="collision"/> says a block stops it: the move then clears the axis's fraction and places the
    /// projectile where the block aligns it, or flush against the block's face when that does not move it backwards.
    /// </summary>
    private static bool MoveProjectileAxis(
        EnemyProjectile projectile,
        RoomLevelData level,
        Axis axis,
        SignedEightEight speed,
        EnemyProjectileBlockCollision collision)
    {
        FixedCoordinate current = axis == Axis.Horizontal ? projectile.Position.X : projectile.Position.Y;
        int position = current.Whole;
        bool movingNegative = speed.IsNegative;
        FixedCoordinate next = current.MovedBy(speed.ToSixteenSixteenDelta());
        int nextPosition = next.Whole;

        int movementRadius = axis == Axis.Horizontal ? projectile.Radius.X : projectile.Radius.Y;
        int perpendicularPosition = axis == Axis.Horizontal
            ? projectile.Position.Y.Whole
            : projectile.Position.X.Whole;
        int perpendicularRadius = axis == Axis.Horizontal
            ? projectile.Radius.Y
            : projectile.Radius.X;

        // The block scan checks every room block crossed by the projectile's perpendicular diameter
        // (EnemyProjectileBlockScan). The positive edge is the radius's last pixel, hence radius - 1. The edge wraps as
        // CoordinateAxis states.
        int movementEdge = TerrainScanEdges.Edge(nextPosition +
            (movingNegative ? -movementRadius : movementRadius - 1));
        var scan = EnemyProjectileBlockScan.Across(
            movementEdge,
            TerrainScanEdges.Edge(perpendicularPosition - perpendicularRadius),
            TerrainScanEdges.Edge(perpendicularPosition + perpendicularRadius - 1),
            axis);
        EnemyProjectileBlockReaction reaction = collision.Reaction(level, new EnemyProjectileScannedMove(projectile, scan, movingNegative, movementEdge));
        if (reaction != EnemyProjectileBlockReaction.Passes)
        {
            // The horizontal and vertical block scans stop the projectile flush against the block boundary it reached
            // and clear its fraction of a pixel, so a fast downward projectile lands on the floor rather than above it.
            int snappedPosition = reaction is EnemyProjectileBlockReaction.StopsAligned aligned
                ? aligned.Position
                : movingNegative
                    ? TerrainScanEdges.Edge(RoomGridConversions.BlockEnd(movementEdge) + movementRadius + 1)
                    : TerrainScanEdges.Edge(RoomGridConversions.BlockStart(movementEdge) - movementRadius);
            bool snapDoesNotMoveBackwards = reaction is EnemyProjectileBlockReaction.StopsAligned || (movingNegative
                ? snappedPosition <= position
                : snappedPosition >= position);

            if (axis == Axis.Horizontal)
            {
                projectile.Position = projectile.Position.WithFractionX(PixelFraction.None);
                if (snapDoesNotMoveBackwards)
                    projectile.Position = projectile.Position.WithWholeX(snappedPosition);
            }
            else
            {
                projectile.Position = projectile.Position.WithFractionY(PixelFraction.None);
                if (snapDoesNotMoveBackwards)
                    projectile.Position = projectile.Position.WithWholeY(snappedPosition);
            }
            return true;
        }

        if (axis == Axis.Horizontal)
        {
            projectile.Position = projectile.Position.WithX(next);
        }
        else
        {
            projectile.Position = projectile.Position.WithY(next);
        }
        return false;
    }

    /// <summary>
    /// Any block type other than air,
    /// read directly without following extensions.
    /// </summary>
    internal static bool NonAirBlockAt(RoomLevelData level, Position16<RoomPixels> pixel) =>
        level.GetCollisionBlock(pixel.ToRoomBlock()).CollisionType != RoomCollisionType.Air;

    /// <summary>Checks radius overlap and publishes touch-animation or damage behavior for a projectile that reaches Samus.</summary>
    private static KnockbackHorizontalDirection? ResolveEnemyProjectileSamusCollision(
        EnemyProjectile projectile,
        SamusState samus)
    {
        // The Samus-collision pass already made the pass-level invincibility/contact-damage decision.
        // Do not re-read the timer here: the first hit writes it, but the game's loop
        // deliberately continues testing the other projectiles in this same pass.
        // The pass skips a projectile with no hitbox before overlap testing.
        if (!projectile.CanDamageSamus || !projectile.HasHitbox)
            return null;

        // Samus keeps a block of wall or door between her body and the room's left and top edges, and no projectile that
        // harms her reaches more than 16 pixels from its centre on either axis, save the Torizos' sonic booms, which fly
        // across at a Torizo's height: one that has passed beyond those edges never reaches her. She lies above the room's
        // top only on a departing up-elevator, near which no enemy fires.
        int xDistance = Math.Abs(projectile.Position.X.Whole - samus.Position.X.Whole);
        int yDistance = Math.Abs(projectile.Position.Y.Whole - samus.Position.Y.Whole);
        if (xDistance >= projectile.Radius.X + samus.Kinematics.Radius.X ||
            yDistance >= projectile.Radius.Y + samus.Kinematics.Radius.Y)
        {
            return null;
        }

        SamusSuitDamage.Hurt(samus, projectile.Damage);
        samus.InvincibilityTimer = WrappingCountdown.Start(EnemyCollisionDefinitions.SamusProjectileHitInvincibilityUpdates);
        KnockbackHorizontalDirection knockbackXDirection = samus.Position.X.Whole >= projectile.Position.X.Whole
            ? KnockbackHorizontalDirection.Right
            : KnockbackHorizontalDirection.Left;

        // Contact installs the definition's touch animation before consulting the persists-on-contact
        // property. Mother Brain's turret bullet depends on that ordering: it survives contact
        // but immediately changes to its smoke animation. Keeping this in the common path also
        // prevents later persistent projectile families from needing bespoke hit effects.
        bool startsTouchReaction = EnemyProjectileKinds.Get(projectile.Kind).SamusTouch == EnemyProjectileSamusTouch.StartsTouchReaction;
        // Every touch animation without a typed reaction of its own is the shared delete animation.
        if (BehaviorOf(projectile).TouchAnimation() is { } touchAnimation)
            projectile.StartAnimation(touchAnimation);
        else if (startsTouchReaction)
            projectile.StartAnimation(EnemyProjectileDeleteAnimation.Instance);

        // Generic enemy-projectile contact deletes Ridley's fireball. Afterburn is a wall-
        // impact feature of the fireball's wall hit, so a Samus contact does not create the wall bloom.
        if (!projectile.PersistsOnSamusContact)
            projectile.Delete();

        return knockbackXDirection;
    }

    /// <summary>Advances a projectile's typed animation one projectile pass.</summary>
    /// <param name="projectile">The projectile whose animation advances.</param>
    /// <param name="samus">Samus, for cues that act on her.</param>
    /// <param name="camera">The camera of the pass, for cues that test the screen.</param>
    /// <param name="shortCounter">The enemies' short update count for the pass, which cue cadences sample.</param>
    /// <param name="samusBombs">Samus's bombs, whose power bomb explosion a cue may test.</param>
    private void AdvanceEnemyProjectileAnimations(
        EnemyProjectile projectile,
        SamusState samus,
        Position16<RoomPixels> camera,
        Compatibility.ShortFrameCount shortCounter,
        SamusBombProjectileSystem samusBombs)
    {
        // Every spawn starts an animation, so only a projectile its own update ended this pass plays none, and it runs
        // nothing.
        if (projectile.Animation is { } animation)
            animation.Advance(new(projectile, samus, camera, shortCounter, Ensure.NotNull(samusBombs), this));
    }

    /// <summary>
    /// Runs the death actor's explosion-sprite cue: it spawns the effect actor of <paramref name="kind"/> the cue names and
    /// draws one random sample; <paramref name="spread"/>'s scatter fields read that sample's fields that offset X and Y
    /// around the death actor, before both are centred back by its radius. Effect actors are separate from enemy
    /// projectiles, so these decorations never consume one of the enemy-projectile places.
    /// </summary>
    internal void SpawnRandomEnemyDeathSprite(EnemyProjectile projectile, RoomEffectActorKind kind, EnemyDeathSpriteSpread spread)
    {
        int random = _room.Random.Generate();
        // The death actor stands where its enemy died, which can lie above or left of the room's origin (a fly that has
        // climbed through the ceiling, a Bull braking past Samus), so a sprite scattered across the origin continues from
        // the space's far end, as OffRoomPlaces places it.
        _ = _effectActors.SpawnRoomEffectActor(new(
            OffRoomPlaces.Place(projectile.Position.X.Whole + spread.HorizontalScatter.Of(random) - spread.Radius),
            OffRoomPlaces.Place(projectile.Position.Y.Whole + spread.VerticalScatter.Of(random) - spread.Radius)),
            kind, graphics: SpriteGraphics.SharedStandard);
    }

    /// <summary>
    /// Moves a projectile to a random place within <paramref name="scatter"/>'s fields: each axis draws again until its
    /// offset lands at or past the field's centre, and one chance in two for each sign
    /// (<see cref="EnemyProjectileScatterDefinitions.XOffsetTurnsLeft"/>,
    /// <see cref="EnemyProjectileScatterDefinitions.YOffsetTurnsUp"/>) turns it left or up. Bomb Torizo's sonic boom
    /// striking a wall scatters its debris this way.
    /// </summary>
    internal void Scatter(EnemyProjectile projectile, EnemyProjectileScatter scatter)
    {
        Ensure.NotNull(projectile);
        // Each centre must lie among its field's values (#1627): the redraw ends only on a draw at or past the centre, so a
        // centre past the field's last value would redraw for good.
        RandomSampleField xRange = RequireLowHalfField(scatter.XRange);
        int xCenter = Ensure.BetweenInclusive(0, xRange.Outcomes - 1, scatter.Center.X);
        RandomSampleField yRange = RequireLowHalfField(scatter.YRange);
        int yCenter = Ensure.BetweenInclusive(0, yRange.Outcomes - 1, scatter.Center.Y);
        int signSample = _room.Random.Generate();

        int xOffset;
        do
        {
            xOffset = _room.Random.Next(xRange) - xCenter;
        }
        while (xOffset < 0);
        if (EnemyProjectileScatterDefinitions.XOffsetTurnsLeft.IsTakenBy(signSample))
            xOffset = -xOffset;
        // The scattered projectiles burst against the room's walls and floors, a Bomb Torizo's sonic boom among them, so a
        // scatter toward the room's left or top edge can carry one past it.
        projectile.Position = OffRoomPlaces.WithWholeX(projectile.Position, projectile.Position.X.Whole + xOffset);

        int yOffset;
        do
        {
            yOffset = _room.Random.Next(yRange) - yCenter;
        }
        while (yOffset < 0);
        if (EnemyProjectileScatterDefinitions.YOffsetTurnsUp.IsTakenBy(signSample))
            yOffset = -yOffset;
        projectile.Position = OffRoomPlaces.WithWholeY(projectile.Position, projectile.Position.Y.Whole + yOffset);
    }

    /// <summary>
    /// <paramref name="field"/>, checked to read only the range of the random choice the scatter authors.
    /// </summary>
    private static RandomSampleField RequireLowHalfField(RandomSampleField field)
    {
        _ = Ensure.BetweenInclusive(0, LowHalfBits, field.Shift + field.Width);
        return field;
    }

    /// <summary>The width of the part of the sample a scatter's range reads.</summary>
    private const int LowHalfBits = 8;

    /// <summary>
    /// Spawns a dust cloud, smoke or an explosion, drawn from the pictures every room shares, playing the animation the
    /// spawner chooses: Rinka deaths, Boulder's impact clouds, the bosses' explosions and every other spawner share it.
    /// </summary>
    internal void SpawnDustOrExplosion(
        Position16<RoomPixels> position,
        DustExplosionAnimation animation)
    {
        // The animation is chosen before a place is taken, so an undefined one fails without taking a place. The spawner
        // chooses it: the Rinka's is the small explosion, and the Boulder's its impact cloud.
        EnemyProjectile? projectile = AllocateEnemyProjectile(RoomEnemyProjectileKind.DustOrExplosion, DustExplosionProjectileBehavior.Instance,
            graphics: SpriteGraphics.SharedStandard, new DustExplosionProjectileAnimation(animation));
        if (projectile is null)
            return;

        projectile.Position = projectile.Position.WithWhole(position);
    }

    /// <summary>Creates the common dud shot for a protected head hitbox.</summary>
    internal void CreateEnemyProjectileDudShot(SamusProjectile projectile)
    {
        _ = _effectActors.SpawnRoomEffectActor(
            projectile.Position.Whole,
            RoomEffectActorKind.DudShot,
            graphics: SpriteGraphics.SharedStandard);
        QueueDudSound(SoundRequestPriority.Low);
        projectile.Spend();
    }

    /// <summary>
    /// Creates the dud shot for an exploding bomb an enemy shrugs off: the dud flash where the bomb lies and the dud sound.
    /// The bomb pass has already marked the bomb as having hit.
    /// </summary>
    internal void CreateBombDudShot(SamusBomb bomb)
    {
        _ = _effectActors.SpawnRoomEffectActor(
            Ensure.NotNull(bomb).Position.Whole,
            RoomEffectActorKind.DudShot,
            graphics: SpriteGraphics.SharedStandard);
        QueueDudSound(SoundRequestPriority.Low);
    }

    /// <summary>
    /// Deletes an enemy projectile once it is vertically off screen, as Dragon's fireball and both n00b-tube shards do
    /// each update.
    /// </summary>
    /// <remarks>
    /// A position above the camera's top row is kept; only a
    /// difference of <see cref="EnemyProjectileOffScreenDefinitions.VerticalDeletionDepth"/>
    /// or more below it deletes the projectile.
    /// </remarks>
    internal static void DeleteEnemyProjectileIfVerticallyOffScreen(
        EnemyProjectile projectile,
        int cameraY)
    {
        // A fireball or shard that flies up past the room's top keeps a wrapped height, so its distance below the
        // camera's top row is measured the short way, as CameraPastRoomEdge.FromCamera gives it: it lies above the camera
        // and is kept, not read as lying far below it.
        if (CameraPastRoomEdge.FromCamera(projectile.Position.Y.Whole, cameraY) >= EnemyProjectileOffScreenDefinitions.VerticalDeletionDepth)
            projectile.Delete();
    }

    /// <summary>
    /// Draws a one-in-four chance from the room's next random number: it comes up for the top quarter of the range.
    /// </summary>
    internal bool TakeRandomAnimationChange() => EnemyProjectileRandomBranchDefinitions.RandomAnimationChange.IsTakenBy(_room.Random.Generate());
}
