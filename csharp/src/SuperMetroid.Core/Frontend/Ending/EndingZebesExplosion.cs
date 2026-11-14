using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Zebes exploding in the ending: the finale and burst displays and the crossfade between them.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingZebesExplosion
{
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    private readonly EndingScreen screen ;
    /// <summary>Dynamic cinematic actors in slot traversal order.</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, palettes, the shared sprites and stars, and the artwork they share.</summary>
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

    /// <summary>Starts the planet explosion's palette crossfade and spawns its actors.</summary>
    internal void SetupZebesExplosion()
    {
        _stage.LoadColours(EndingColourLoad.Explosion);
        sprites.Clear();
        _stage.SpawnSprite(EndingActorDefinitions.ExplodingZebes, EndingSpriteRole.ExplodingZebes);
        _stage.SpawnSprite(EndingActorDefinitions.ExplosionLava, EndingSpriteRole.ExplosionLava);
        _stage.SpawnSprite(EndingActorDefinitions.ExplosionGlow, EndingSpriteRole.ExplosionGlow);
        _stage.SpawnSprite(EndingActorDefinitions.ExplosionStars, EndingSpriteRole.ExplosionStars);
        _stage.FloorPosition = new(_stage.FloorPosition.X.WithWhole(0), default);
        _stage.FloorZoom = EndingMotionDefinitions.EscapeInitialZoom;
        _stage.FloorAngle = RotationAngle.Zero;
        _stage.Brightness = 0;
        _stage.RestartFadeCadence();
        BeginExplosionCrossfade();
        _stage.Phase = EndingCreditsPhase.FadeInZebesExplosion;
    }

    /// <summary>Current palette interpolation step during the explosion crossfade.</summary>
    private int explosionFadeStep;

    /// <summary>Whether the explosion's palette interpolation still owns the colour writes.</summary>
    internal bool ExplosionCrossfadeActive => _stage.Phase is EndingCreditsPhase.FadeInZebesExplosion
        or EndingCreditsPhase.ZebesExplosionCrossfade;

    /// <summary>Stops cloud palette effects and seeds the planet object's first fade step.</summary>
    private void BeginExplosionCrossfade()
    {
        // The cloud palette effects end before the crossfade begins; otherwise their next record would show over it.
        // The crossfade fades the escape's far background out and the explosion's planet and lava in, from colours the
        // import scales at every level (#1512).
        _stage.ResetColourEffects();
        explosionFadeStep = 0;
        screen.Colours.Fade(EndingFade.ExplosionLava, 0);
        screen.Colours.Fade(EndingFade.ExplodingPlanet, 0);
        _stage.PhaseTimer = EndingExplosionFadeDefinitions.InitialCountdown;
    }

    /// <summary>Interpolates explosion palettes on alternating calls and then enters the explosion's hold.</summary>
    internal void StepExplosionCrossfade()
    {
        _stage.FloorZoom = _stage.FloorZoom.Widened(EndingExplosionFadeDefinitions.ZoomStep);
        if (EndingCadenceDefinitions.ExplosionFadeStepEverySecondUpdate.IsDue(_stage.PhaseTimer))
        {
            explosionFadeStep++;
            screen.Colours.Fade(EndingFade.ExplosionBackground, EndingExplosionFadeDefinitions.Steps - explosionFadeStep);
            screen.Colours.Fade(EndingFade.ExplosionLava, explosionFadeStep);
            screen.Colours.Fade(EndingFade.ExplodingPlanet, explosionFadeStep);
        }
        if (--_stage.PhaseTimer < 0)
        {
            _stage.PhaseTimer = EndingExplosionFadeDefinitions.HoldUpdates;
            _flyaway.PrepareFlyaway();
            _stage.Phase = EndingCreditsPhase.ZebesExplosionHold;
        }
    }
}
