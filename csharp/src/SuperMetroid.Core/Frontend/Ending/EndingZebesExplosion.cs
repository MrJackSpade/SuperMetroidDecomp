using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Zebes exploding in the ending: the finale and burst displays and the crossfade between them.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingZebesExplosion
{
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    private readonly EndingScreen screen ;
    /// <summary>Dynamic cinematic actors in slot traversal order.</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;
    /// <summary>The gunship's flight from Zebes: the planet escape's speeds, the stars, the fade and the flight artwork.</summary>
    private readonly EndingFlyaway _flyaway;
    /// <summary>The ending's installed artwork: its text, font, staff credits, turning floor, object and palette assets.</summary>
    private readonly EndingArtwork _artwork;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingZebesExplosion(EndingScreen screen, List<EndingSprite> sprites, EndingStage stage, EndingFlyaway flyaway, EndingArtwork artwork)
    {
        this.screen = screen;
        this.sprites = sprites;
        _stage = stage;
        _flyaway = flyaway;
        _artwork = artwork;
    }

    /// <summary>Loads the planet explosion's turning-floor and sprite art and starts its palette crossfade.</summary>
    internal void SetupZebesExplosion()
    {
        _stage.RequireFloorArtwork();
        _artwork.RequireExplosionArtwork();
        _stage.LoadColours(EndingColourLoad.ExplosionSprites);
        sprites.Clear();
        _stage.SpawnSprite(EndingActorDefinitions.ExplodingZebes, EndingSpriteRole.ExplodingZebes);
        _stage.SpawnSprite(EndingActorDefinitions.ExplosionLava, EndingSpriteRole.ExplosionLava);
        _stage.SpawnSprite(EndingActorDefinitions.ExplosionGlow, EndingSpriteRole.ExplosionGlow);
        _stage.SpawnSprite(EndingActorDefinitions.ExplosionStars, EndingSpriteRole.ExplosionStars);
        _stage.FloorPosition = new(_stage.FloorPosition.X.WithWhole(0), default);
        _stage.FloorZoom = EndingMotionDefinitions.EscapeInitialScale;
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
        or EndingCreditsPhase.ZebesExplosionPaletteCrossfade;

    /// <summary>Stops cloud palette effects and seeds the planet object's first fade step.</summary>
    private void BeginExplosionCrossfade()
    {
        // Func115 clears the previous cloud palette programs before taking its source
        // snapshot; otherwise their next instruction can overwrite the new sprite fade.
        // The snapshot holds the escape's far background and the explosion's sprite colours just loaded,
        // which the import scales at every level (#1512).
        _stage.ResetPaletteFx();
        explosionFadeStep = 0;
        screen.Colours.Fade(EndingFade.ExplosionLava, 0);
        screen.Colours.Fade(EndingFade.ExplodingPlanet, 0);
        _stage.PhaseTimer = EndingExplosionFadeDefinitions.InitialCountdown;
    }

    /// <summary>Interpolates explosion palettes on alternating calls and then enters the explosion's hold.</summary>
    internal void StepExplosionCrossfade()
    {
        _stage.FloorZoom += EndingExplosionFadeDefinitions.ZoomStep;
        if ((_stage.PhaseTimer & 1) == 0)
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
