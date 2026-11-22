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
internal sealed partial class CrawlersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>
    /// A crawler set up in its animation family that crawls by the shared crawl: Sciser, Zero and Viola each in their own,
    /// Zeela, Sova and both Zoomers in the shared family; or an orange Zoomer (#2146).
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    [field: NonSerialized]
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<CrawlerSpawnSettings>([EnemyDefinitionId.Sciser], (occupant, settings, place, common) =>
            EnemyOccupant.Running(new SurfaceCrawlerEntity(this, occupant, place, common, CrawlerAnimationFamily.Sciser, settings)));
        registrar.Spawns<CrawlerSpawnSettings>([EnemyDefinitionId.Zero], (occupant, settings, place, common) =>
            EnemyOccupant.Running(new SurfaceCrawlerEntity(this, occupant, place, common, CrawlerAnimationFamily.Zero, settings)));
        registrar.Spawns<ViolaSpawnSettings>([EnemyDefinitionId.Viola], (occupant, settings, place, common) =>
            EnemyOccupant.Running(new SurfaceCrawlerEntity(this, occupant, place, common, CrawlerAnimationFamily.Viola, OnBlockSide(settings))));
        registrar.Spawns<CrawlerSpawnSettings>(
            [EnemyDefinitionId.Zeela, EnemyDefinitionId.Sova, EnemyDefinitionId.Zoomer, EnemyDefinitionId.ArmoredZoomer],
            (occupant, settings, place, common) =>
                EnemyOccupant.Running(new SurfaceCrawlerEntity(this, occupant, place, common, CrawlerAnimationFamily.Shared, settings)));
        registrar.Spawns<OrangeZoomerSpawnSettings>([EnemyDefinitionId.OrangeZoomer], (occupant, settings, place, common) =>
            EnemyOccupant.Running(new OrangeZoomerEntity(this, occupant, place, common, settings)));
    }

    /// <summary>
    /// A Viola's settings as a crawler's: it starts crawling along the side of the block it is placed against, at its
    /// placement's speed or the default one.
    /// </summary>
    private static CrawlerSpawnSettings OnBlockSide(ViolaSpawnSettings viola) =>
        new(Ensure.NotNull(viola).BlockSide, viola.BlockSide, viola.CrawlSpeed);

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

    /// <summary>
    /// The crawl a crawler on its surface takes: along a side wall it crawls vertically, along a floor or ceiling
    /// horizontally; its entity decides how, the orange Zoomer by its own pursuit.
    /// </summary>
    internal static CrawlerAction CrawlActionOn(CrawlerEnemyState state) => state.Surface switch
    {
        CrawlerSurfaceOrientation.OnLeftWall or CrawlerSurfaceOrientation.OnRightWall => CrawlerAction.CrawlingVertically,
        CrawlerSurfaceOrientation.OnCeiling or CrawlerSurfaceOrientation.OnFloor => CrawlerAction.CrawlingHorizontally,
        _ => throw new InvalidOperationException($"Undefined {nameof(CrawlerSurfaceOrientation)} {state.Surface}."),
    };

    /// <summary>Puts the crawler on <paramref name="surface"/>: it shows that surface and crawls along it.</summary>
    private static void TakeSurface(CrawlerEnemyState state, CrawlerSurfaceOrientation surface)
    {
        state.Surface = surface;
        state.Action = CrawlActionOn(state);
    }

    /// <summary>
    /// Initializes a crawler of any species through the shared setup. Each species differs only by the family whose
    /// animations it shows on each surface and switches between at corners.
    /// </summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static CrawlerEnemyState InitializeCrawler(CrawlerSpawnSettings settings, RoomEnemyPlace place, CrawlerAnimationFamily animationFamily)
    {
        CrawlerSurfaceOrientation orientation = settings.Orientation;
        CrawlerSurfaceOrientation blockSide = settings.BlockSide;
        SignedEightEight? crawlSpeed = settings.CrawlSpeed;

        return SetUpCrawler(place, animationFamily, orientation, blockSide, crawlSpeed);
    }

    /// <summary>
    /// The setup every crawler shares: it settles for its first update before it crawls on <paramref name="orientation"/>,
    /// showing <paramref name="animationFamily"/>'s animations, its velocity the crawl speed signed for
    /// <paramref name="blockSide"/>; one placed without a crawl speed takes the default speed on its first landing.
    /// </summary>
    private static CrawlerEnemyState SetUpCrawler(RoomEnemyPlace place, CrawlerAnimationFamily animationFamily, CrawlerSurfaceOrientation orientation,
        CrawlerSurfaceOrientation blockSide, SignedEightEight? crawlSpeed)
    {
        var state = new CrawlerEnemyState(CrawlerAction.Settling)
        {
            TakesDefaultSpeedOnLanding = crawlSpeed is null,
            SurfaceFamily = animationFamily,
            Surface = orientation,
        };

        state.Playback = new(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
            animationFamily, orientation)), AnimationPlace.StartOfAnimation);
        place.Frame = EnemyFrameId.PointAtOrigin;
        place.AnimationClock.StepOnNextUpdate();

        state.Velocity = ResetCrawlerVelocities(blockSide, crawlSpeed, state.Velocity);
        return state;
    }

    /// <summary>Shared crawler velocity reset, also called by Yard after landing.</summary>
    /// <param name="blockSide">The side of its block the crawler holds, which signs the velocity.</param>
    /// <param name="crawlSpeed">The crawl speed on both axes, or none to keep <paramref name="velocity"/>.</param>
    /// <param name="velocity">The velocity kept when there is no crawl speed.</param>
    /// <returns>The reset velocity: <paramref name="velocity"/> or the crawl speed on both axes, signed by the block side.</returns>
    internal static PixelVelocity<RoomPixels> ResetCrawlerVelocities(
        CrawlerSurfaceOrientation blockSide, SignedEightEight? crawlSpeed, PixelVelocity<RoomPixels> velocity)
    {
        if (crawlSpeed is SignedEightEight speed)
            velocity = new(speed, speed);

        return SignedForBlockSide(blockSide, velocity);
    }

    /// <summary>A crawl velocity signed for the side of its block the crawler holds.</summary>
    private static PixelVelocity<RoomPixels> SignedForBlockSide(
        CrawlerSurfaceOrientation blockSide, PixelVelocity<RoomPixels> velocity) => blockSide switch
        {
            CrawlerSurfaceOrientation.OnLeftWall => velocity.ReversedX(),
            CrawlerSurfaceOrientation.OnCeiling => velocity.ReversedY(),
            CrawlerSurfaceOrientation.OnRightWall or CrawlerSurfaceOrientation.OnFloor => velocity,
            _ => throw new InvalidOperationException($"Undefined {nameof(CrawlerSurfaceOrientation)} {blockSide}."),
        };

    /// <summary>Runs the orange Zoomer's setup, the shared one on its family's surfaces; its placement always names its speed.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static CrawlerEnemyState InitializeOrangeZoomer(OrangeZoomerSpawnSettings settings, RoomEnemyPlace place)
    {
        SignedEightEight crawlSpeed = settings.CrawlSpeed;

        CrawlerSurfaceOrientation orientation = settings.Orientation;
        var state = new CrawlerEnemyState(CrawlerAction.Settling)
        {
            SurfaceFamily = CrawlerAnimationFamily.Shared,
            Surface = orientation,
        };
        state.Playback = new(CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
            CrawlerAnimationFamily.Shared, orientation)), AnimationPlace.StartOfAnimation);
        place.Frame = EnemyFrameId.PointAtOrigin;
        place.AnimationClock.StepOnNextUpdate();
        state.Velocity = SignedForBlockSide(settings.BlockSide, new PixelVelocity<RoomPixels>(crawlSpeed, crawlSpeed));
        return state;
    }

    /// <summary>What <see cref="RunOrangeZoomerMain"/> does for each <see cref="CrawlerAction"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<CrawlerAction, Action<RoomEnemyPlace, SamusState, RoomLevelData, CrawlerEnemyState>> RunOrangeZoomerMainForCrawlerAction => field ??= new(new Dictionary<CrawlerAction, Action<RoomEnemyPlace, SamusState, RoomLevelData, CrawlerEnemyState>>
    {
        [CrawlerAction.Settling] = (place, samus, level, state) => state.Action = CrawlActionOn(state),
        [CrawlerAction.CrawlingVertically] = (place, samus, level, state) => RunCrawlerVertical(place, state, level, samus),
        [CrawlerAction.CrawlingHorizontally] = (place, samus, level, state) => RunCrawlerHorizontal(place, state, level, samus),
        [CrawlerAction.Falling] = (place, samus, level, state) => RunCrawlerFalling(place, state, level),
    });

    /// <summary>
    /// Runs the orange Zoomer, which crawls as the other crawlers do except that, while attached, it reverses its
    /// tangent to pursue Samus and falls off during a Super Missile wall-impact earthquake.
    /// </summary>
    private void RunOrangeZoomerMain(
        RoomEnemyPlace place, CrawlerEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        RunOrangeZoomerMainForCrawlerAction[state.Action](place, samus, level, state);
    }

    /// <summary>Changes an attached orange Zoomer to falling during the Super Missile earthquake.</summary>
    private void TriggerCrawlerEarthquakeFall(CrawlerEnemyState state)
    {
        if (_earthquake.IsShakingWith(SuperMissileDefinitions.WallImpactEarthquake, SuperMissileDefinitions.EarthquakeDuration))
            BeginCrawlerFall(state);
    }

    /// <summary>What <see cref="RunCrawlerMain"/> does for each <see cref="CrawlerAction"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<CrawlerAction, Action<RoomEnemyPlace, RoomLevelData, CrawlerEnemyState>> RunCrawlerMainForCrawlerAction => field ??= new(new Dictionary<CrawlerAction, Action<RoomEnemyPlace, RoomLevelData, CrawlerEnemyState>>
    {
        [CrawlerAction.Settling] = (place, level, state) => state.Action = CrawlActionOn(state),
        [CrawlerAction.CrawlingVertically] = (place, level, state) => RunCrawlerVertical(place, state, level, pursued: null),
        [CrawlerAction.Falling] = (place, level, state) => RunCrawlerFalling(place, state, level),
        [CrawlerAction.CrawlingHorizontally] = (place, level, state) => RunCrawlerHorizontal(place, state, level, pursued: null),
    });

    /// <summary>Runs shared crawler main AI.</summary>
    private void RunCrawlerMain(RoomEnemyPlace place, CrawlerEnemyState state, RoomLevelData level)
    {
        RunCrawlerMainForCrawlerAction[state.Action](place, level, state);
    }

    /// <summary>
    /// Moves while attached to a vertical wall; an orange Zoomer, which pursues <paramref name="pursued"/>, turns along the
    /// wall towards her after each move that reaches no corner. Other crawlers pursue no one.
    /// </summary>
    private void RunCrawlerVertical(
        RoomEnemyPlace place,
        CrawlerEnemyState state,
        RoomLevelData level,
        SamusState? pursued)
    {
        // Sets the next action but still moves along the wall in this update; the fall begins on the next.
        TriggerCrawlerEarthquakeFall(state);
        SignedSixteenSixteen wallProbe = WithProbeMargin(state.Velocity.X, CrawlerMotionDefinitions.ProbeMargin);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, wallProbe))
        {
            state.ConsecutiveMissedProbes = 0;
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, place);
            SignedSixteenSixteen tangent = state.Velocity.Y.ToSixteenSixteenDelta();
            if (_terrain.MoveEnemyVertically(level, place, tangent))
            {
                // Inside corner: the wall normal changes from X to Y.
                state.Velocity = state.Velocity.ReversedX();
                SetCrawlerVerticalSurfaceAnimation(place, state);
                return;
            }

            if (pursued is { } samus && samus.Position.Y.Whole >= place.Position.Y.Whole == state.Velocity.Y.IsNegative)
                state.Velocity = state.Velocity.ReversedY();
            return;
        }

        if (state.MissedSurfaceTooOften())
        {
            BeginCrawlerFall(state);
            return;
        }

        // Outside corner: reverse the tangent for up to its missed probes while the
        // animation rotates the actor around the exposed block edge.
        state.Velocity = state.Velocity.ReversedY();
        SetCrawlerVerticalSurfaceAnimation(place, state);
    }

    /// <summary>
    /// Moves while attached to a floor or ceiling, accounting for slopes and repeated loss of surface contact; an orange
    /// Zoomer, which pursues <paramref name="pursued"/>, turns along the surface towards her after each move that reaches no
    /// corner. Other crawlers pursue no one.
    /// </summary>
    private void RunCrawlerHorizontal(
        RoomEnemyPlace place,
        CrawlerEnemyState state,
        RoomLevelData level,
        SamusState? pursued)
    {
        TriggerCrawlerEarthquakeFall(state);
        SignedSixteenSixteen surfaceProbe = WithProbeMargin(state.Velocity.Y, CrawlerMotionDefinitions.ProbeMargin);
        if (_terrain.MoveEnemyVertically(level, place, surfaceProbe))
        {
            state.ConsecutiveMissedProbes = 0;
            SignedSixteenSixteen tangent = GetCrawlerSlopeAdjustedHorizontalDisplacement(place, state, level);
            if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, tangent))
            {
                // Inside corner: the surface normal changes from Y to X.
                state.Velocity = state.Velocity.ReversedY();
                SetCrawlerHorizontalSurfaceAnimation(place, state);
            }
            else
            {
                EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, place);
                if (pursued is { } samus && samus.Position.X.Whole >= place.Position.X.Whole == state.Velocity.X.IsNegative)
                    state.Velocity = state.Velocity.ReversedX();
            }
            return;
        }

        if (state.MissedSurfaceTooOften())
        {
            BeginCrawlerFall(state);
            return;
        }

        state.Velocity = state.Velocity.ReversedX();
        SetCrawlerHorizontalSurfaceAnimation(place, state);
    }

    /// <summary>Runs the earthquake/outside-corner falling owner.</summary>
    private void RunCrawlerFalling(
        RoomEnemyPlace place,
        CrawlerEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.FallingYVelocity;
        if (_terrain.MoveEnemyVertically(level, place, displacement))
        {
            // A crawler placed without a crawl speed takes the default speed after its first landing.
            if (state.TakesDefaultSpeedOnLanding)
            {
                state.Velocity = CrawlerMotionDefinitions.DefaultCrawlSpeed;
            }
            state.FallingYVelocity = SignedSixteenSixteen.Zero;
            state.ConsecutiveMissedProbes = 0;
            state.Action = state.NonFallingAction;
            return;
        }

        state.ApplyFallingGravity();
    }

    /// <summary>Remembers the attached surface action before entering gravity-controlled falling.</summary>
    private static void BeginCrawlerFall(CrawlerEnemyState state)
    {
        state.NonFallingAction = state.Action;
        state.Action = CrawlerAction.Falling;
    }

    /// <summary>Turns a crawler off a wall onto the floor or ceiling its vertical tangent leads to.</summary>
    internal static void SetCrawlerVerticalSurfaceAnimation(
        RoomEnemyPlace place,
        CrawlerEnemyState state)
    {
        CrawlerSurfaceOrientation orientation = state.Velocity.Y.IsNegative
            ? CrawlerSurfaceOrientation.OnCeiling
            : CrawlerSurfaceOrientation.OnFloor;
        PutCrawlerOnSurface(place, state, orientation);
    }

    /// <summary>Turns a crawler off a floor or ceiling onto the wall its horizontal tangent leads to.</summary>
    internal static void SetCrawlerHorizontalSurfaceAnimation(
        RoomEnemyPlace place,
        CrawlerEnemyState state)
    {
        CrawlerSurfaceOrientation orientation = state.Velocity.X.IsNegative
            ? CrawlerSurfaceOrientation.OnLeftWall
            : CrawlerSurfaceOrientation.OnRightWall;
        PutCrawlerOnSurface(place, state, orientation);
    }

    /// <summary>
    /// Puts the crawler on <paramref name="orientation"/>: it starts its family's animation for that surface with a fresh
    /// frame timer and crawls along it.
    /// </summary>
    private static void PutCrawlerOnSurface(
        RoomEnemyPlace place,
        CrawlerEnemyState state,
        CrawlerSurfaceOrientation orientation)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, CrawlerAnimations.StartingWith(CrawlerSurfaceAnimations.AnimationFor(
            state.SurfaceFamily, orientation)));
        TakeSurface(state, orientation);
    }

    /// <summary>Converts the current crawler velocity to a horizontal move adjusted for the supporting slope.</summary>
    private static SignedSixteenSixteen GetCrawlerSlopeAdjustedHorizontalDisplacement(
        RoomEnemyPlace place,
        CrawlerEnemyState state,
        RoomLevelData level) =>
        GetSurfaceSlopeAdjustedHorizontalDisplacement(place, state.Velocity, level);

    /// <summary>
    /// The crawl's horizontal step along the slope it stands on, which the common crawlers and Yard share.
    /// </summary>
    internal static SignedSixteenSixteen GetSurfaceSlopeAdjustedHorizontalDisplacement(
        RoomEnemyPlace place,
        PixelVelocity<RoomPixels> velocity,
        RoomLevelData level)
    {
        int surfaceY = velocity.Y.IsNegative
            ? place.Position.Y.Whole - place.Radius.Y
            : place.Position.Y.Whole + place.Radius.Y - 1;
        Position16<RoomBlocks> surface = new Position16<RoomPixels>(place.Position.X.Whole, surfaceY).ToRoomBlock();
        if (level.Size.Contains(surface))
        {
            RoomCollisionBlock block = level.GetCollisionBlock(surface);
            SlopeShape slopeShape = block.Behavior.Slope.Shape;
            if (block.CollisionType == RoomCollisionType.Slope && !slopeShape.IsSquare)
            {
                return CrawlerSlopes.Scale(velocity.X, slopeShape);
            }
        }
        return velocity.X.ToSixteenSixteenDelta();
    }


    /// <summary>The velocity's step with a whole-pixel probe margin added in the direction it moves.</summary>
    internal static SignedSixteenSixteen WithProbeMargin(SignedEightEight velocity, int wholePixels)
    {
        SignedSixteenSixteen margin = SignedSixteenSixteen.FromWholePixels(wholePixels);
        return velocity.ToSixteenSixteenDelta() + velocity.Along(margin);
    }

    /// <summary>Runs the shared crawler main AI.</summary>
    internal void RunCrawlerMainAi(RoomEnemyPlace place, CrawlerEnemyState crawlerEnemyState, IEnemyAiFrame frame) => RunCrawlerMain(place, crawlerEnemyState, frame.Level);

    /// <summary>Runs orange Zoomer's Samus-tracking main AI.</summary>
    internal void RunOrangeZoomerMainAi(RoomEnemyPlace place, CrawlerEnemyState crawlerEnemyState, IEnemyAiFrame frame) =>
        RunOrangeZoomerMain(place, crawlerEnemyState, frame.Samus, frame.Level);

    /// <summary>Runs the crawlers' surface animations (#641).</summary>
    internal static void AdvanceCrawlerTypedAnimation(RoomEnemyPlace place, CrawlerEnemyState crawlerEnemyState) =>
        EnemyAnimationPlayback.AdvanceSilently(place, crawlerEnemyState, CrawlerAnimations.Segments,
            static (CrawlerEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);
}
