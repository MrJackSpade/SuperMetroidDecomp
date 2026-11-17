using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, colours, the shared sprites and stars, and the artwork they share.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingStage
{
    /// <summary>Saved clear-time hours, as the game clock or the configured override supplies them, used to select the post-credits reward.</summary>
    private readonly int gameTimeHours;
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    private readonly EndingScreen screen ;
    /// <summary>The ending's live actors in the order they run and draw (<see cref="EndingCastOrderDefinitions"/>).</summary>
    private readonly List<EndingSprite> sprites ;

    /// <summary>Builds the component from the collaborators it uses and the installed scene backdrops (#1544).</summary>
    internal EndingStage(int gameTimeHours, EndingScreen screen, List<EndingSprite> sprites, EndingBackdrops backdrops)
    {
        Backdrops = Ensure.NotNull(backdrops);
        this.gameTimeHours = gameTimeHours;
        this.screen = screen;
        this.sprites = sprites;
    }

    /// <summary>What the ending's colours show (#1512).</summary>
    internal EndingColours Colours => screen.Colours;

    /// <summary>Countdown read by the current cinematic phase.</summary>
    internal int PhaseTimer { get; set; }

    /// <summary>The screen's brightness and the cadence of its fades, shared through scene and colour handoffs.</summary>
    internal ScreenFade Fade { get; } = new(0);

    /// <summary>Master brightness during cinematic fades, from black through full.</summary>
    internal int Brightness => Fade.Level;

    /// <summary>Turning-floor translation; only X uses a subpixel because the game's Y has no fraction.</summary>
    internal FixedVector<FloorPixels> FloorPosition { get; set; }

    /// <summary>
    /// The turning floor's zoom, initially its own size. Every scene starts it at an authored zoom and moves it by a few
    /// 256ths an update toward a bound it stops at, so it stays positive and small.
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
        Offset16<ScreenPixels> position,
        EndingSpriteColouring colouring,
        EndingSceneAnimation animation,
        EndingSpriteRole role) =>
        Place(new EndingPictureSprite(new(position, EndingShades.Drawn(colouring), EndingSceneAnimations.Pictures, animation), role));

    /// <summary>Adds the typed text <paramref name="definition"/> authors in its place in the running order (#2195).</summary>
    /// <returns>The actor added.</returns>
    internal EndingSprite SpawnText(EndingTextDefinition definition, EndingSpriteRole role) =>
        Place(new EndingTextSprite(new(definition.Position, EndingShades.Drawn(definition.Colouring), EndingTextAnimations.Texts, definition.Text), role));

    /// <summary>
    /// Adds <paramref name="added"/> in its place in the running order: one the ending shows one of at a time replaces the
    /// one already shown and takes its fixed place after every other actor; any other follows the actors that spawned
    /// before it.
    /// </summary>
    private EndingSprite Place(EndingSprite added)
    {
        EndingSpriteRole role = added.Role;
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(added), role, "Undefined ending sprite role.");
        int rank = SoleActorRank(role);
        if (rank >= 0)
            sprites.RemoveAll(actor => actor.Role == role);
        // A sole actor stands before the first sole actor ranked after it; any other actor before the first sole actor.
        int place = sprites.FindIndex(actor => SoleActorRank(actor.Role) > rank && SoleActorRank(actor.Role) >= 0);
        sprites.Insert(place >= 0 ? place : sprites.Count, added);
        if (role == EndingSpriteRole.ExplosionStarsLeft)
            added.StarsMotion = EndingStarsMotion.WaitingForFlyaway;
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
    /// Scene boundaries end the running colour effects (#1544): the steps they showed stop showing, as the effects
    /// that showed them have ended.
    /// </summary>
    internal void ResetColourEffects() => screen.Colours.ResetColourEffects();

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
    private EndingBackdrops RequireBackdrops() => Backdrops;

    /// <summary>Moves animal and star actors and deletes subordinate explosion layers at their handoffs.</summary>
    internal void StepEndingSpriteMotion(EndingSprite wrapper)
    {
        ICinematicActor<EndingSceneCue> sprite = wrapper.Sprite;
        switch (wrapper.Role)
        {
            case EndingSpriteRole.AnimalEscape:
                sprite.Position = new(
                    sprite.Position.X.WithWholePixels(sprite.Position.X.WholePixels + EndingAnimalEscapeDefinitions.XVelocity),
                    IntroCinematicMotion.AddFraction(sprite.Position.Y, EndingAnimalEscapeDefinitions.YFractionVelocity));
                if (sprite.ScreenPlace.X >= EndingAnimalEscapeDefinitions.DeleteX)
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
                // The right starfield does not wait for the flyaway until its animation has it wait.
                if (wrapper.StarsMotion is { } starsMotion)
                    StepFlyawayStars(wrapper, starsMotion);
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionAfterglow:
                // The finale deletes the afterglow at the same handoff out of the last phase.
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            // These actors have no motion of their own.
            case EndingSpriteRole.SideCloudRightLeading:
            case EndingSpriteRole.SideCloudLeftLeading:
            case EndingSpriteRole.SideCloudRightTrailing:
            case EndingSpriteRole.SideCloudLeftTrailing:
            case EndingSpriteRole.TopCloudPattern:
            case EndingSpriteRole.TopCloudEdge:
            case EndingSpriteRole.BottomCloudEdge:
            case EndingSpriteRole.BottomCloudPattern:
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
    private void StepFlyawayStars(EndingSprite stars, EndingStarsMotion motion)
    {
        ICinematicActor<EndingSceneCue> sprite = stars.Sprite;
        switch (motion)
        {
            case EndingStarsMotion.WaitingForFlyaway:
                if (Phase == EndingCreditsPhase.PlanetEscapeFast)
                {
                    stars.StarsMotion = EndingStarsMotion.Moving;
                    stars.FlyawayVelocity = EndingMotionDefinitions.StarsFlyawayInitialVelocity;
                }
                break;
            case EndingStarsMotion.Moving:
                stars.FlyawayVelocity -= EndingMotionDefinitions.StarsFlyawayDeceleration;
                sprite.Position = sprite.Position.WithX(sprite.Position.X + stars.FlyawayVelocity);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EndingStarsMotion)} {motion}.");
        }
    }

    /// <summary>The colour effects the ending runs, which its colours own (#1597).</summary>
    internal ColourEffectSystem ColourEffects => screen.Colours.ColourEffects;

    /// <summary>The ending scenes' installed backdrops (#1544).</summary>
    internal EndingBackdrops Backdrops { get; private set; }

    /// <summary>Starts a fast fade: the next step lands two updates from now, and one every two updates after it.</summary>
    internal void StartFastFade() => Fade.StepEvery(EndingCadenceDefinitions.FastFadeUpdatesPerStep);

    /// <summary>Starts the post-credits reveal's slow fade: the next step lands two updates from now, then one every four updates.</summary>
    internal void StartPostCreditsFade() =>
        Fade.SetCadence(EndingCadenceDefinitions.PostCreditsFadeStepDelay, EndingCadenceDefinitions.PostCreditsFadeFirstWait);

    /// <summary>Selects the backdrops later scenes show (#1544).</summary>
    internal void BindBackdrops(EndingBackdrops value) => Backdrops = Ensure.NotNull(value);
}
