using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Zebes exploding in the ending: the finale and burst displays and the crossfade between them.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingZebesExplosion
{
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    private readonly EndingScreen screen ;
    /// <summary>The ending's actors in their running order, in which they step and draw (<see cref="EndingCastOrderDefinitions"/>).</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, colours, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;
    /// <summary>The gunship's flight from Zebes: the planet escape's speeds, the stars and the fade.</summary>
    private readonly EndingFlyaway _flyaway;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingZebesExplosion(EndingScreen screen, List<EndingSprite> sprites, EndingStage stage, EndingFlyaway flyaway)
    {
        this.screen = screen;
        this.sprites = sprites;
        _stage = stage;
        _flyaway = flyaway;
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
        _stage.FloorPosition = new(_stage.FloorPosition.X.WithWholePixels(0), default);
        _stage.FloorZoom = EndingMotionDefinitions.EscapeInitialZoom;
        _stage.FloorAngle = RotationAngle.Zero;
        _stage.Fade.Show(0);
        _stage.StartFastFade();
        BeginExplosionCrossfade();
        _stage.Phase = EndingCreditsPhase.FadeInZebesExplosion;
    }

    /// <summary>
    /// The explosion's colour crossfade (#2488): a step every second update from its second, its last ending the crossfade;
    /// none before the explosion is set up.
    /// </summary>
    private PacedFadeSteps? crossfade;

    /// <summary>Whether the explosion's crossfade still sets the scene's colours.</summary>
    internal bool ExplosionCrossfadeActive => _stage.Phase is EndingCreditsPhase.FadeInZebesExplosion
        or EndingCreditsPhase.ZebesExplosionCrossfade;

    /// <summary>Stops cloud colour effects and seeds the planet's first fade step.</summary>
    private void BeginExplosionCrossfade()
    {
        // The cloud colour effects end before the crossfade begins; otherwise their next step would show over it.
        // The crossfade fades the escape's far background out and the explosion's planet and lava in, from colours the
        // import scales at every level (#1512).
        _stage.ResetColourEffects();
        crossfade = new PacedFadeSteps(EndingExplosionFadeDefinitions.Steps, EndingCadenceDefinitions.ExplosionCrossfadeUpdatesPerStep,
            firstStepUpdate: EndingCadenceDefinitions.ExplosionCrossfadeUpdatesPerStep);
        screen.Colours.Fade(EndingFade.ExplosionLava, 0);
        screen.Colours.Fade(EndingFade.ExplodingPlanet, 0);
    }

    /// <summary>
    /// Crossfades the explosion's colours on alternating updates and, on the update that takes the crossfade's last step,
    /// enters the explosion's hold.
    /// </summary>
    internal void StepExplosionCrossfade()
    {
        PacedFadeSteps fade = crossfade ?? throw new InvalidOperationException("The explosion crossfades with no crossfade set up.");
        _stage.FloorZoom = _stage.FloorZoom.Widened(EndingExplosionFadeDefinitions.ZoomStep);
        if (fade.Advance())
        {
            screen.Colours.Fade(EndingFade.ExplosionBackground, fade.Left);
            screen.Colours.Fade(EndingFade.ExplosionLava, fade.Taken);
            screen.Colours.Fade(EndingFade.ExplodingPlanet, fade.Taken);
        }
        if (fade.IsComplete)
        {
            _stage.PhaseTimer = EndingExplosionFadeDefinitions.HoldUpdates;
            _flyaway.PrepareFlyaway();
            _stage.Phase = EndingCreditsPhase.ZebesExplosionHold;
        }
    }
}
