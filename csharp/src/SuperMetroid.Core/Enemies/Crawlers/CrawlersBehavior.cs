using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The Crawlers species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CrawlersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
    [.. CrawlersPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new CrawlerEntity(this, slot, common);

    /// <summary>Performs wall, floor, and slope probes for attached crawlers and falling Zoomers.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Signals the Super Missile quake that knocks orange Zoomers from surfaces.</summary>
    private readonly RoomEarthquake _earthquake;

    /// <summary>Binds common crawler motion and orange-Zoomer pursuit to terrain and quake state.</summary>
    internal CrawlersBehavior(EnemyTerrain terrain, RoomEarthquake earthquake)
    {
        _terrain = terrain;
        _earthquake = earthquake;
    }

    /// <summary>Carries out a crawler animation's cue: each surface's first step sets the movement action main AI runs.</summary>
    private static CrawlerSegment? ApplyCrawlerCue(CrawlerEnemyState state, CrawlerCue cue)
    {
        switch (cue.Kind)
        {
            case CrawlerCueKind.SetMovement:
                state.Action = cue.Action;
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CrawlerCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>
    /// Initializes a crawler of any species through the shared initializer. Each species differs only by its initial
    /// animation family and the surface family whose animations it switches between at corners: the species' own, or
    /// the one its population entry names when the species gives none.
    /// </summary>
    internal static void InitializeCrawler(
        RoomEnemySlot slot,
        CrawlerAnimationFamily animationFamily,
        CrawlerAnimationFamily? speciesSurfaceFamily = null)
    {
        // The population entry names the starting surface. The Viola's setup alone takes its block side
        // instead, and a Viola's entry names no surface.
        CrawlerSurfaceOrientation orientation;
        CrawlerSurfaceOrientation blockSide;
        SignedEightEight? crawlSpeed;
        CrawlerAnimationFamily surfaceFamily;
        if (animationFamily == CrawlerAnimationFamily.Viola)
        {
            ViolaSpawnSettings viola = slot.SpawnSettings<ViolaSpawnSettings>();
            orientation = viola.BlockSide;
            blockSide = viola.BlockSide;
            crawlSpeed = viola.CrawlSpeed;
            surfaceFamily = viola.SurfaceFamily;
        }
        else
        {
            CrawlerSpawnSettings settings = slot.SpawnSettings<CrawlerSpawnSettings>();
            orientation = settings.Orientation;
            blockSide = settings.BlockSide;
            crawlSpeed = settings.CrawlSpeed;
            surfaceFamily = settings.SurfaceFamily;
        }

        var state = new CrawlerEnemyState
        {
            TakesDefaultSpeedOnLanding = crawlSpeed is null,
            SurfaceFamily = speciesSurfaceFamily ?? surfaceFamily,
        };
        Bind(slot, state);

        state.Playback = new(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
            animationFamily, orientation)), AnimationPlace.StartOfAnimation);
        slot.Frame = EnemyFrameId.Empty;
        slot.AnimationClock.StepOnNextUpdate();
        state.Action = CrawlerAction.AwaitingAnimationCue;

        state.Velocity = ResetCrawlerVelocities(blockSide, crawlSpeed, state.Velocity);
    }

    /// <summary>Shared crawler velocity reset, also called by Yard after landing.</summary>
    /// <param name="blockSide">The side of its block the crawler holds, which signs the velocity.</param>
    /// <param name="crawlSpeed">The crawl speed on both axes, or none to keep <paramref name="velocity"/>.</param>
    /// <param name="velocity">The velocity kept when there is no crawl speed.</param>
    /// <returns>The reset velocity: <paramref name="velocity"/> or the crawl speed on both axes, signed by the block side.</returns>
    internal static EightEightVector<RoomPixels> ResetCrawlerVelocities(
        CrawlerSurfaceOrientation blockSide, SignedEightEight? crawlSpeed, EightEightVector<RoomPixels> velocity)
    {
        if (crawlSpeed is SignedEightEight speed)
            velocity = new(speed, speed);

        return SignedForBlockSide(blockSide, velocity);
    }

    /// <summary>A crawl velocity signed for the side of its block the crawler holds.</summary>
    private static EightEightVector<RoomPixels> SignedForBlockSide(
        CrawlerSurfaceOrientation blockSide, EightEightVector<RoomPixels> velocity) => blockSide switch
        {
            CrawlerSurfaceOrientation.UpsideRight => velocity.ReversedX(),
            CrawlerSurfaceOrientation.UpsideDown => velocity.ReversedY(),
            CrawlerSurfaceOrientation.UpsideLeft or CrawlerSurfaceOrientation.UpsideUp => velocity,
            _ => throw new InvalidOperationException($"Undefined {nameof(CrawlerSurfaceOrientation)} {blockSide}."),
        };

    /// <summary>Runs the separate orange-Zoomer initializer.</summary>
    internal static void InitializeHZoomer(RoomEnemySlot slot)
    {
        CrawlerSpawnSettings settings = slot.SpawnSettings<CrawlerSpawnSettings>();
        SignedEightEight crawlSpeed = settings.CrawlSpeed ??
            throw new InvalidDataException("An orange Zoomer's placement keeps its velocity, which its initializer has no speed for.");

        var state = new CrawlerEnemyState
        {
            Action = CrawlerAction.HZoomerAwaitingAnimationCue,
            SurfaceFamily = settings.SurfaceFamily,
        };
        Bind(slot, state);
        CrawlerSurfaceOrientation orientation = settings.Orientation;
        state.Playback = new(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
            CrawlerAnimationFamily.HZoomer, orientation)), AnimationPlace.StartOfAnimation);
        slot.Frame = EnemyFrameId.Empty;
        slot.AnimationClock.StepOnNextUpdate();
        state.Velocity = SignedForBlockSide(settings.BlockSide, new EightEightVector<RoomPixels>(crawlSpeed, crawlSpeed));
    }

    /// <summary>
    /// Runs the orange Zoomer. This is not an alias of common crawler main: while attached,
    /// orange Zoomer reverses its tangent to pursue Samus and falls off during a Super Missile wall-impact earthquake.
    /// </summary>
    private void RunHZoomerMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("HZoomer AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("HZoomer movement requires room collision data.");
        CrawlerEnemyState state = slot.RequireSpeciesState<CrawlerEnemyState>();
        switch (state.Action)
        {
            case CrawlerAction.HZoomerAwaitingAnimationCue:
                return;
            case CrawlerAction.HZoomerCrawlingVertically:
                RunHZoomerVertical(slot, state, samus, level);
                return;
            case CrawlerAction.HZoomerCrawlingHorizontally:
                RunHZoomerHorizontal(slot, state, samus, level);
                return;
            case CrawlerAction.Falling:
                RunCrawlerFalling(slot, state, level);
                return;
            case CrawlerAction.AwaitingAnimationCue:
            case CrawlerAction.CrawlingVertically:
            case CrawlerAction.CrawlingHorizontally:
            default:
                throw new InvalidDataException(
                    $"HZoomer has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Tracks Samus while attached to a wall, reversing its vertical tangent or falling after lost contact.</summary>
    private void RunHZoomerVertical(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        TriggerCrawlerEarthquakeFall(state);
        SignedSixteenSixteen wallProbe = WithProbeMargin(state.Velocity.X, 1);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, wallProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            SignedSixteenSixteen tangent = state.Velocity.Y.ToSixteenSixteenDelta();
            if (_terrain.MoveEnemyVertically(level, slot, tangent))
            {
                state.Velocity = state.Velocity.ReversedX();
                SetHZoomerVerticalAnimation(slot, state);
                return;
            }

            bool samusBelow = CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole) >= 0;
            bool movingDown = !state.Velocity.Y.IsNegative;
            if (samusBelow != movingDown)
                state.Velocity = state.Velocity.ReversedY();
            return;
        }

        state.ConsecutiveTurnCounter = WrappingCounter.IncrementCount(state.ConsecutiveTurnCounter);
        if (!WrappingCounter.IsBehind(state.ConsecutiveTurnCounter, 4))
        {
            BeginCrawlerFall(state);
            return;
        }
        state.Velocity = state.Velocity.ReversedY();
        SetHZoomerVerticalAnimation(slot, state);
    }

    /// <summary>Tracks Samus along a floor or ceiling, accounting for slopes and repeated loss of surface contact.</summary>
    private void RunHZoomerHorizontal(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        TriggerCrawlerEarthquakeFall(state);
        SignedSixteenSixteen surfaceProbe = WithProbeMargin(state.Velocity.Y, 1);
        if (_terrain.MoveEnemyVertically(level, slot, surfaceProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            SignedSixteenSixteen tangent = GetCrawlerSlopeAdjustedHorizontalDisplacement(slot, state, level);
            if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, tangent))
            {
                state.Velocity = state.Velocity.ReversedY();
                SetHZoomerHorizontalAnimation(slot, state);
                return;
            }

            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            bool samusRight = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) >= 0;
            bool movingRight = !state.Velocity.X.IsNegative;
            if (samusRight != movingRight)
                state.Velocity = state.Velocity.ReversedX();
            return;
        }

        state.ConsecutiveTurnCounter = WrappingCounter.IncrementCount(state.ConsecutiveTurnCounter);
        if (!WrappingCounter.IsBehind(state.ConsecutiveTurnCounter, 4))
        {
            BeginCrawlerFall(state);
            return;
        }
        state.Velocity = state.Velocity.ReversedX();
        SetHZoomerHorizontalAnimation(slot, state);
    }

    /// <summary>Changes an attached orange Zoomer to falling during the Super Missile earthquake.</summary>
    private void TriggerCrawlerEarthquakeFall(CrawlerEnemyState state)
    {
        if (_earthquake.IsShakingWith(SuperMissileDefinitions.WallImpactEarthquake, SuperMissileDefinitions.EarthquakeDuration))
            BeginCrawlerFall(state);
    }

    /// <summary>Selects orange Zoomer's wall-climbing animation from its vertical velocity sign.</summary>
    private static void SetHZoomerVerticalAnimation(
        RoomEnemySlot slot,
        CrawlerEnemyState state) =>
        SetHZoomerAnimation(
            slot,
            state,
            state.Velocity.Y.IsNegative
                ? CrawlerSurfaceAnimations.AnimationFor(
                    CrawlerAnimationFamily.HZoomer, CrawlerSurfaceOrientation.UpsideDown)
                : CrawlerSurfaceAnimations.AnimationFor(
                    CrawlerAnimationFamily.HZoomer, CrawlerSurfaceOrientation.UpsideUp));

    /// <summary>Selects orange Zoomer's floor or ceiling animation from its horizontal velocity sign.</summary>
    private static void SetHZoomerHorizontalAnimation(
        RoomEnemySlot slot,
        CrawlerEnemyState state) =>
        SetHZoomerAnimation(
            slot,
            state,
            state.Velocity.X.IsNegative
                ? CrawlerSurfaceAnimations.AnimationFor(
                    CrawlerAnimationFamily.HZoomer, CrawlerSurfaceOrientation.UpsideRight)
                : CrawlerSurfaceAnimations.AnimationFor(
                    CrawlerAnimationFamily.HZoomer, CrawlerSurfaceOrientation.UpsideLeft));

    /// <summary>Starts the chosen orange-Zoomer animation with fresh frame and loop timers.</summary>
    private static void SetHZoomerAnimation(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        CrawlerSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, CrawlerAnimations.StartingWith(segment));
    }

    /// <summary>Runs shared crawler main AI.</summary>
    private void RunCrawlerMain(RoomEnemySlot slot, RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Crawler movement requires active room collision data.");
        CrawlerEnemyState state = slot.RequireSpeciesState<CrawlerEnemyState>();
        switch (state.Action)
        {
            case CrawlerAction.AwaitingAnimationCue:
                return;
            case CrawlerAction.CrawlingVertically:
                RunCrawlerVertical(slot, state, level);
                return;
            case CrawlerAction.Falling:
                RunCrawlerFalling(slot, state, level);
                return;
            case CrawlerAction.CrawlingHorizontally:
                RunCrawlerHorizontal(slot, state, level);
                return;
            case CrawlerAction.HZoomerAwaitingAnimationCue:
            case CrawlerAction.HZoomerCrawlingVertically:
            case CrawlerAction.HZoomerCrawlingHorizontally:
            default:
                throw new InvalidDataException(
                    $"Crawler has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Moves while attached to a vertical wall.</summary>
    private void RunCrawlerVertical(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level)
    {
        // Sets the next action but still executes this attached movement
        // call. Falling displacement begins on the following AI frame.
        TriggerCrawlerEarthquakeFall(state);
        SignedSixteenSixteen wallProbe = WithProbeMargin(state.Velocity.X, 1);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, wallProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            SignedSixteenSixteen tangent = state.Velocity.Y.ToSixteenSixteenDelta();
            if (!_terrain.MoveEnemyVertically(level, slot, tangent))
                return;

            // Inside corner: the wall normal changes from X to Y.
            state.Velocity = state.Velocity.ReversedX();
            SetCrawlerVerticalSurfaceAnimation(slot, state);
            return;
        }

        state.ConsecutiveTurnCounter = WrappingCounter.IncrementCount(state.ConsecutiveTurnCounter);
        if (!WrappingCounter.IsBehind(state.ConsecutiveTurnCounter, 4))
        {
            BeginCrawlerFall(state);
            return;
        }

        // Outside corner: reverse the tangent for up to three frames while the
        // animation rotates the actor around the exposed block edge.
        state.Velocity = state.Velocity.ReversedY();
        SetCrawlerVerticalSurfaceAnimation(slot, state);
    }

    /// <summary>Moves while attached to a floor or ceiling.</summary>
    private void RunCrawlerHorizontal(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level)
    {
        TriggerCrawlerEarthquakeFall(state);
        SignedSixteenSixteen surfaceProbe = WithProbeMargin(state.Velocity.Y, 1);
        if (_terrain.MoveEnemyVertically(level, slot, surfaceProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            SignedSixteenSixteen tangent = GetCrawlerSlopeAdjustedHorizontalDisplacement(slot, state, level);
            if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, tangent))
            {
                // Inside corner: the surface normal changes from Y to X.
                state.Velocity = state.Velocity.ReversedY();
                SetCrawlerHorizontalSurfaceAnimation(slot, state);
            }
            else
            {
                EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            }
            return;
        }

        state.ConsecutiveTurnCounter = WrappingCounter.IncrementCount(state.ConsecutiveTurnCounter);
        if (!WrappingCounter.IsBehind(state.ConsecutiveTurnCounter, 4))
        {
            BeginCrawlerFall(state);
            return;
        }

        state.Velocity = state.Velocity.ReversedX();
        SetCrawlerHorizontalSurfaceAnimation(slot, state);
    }

    /// <summary>Runs the earthquake/outside-corner falling owner.</summary>
    private void RunCrawlerFalling(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.FallingYVelocity;
        if (_terrain.MoveEnemyVertically(level, slot, displacement))
        {
            // A crawler placed without a crawl speed takes the default speed after its first landing.
            if (state.TakesDefaultSpeedOnLanding)
            {
                state.Velocity = new EightEightVector<RoomPixels>(SignedEightEight.FromParts(0, 128), SignedEightEight.FromParts(0, 128));
            }
            state.FallingYVelocity = SignedSixteenSixteen.Zero;
            state.ConsecutiveTurnCounter = 0;
            state.Action = state.NonFallingAction;
            return;
        }

        state.ApplyFallingGravity();

        // The fall contains this zero-velocity escape even though ordinary falls
        // immediately gain gravity. Retain it for modified parameter/state combinations.
        if (state.FallingYVelocity == SignedSixteenSixteen.Zero)
            state.Action = CrawlerAction.CrawlingVertically;
    }

    /// <summary>Remembers the attached surface action before entering gravity-controlled falling.</summary>
    private static void BeginCrawlerFall(CrawlerEnemyState state)
    {
        state.NonFallingAction = state.Action;
        state.Action = CrawlerAction.Falling;
    }

    /// <summary>Chooses a shared crawler's wall animation from its vertical tangent direction.</summary>
    internal static void SetCrawlerVerticalSurfaceAnimation(
        RoomEnemySlot slot,
        CrawlerEnemyState state)
    {
        CrawlerSurfaceOrientation orientation = state.Velocity.Y.IsNegative
            ? CrawlerSurfaceOrientation.UpsideDown
            : CrawlerSurfaceOrientation.UpsideUp;
        SetCrawlerAnimationFromTable(slot, state, orientation);
    }

    /// <summary>Chooses a shared crawler's floor or ceiling animation from its horizontal tangent direction.</summary>
    internal static void SetCrawlerHorizontalSurfaceAnimation(
        RoomEnemySlot slot,
        CrawlerEnemyState state)
    {
        CrawlerSurfaceOrientation orientation = state.Velocity.X.IsNegative
            ? CrawlerSurfaceOrientation.UpsideRight
            : CrawlerSurfaceOrientation.UpsideLeft;
        SetCrawlerAnimationFromTable(slot, state, orientation);
    }

    /// <summary>Starts the crawler's surface family's animation for <paramref name="orientation"/> and resets animation timers.</summary>
    private static void SetCrawlerAnimationFromTable(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        CrawlerSurfaceOrientation orientation)
    {
        // The orange Zoomer turns corners with its own selection; only the shared crawlers come here.
        if (state.SurfaceFamily is CrawlerAnimationFamily.HZoomer)
            throw new InvalidDataException("The orange Zoomer's animations are not a shared crawler surface family.");
        state.Playback = EnemyAnimationPlayback.Start(slot, CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
            state.SurfaceFamily, orientation)));
    }

    /// <summary>Converts the current crawler velocity to a horizontal move adjusted for the supporting slope.</summary>
    private static SignedSixteenSixteen GetCrawlerSlopeAdjustedHorizontalDisplacement(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level) =>
        GetSurfaceSlopeAdjustedHorizontalDisplacement(slot, state.Velocity, level);

    /// <summary>
    /// Shared geometry from the crawlers' slope-processing horizontal mover. Both the common
    /// crawlers and Yard multiply their tangent by the game's non-square-slope factor.
    /// </summary>
    internal static SignedSixteenSixteen GetSurfaceSlopeAdjustedHorizontalDisplacement(
        RoomEnemySlot slot,
        EightEightVector<RoomPixels> velocity,
        RoomLevelData level)
    {
        int surfaceY = velocity.Y.IsNegative
            ? CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y)
            : CoordinateAxis.Wrap(slot.Position.Y.Whole + slot.Radius.Y - 1);
        int blockX = RoomGridConversions.PixelToBlock(slot.Position.X.Whole);
        int blockY = RoomGridConversions.PixelToBlock(surfaceY);
        if ((uint)blockX < level.Size.X &&
            (uint)blockY < level.Size.Y)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY)));
            SlopeShape slopeShape = block.Behavior.Slope.Shape;
            if (block.CollisionType == RoomCollisionType.Slope && !slopeShape.IsSquare)
            {
                return CrawlerSlopes.Scale(velocity.X, slopeShape);
            }
        }
        return velocity.X.ToSixteenSixteenDelta();
    }

    /// <summary>Gives the occupant's entity its freshly initialized state.</summary>
    private static void Bind(RoomEnemySlot slot, CrawlerEnemyState state) =>
        slot.SetSpeciesState(state);

    /// <summary>The velocity's step with a whole-pixel probe margin added in the direction it moves.</summary>
    internal static SignedSixteenSixteen WithProbeMargin(SignedEightEight velocity, int wholePixels)
    {
        SignedSixteenSixteen margin = SignedSixteenSixteen.FromWholePixels(wholePixels);
        return velocity.ToSixteenSixteenDelta() + (velocity.IsNegative ? -margin : margin);
    }


    /// <summary>Runs Sciser's initializer wrapper.</summary>
    internal static void InitializeSciserAi(RoomEnemySlot slot) =>
        InitializeCrawler(slot, CrawlerAnimationFamily.Sciser, speciesSurfaceFamily: CrawlerAnimationFamily.Sciser);

    /// <summary>Runs Zero's initializer wrapper.</summary>
    internal static void InitializeZeroAi(RoomEnemySlot slot) =>
        InitializeCrawler(slot, CrawlerAnimationFamily.Zero, speciesSurfaceFamily: CrawlerAnimationFamily.Zero);

    /// <summary>Runs Viola's initializer wrapper.</summary>
    internal static void InitializeViolaAi(RoomEnemySlot slot) =>
        InitializeCrawler(slot, CrawlerAnimationFamily.Viola, speciesSurfaceFamily: CrawlerAnimationFamily.Viola);

    /// <summary>Runs the shared initializer wrapper Zeela, Sova, Zoomer and MZoomer use.</summary>
    internal static void InitializeSharedCrawlerAi(RoomEnemySlot slot) =>
        InitializeCrawler(slot, CrawlerAnimationFamily.Shared);

    /// <summary>Runs the shared crawler main AI.</summary>
    internal void RunCrawlerMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) => RunCrawlerMain(slot, frame.Level);

    /// <summary>Runs orange Zoomer's Samus-tracking main AI.</summary>
    internal void RunHZoomerMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunHZoomerMain(slot, frame.Samus, frame.Level);

    /// <summary>Runs the crawlers' surface animations (#641).</summary>
    internal static void AdvanceCrawlerTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, CrawlerAnimations.Segments,
            static (CrawlerEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyCrawlerCue(state, cue) is { } target
                ? new AnimationContinuation<CrawlerSegment>(CrawlerAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
}
