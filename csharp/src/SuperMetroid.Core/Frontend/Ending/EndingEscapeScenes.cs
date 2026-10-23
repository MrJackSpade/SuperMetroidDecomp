using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's escape scenes: their setup, the clouds, and the ending sprites and their cues.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingEscapeScenes
{
    /// <summary>Queues the authored ending music transitions.</summary>
    private readonly CartridgeAudioState audio;
    /// <summary>Saved clear-time hours, as the game clock or the configured override supplies them, used to select the post-credits reward.</summary>
    private readonly int gameTimeHours;
    /// <summary>Saved clear-time minutes displayed with the clear time.</summary>
    private readonly int gameTimeMinutes;
    /// <summary>The ending's screen: its palette and the graphics transfers its scenes have made (#1358).</summary>
    private readonly EndingScreen screen ;
    /// <summary>Dynamic cinematic actors in native slot traversal order.</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the Mode 7 view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;
    /// <summary>The ending's installed artwork: its text, font, staff credits, Mode 7, object and palette assets and their loads.</summary>
    private readonly EndingArtwork _artwork;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingEscapeScenes(CartridgeAudioState audio, int gameTimeHours, int gameTimeMinutes, EndingScreen screen, List<EndingSprite> sprites, EndingStage stage, EndingArtwork artwork)
    {
        this.audio = audio;
        this.gameTimeHours = gameTimeHours;
        this.gameTimeMinutes = gameTimeMinutes;
        this.screen = screen;
        this.sprites = sprites;
        _stage = stage;
        _artwork = artwork;
    }

    /// <summary>Loads the first cloud scene, resets its Mode-7 transform and queues escape music.</summary>
    internal void SetupEscapeSceneA()
    {
        _stage.LoadStaticPalette(EndingPaletteId.Escape, 0, 256, 0);
        _stage.LoadFloor(EndingFloorSceneId.EscapeA);
        _artwork.LoadEscapeCloudCharacters();
        sprites.Clear();
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudRightTop, EndingSpriteRole.CloudRightA);
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudLeftTop, EndingSpriteRole.CloudLeftA);
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudRightBottom, EndingSpriteRole.CloudRightB);
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudLeftBottom, EndingSpriteRole.CloudLeftB);
        _stage.FloorPosition = new(_stage.FloorPosition.X.WithWhole(0), default);
        _stage.FloorZoom = EndingCreditsRomData.Motion.EscapeInitialScale;
        _stage.FloorAngle = EndingCreditsRomData.Motion.EscapeInitialAngle.ToAngle();
        _stage.Brightness = 0;
        _stage.PostCreditsScroll = Position16<BgPlanePixels>.Zero;
        audio.QueueMusicDelayed8(MusicCommand.Stop);
        audio.QueueMusicDelayed8(MusicCommand.LoadData(
            EndingCreditsRomData.Music.EscapeDataIndex));
        audio.QueueMusicDelayed(
            MusicCommand.SelectTrack(EndingCreditsRomData.Music.Track),
            MusicCommandDelay.FromDelayedYArgument(EndingCreditsRomData.Music.DelayArgument));
        _stage.Phase = EndingCreditsPhase.WaitForEscapeMusic;
    }

    /// <summary>Installs the second cloud backdrop and grey-cloud palette effect before its fade-in.</summary>
    internal void SetupEscapeSceneB()
    {
        _stage.ResetPaletteFx();
        _stage.PaletteFxSystem.SpawnDefinition(EndingPaletteFxDefinitions.GreyClouds, 0);
        _stage.LoadFloor(EndingFloorSceneId.EscapeB);
        _artwork.LoadEscapeCloudCharacters();
        sprites.Clear();
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudTopA, EndingSpriteRole.CloudTopA);
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudTopB, EndingSpriteRole.CloudTopB);
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudBottomA, EndingSpriteRole.CloudBottomA);
        _stage.SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudBottomB, EndingSpriteRole.CloudBottomB);
        _stage.FloorPosition = new(_stage.FloorPosition.X.WithWhole(0), default);
        _stage.FloorZoom = EndingCreditsRomData.Motion.EscapeInitialScale;
        _stage.FloorAngle = EndingCreditsRomData.Motion.EscapeInitialAngle.ToAngle();
        _stage.Brightness = 0;
        _stage.RestartFadeCadence();
        _stage.Phase = EndingCreditsPhase.FadeInEscapeSceneB;
    }

    /// <summary>Advances each cloud's zoom-relative motion and its private instruction stream.</summary>
    internal void StepEscapeClouds()
    {
        foreach (EndingSprite wrapper in sprites)
        {
            IntroDiscoverySprite sprite = wrapper.Sprite;
            EndingCloudMotion.Step(wrapper, _stage.FloorZoom);
            sprite.Step();
        }
    }

    /// <summary>Visits live cinematic slots from high to low, allowing callback replacements in lower slots to run this frame.</summary>
    internal void StepSprites()
    {
        // Native traversal re-reads each slot. A callback can replace a lower slot,
        // which then runs later in this same frame; higher slots wait until next frame.
        for (int slot = EndingSpriteSlots.Count - 1; slot >= 0; slot--)
        {
            EndingSprite? wrapper = sprites.Find(actor => actor.TraversalSlot == slot && actor.Sprite.IsActive);
            if (wrapper is null) continue;
            _stage.StepEndingSpritePreInstruction(wrapper);
            wrapper.Sprite.Step(HandleSpriteCue);
        }
        sprites.RemoveAll(wrapper => !wrapper.Sprite.IsActive);
    }

    /// <summary>Carries out the ending sprite programs' cues that spawn actors, trigger palette effects or advance the cinematic phase.</summary>
    private void HandleSpriteCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.FadeExplosionPalette:
                _stage.PaletteFxSystem.SpawnDefinition(EndingPaletteFxDefinitions.FadePlanet, 0);
                new FrontendPalettes(screen.Palette).EndingExplosionGlow.Write(0, new Bgr555(1, 0, 0));
                return;

            case CinematicSpriteCueKind.SpawnExplosionSilhouette:
                _stage.SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionSilhouette,
                    EndingSpriteRole.ExplosionSilhouette);
                new FrontendPalettes(screen.Palette).EndingBackdrop.Write(0, EndingCreditsRomData.Rendering.WhiteColor);
                return;

            case CinematicSpriteCueKind.StartZebesExplosion:
                ExplosionBurstDisplay = true;
                _stage.PaletteFxSystem.SpawnDefinition(EndingPaletteFxDefinitions.Supernova, 0);
                _stage.PaletteFxSystem.SpawnDefinition(EndingPaletteFxDefinitions.Explosion, 0);
                _stage.PaletteFxSystem.SpawnDefinition(EndingPaletteFxDefinitions.WideExplosion, 0);
                _stage.SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionStarsRight,
                    EndingSpriteRole.ExplosionStarsRight);
                _stage.SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionStarsLeft,
                    EndingSpriteRole.ExplosionStarsLeft);
                return;

            case CinematicSpriteCueKind.ExplosionFinale:
                ExplosionFinaleDisplay = true;
                _stage.PaletteFxSystem.SpawnDefinition(EndingPaletteFxDefinitions.SupernovaFinale, 0);
                _stage.SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionAfterglow,
                    EndingSpriteRole.ExplosionAfterglow);
                return;

            case CinematicSpriteCueKind.EndZebesExplosion:
                // F32B disables TM/TS without deleting the actors: the stars must still
                // advance and become visible again when Func120 starts the flyaway.
                var flash = new FrontendPalettes(screen.Palette);
                flash.EndingBackdrop.Write(0, EndingCreditsRomData.Rendering.WhiteColor);
                flash.EndingObjectTransparent.Write(0, EndingCreditsRomData.Rendering.WhiteColor);
                flash.EndingLogoBackground.Fill(EndingCreditsRomData.Rendering.WhiteColor);
                _stage.PhaseTimer = 120;
                _stage.Phase = EndingCreditsPhase.WaitForPlanetEscapeMusic;
                return;

            case CinematicSpriteCueKind.SpawnCompletedText:
                _stage.SpawnSprite(
                    EndingCreditsRomData.Sprites.CompletedSuccessfullyText,
                    EndingSpriteRole.CompletedSuccessfullyText);
                return;

            case CinematicSpriteCueKind.SpawnClearTime:
                _stage.SpawnSprite(
                    EndingCreditsRomData.Sprites.ClearTimeText,
                    EndingSpriteRole.ClearTimeText);
                return;

            case CinematicSpriteCueKind.SpawnHoursTens:
                SpawnDigit(gameTimeHours / 10, EndingCreditsRomData.Text.HoursTensX);
                return;

            case CinematicSpriteCueKind.SpawnHoursUnits:
                SpawnDigit(gameTimeHours % 10, EndingCreditsRomData.Text.HoursUnitsX);
                return;

            case CinematicSpriteCueKind.SpawnColon:
                _stage.SpawnSprite(
                    EndingCreditsRomData.Sprites.ClearTimeColon,
                    EndingSpriteRole.ClearTimeDigit);
                return;

            case CinematicSpriteCueKind.SpawnMinutesTens:
                SpawnDigit(gameTimeMinutes / 10, EndingCreditsRomData.Text.MinutesTensX);
                return;

            case CinematicSpriteCueKind.SpawnMinutesUnits:
                SpawnDigit(gameTimeMinutes % 10, EndingCreditsRomData.Text.MinutesUnitsX);
                return;

            case CinematicSpriteCueKind.TransitionToCredits:
                _stage.RestartFadeCadence();
                _stage.Phase = EndingCreditsPhase.FadeOutToCredits;
                return;

            case CinematicSpriteCueKind.SetPreInstruction:
            case CinematicSpriteCueKind.FadeInZebesText:
            case CinematicSpriteCueKind.FadeOutZebesText:
            case CinematicSpriteCueKind.GreyOut:
            case CinematicSpriteCueKind.Launch:
            case CinematicSpriteCueKind.LaunchHead:
            case CinematicSpriteCueKind.PrepareHead:
            case CinematicSpriteCueKind.Shoot:
            case CinematicSpriteCueKind.SpawnFinalExplosionWave:
            case CinematicSpriteCueKind.SpawnInitialExplosionWave:
            case CinematicSpriteCueKind.SpawnJapaneseZebesTextIfNeeded:
            case CinematicSpriteCueKind.SpawnParticles:
            case CinematicSpriteCueKind.SpawnRinkas0And1:
            case CinematicSpriteCueKind.SpawnRinkas2And3:
            case CinematicSpriteCueKind.SpawnSuitedJump:
            case CinematicSpriteCueKind.SpawnSuitlessJump:
            case CinematicSpriteCueKind.StartFlyingToZebes:
            case CinematicSpriteCueKind.StartIntroPage2:
            case CinematicSpriteCueKind.StartIntroPage3:
            case CinematicSpriteCueKind.StartIntroPage4:
            case CinematicSpriteCueKind.StartIntroPage5:
            case CinematicSpriteCueKind.StartRinkaMoving:
            default:
                throw new InvalidOperationException($"The ending sprite programs publish no {cue} cue.");
        }
    }

    /// <summary>Chooses a clamped digit's authored sprite list for one clear-time column.</summary>
    private void SpawnDigit(int digit, ushort x)
    {
        int normalized = Math.Clamp(digit, 0, 9);
        _stage.SpawnSprite(
            new(x, EndingCreditsRomData.Sprites.ClearTimeDigitY),
            0,
            ClearTimeDigitProgram(normalized),
            EndingSpriteRole.ClearTimeDigit);
    }

    /// <summary>The program showing clear-time <paramref name="digit"/>.</summary>
    private static CinematicSpriteProgram ClearTimeDigitProgram(int digit) => (uint)digit < (uint)EndingCreditsState.ClearTimeDigitPrograms.Length
        ? EndingCreditsState.ClearTimeDigitPrograms[digit]
        : throw new ArgumentOutOfRangeException(nameof(digit), digit, "A clear-time digit is 0..9.");

    /// <summary>Installs the two-piece Samus reward art selected by clear-time thresholds.</summary>
    internal void SpawnEndingRewardActors()
    {
        sprites.Clear();
        switch (_stage.EndingReward)
        {
            case EndingReward.Suitless:
                _stage.SpawnSprite(EndingCreditsRomData.Sprites.SuitlessRewardBody, EndingSpriteRole.RewardSamus);
                _stage.SpawnSprite(EndingCreditsRomData.Sprites.SuitlessRewardHead, EndingSpriteRole.RewardSamus);
                break;
            case EndingReward.Helmetless:
                _stage.SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardBody, EndingSpriteRole.RewardSamus);
                _stage.SpawnSprite(EndingCreditsRomData.Sprites.HelmetlessRewardHead, EndingSpriteRole.RewardSamus);
                break;
            case EndingReward.Armored:
                _stage.SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardBody, EndingSpriteRole.RewardSamus);
                _stage.SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardHead, EndingSpriteRole.RewardSamus);
                break;
            default:
                throw new InvalidOperationException($"Undefined ending reward {_stage.EndingReward}.");
        }
    }

    /// <summary>Whether the explosion installed its finale arrangement.</summary>
    internal bool ExplosionFinaleDisplay { get; private set; }

    /// <summary>Whether the explosion installed its initial burst arrangement.</summary>
    internal bool ExplosionBurstDisplay { get; private set; }
}
