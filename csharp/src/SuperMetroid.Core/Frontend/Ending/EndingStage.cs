using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What every ending scene draws on: brightness, the turning-floor view, colours, the shared sprites and stars, and the artwork they share.</summary>
internal sealed partial class EndingStage
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

    /// <summary>The screen's brightness and the cadence of its fades, shared through scene and colour handoffs.</summary>
    internal ScreenFade Fade { get; } = new(DisplayBrightness.Black);

    /// <summary>The screen's brightness through the scene's fades, from black through full.</summary>
    internal DisplayBrightness Brightness => Fade.Level;

    /// <summary>The turning floor's view, which the escape, the explosion and the planet escape share (#2541).</summary>
    internal EndingTurningFloor Floor { get; } = new();

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
    internal void SpawnText(EndingTextDefinition definition, EndingSpriteRole role) =>
        Place(new EndingTextSprite(new(definition.Position, EndingShades.Drawn(definition.Colouring), EndingTextAnimations.Texts, definition.Text), role));

    /// <summary>
    /// Adds <paramref name="added"/> in its place in the running order: one the ending shows one of at a time replaces the
    /// one already shown and takes its fixed place after every other actor; any other follows the actors that spawned
    /// before it.
    /// </summary>
    private EndingSprite Place(EndingSprite added)
    {
        EndingSpriteRole role = added.Role;
        int? rank = SoleActorRank(role);
        if (rank is not null)
            sprites.RemoveAll(actor => actor.Role == role);
        // A sole actor stands before the first sole actor ranked after it; any other actor before the first sole actor.
        int place = sprites.FindIndex(actor => SoleActorRank(actor.Role) is int other && (rank is not int own || other > own));
        sprites.Insert(place >= 0 ? place : sprites.Count, added);
        return added;
    }

    /// <summary>
    /// Where <paramref name="role"/> stands among the actors the ending shows one of at a time, zero for the first to run, or
    /// null for an actor that runs in spawn order ahead of them.
    /// </summary>
    private static int? SoleActorRank(EndingSpriteRole role) =>
        Array.IndexOf(EndingCastOrderDefinitions.SoleActorsLast, role) is int rank and >= 0 ? rank : null;

    /// <summary>Adds an actor from its definition's position, colouring and animation.</summary>
    internal void SpawnSprite(EndingSpriteDefinition definition, EndingSpriteRole role) =>
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
    internal void LoadColours(EndingColourLoad load) => ColourLoads[load](screen.Colours, Backdrops);

    /// <summary>
    /// What each colour load does: a scene's whole screen of colours starts the scene with its backdrop, and a load within a
    /// scene restores some of its colours, showing no backdrop of its own.
    /// </summary>
    private static IdentityTable<EndingColourLoad, Action<EndingColours, EndingBackdrops>> ColourLoads => field ??= new(new Dictionary<EndingColourLoad, Action<EndingColours, EndingBackdrops>>
    {
        [EndingColourLoad.Escape] = static (colours, backdrops) => colours.StartScene(EndingColourLoad.Escape, backdrops.Escape),
        [EndingColourLoad.Explosion] = static (colours, backdrops) => colours.StartScene(EndingColourLoad.Explosion, backdrops.Explosion),
        [EndingColourLoad.Credits] = static (colours, backdrops) => colours.StartScene(EndingColourLoad.Credits, backdrops.Credits),
        [EndingColourLoad.PostCredits] = static (colours, backdrops) => colours.StartScene(EndingColourLoad.PostCredits, backdrops.PostCredits),
        [EndingColourLoad.ExplosionBackgrounds] = static (colours, _) => colours.Restore(EndingColourLoad.ExplosionBackgrounds),
        [EndingColourLoad.FinalGunship] = static (colours, _) => colours.Restore(EndingColourLoad.FinalGunship),
        [EndingColourLoad.RestoredShot] = static (colours, _) => colours.Restore(EndingColourLoad.RestoredShot),
        [EndingColourLoad.LogoInitial] = static (colours, _) => colours.Restore(EndingColourLoad.LogoInitial),
    });


    /// <summary>Whether the exploding planet's actor is still shown.</summary>
    internal bool ShowsExplodingPlanet => sprites.Any(actor => actor.Role == EndingSpriteRole.ExplodingZebes && actor.Sprite.IsActive);

    /// <summary>The colour effects the ending runs, which its colours own (#1597).</summary>
    internal ColourEffectSystem ColourEffects => screen.Colours.ColourEffects;

    /// <summary>The ending scenes' installed backdrops (#1544).</summary>
    internal EndingBackdrops Backdrops { get; private set; }

    /// <summary>Starts a fast fade, a step every <see cref="EndingCadenceDefinitions.FastFadeUpdatesPerStep"/> from now.</summary>
    internal void StartFastFade() => Fade.StepEvery(EndingCadenceDefinitions.FastFadeUpdatesPerStep);

    /// <summary>Starts the post-credits reveal's slow fade: its first step lands soon, then the rest at the slow fade's pace.</summary>
    internal void StartPostCreditsFade() =>
        Fade.SetCadence(EndingCadenceDefinitions.PostCreditsFadeStepDelay, EndingCadenceDefinitions.PostCreditsFadeFirstWait);

    /// <summary>Selects the backdrops later scenes show (#1544).</summary>
    internal void BindBackdrops(EndingBackdrops value) => Backdrops = Ensure.NotNull(value);
}
