using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Zebes exploding in the ending: its setup and the colour crossfade into it.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingZebesExplosion
{
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    private readonly EndingScreen screen ;
    /// <summary>The ending's actors in their running order, in which they step and draw (<see cref="EndingCastOrderDefinitions"/>).</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>What every ending scene draws on: brightness, the turning-floor view, colours, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingZebesExplosion(EndingScreen screen, List<EndingSprite> sprites, EndingStage stage)
    {
        this.screen = screen;
        this.sprites = sprites;
        _stage = stage;
    }

    /// <summary>Starts the planet explosion's colour crossfade and spawns its actors.</summary>
    internal void SetupZebesExplosion()
    {
        _stage.LoadColours(EndingColourLoad.Explosion);
        sprites.Clear();
        _stage.SpawnSprite(EndingActorDefinitions.ExplodingZebes, EndingSpriteRole.ExplodingZebes);
        _stage.SpawnSprite(EndingActorDefinitions.ExplosionLava, EndingSpriteRole.ExplosionLava);
        _stage.SpawnSprite(EndingActorDefinitions.ExplosionGlow, EndingSpriteRole.ExplosionGlow);
        _stage.SpawnSprite(EndingActorDefinitions.ExplosionStars, EndingSpriteRole.ExplosionStars);
        _stage.Floor.SetUp(Offset16<FloorPixels>.Zero, EndingMotionDefinitions.EscapeInitialZoom, RotationAngle.Zero);
        _stage.Fade.Show(0);
        _stage.StartFastFade();
        BeginExplosionCrossfade();
    }

    /// <summary>
    /// The explosion's colour crossfade (#2488): a step every second update from its second, its last ending the crossfade.
    /// The explosion's setup shows its start; only the explosion's crossfading phases take its steps.
    /// </summary>
    private readonly PacedFadeSteps crossfade = new(EndingExplosionFadeDefinitions.Steps,
        EndingCadenceDefinitions.ExplosionCrossfadeUpdatesPerStep, firstStepUpdate: EndingCadenceDefinitions.ExplosionCrossfadeUpdatesPerStep);

    /// <summary>Stops cloud colour effects and shows the planet's first fade step.</summary>
    private void BeginExplosionCrossfade()
    {
        // The cloud colour effects end before the crossfade begins; otherwise their next step would show over it.
        // The crossfade fades the escape's far background out and the explosion's planet and lava in, from colours the
        // import scales at every level (#1512).
        _stage.ResetColourEffects();
        screen.Colours.Fade(EndingFade.ExplosionLava, 0);
        screen.Colours.Fade(EndingFade.ExplodingPlanet, 0);
    }

    /// <summary>
    /// Crossfades the explosion's colours on alternating updates while zooming the floor out.
    /// </summary>
    /// <returns>Whether the crossfade has taken its last step, which ends it.</returns>
    internal bool StepExplosionCrossfade()
    {
        _stage.Floor.Widen(EndingExplosionFadeDefinitions.ZoomStep);
        if (crossfade.AdvanceTakingStep())
        {
            screen.Colours.Fade(EndingFade.ExplosionBackground, crossfade.Left);
            screen.Colours.Fade(EndingFade.ExplosionLava, crossfade.Taken);
            screen.Colours.Fade(EndingFade.ExplodingPlanet, crossfade.Taken);
        }
        return crossfade.IsComplete;
    }
}
