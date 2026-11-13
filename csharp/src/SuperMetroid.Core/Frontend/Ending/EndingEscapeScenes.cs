using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's escape scenes: their setup, the clouds, and the ending sprites and their cues.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingEscapeScenes
{
    /// <summary>Queues the authored ending music transitions.</summary>
    private readonly AudioQueues audio;
    /// <summary>Saved clear-time hours, as the game clock or the configured override supplies them, used to select the post-credits reward.</summary>
    private readonly int gameTimeHours;
    /// <summary>Saved clear-time minutes displayed with the clear time.</summary>
    private readonly int gameTimeMinutes;
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    private readonly EndingScreen screen ;
    /// <summary>Dynamic cinematic actors in slot traversal order.</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;
    /// <summary>The ending's installed artwork: its text, font, staff credits, turning floor, object and palette assets.</summary>
    private readonly EndingArtwork _artwork;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingEscapeScenes(AudioQueues audio, int gameTimeHours, int gameTimeMinutes, EndingScreen screen, List<EndingSprite> sprites, EndingStage stage, EndingArtwork artwork)
    {
        this.audio = audio;
        this.gameTimeHours = gameTimeHours;
        this.gameTimeMinutes = gameTimeMinutes;
        this.screen = screen;
        this.sprites = sprites;
        _stage = stage;
        _artwork = artwork;
    }

    /// <summary>Loads the first cloud scene, resets its turning-floor transform and queues escape music.</summary>
    internal void SetupEscapeSceneA()
    {
        _stage.LoadColours(EndingColourLoad.Escape);
        sprites.Clear();
        _stage.SpawnSprite(EndingActorDefinitions.EscapeACloudRightTop, EndingSpriteRole.CloudRightA);
        _stage.SpawnSprite(EndingActorDefinitions.EscapeACloudLeftTop, EndingSpriteRole.CloudLeftA);
        _stage.SpawnSprite(EndingActorDefinitions.EscapeACloudRightBottom, EndingSpriteRole.CloudRightB);
        _stage.SpawnSprite(EndingActorDefinitions.EscapeACloudLeftBottom, EndingSpriteRole.CloudLeftB);
        _stage.FloorPosition = new(_stage.FloorPosition.X.WithWhole(0), default);
        _stage.FloorZoom = EndingMotionDefinitions.EscapeInitialZoom;
        _stage.FloorAngle = EndingMotionDefinitions.EscapeInitialAngle.ToAngle();
        _stage.Brightness = 0;
        _artwork.PostCreditsText.Drop = 0;
        audio.QueueMusic(MusicRequest.Stop);
        audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.ZebesExplosion));
        audio.ScheduleMusic(
            MusicRequest.SelectTrack(EndingSequenceDefinitions.Music.Track),
            MusicQueueDelay.AtLeastShortest(EndingSequenceDefinitions.Music.DelayArgument));
        _stage.Phase = EndingCreditsPhase.WaitForEscapeMusic;
    }

    /// <summary>Installs the second cloud backdrop and grey-cloud palette effect before its fade-in.</summary>
    internal void SetupEscapeSceneB()
    {
        _stage.ResetPaletteFx();
        _stage.PaletteFxSystem.Start(EndingPaletteFxDefinitions.GreyClouds, 0);
        sprites.Clear();
        _stage.SpawnSprite(EndingActorDefinitions.EscapeBCloudTopA, EndingSpriteRole.CloudTopA);
        _stage.SpawnSprite(EndingActorDefinitions.EscapeBCloudTopB, EndingSpriteRole.CloudTopB);
        _stage.SpawnSprite(EndingActorDefinitions.EscapeBCloudBottomA, EndingSpriteRole.CloudBottomA);
        _stage.SpawnSprite(EndingActorDefinitions.EscapeBCloudBottomB, EndingSpriteRole.CloudBottomB);
        _stage.FloorPosition = new(_stage.FloorPosition.X.WithWhole(0), default);
        _stage.FloorZoom = EndingMotionDefinitions.EscapeInitialZoom;
        _stage.FloorAngle = EndingMotionDefinitions.EscapeInitialAngle.ToAngle();
        _stage.Brightness = 0;
        _stage.RestartFadeCadence();
        _stage.Phase = EndingCreditsPhase.FadeInEscapeSceneB;
    }

    /// <summary>Advances each cloud's zoom-relative motion and its private cue stream.</summary>
    internal void StepEscapeClouds()
    {
        foreach (EndingSprite wrapper in sprites)
        {
            IntroDiscoverySprite sprite = wrapper.Sprite;
            EndingCloudMotion.Step(wrapper, _stage.FloorZoom);
            sprite.Step();
        }
    }

    /// <summary>
    /// Runs the live actors in running order. An actor a step spawns after the running one runs later in this same pass;
    /// one spawned before it waits for the next pass.
    /// </summary>
    /// <exception cref="InvalidOperationException">An actor's own step replaced it.</exception>
    internal void StepSprites()
    {
        for (int place = 0; place < sprites.Count; place++)
        {
            EndingSprite wrapper = sprites[place];
            if (!wrapper.Sprite.IsActive) continue;
            _stage.StepEndingSpriteFrameBehavior(wrapper);
            wrapper.Sprite.Step(HandleSpriteCue);
            place = sprites.IndexOf(wrapper);
            if (place < 0)
                throw new InvalidOperationException($"The ending's {wrapper.Role} actor was replaced by its own step.");
        }
        sprites.RemoveAll(wrapper => !wrapper.Sprite.IsActive);
    }

    /// <summary>Carries out the ending sprite animations' cues that spawn actors, trigger palette effects or advance the cinematic phase.</summary>
    private void HandleSpriteCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.FadeExplosionPalette:
                _stage.PaletteFxSystem.Start(EndingPaletteFxDefinitions.FadePlanet, 0);
                screen.Colours.Flash(EndingFlash.PlanetGlow, null);
                return;

            case CinematicSpriteCueKind.SpawnExplosionSilhouette:
                _stage.SpawnSprite(
                    EndingActorDefinitions.ExplosionSilhouette,
                    EndingSpriteRole.ExplosionSilhouette);
                screen.Colours.Flash(EndingFlash.SilhouetteBackdrop, EndingScreenDefinitions.WhiteColor);
                return;

            case CinematicSpriteCueKind.StartZebesExplosion:
                ExplosionBurstDisplay = true;
                _stage.PaletteFxSystem.Start(EndingPaletteFxDefinitions.Supernova, 0);
                _stage.PaletteFxSystem.Start(EndingPaletteFxDefinitions.Explosion, 0);
                _stage.PaletteFxSystem.Start(EndingPaletteFxDefinitions.WideExplosion, 0);
                _stage.SpawnSprite(
                    EndingActorDefinitions.ExplosionStarsRight,
                    EndingSpriteRole.ExplosionStarsRight);
                _stage.SpawnSprite(
                    EndingActorDefinitions.ExplosionStarsLeft,
                    EndingSpriteRole.ExplosionStarsLeft);
                return;

            case CinematicSpriteCueKind.ExplosionFinale:
                ExplosionFinaleDisplay = true;
                _stage.PaletteFxSystem.Start(EndingPaletteFxDefinitions.SupernovaFinale, 0);
                _stage.SpawnSprite(
                    EndingActorDefinitions.ExplosionAfterglow,
                    EndingSpriteRole.ExplosionAfterglow);
                return;

            case CinematicSpriteCueKind.EndZebesExplosion:
                // The scene hides the sprite layer without deleting the actors: the stars must still
                // advance and become visible again when the flyaway starts.
                screen.Colours.Flash(EndingFlash.ExplosionWhiteout, EndingScreenDefinitions.WhiteColor);
                _stage.PhaseTimer = 120;
                _stage.Phase = EndingCreditsPhase.WaitForPlanetEscapeMusic;
                return;

            case CinematicSpriteCueKind.SpawnCompletedText:
                _stage.SpawnSprite(
                    EndingActorDefinitions.CompletedSuccessfullyText,
                    EndingSpriteRole.CompletedSuccessfullyText);
                return;

            case CinematicSpriteCueKind.SpawnClearTime:
                _stage.SpawnSprite(
                    EndingActorDefinitions.ClearTimeText,
                    EndingSpriteRole.ClearTimeText);
                return;

            case CinematicSpriteCueKind.SpawnHoursTens:
                SpawnDigit(gameTimeHours / 10, EndingResultTextLayout.HoursTensX);
                return;

            case CinematicSpriteCueKind.SpawnHoursUnits:
                SpawnDigit(gameTimeHours % 10, EndingResultTextLayout.HoursUnitsX);
                return;

            case CinematicSpriteCueKind.SpawnColon:
                _stage.SpawnSprite(
                    EndingActorDefinitions.ClearTimeColon,
                    EndingSpriteRole.ClearTimeDigit);
                return;

            case CinematicSpriteCueKind.SpawnMinutesTens:
                SpawnDigit(gameTimeMinutes / 10, EndingResultTextLayout.MinutesTensX);
                return;

            case CinematicSpriteCueKind.SpawnMinutesUnits:
                SpawnDigit(gameTimeMinutes % 10, EndingResultTextLayout.MinutesUnitsX);
                return;

            case CinematicSpriteCueKind.TransitionToCredits:
                _stage.RestartFadeCadence();
                _stage.Phase = EndingCreditsPhase.FadeOutToCredits;
                return;

            case CinematicSpriteCueKind.SetFrameBehavior:
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
    private void SpawnDigit(int digit, int x)
    {
        int normalized = Math.Clamp(digit, 0, 9);
        _stage.SpawnSprite(
            new(x, EndingActorDefinitions.ClearTimeDigitY),
            EndingActorDefinitions.ClearTimeDigitColouring,
            ClearTimeDigitAnimation(normalized),
            EndingSpriteRole.ClearTimeDigit);
    }

    /// <summary>The animation showing clear-time <paramref name="digit"/>.</summary>
    private static CinematicSpriteSegment ClearTimeDigitAnimation(int digit) => (uint)digit < (uint)EndingCreditsState.ClearTimeDigitAnimations.Length
        ? EndingCreditsState.ClearTimeDigitAnimations[digit]
        : throw new ArgumentOutOfRangeException(nameof(digit), digit, "A clear-time digit is 0..9.");

    /// <summary>Installs the two-piece Samus reward art selected by clear-time thresholds.</summary>
    internal void SpawnEndingRewardActors()
    {
        sprites.Clear();
        foreach (EndingSpriteDefinition actor in EndingRewardCast.Idle(_stage.EndingReward))
            _stage.SpawnSprite(actor, EndingSpriteRole.RewardSamus);
    }

    /// <summary>Whether the explosion installed its finale arrangement.</summary>
    internal bool ExplosionFinaleDisplay { get; private set; }

    /// <summary>Whether the explosion installed its initial burst arrangement.</summary>
    internal bool ExplosionBurstDisplay { get; private set; }
}
