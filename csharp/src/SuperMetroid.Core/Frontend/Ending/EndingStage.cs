using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, palettes, the shared sprites and stars, and the artwork they share.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingStage
{
    /// <summary>Saved clear-time hours, as the game clock or the configured override supplies them, used to select the post-credits reward.</summary>
    private readonly int gameTimeHours;
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    private readonly EndingScreen screen ;
    /// <summary>The ending's live actors in the order they run and draw (<see cref="EndingCastOrderDefinitions"/>).</summary>
    private readonly List<EndingSprite> sprites ;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingStage(int gameTimeHours, EndingScreen screen, List<EndingSprite> sprites)
    {
        this.gameTimeHours = gameTimeHours;
        this.screen = screen;
        this.sprites = sprites;
    }

    /// <summary>What the ending's colours show (#1512).</summary>
    internal EndingColours Colours => screen.Colours;

    /// <summary>Countdown read by the current cinematic phase.</summary>
    internal int PhaseTimer { get; set; }

    /// <summary>Shared fade progression through scene and palette handoffs.</summary>
    private int fadeCounter;

    /// <summary>Master brightness during cinematic fades, from 0 (black) through 15 (full); every fade step clamps to that range.</summary>
    [IntegerQuantity("screen brightness level, zero through fifteen")]
    internal int Brightness { get; set; }

    /// <summary>Turning-floor translation; only X uses a subpixel because the game's Y has no fraction.</summary>
    internal FixedPosition<FloorPixels> FloorPosition { get; set; }

    /// <summary>
    /// The turning floor's zoom, initially its own size. Every scene starts it at an authored zoom and moves it by a few
    /// 256ths per call toward a bound it stops at, so it stays positive and small.
    /// </summary>
    internal FloorZoom FloorZoom { get; set; } = FloorZoom.OwnSize;

    /// <summary>Turning-floor angle, in 256ths of a turn, shared by the escape and planet scenes.</summary>
    internal RotationAngle FloorAngle { get; set; }

    /// <summary>Current cinematic phase, including terminal display states.</summary>
    public EndingCreditsPhase Phase { get; internal set; }

    /// <summary>Selects suitless, helmetless or armored presentation from the clear-time hour thresholds.</summary>
    public EndingReward EndingReward =>
        gameTimeHours < EndingSequenceDefinitions.Rewards.SuitlessMaximumHoursExclusive
        ? EndingReward.Suitless
        : gameTimeHours < EndingSequenceDefinitions.Rewards.HelmetlessMaximumHoursExclusive
            ? EndingReward.Helmetless
            : EndingReward.Armored;

    /// <summary>
    /// Adds an actor in its place in the running order: one the ending shows one of at a time replaces the one already shown
    /// and takes its fixed place after every other actor; any other follows the actors that spawned before it.
    /// </summary>
    /// <returns>The actor added.</returns>
    internal EndingSprite SpawnSprite(
        Position16<ScreenPixels> position,
        EndingSpriteColouring colouring,
        CinematicSpriteSegment segment,
        EndingSpriteRole role)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role), role, "Undefined ending sprite role.");
        int rank = SoleActorRank(role);
        if (rank >= 0)
            sprites.RemoveAll(actor => actor.Role == role);
        var added = new EndingSprite(new IntroDiscoverySprite(position, EndingShades.Drawn(colouring), segment), role);
        // A sole actor stands before the first sole actor ranked after it; any other actor before the first sole actor.
        int place = sprites.FindIndex(actor => SoleActorRank(actor.Role) > rank && SoleActorRank(actor.Role) >= 0);
        sprites.Insert(place >= 0 ? place : sprites.Count, added);
        if (role == EndingSpriteRole.ExplosionStarsLeft)
            added.Sprite.InstallFrameBehavior(CinematicSpriteFrameBehavior.ZebesExplosionStarsWaitForFlyaway);
        return added;
    }

    /// <summary>
    /// Where <paramref name="role"/> stands among the actors the ending shows one of at a time, zero for the first to run, or
    /// minus one for an actor that runs in spawn order ahead of them.
    /// </summary>
    private static int SoleActorRank(EndingSpriteRole role) => Array.IndexOf(EndingCastOrderDefinitions.SoleActorsLast, role);

    /// <summary>Installs a sprite from its authored position, attributes and animation.</summary>
    /// <returns>The actor added.</returns>
    internal EndingSprite SpawnSprite(EndingSpriteDefinition definition, EndingSpriteRole role) =>
        SpawnSprite(
            definition.Position,
            definition.Colouring,
            definition.Animation,
            role);

    /// <summary>
    /// Scene boundaries clear the palette effects' slots (#1544): the records they showed stop showing, as the effects
    /// that showed them have ended.
    /// </summary>
    internal void ResetPaletteFx() => screen.Colours.ResetPaletteFx();

    /// <summary>
    /// Shows the installed colours <paramref name="load"/> (#1512, #1544, #1597): a scene's whole screen of colours also
    /// shows that scene's own backdrop behind every layer; a load within a scene restores some of its colours.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The load is not one the ending makes.</exception>
    internal void LoadColours(EndingColourLoad load) => screen.Colours.Load(load, load switch
    {
        EndingColourLoad.Escape => RequireBackdrops().Escape,
        EndingColourLoad.Explosion => RequireBackdrops().Explosion,
        EndingColourLoad.Credits => RequireBackdrops().Credits,
        EndingColourLoad.PostCredits => RequireBackdrops().PostCredits,
        EndingColourLoad.ExplosionBackgrounds or EndingColourLoad.FinalGunship or EndingColourLoad.RestoredShot or EndingColourLoad.LogoInitial => null,
        _ => throw new ArgumentOutOfRangeException(nameof(load), load, "Undefined ending colour load."),
    });

    /// <summary>The installed scene backdrops.</summary>
    /// <exception cref="InvalidOperationException">The backdrops are not bound.</exception>
    private EndingBackdrops RequireBackdrops() =>
        Backdrops ?? throw new InvalidOperationException("Ending scenes require their installed backdrops.");

    /// <summary>Moves animal and star actors and deletes subordinate explosion layers at their handoffs.</summary>
    internal void StepEndingSpriteFrameBehavior(EndingSprite wrapper)
    {
        IntroDiscoverySprite sprite = wrapper.Sprite;
        switch (wrapper.Role)
        {
            case EndingSpriteRole.AnimalEscape:
                sprite.Position = new(
                    sprite.Position.X.WithWhole(CoordinateAxis.Wrap(sprite.Position.X.Whole + EndingAnimalEscapeDefinitions.XVelocity)),
                    IntroCinematicMotion.AddFraction(sprite.Position.Y, EndingAnimalEscapeDefinitions.YFractionVelocity));
                if (sprite.Position.X.Whole >= EndingAnimalEscapeDefinitions.DeleteX)
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionGlow:
            case EndingSpriteRole.ExplosionStars:
            case EndingSpriteRole.ExplosionLava:
                // The layers follow the planet actor's animation, not a
                // cinematic timer. These subordinate layers disappear with that actor.
                if (!sprites.Any(actor => actor.Role == EndingSpriteRole.ExplodingZebes && actor.Sprite.IsActive))
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionStarsRight:
            case EndingSpriteRole.ExplosionStarsLeft:
                // The right starfield runs no flyaway frame behaviour until its animation installs one.
                if (sprite.FrameBehavior is CinematicSpriteFrameBehavior.ZebesExplosionStarsWaitForFlyaway or CinematicSpriteFrameBehavior.ZebesExplosionStarsMoving)
                    StepFlyawayStars(wrapper, sprite.FrameBehavior.Value);
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionAfterglow:
                // The finale deletes the afterglow at the same handoff out of the last phase.
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            // These actors run no frame behaviour.
            case EndingSpriteRole.CloudRightA:
            case EndingSpriteRole.CloudLeftA:
            case EndingSpriteRole.CloudRightB:
            case EndingSpriteRole.CloudLeftB:
            case EndingSpriteRole.CloudTopA:
            case EndingSpriteRole.CloudTopB:
            case EndingSpriteRole.CloudBottomA:
            case EndingSpriteRole.CloudBottomB:
            case EndingSpriteRole.ExplodingZebes:
            case EndingSpriteRole.ExplosionSilhouette:
            case EndingSpriteRole.OperationWasText:
            case EndingSpriteRole.CompletedSuccessfullyText:
            case EndingSpriteRole.ClearTimeText:
            case EndingSpriteRole.ClearTimeDigit:
            case EndingSpriteRole.RewardSamus:
                break;
            default:
                throw new InvalidOperationException($"Undefined ending sprite role {wrapper.Role}.");
        }
    }

    /// <summary>Starts starfield motion at flyaway and advances its velocity, which slows on every update.</summary>
    private void StepFlyawayStars(EndingSprite stars, CinematicSpriteFrameBehavior frameBehavior)
    {
        IntroDiscoverySprite sprite = stars.Sprite;
        switch (frameBehavior)
        {
            case CinematicSpriteFrameBehavior.ZebesExplosionStarsWaitForFlyaway:
                if (Phase == EndingCreditsPhase.PlanetEscapeFast)
                {
                    sprite.InstallFrameBehavior(CinematicSpriteFrameBehavior.ZebesExplosionStarsMoving);
                    stars.FlyawayVelocity = EndingMotionDefinitions.StarsFlyawayInitialVelocity;
                }
                break;
            case CinematicSpriteFrameBehavior.ZebesExplosionStarsMoving:
                stars.FlyawayVelocity -= EndingMotionDefinitions.StarsFlyawayDeceleration;
                sprite.Position = sprite.Position.WithX(sprite.Position.X.MovedBy(stars.FlyawayVelocity));
                break;
            case CinematicSpriteFrameBehavior.NoOp:
            case CinematicSpriteFrameBehavior.MetroidEgg:
            case CinematicSpriteFrameBehavior.MetroidEggDeleteAfterCrossFade:
            case CinematicSpriteFrameBehavior.MetroidEggParticle:
            case CinematicSpriteFrameBehavior.MetroidEggSlimeDrops:
            case CinematicSpriteFrameBehavior.BabyMetroidBeingDelivered:
            case CinematicSpriteFrameBehavior.BabyMetroidBeingExamined:
            case CinematicSpriteFrameBehavior.IntroMotherBrainCrossFading:
            case CinematicSpriteFrameBehavior.IntroRinkaMovingHitsSamus:
            case CinematicSpriteFrameBehavior.IntroRinkaMovingMissesSamus:
            case CinematicSpriteFrameBehavior.ConfusedBabyWaitingForHatch:
            case CinematicSpriteFrameBehavior.ConfusedBabyHatched:
            case CinematicSpriteFrameBehavior.ConfusedBabyIdling:
            case CinematicSpriteFrameBehavior.ConfusedBabyDancing:
            case CinematicSpriteFrameBehavior.CeresStars:
            case CinematicSpriteFrameBehavior.CeresLargeAsteroids:
            case CinematicSpriteFrameBehavior.CeresUnderAttack:
            case CinematicSpriteFrameBehavior.CeresSmallAsteroid:
            case CinematicSpriteFrameBehavior.CeresPurpleSpaceVortex:
            case CinematicSpriteFrameBehavior.CeresStationaryVortexNoOp:
            case CinematicSpriteFrameBehavior.CeresExplosionSpawnerRepeating:
            case CinematicSpriteFrameBehavior.CeresExplosion:
            case CinematicSpriteFrameBehavior.Zebes:
            case CinematicSpriteFrameBehavior.ZebesSlideSceneAway:
            case CinematicSpriteFrameBehavior.ZebesStarsCompletionWait:
            case CinematicSpriteFrameBehavior.ZebesStarsCompletionSlideSceneAway:
            case CinematicSpriteFrameBehavior.ZebesStarsWait:
            case CinematicSpriteFrameBehavior.ZebesStarsSlideSceneAway:
            case CinematicSpriteFrameBehavior.SuperMetroidIconTopHalf:
            case CinematicSpriteFrameBehavior.SuperMetroidIconBottomHalf:
            case CinematicSpriteFrameBehavior.RewardSuitlessUpperNoOp:
            case CinematicSpriteFrameBehavior.RewardBodyFlight:
            case CinematicSpriteFrameBehavior.RewardHeadFlight:
            case CinematicSpriteFrameBehavior.RewardLanding:
            default:
                throw new InvalidOperationException(
                    $"Flyaway stars do not run {frameBehavior}.");
        }
    }

    /// <summary>The palette effects the ending runs, which its colours own (#1597).</summary>
    internal RoomPaletteFxSystem PaletteFxSystem => screen.Colours.PaletteFx;

    /// <summary>The ending scenes' installed backdrops (#1544).</summary>
    internal EndingBackdrops? Backdrops { get; private set; }

    /// <summary>Raises screen brightness one step every two calls until full brightness.</summary>
    internal bool StepFastFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            Brightness = Math.Min(15, Brightness + 1);
        }
        return Brightness == 15;
    }

    /// <summary>Lowers screen brightness one step every two calls until blank.</summary>
    internal bool StepFastFadeOut()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            Brightness = Math.Max(0, Brightness - 1);
        }
        return Brightness == 0;
    }

    /// <summary>Raises screen brightness one step every four calls for the post-credits reveal.</summary>
    internal bool StepSlowFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 3;
            Brightness = Math.Min(15, Brightness + 1);
        }
        return Brightness == 15;
    }

    /// <summary>Restarts the fade cadence: the next fade step lands two calls from now.</summary>
    internal void RestartFadeCadence() => fadeCounter = 1;

    /// <summary>Selects the backdrops later scenes show (#1544).</summary>
    internal void BindBackdrops(EndingBackdrops? value) => Backdrops = value;
}
