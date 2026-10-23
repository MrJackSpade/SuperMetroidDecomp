using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Zebes exploding in the ending: the finale and burst displays and the crossfade between them.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingZebesExplosion
{
    /// <summary>The ending's screen: its palette and the graphics transfers its scenes have made (#1358).</summary>
    private readonly EndingScreen screen ;
    /// <summary>Dynamic cinematic actors in native slot traversal order.</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the Mode 7 view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;
    /// <summary>The gunship's flight from Zebes: the planet escape's speeds, the stars, the fade and the flight artwork's uploads.</summary>
    private readonly EndingFlyaway _flyaway;
    /// <summary>The ending's installed artwork: its text, font, staff credits, Mode 7, object and palette assets and their loads.</summary>
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

    /// <summary>Loads the planet explosion's Mode-7 and OBJ art and starts its palette crossfade.</summary>
    internal void SetupZebesExplosion()
    {
        _stage.LoadFloor(EndingFloorSceneId.PlanetExplosion);
        _artwork.LoadEndingObjectCharacters();
        _stage.LoadStaticPalette(EndingPaletteId.Explosion,
            EndingCreditsRomData.Rendering.PaletteHalfBytes,
            EndingCreditsRomData.Rendering.PaletteHalfBytes,
            EndingCreditsRomData.Rendering.PaletteHalfBytes);
        sprites.Clear();
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.ExplodingZebes, EndingSpriteRole.ExplodingZebes);
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.ExplosionLava, EndingSpriteRole.ExplosionLava);
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.ExplosionGlow, EndingSpriteRole.ExplosionGlow);
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.ExplosionStars, EndingSpriteRole.ExplosionStars);
        _stage.Mode7 = new(_stage.Mode7.X.WithWhole(0), default);
        _stage.FloorZoom = EndingCreditsRomData.Motion.EscapeInitialScale;
        _stage.FloorAngle = SnesAngle.Zero;
        _stage.Brightness = 0;
        _stage.RestartFadeCadence();
        BeginExplosionCrossfade();
        _stage.Phase = EndingCreditsPhase.FadeInZebesExplosion;
    }

    /// <summary>CGRAM snapshot taken before the planet explosion's object/background crossfade.</summary>
    private Bgr555[] explosionFadeSource = [];

    /// <summary>Current native palette interpolation step during the explosion crossfade.</summary>
    private int explosionFadeStep;

    /// <summary>Whether the explosion's palette interpolation still owns CGRAM writes.</summary>
    internal bool ExplosionCrossfadeActive => _stage.Phase is EndingCreditsPhase.FadeInZebesExplosion
        or EndingCreditsPhase.ZebesExplosionPaletteCrossfade;

    /// <summary>Stops cloud palette effects, captures CGRAM and seeds the planet object's first fade step.</summary>
    private void BeginExplosionCrossfade()
    {
        // Func115 clears the previous cloud palette programs before taking its source
        // snapshot; otherwise their next instruction can overwrite the new OBJ fade.
        _stage.ResetPaletteFx();
        explosionFadeSource = new CgramPalettes(screen.Palette).Image.ToArray();
        explosionFadeStep = 0;
        ApplyExplosionFade(EndingExplosionFadeDefinitions.FirstObjectStart, 0);
        ApplyExplosionFade(EndingExplosionFadeDefinitions.SecondObjectStart, 0);
        _stage.PhaseTimer = EndingExplosionFadeDefinitions.InitialCountdown;
    }

    /// <summary>Interpolates explosion palettes on alternating calls and enters the sixteen-chunk flyaway transfer.</summary>
    internal void StepExplosionCrossfade()
    {
        _stage.FloorZoom += EndingExplosionFadeDefinitions.ZoomStep;
        if ((_stage.PhaseTimer & 1) == 0)
        {
            explosionFadeStep++;
            ApplyExplosionFade(EndingExplosionFadeDefinitions.BackgroundStart,
                EndingExplosionFadeDefinitions.Steps - explosionFadeStep);
            ApplyExplosionFade(EndingExplosionFadeDefinitions.FirstObjectStart, explosionFadeStep);
            ApplyExplosionFade(EndingExplosionFadeDefinitions.SecondObjectStart, explosionFadeStep);
        }
        if (--_stage.PhaseTimer < 0)
        {
            _stage.PhaseTimer = 16;
            _flyaway.PrepareFlyawayUploads();
            _stage.Phase = EndingCreditsPhase.ZebesExplosionTileUpload;
        }
    }

    /// <summary>Scales one native explosion palette range from the captured CGRAM source by the current fade factor.</summary>
    [PpuLayoutBoundary("codec: native ending fade over its CGRAM index range")]
    private void ApplyExplosionFade(int start, int factor)
    {
        for (int i = start; i < start + EndingExplosionFadeDefinitions.Colors; i++)
        {
            new CgramPalettes(screen.Palette).All.Write(i, explosionFadeSource[i].Map((_, channel) =>
                channel * factor / EndingExplosionFadeDefinitions.Steps));
        }
    }
}
