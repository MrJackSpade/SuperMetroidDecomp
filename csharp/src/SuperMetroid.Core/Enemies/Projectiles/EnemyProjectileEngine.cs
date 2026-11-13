using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The enemy-projectile engine: the room's eighteen enemy-projectile places, spawning a fresh projectile into a free
/// place and its definition setup, the per-frame pass (frame behaviours, then each animation), the animation runner,
/// both draw-priority passes and the Samus collision passes. Species behavior arrives through registered handlers; the
/// engine names no species.
/// <para>
/// The places are ordered, and gameplay reads the order: spawns take the free place searched from the last down, and
/// every pass (movement and animations, collisions, drawing) runs from the last place down, so a projectile spawned below
/// the one being run runs later in the same pass and one spawned above it waits for the next.
/// </para>
/// </summary>
[RoomLifetime]
internal sealed class EnemyProjectileEngine
{
    /// <summary>The owners whose projectile handlers make up the dispatch table.</summary>
    private readonly List<IEnemyProjectileHandlerSource> _handlerSources = [];

    /// <summary>
    /// Adds an owner whose projectile handlers join the table. Owners are added while the enemy system is
    /// composed, before the first dispatch builds the table.
    /// </summary>
    internal void AddHandlerSource(IEnemyProjectileHandlerSource source) => _handlerSources.Add(source);

    /// <summary>The dispatch table, built from the handler owners on the first dispatch, after composition added them all.</summary>
    private EnemyProjectileHandlerTable Handlers => field ??= BuildHandlers();

    /// <summary>Registers the stationary behaviour and every species-owned projectile handler.</summary>
    private EnemyProjectileHandlerTable BuildHandlers()
    {
        var table = new EnemyProjectileHandlerTable();
        // A stationary projectile does not move itself: it holds still, or its owner positions it.
        table.AddFrameBehavior(static _ => { }, EnemyProjectileFrameBehavior.Stationary);
        table.AddCue<EnemyProjectileCommonCue>((context, cue) => ApplyCommonCue(context.Projectile, cue));
        foreach (IEnemyProjectileHandlerSource source in _handlerSources)
            source.AddEnemyProjectileHandlers(table);
        return table;
    }

    /// <summary>Carries out one shared command of a typed projectile animation.</summary>
    private void ApplyCommonCue(EnemyProjectile projectile, EnemyProjectileCommonCue cue)
    {
        switch (cue)
        {
            case EnemyProjectileCommonCue.Command { Kind: EnemyProjectileCommonCueKind.Delete }:
                projectile.Delete();
                break;
            case EnemyProjectileCommonCue.SetFrameBehavior setFrameBehavior:
                projectile.FrameBehavior = setFrameBehavior.FrameBehavior;
                break;
            case EnemyProjectileCommonCue.Command { Kind: EnemyProjectileCommonCueKind.ClearFrameBehavior }:
                projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
                break;
            case EnemyProjectileCommonCue.Scatter scatter:
                Scatter(projectile, scatter.Spread);
                break;
            case EnemyProjectileCommonCue.Command { Kind: EnemyProjectileCommonCueKind.ReturnToOrigin }:
                projectile.Position = projectile.Position.WithWhole(projectile.StateOf<AnchoredProjectileState>().Anchor);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EnemyProjectileCommonCueKind)} {cue}.");
        }
    }

    /// <summary>Supplies room RNG and artwork for projectile effects.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes projectiles from the room's enemy artwork.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Owns dust and explosion sprites separate from the projectile places.</summary>
    private readonly RoomSpriteObjects _spriteObjects;
    /// <summary>Publishes room quake effects requested by projectile cues.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues species projectile sounds under queue limits.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the physical pool to room sprites, earthquakes and audio.</summary>
    internal EnemyProjectileEngine(EnemyRoomBinding room, EnemyPresentation presentation, RoomSpriteObjects spriteObjects,
        RoomEarthquake earthquake, EnemyAudioQueue audio)
    {
        _room = room;
        _presentation = presentation;
        _spriteObjects = spriteObjects;
        _earthquake = earthquake;
        _audio = audio;
    }
    /// <summary>The number of enemy-projectile places.</summary>
    internal const int PlaceCount = 18;

    /// <summary>
    /// Each place's latest projectile, live or gone: the place is free while its projectile is gone or has freed it.
    /// Until the room spawns into a place, it holds a gone projectile standing where the room before left that place's.
    /// </summary>
    private readonly EnemyProjectile[] _places =
        Enumerable.Range(0, PlaceCount)
            .Select(place => new EnemyProjectile(place, default))
            .ToArray();
    /// <summary>The frame counter of 0 through 255 sampled by the current projectile pass; verification drives it to step animations.</summary>
    internal int CurrentFrame8 { get; set; }

    /// <summary>The pixel offset within a 16-pixel block.</summary>
    private const int BlockPixelMask = 15;

    /// <summary>The pixel offset within an 8-pixel half block.</summary>
    private const int HalfBlockPixelMask = 7;

    /// <summary>The frame clock's count for the current projectile pass.</summary>
    internal int CurrentFrame16 { get; private set; }

    /// <summary>Each place's latest projectile, in place order, live or gone.</summary>
    public IReadOnlyList<EnemyProjectile> EnemyProjectiles => _places;

    /// <summary>
    /// Starts each place from what the room this one replaces left in it (<see cref="RoomHandOver"/>): a gone projectile
    /// at that place's whole position and velocity, which the room's first spawn there starts from. Called before the
    /// room's population loads.
    /// </summary>
    internal void TakeOverFrom(RoomHandOver handOver)
    {
        Ensure.NotNull(handOver);
        for (int place = 0; place < _places.Length; place++)
            _places[place] = new EnemyProjectile(place, handOver.EnemyProjectile(place));
    }

    /// <summary>Leaves what each place's latest projectile left there for the room that replaces this one (<see cref="RoomHandOver"/>).</summary>
    internal void HandOverTo(RoomHandOver handOver) =>
        Ensure.NotNull(handOver).LeaveEnemyProjectiles(_places.Select(projectile => projectile.LeftInPlace));

    /// <summary>
    /// Queues the dud sound a rejected Samus shot makes, at <paramref name="priority"/>: a protected body part's dud competes at
    /// low priority, an indestructible projectile's and Ridley's tail's at normal priority.
    /// </summary>
    internal void QueueDudSound(SoundRequestPriority priority) =>
        _audio.QueueEnemySound(SamusSound.DudShot, priority);

    /// <summary>
    /// Tests enemy projectiles against Samus's projectiles and handles each hit.
    /// Flag-one Nuclear Waffle links create duds and remain alive; flag-zero
    /// projectiles such as Kago's bugs switch to their shot list.
    /// </summary>
    public int ResolveEnemyProjectileSamusProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);
        _room.EnsureLoaded();

        int hitCount = 0;
        for (int enemyProjectileIndex = _places.Length - 1;
             enemyProjectileIndex >= 0;
             enemyProjectileIndex--)
        {
            EnemyProjectile enemyProjectile =
                _places[enemyProjectileIndex];
            if (!enemyProjectile.IsActive ||
                !enemyProjectile.BlocksSamusProjectiles ||
                enemyProjectile.CollisionOption == EnemyProjectileCollisionOption.NoCollision)
                continue;

            foreach (SamusProjectile shot in projectiles.Slots)
            {
                if (!shot.HasEnemyCollisionPayload)
                    continue;

                SamusProjectileType shotType = shot.Type;
                if (shotType.Family is SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb or
                    SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion)
                    continue;

                if (!ShotAndProjectileSquares.Meet(shot.Position.Whole, enemyProjectile.Position.Whole))
                    continue;

                // The collision handler only marks non-plasma shots spent. It does not call the
                // ordinary enemy impact and therefore cannot turn a freshly fired
                // Super Missile (or its invisible link) into an explosion in the producer
                // frame. Its own frame behaviour consumes the mark on the next projectile pass.
                if (!shot.Type.HasAnyBeam(SamusBeamFlags.Plasma))
                    shot.IsSpent = true;

                if (enemyProjectile.CollisionOption == EnemyProjectileCollisionOption.IndestructibleDud)
                {
                    QueueDudSound(SoundRequestPriority.Normal);
                }
                else
                {
                    // Every shot list without a typed reaction of its own is the shared delete list.
                    enemyProjectile.StartAnimation(Handlers.TryGetShotAnimation(enemyProjectile.Kind, out Func<EnemyProjectileAnimation>? shotAnimation)
                        ? shotAnimation()
                        : EnemyProjectileDeleteAnimation.Instance);
                    enemyProjectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;

                    // A shot that destroys a projectile strips its behaviours and keeps only its damage: it no
                    // longer blocks shots or outlasts touching Samus, and it harms her on contact, whatever its
                    // definition said. Every destructible projectile is stripped alike, not only the Kago's.
                    enemyProjectile.BlocksSamusProjectiles = false;
                    enemyProjectile.PersistsOnSamusContact = false;
                    enemyProjectile.CanDamageSamus = true;
                }
                hitCount++;
            }
        }

        return hitCount;
    }

    /// <summary>One projectile pass: every live projectile's frame behaviour, then its animation.</summary>
    internal void RunPass(
        RoomLevelData level,
        SamusState? samus,
        Position16<RoomPixels> camera,
        int frameCycle,
        SamusBombProjectileSystem? samusBombs,
        int frameCounter)
    {
        Ensure.NotNull(level);

        // The pass runs the places from the last down, reading each place when it reaches it rather than a pass-start
        // snapshot. The distinction is observable whenever a frame behaviour or animation cue spawns another projectile:
        // spawns search from the last place down, so one spawned below the place being run is reached later in THIS
        // pass, and one spawned above it waits for the next.
        CurrentFrame8 = frameCycle;
        CurrentFrame16 = frameCounter;
        for (int place = _places.Length - 1; place >= 0; place--)
        {
            EnemyProjectile projectile = _places[place];
            if (!projectile.IsActive)
                continue;

            RunEnemyProjectileFrameBehavior(
                projectile,
                level,
                samus,
                camera,
                frameCycle,
                samusBombs);
            // The animation that runs next is that of whatever projectile the place holds after the frame behaviour, live
            // or not. A projectile that ended and spawned its replacement into its own place hands the rest of the pass
            // to the replacement, whose animation starts in this pass. One that freed its place without ending finishes
            // its update with its animation (a power-bombed Draygon goop that reaches Samus sticks to her through its
            // touch animation, PowerBombedDraygonShotFinishesItsUpdate). One that ended plays no animation, so nothing runs.
            AdvanceEnemyProjectileAnimations(_places[place], samus, camera);
        }
    }

    /// <summary>Publishes projectile hurt requests after room objects; the next Samus phase consumes them.</summary>
    public void ResolveEnemyProjectileSamusHits(SamusState? samus)
    {
        _room.EnsureLoaded();
        // Gameplay runs every projectile's update first, then enters the
        // separate Samus-collision pass. That pass samples invincibility and
        // contact-damage state once at entry; damage from one overlapping projectile does
        // not abort the remaining descending scan of the places. This distinction is observable for
        // Puromi/Nuclear Waffle, whose four persistent body links begin co-located.
        if (samus is null || !samus.InvincibilityTimer.IsZero ||
            samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
            return;

        KnockbackHorizontalDirection? finalKnockbackXDirection = null;
        for (int index = _places.Length - 1; index >= 0; index--)
        {
            EnemyProjectile projectile = _places[index];
            if (projectile.IsActive)
            {
                finalKnockbackXDirection =
                    ResolveEnemyProjectileSamusCollision(projectile, samus) ??
                    finalKnockbackXDirection;
            }
        }

        // The contact only publishes the five-frame request and overwrites its horizontal
        // direction for every hit. Samus's update has already run this frame: do not initialize
        // a hurt pose or move Samus here. The next frame consumes the final request.
        if (finalKnockbackXDirection.HasValue)
        {
            samus.KnockbackXDirection = finalKnockbackXDirection.Value;
            samus.KnockbackTimer = 5;
        }
    }

    /// <summary>
    /// Draws the room's sprite objects and then the projectiles in front of Samus. The game draws them before Samus,
    /// so they are drawn over her.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="camera">Camera position in whole room pixels.</param>
    /// <param name="timeIsFrozen">Whether time is frozen, which stops the earthquake displacement.</param>
    public void DrawEnemyProjectilesInFrontOfSamus(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera,
        bool timeIsFrozen = false)
    {
        Ensure.NotNull(sprites);
        _room.EnsureLoaded();

        // Sprite objects are drawn once, with the projectiles in front of Samus. Drawing them again with the
        // projectiles behind her would duplicate explosions, dust, and Spark's trail in the same frame.
        _spriteObjects.DrawRoomSpriteObjects(sprites, camera);
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
        bool timeIsFrozen = false)
    {
        Ensure.NotNull(sprites);
        _room.EnsureLoaded();

        DrawEnemyProjectilesAt(
            sprites,
            camera,
            EnemyProjectileDepth.BehindSamus, timeIsFrozen);
    }

    /// <summary>
    /// Draws the projectiles at <paramref name="depth"/>, the last place first: among projectiles at one depth, one in a
    /// later place is drawn over one in an earlier place.
    /// </summary>
    internal void DrawEnemyProjectilesAt(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera,
        EnemyProjectileDepth depth,
        bool timeIsFrozen)
    {
        Offset16<RoomPixels> shake = _earthquake.GetEnemyProjectileShake(timeIsFrozen);
        for (int place = _places.Length - 1; place >= 0; place--)
        {
            EnemyProjectile projectile = _places[place];

            if (!projectile.IsActive ||
                projectile.Depth != depth ||
                projectile.Frame is not { } frame)
                continue;

            // Every placement is published wherever it lies; the display clips it to the view (#1520).
            Position16<ScreenPixels> screen = (projectile.Position.Whole + shake).ToScreen(camera);

            // The blank frame, and the animation frames showing the empty picture, draw nothing (#1513).
            if (!EnemyProjectileEmptyFrames.DrawsNothing(frame))
                _presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.EnemyProjectile, place),
                    EnemySheetDefinitions.ProjectileFrameName(frame), screen, projectile.Graphics);
        }
    }

    /// <summary>
    /// Spawns a fresh projectile into the first free place searched from the last down, starting from what the place's
    /// previous projectile left there (<see cref="EnemyProjectileHandOver"/>), or returns null when every place is held.
    /// The new projectile is gone until its spawner gives it a kind.
    /// </summary>
    internal EnemyProjectile? AllocateEnemyProjectile()
    {
        for (int place = _places.Length - 1; place >= 0; place--)
        {
            EnemyProjectile previous = _places[place];
            if (!previous.IsActive)
                return _places[place] = new EnemyProjectile(place, previous.LeftInPlace);
        }
        return null;
    }

    /// <summary>
    /// Copies the definition record a projectile spawn installs.
    /// Family initializers may then replace fields exactly as their
    /// authored initializer does; centralizing the copy prevents each projectile kind
    /// from inventing subtly different radius/property semantics.
    /// </summary>
    internal static void InitializeEnemyProjectileFromDefinition(
        EnemyProjectile projectile,
        RoomEnemyProjectileKind kind,
        SpriteGraphics graphics)
    {
        EnemyProjectileDefinition definition = EnemyProjectileKinds.Get(kind);
        projectile.Kind = kind;
        projectile.FrameBehavior = definition.FrameBehavior;
        // The spawn starts the animation timer at one, so the first step runs on the first pass. A kind without
        // a typed start has its spawner start the animation its producer chooses.
        projectile.FrameHold = WrappingCountdown.Start(1);
        if (EnemyProjectileTypedStarts.Initial(kind) is { } animation)
            projectile.StartAnimation(animation);

        // The spawn shows the blank frame before the first animation tick. It is intentionally not the animation's first
        // frame; advances to that on its own pass.
        projectile.Frame = EnemyProjectileFrameId.Blank;
        projectile.Radius = definition.Radius;
        projectile.Damage = definition.Damage;
        projectile.InvincibilityFrames = 96;
        projectile.Depth = definition.Depth;
        projectile.CanDamageSamus = definition.CanDamageSamus;
        projectile.PersistsOnSamusContact = definition.PersistsOnSamusContact;
        projectile.BlocksSamusProjectiles = definition.BlocksSamusProjectiles;
        projectile.CollisionOption = EnemyProjectileCollisionOption.RunShotList;
        projectile.Graphics = graphics;
    }

    /// <summary>Runs the projectile's current frame behaviour before its animation advances.</summary>
    private void RunEnemyProjectileFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level,
        SamusState? samus,
        Position16<RoomPixels> camera,
        int frameCycle,
        SamusBombProjectileSystem? samusBombs)
    {
        EnemyProjectileFrameBehavior id = projectile.FrameBehavior;
        if (!Handlers.TryGetFrameBehavior(id, out EnemyProjectileFrameBehaviorHandler? handler))
        {
            throw new InvalidDataException(
                $"Enemy projectile frame behaviour {projectile.FrameBehavior} is not translated.");
        }
        handler(new EnemyProjectileFrameBehaviorContext(projectile, level, samus, camera, frameCycle, samusBombs));
    }

    /// <summary>Moves one projectile axis through each crossed room block and clamps it flush at the first collision.</summary>
    internal static bool MoveProjectileAxis(
        EnemyProjectile projectile,
        RoomLevelData level,
        Axis axis) =>
        MoveProjectileAxis(projectile, level, axis, axis == Axis.Horizontal ? projectile.Velocity.X : projectile.Velocity.Y);

    /// <summary>
    /// Moves one projectile axis by <paramref name="speed"/> instead of the projectile's own velocity on that axis, through
    /// each crossed room block, clamping it flush at the first collision.
    /// </summary>
    internal static bool MoveProjectileAxis(
        EnemyProjectile projectile,
        RoomLevelData level,
        Axis axis,
        SignedEightEight speed)
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

        // The block scan checks every room block crossed by the projectile's perpendicular
        // diameter (EnemyProjectileBlockScan). The positive edge is inclusive, hence radius - 1; using +radius made
        // right/down projectiles collide one pixel earlier than their counterparts.
        // The edge wraps past the room coordinates' 65535, as the probe's block bounds check expects.
        int movementEdge = CoordinateAxis.Wrap(nextPosition +
            (movingNegative ? -movementRadius : movementRadius - 1));
        var scan = EnemyProjectileBlockScan.Across(
            movementEdge,
            CoordinateAxis.Wrap(perpendicularPosition - perpendicularRadius),
            CoordinateAxis.Wrap(perpendicularPosition + perpendicularRadius - 1),
            axis);
        for (int tested = 0; tested <= scan.ExtraBlocks; tested++)
        {
            if (ProjectileAxisProbeHitsRoom(
                    level,
                    projectile,
                    scan.Block(tested, level.Size.X),
                    axis,
                    movingNegative: movingNegative,
                    targetEdge: movementEdge,
                    slopeAlignedPosition: out int? slopeAlignedPosition))
            {
                // The horizontal and vertical block scans do not merely reject the
                // attempted movement. They clear the subposition and place the
                // projectile flush against the 16-pixel block boundary it reached.
                // Without this correction a fast downward projectile retains its last
                // pre-collision coordinate and visibly hovers above the floor.
                int snappedPosition = slopeAlignedPosition ?? (movingNegative
                    ? CoordinateAxis.Wrap((movementEdge | BlockPixelMask) + movementRadius + 1)
                    : CoordinateAxis.Wrap((movementEdge & ~BlockPixelMask) - movementRadius));
                bool snapDoesNotMoveBackwards = slopeAlignedPosition.HasValue || (movingNegative
                    ? snappedPosition <= position
                    : snappedPosition >= position);

                if (axis == Axis.Horizontal)
                {
                    projectile.Position = projectile.Position.WithFractionX(0);
                    if (snapDoesNotMoveBackwards)
                        projectile.Position = projectile.Position.WithWholeX(snappedPosition);
                }
                else
                {
                    projectile.Position = projectile.Position.WithFractionY(0);
                    if (snapDoesNotMoveBackwards)
                        projectile.Position = projectile.Position.WithWholeY(snappedPosition);
                }
                return true;
            }
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

    /// <summary>Tests a leading-edge block through extension and slope geometry for one projectile move.</summary>
    private static bool ProjectileAxisProbeHitsRoom(
        RoomLevelData level,
        EnemyProjectile projectile,
        Position16<RoomBlocks> block,
        Axis axis,
        bool movingNegative,
        int targetEdge,
        out int? slopeAlignedPosition)
    {
        slopeAlignedPosition = null;
        if (block.X >= (uint)level.Size.X ||
            block.Y >= (uint)level.Size.Y)
        {
            return true;
        }

        // Resolve horizontal- and vertical-extension links before dispatching the final block kind, just as
        // the projectile collision loop does. Air is open, while a door
        // remains a wall to an enemy projectile.
        if (EnemyTerrain.ResolveEnemyCollisionBlock(level, block) is not { } collisionBlock)
            return true;

        RoomCollisionType type = collisionBlock.CollisionType;
        if (type == RoomCollisionType.Slope && !collisionBlock.Behavior.Slope.IsNonSquare)
        {
            // The horizontal scan tests only occupied eight-pixel quadrants touched by the
            // leading edge. Empty halves of square slopes are not solid walls.
            int perpendicularPosition = (axis == Axis.Horizontal) ? projectile.Position.Y.Whole : projectile.Position.X.Whole;
            int perpendicularRadius = (axis == Axis.Horizontal) ? projectile.Radius.Y : projectile.Radius.X;
            int blockStart = ((axis == Axis.Horizontal) ? block.Y : block.X) << 4;
            int firstPixel = Math.Max(blockStart, perpendicularPosition - perpendicularRadius);
            int lastPixel = Math.Min(blockStart + 15, perpendicularPosition + perpendicularRadius - 1);
            for (int half = firstPixel >> 3; half <= lastPixel >> 3; half++)
            {
                bool edgeInSecondHalf = (targetEdge & 8) != 0;
                bool halfIsSecond = (half & 1) != 0;
                SquareSlopeQuadrant quadrant = axis == Axis.Horizontal
                    ? SquareSlopeQuadrant.Showing(collisionBlock.Behavior, SquareSlopeQuadrant.ColumnFor(edgeInSecondHalf), SquareSlopeQuadrant.RowFor(halfIsSecond))
                    : SquareSlopeQuadrant.Showing(collisionBlock.Behavior, SquareSlopeQuadrant.ColumnFor(halfIsSecond), SquareSlopeQuadrant.RowFor(edgeInSecondHalf));
                if (!SquareSlopes.IsSolid(collisionBlock.Behavior, quadrant))
                    continue;

                // The square-slope reaction places the projectile at an eight-pixel
                // boundary before the caller's full-block clamp.
                int radius = (axis == Axis.Horizontal) ? projectile.Radius.X : projectile.Radius.Y;
                slopeAlignedPosition = movingNegative
                    ? CoordinateAxis.Wrap((targetEdge | HalfBlockPixelMask) + radius + 1)
                    : CoordinateAxis.Wrap((targetEdge & ~HalfBlockPixelMask) - radius);
                return true;
            }
            return false;
        }

        if (type == RoomCollisionType.Slope && collisionBlock.Behavior.Slope.IsNonSquare)
        {
            // Horizontal motion ignores non-square slopes; the following
            // vertical pass owns their height profile. Treating one as a full wall can
            // consume every Kago falling frame before Y collision is ever attempted.
            if ((axis == Axis.Horizontal))
                return false;

            // Vertical projectile collision uses the same 16-sample slope
            // profiles as Samus's. Only the column containing the projectile center owns
            // non-square geometry; neighboring columns touched by its radius do not turn
            // the slope into a full-height wall.
            if (block.X != projectile.Position.X.Whole >> 4)
                return false;

            bool movingUp = movingNegative;
            if (movingUp != collisionBlock.Behavior.Slope.FlipsVertically)
                return false;

            int height = EnemyTerrain.ReadNonSquareSlopeHeight(
                collisionBlock.Behavior,
                (collisionBlock.Behavior.Slope.FlipsHorizontally
                    ? projectile.Position.X.Whole ^ BlockPixelMask
                    : projectile.Position.X.Whole) & BlockPixelMask);
            int edgeWithinBlock = movingUp
                ? (targetEdge & BlockPixelMask) ^ BlockPixelMask
                : targetEdge & BlockPixelMask;
            int adjustment = height - edgeWithinBlock - 1;
            if (movingUp)
            {
                if (adjustment > 0)
                    return false;
            }
            else if (height - edgeWithinBlock != 1 && adjustment >= 0)
            {
                return false;
            }

            slopeAlignedPosition = movingUp
                ? CoordinateAxis.Wrap((block.Y << 4) + 16 - height + projectile.Radius.Y)
                : CoordinateAxis.Wrap((block.Y << 4) + height - projectile.Radius.Y);
            return true;
        }

        return EnemyProjectileBlockIsWall(type);
    }

    /// <summary>
    /// The unconditional rows of the enemy-projectile block
    /// reaction tables. Slopes and extensions are resolved before reaching this test.
    /// Spike blocks stop projectiles; grapple blocks do not.
    /// </summary>
    private static bool EnemyProjectileBlockIsWall(RoomCollisionType type) => type is
        RoomCollisionType.Slope or
        RoomCollisionType.HorizontalExtension or
        RoomCollisionType.SolidBlock or
        RoomCollisionType.DoorBlock or
        RoomCollisionType.SpikeBlock or
        RoomCollisionType.SpecialBlock or
        RoomCollisionType.ShootableBlock or
        RoomCollisionType.VerticalExtension or
        RoomCollisionType.BombableBlock;

    /// <summary>
    /// Any block type other than air,
    /// read directly without following extensions.
    /// </summary>
    internal static bool NonAirBlockAt(RoomLevelData level, Position16<RoomPixels> pixel) =>
        level.GetCollisionBlock(new(CoordinateAxis.Wrap(pixel.X >> 4), CoordinateAxis.Wrap(pixel.Y >> 4))).CollisionType != RoomCollisionType.Air;

    /// <summary>Checks radius overlap and publishes touch-list or damage behavior for a projectile that reaches Samus.</summary>
    internal KnockbackHorizontalDirection? ResolveEnemyProjectileSamusCollision(
        EnemyProjectile projectile,
        SamusState samus)
    {
        // The Samus-collision pass already made the pass-level invincibility/contact-damage decision.
        // Do not re-read the timer here: the first hit writes it, but the game's loop
        // deliberately continues testing the other projectiles in this same pass.
        // The pass skips either zero radius before overlap testing. A zero
        // radius is not a point-shaped hazard: decorative wall shards use it to
        // remain harmless even while passing straight through Samus's body.
        if (!projectile.CanDamageSamus || projectile.Radius.X == 0 || projectile.Radius.Y == 0)
            return null;

        int xDistance = CoordinateAxis.Distance(projectile.Position.X.Whole, samus.Position.X.Whole);
        int yDistance = CoordinateAxis.Distance(projectile.Position.Y.Whole, samus.Position.Y.Whole);
        if (xDistance >= projectile.Radius.X + samus.Kinematics.Radius.X ||
            yDistance >= projectile.Radius.Y + samus.Kinematics.Radius.Y)
        {
            return null;
        }

        SamusSuitDamage.Hurt(samus, projectile.Damage);
        samus.InvincibilityTimer = WrappingCountdown.FromRaw(projectile.InvincibilityFrames);
        KnockbackHorizontalDirection knockbackXDirection = CoordinateAxis.Difference(
            samus.Position.X.Whole, projectile.Position.X.Whole) >= 0
            ? KnockbackHorizontalDirection.Right
            : KnockbackHorizontalDirection.Left;

        // Contact installs the definition's touch animation before consulting the persists-on-contact
        // property. Mother Brain's turret bullet depends on that ordering: it survives contact
        // but immediately changes to its smoke list. Keeping this in the common path also
        // prevents later persistent projectile families from needing bespoke hit effects.
        bool hasTouchList = EnemyProjectileKinds.Get(projectile.Kind).SamusTouch == EnemyProjectileSamusTouch.StartsTouchReaction;
        // Every touch list without a typed reaction of its own is the shared delete list.
        if (Handlers.TryGetTouchAnimation(projectile.Kind, out Func<EnemyProjectileAnimation>? touchAnimation))
            projectile.StartAnimation(touchAnimation());
        else if (hasTouchList)
            projectile.StartAnimation(EnemyProjectileDeleteAnimation.Instance);

        // Generic enemy-projectile contact deletes Ridley's fireball. Afterburn is a wall-
        // impact feature of the fireball's wall hit, so a Samus contact does not create the wall bloom.
        if (!projectile.PersistsOnSamusContact)
            projectile.Delete();

        return knockbackXDirection;
    }

    /// <summary>Advances a projectile's typed animation one projectile pass.</summary>
    internal void AdvanceEnemyProjectileAnimations(
        EnemyProjectile projectile,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        if (projectile.Animation is { } animation)
        {
            animation.Advance(new(projectile, samus, camera), Handlers);
            return;
        }

        // A projectile its frame behaviour ended this pass, or one spawned without an animation, runs nothing.
        if (!projectile.IsActive)
            return;
        throw new InvalidOperationException($"Enemy projectile {projectile.Kind} in place {projectile.Place} runs no program.");
    }

    /// <summary>
    /// Runs the death actor's two explosion cues. Both consume one object-number operand and one
    /// random sample; <paramref name="xScatter"/> and <paramref name="yScatter"/> read that sample's fields that offset X and Y
    /// around the death actor, before both are centred back by <paramref name="center"/>.
    /// Sprite objects are separate from enemy projectiles, so these decorations never
    /// consume one of the eighteen enemy-projectile places.
    /// </summary>
    internal void SpawnRandomEnemyDeathSprite(
        EnemyProjectile projectile,
        RoomSpriteObjectKind kind,
        RandomSampleField xScatter,
        RandomSampleField yScatter,
        int center)
    {
        int random = _room.Random.Generate();
        _ = _spriteObjects.SpawnRoomSpriteObject(new(
            CoordinateAxis.Wrap(projectile.Position.X.Whole + xScatter.Of(random) - center),
            CoordinateAxis.Wrap(projectile.Position.Y.Whole + yScatter.Of(random) - center)),
            kind, graphics: default);
    }

    /// <summary>
    /// Uses RNG rejection and random signs to reposition a projectile within <paramref name="scatter"/>'s fields.
    /// Each axis rejects negative candidate offsets, while two independent bits from the first random sample choose
    /// the eventual signs. This peculiar rejection loop is observable in Bomb Torizo's sonic-boom wall impact, so a
    /// host RNG approximation would produce a visibly different debris cloud and desynchronize later randomness.
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
        if (EnemyProjectileScatter.NegativeX.IsSetIn(signSample))
            xOffset = -xOffset;
        projectile.Position = projectile.Position.WithWrappedWholeX(projectile.Position.X.Whole + xOffset);

        int yOffset;
        do
        {
            yOffset = _room.Random.Next(yRange) - yCenter;
        }
        while (yOffset < 0);
        if (EnemyProjectileScatter.NegativeY.IsSetIn(signSample))
            yOffset = -yOffset;
        projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole + yOffset);
    }

    /// <summary>
    /// <paramref name="field"/>, checked to read only the sample modulo 256, the only range the scatter authors.
    /// </summary>
    private static RandomSampleField RequireLowHalfField(RandomSampleField field)
    {
        _ = Ensure.BetweenInclusive(0, LowHalfBits, field.Shift + field.Width);
        return field;
    }

    /// <summary>The width of the sample modulo 256, the only part of the sample a scatter's range reads.</summary>
    private const int LowHalfBits = 8;

    /// <summary>
    /// Allocates the shared room-graphics dust/explosion actor.
    /// The caller selects the animation the spawn parameter indexes; this is shared by
    /// special Rinka deaths, Boulder's impact clouds, and later families that invoke the same
    /// engine primitive.
    /// </summary>
    internal void SpawnRoomGraphicsDustExplosion(
        Position16<RoomPixels> position,
        MiscDustAnimation animation)
    {
        MiscDustProjectileSegment segment = MiscDustProjectiles.Animation(animation);
        EnemyProjectile? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MiscDustExplosion,
            graphics: default);
        projectile.Position = projectile.Position.WithWhole(position);

        // The dust-cloud-or-explosion initializer ignores the definition's placeholder list and
        // indexes this literal thirty-entry table with the spawner's choice: the Rinka's
        // is the small explosion, and the Boulder's its impact cloud.
        projectile.StartAnimation(new MiscDustProjectileAnimation(segment));
    }

    /// <summary>
    /// Allocates the light or dark Ceres falling-tile actor requested by room main
    /// main. The X coordinate and light or dark variant come directly from the room's
    /// current random number; this initializer owns only projectile fields. The room
    /// main supplies the tile's animation and the graphics it draws in.
    /// </summary>
    public void SpawnCeresFallingDebris(int xPosition, bool dark, EnemyProjectileAnimation animation, SpriteGraphics graphics)
    {
        _room.EnsureLoaded();
        EnemyProjectile? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        RoomEnemyProjectileKind kind = dark
            ? RoomEnemyProjectileKind.CeresFallingDebrisDark
            : RoomEnemyProjectileKind.CeresFallingDebrisLight;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphics);
        projectile.Position = projectile.Position.WithWhole(new(xPosition, 42));
        projectile.Velocity = new(SignedEightEight.Zero, SignedEightEight.FromParts(0, 16));
        projectile.StartAnimation(animation);
    }

    /// <summary>Deletes a gunship dust cloud when its animation ends.</summary>
    internal static void AddGunshipDustCues(EnemyProjectileHandlerTable table) =>
        table.AddCue<GunshipDustCue>((context, cue) =>
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case GunshipDustCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(GunshipDustCue)} {cue}.");
            }
        });

    /// <summary>
    /// Spawns the save station's projectile with the room's graphics and initializes it.
    /// The game derives the actor origin from the currently running save-station
    /// room object: one block right and two blocks above its trigger block.
    /// </summary>
    public void SpawnSaveStationElectricity(Position16<RoomBlocks> roomObjectBlock)
    {
        _room.EnsureLoaded();

        EnemyProjectile? projectile = AllocateEnemyProjectile();
        if (projectile is null)
        {
            // The projectile spawn deliberately drops a spawn when all eighteen places
            // are occupied. This is the game's finite-pool behavior, not a host error.
            return;
        }

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.SaveStationElectricity,
            graphics: default);
        int blockX = roomObjectBlock.X;
        int blockY = roomObjectBlock.Y;
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(16 * (blockX + 1)), CoordinateAxis.Wrap(16 * (blockY - 2))));
    }

    /// <summary>Allocates one of the gunship liftoff's six dust clouds, <paramref name="definition"/>, below Samus.</summary>
    internal void SpawnGunshipLiftoffDustCloud(GunshipDustPlacement definition, SamusState samus)
    {
        EnemyProjectile? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GunshipLiftoffDustCloud,
            graphics: default);
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(samus.Position.X.Whole + definition.XOffset), CoordinateAxis.Wrap(samus.Position.Y.Whole + GunshipDustDefinitions.YOffset)));
        projectile.Velocity = new(SignedEightEight.Zero, SignedEightEight.Zero);
        projectile.StartAnimation(new GunshipDustAnimation(definition.Animation));
    }

    /// <summary>
    /// The four particles of a Skree or Metaree <paramref name="burst"/>, six pixels either side of the source: the outer pair
    /// flies at 1.25 pixels outward and just over 3 up per update, the inner pair at 0.375 outward and just over 4 up, in 8.8.
    /// </summary>
    internal void SpawnMetalParticleBurst(RoomEnemySlot source, MetalParticleBurst burst)
    {
        Ensure.NotNull(burst);
        SpawnSkreeOrMetareeParticle(source, burst.DownRight, 6, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(320, -769));
        SpawnSkreeOrMetareeParticle(source, burst.UpRight, 6, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(96, -1025));
        SpawnSkreeOrMetareeParticle(source, burst.DownLeft, -6, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-320, -769));
        SpawnSkreeOrMetareeParticle(source, burst.UpLeft, -6, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-96, -1025));
    }

    /// <summary>
    /// Spawns one particle through the projectile spawn's definition copy (damage four,
    /// contact enabled, Skree or Metaree animation), then applies the shared particle
    /// initializer's position and 8.8 velocity.
    /// </summary>
    private void SpawnSkreeOrMetareeParticle(
        RoomEnemySlot source,
        RoomEnemyProjectileKind kind,
        int xOffset,
        EightEightVector<RoomPixels> velocity)
    {
        EnemyProjectile? particle = AllocateEnemyProjectile();
        if (particle is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            particle,
            kind,
            SpriteGraphics.SpawnedBy(source));
        particle.Position = particle.Position.WithWhole(new(CoordinateAxis.Wrap(source.Position.X.Whole + xOffset), source.Position.Y.Whole));
        particle.Velocity = velocity;
    }

    /// <summary>Creates the common dud shot for a protected head hitbox.</summary>
    internal void CreateEnemyProjectileDudShot(SamusProjectile projectile)
    {
        _ = _spriteObjects.SpawnRoomSpriteObject(
            projectile.Position.Whole,
            RoomSpriteObjectKind.EnemyProjectileDud,
            graphics: default);
        QueueDudSound(SoundRequestPriority.Low);
        projectile.IsSpent = true;
    }

    /// <summary>
    /// Deletes an enemy projectile once it is vertically off screen, shared by
    /// Dragon's fireball and both n00b-tube shard frame behaviours.
    /// </summary>
    /// <remarks>
    /// A position above the camera's top row (signed-negative difference) is kept; only a
    /// difference of <see cref="EnemyProjectileOffScreenDefinitions.VerticalDeletionDepth"/>
    /// or more below it deletes the projectile.
    /// </remarks>
    internal static void DeleteEnemyProjectileIfVerticallyOffScreen(
        EnemyProjectile projectile,
        int cameraY)
    {
        int screenY = CoordinateAxis.Wrap(projectile.Position.Y.Whole - cameraY);
        if (CoordinateAxis.AsSigned(screenY) >= 0 &&
            screenY >= EnemyProjectileOffScreenDefinitions.VerticalDeletionDepth)
        {
            projectile.Delete();
        }
    }

    /// <summary>A Skree or Metaree particle's downward acceleration per update, in 256ths of a pixel per update.</summary>
    private static readonly SignedEightEight SkreeParticleGravity = SignedEightEight.FromParts(0, 80);

    /// <summary>A falling Ceres tile's downward acceleration per update, in 256ths of a pixel per update.</summary>
    private static readonly SignedEightEight CeresFallingTileGravity = SignedEightEight.FromParts(0, 16);

    /// <summary>Registers Skree/Metaree particle movement with falling acceleration and camera-square deletion.</summary>
    internal static void AddMetalSkreeParticleFrameBehavior(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            context.Projectile.Position = context.Projectile.Position.Advance(context.Projectile.Velocity.ToSixteenSixteen());
            context.Projectile.Velocity = context.Projectile.Velocity.WithY(context.Projectile.Velocity.Y + SkreeParticleGravity);
            if (CoordinateAxis.Wrap(context.Projectile.Position.X.Whole - context.Camera.X) >= 256 ||
                CoordinateAxis.Wrap(context.Projectile.Position.Y.Whole - context.Camera.Y) >= 256)
            {
                context.Projectile.Delete();
            }
        }, EnemyProjectileFrameBehavior.SkreeParticleFlight);
    }

    /// <summary>Registers tile gravity and impact dust/sound when Ceres debris strikes room terrain.</summary>
    internal void AddCeresFallingTileFrameBehavior(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            context.Projectile.Velocity = context.Projectile.Velocity.WithY(context.Projectile.Velocity.Y + CeresFallingTileGravity);
            if (EnemyProjectileEngine.MoveProjectileAxis(context.Projectile, context.Level, axis: Axis.Vertical))
            {
                Position16<RoomPixels> impact = context.Projectile.Position.Whole;
                context.Projectile.Delete();
                SpawnRoomGraphicsDustExplosion(impact, MiscDustAnimation.SmallDustCloud);
                _audio.QueueEnemySound(EnemySound.CeresTileFall, priority: SoundRequestPriority.Normal);
            }
        }, EnemyProjectileFrameBehavior.CeresTileFall);
    }

    /// <summary>
    /// Draws the one-in-four branch: it is taken when the room's next random number divided by 16384
    /// is three.
    /// </summary>
    internal bool TakeOneInFourBranch() =>
        _room.Random.Next(EnemyProjectileRandomBranchDefinitions.OneInFourBranch) == EnemyProjectileRandomBranchDefinitions.OneInFourBranchTaken;

    /// <summary>Sets every behavior bit in <paramref name="flags"/>, as a list's property OR does.</summary>
    internal static void SetPropertyFlags(EnemyProjectile projectile, EnemyProjectilePropertyFlags flags)
    {
        Ensure.NotNull(projectile);
        if (flags.HasFlag(EnemyProjectilePropertyFlags.HarmlessToSamus))
            projectile.CanDamageSamus = false;
        if (flags.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact))
            projectile.PersistsOnSamusContact = true;
        if (flags.HasFlag(EnemyProjectilePropertyFlags.BlocksShots))
            projectile.BlocksSamusProjectiles = true;
    }

    /// <summary>Clears every behavior bit in <paramref name="flags"/>, as a list's property AND clears the bits its mask leaves out.</summary>
    internal static void ClearPropertyFlags(EnemyProjectile projectile, EnemyProjectilePropertyFlags flags)
    {
        Ensure.NotNull(projectile);
        if (flags.HasFlag(EnemyProjectilePropertyFlags.HarmlessToSamus))
            projectile.CanDamageSamus = true;
        if (flags.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact))
            projectile.PersistsOnSamusContact = false;
        if (flags.HasFlag(EnemyProjectilePropertyFlags.BlocksShots))
            projectile.BlocksSamusProjectiles = false;
    }
}
