using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's escape scenes: their setup, the clouds, and the ending sprites and their cues.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
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
    /// <summary>The ending's actors in their running order, in which they step and draw (<see cref="EndingCastOrderDefinitions"/>).</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>What every ending scene draws on: brightness, the turning-floor view, colours, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;
    /// <summary>The ending's installed artwork: its text, font, staff credits, turning floor, sprite and colour assets.</summary>
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

    /// <summary>Loads the side-cloud escape, resets its turning-floor transform and queues escape music.</summary>
    internal void SetupSideCloudEscape()
    {
        _stage.LoadColours(EndingColourLoad.Escape);
        sprites.Clear();
        _stage.SpawnSprite(EndingActorDefinitions.SideCloudRightLeading, EndingSpriteRole.SideCloudRightLeading);
        _stage.SpawnSprite(EndingActorDefinitions.SideCloudLeftLeading, EndingSpriteRole.SideCloudLeftLeading);
        _stage.SpawnSprite(EndingActorDefinitions.SideCloudRightTrailing, EndingSpriteRole.SideCloudRightTrailing);
        _stage.SpawnSprite(EndingActorDefinitions.SideCloudLeftTrailing, EndingSpriteRole.SideCloudLeftTrailing);
        _stage.Floor.SetUp(Offset16<FloorPixels>.Zero, EndingMotionDefinitions.EscapeInitialZoom, EndingMotionDefinitions.EscapeInitialAngle.ToAngle());
        _stage.Fade.Show(0);
        _artwork.PostCreditsText.Drop = 0;
        audio.QueueMusic(MusicRequest.Stop);
        audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.ZebesExplosion));
        audio.ScheduleMusic(
            MusicRequest.SelectTrack(EndingSequenceDefinitions.Music.EscapeSong),
            EndingSequenceDefinitions.Music.CreditsSongDelay);
    }

    /// <summary>Installs the top-and-bottom-cloud escape and its grey-cloud colour effect before its fade-in.</summary>
    internal void SetupTopAndBottomCloudEscape()
    {
        _stage.ResetColourEffects();
        _stage.ColourEffects.StartScreen(EndingColourEffectDefinitions.GreyClouds);
        sprites.Clear();
        _stage.SpawnSprite(EndingActorDefinitions.TopCloudPattern, EndingSpriteRole.TopCloudPattern);
        _stage.SpawnSprite(EndingActorDefinitions.TopCloudEdge, EndingSpriteRole.TopCloudEdge);
        _stage.SpawnSprite(EndingActorDefinitions.BottomCloudEdge, EndingSpriteRole.BottomCloudEdge);
        _stage.SpawnSprite(EndingActorDefinitions.BottomCloudPattern, EndingSpriteRole.BottomCloudPattern);
        _stage.Floor.SetUp(Offset16<FloorPixels>.Zero, EndingMotionDefinitions.EscapeInitialZoom, EndingMotionDefinitions.EscapeInitialAngle.ToAngle());
        _stage.Fade.Show(0);
        _stage.StartFastFade();
    }

    /// <summary>
    /// Runs the live actors in running order in <paramref name="scene"/>, each its own motion and then its animation
    /// (#2489), whose cues go to <paramref name="handleCue"/>: the escapes' clouds, the explosion's actors, the completion
    /// text and the reward. An actor a step spawns after the running one runs later in this same pass; one spawned before it
    /// waits for the next pass.
    /// </summary>
    /// <exception cref="InvalidOperationException">An actor's own step replaced it.</exception>
    internal void StepSprites(IEndingActorScene scene, Action<EndingSprite, EndingSceneCue> handleCue)
    {
        Ensure.NotNull(scene);
        Ensure.NotNull(handleCue);
        for (int place = 0; place < sprites.Count; place++)
        {
            EndingSprite wrapper = sprites[place];
            if (!wrapper.Sprite.IsActive) continue;
            wrapper.StepMotion(scene);
            wrapper.Sprite.Step(cue => handleCue(wrapper, cue));
            place = sprites.IndexOf(wrapper);
            if (place < 0)
                throw new InvalidOperationException($"The ending's {wrapper.Role} actor was replaced by its own step.");
        }
        sprites.RemoveAll(wrapper => !wrapper.Sprite.IsActive);
    }

    /// <summary>
    /// Answers the ending sprite animations' cues that spawn actors, start the stars' wait or trigger colour effects; the
    /// cues that end a phase are the ending's own (<see cref="EndingCreditsState"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The cue is one the ending's own update answers, or undefined.</exception>
    internal void HandleSpriteCue(EndingSprite wrapper, EndingSceneCue cue)
    {
        Ensure.NotNull(wrapper);
        switch (cue)
        {
            case EndingSceneCue.WaitForFlyaway:
                // The right-hand stars wait for the flyaway to carry them away: the cue goes to the motion their role gives them.
                wrapper.Motion.TakeCue(cue);
                return;

            case EndingSceneCue.FadeExplosionColours:
                _stage.ColourEffects.StartScreen(EndingColourEffectDefinitions.FadePlanet);
                screen.Colours.Flash(EndingFlash.PlanetGlow, null);
                return;

            case EndingSceneCue.SpawnExplosionSilhouette:
                _stage.SpawnSprite(
                    EndingActorDefinitions.ExplosionSilhouette,
                    EndingSpriteRole.ExplosionSilhouette);
                screen.Colours.Flash(EndingFlash.SilhouetteBackdrop, RgbColor.White);
                return;

            case EndingSceneCue.StartZebesExplosion:
                ExplosionDisplay = EndingExplosionDisplay.Burst;
                _stage.ColourEffects.StartScreen(EndingColourEffectDefinitions.Supernova);
                _stage.ColourEffects.StartScreen(EndingColourEffectDefinitions.Explosion);
                _stage.ColourEffects.StartScreen(EndingColourEffectDefinitions.WideExplosion);
                _stage.SpawnSprite(
                    EndingActorDefinitions.ExplosionStarsRight,
                    EndingSpriteRole.ExplosionStarsRight);
                _stage.SpawnSprite(
                    EndingActorDefinitions.ExplosionStarsLeft,
                    EndingSpriteRole.ExplosionStarsLeft);
                return;

            case EndingSceneCue.ExplosionFinale:
                ExplosionDisplay = EndingExplosionDisplay.Finale;
                _stage.ColourEffects.StartScreen(EndingColourEffectDefinitions.SupernovaFinale);
                _stage.SpawnSprite(
                    EndingActorDefinitions.ExplosionAfterglow,
                    EndingSpriteRole.ExplosionAfterglow);
                return;

            case EndingSceneCue.SpawnCompletedText:
                _stage.SpawnText(
                    EndingActorDefinitions.CompletedSuccessfullyText,
                    EndingSpriteRole.CompletedSuccessfullyText);
                return;

            case EndingSceneCue.SpawnClearTime:
                _stage.SpawnText(
                    EndingActorDefinitions.ClearTimeText,
                    EndingSpriteRole.ClearTimeText);
                return;

            case EndingSceneCue.SpawnHoursTens:
                SpawnDigit(gameTimeHours / 10, EndingResultTextLayout.HoursTensX);
                return;

            case EndingSceneCue.SpawnHoursUnits:
                SpawnDigit(gameTimeHours % 10, EndingResultTextLayout.HoursUnitsX);
                return;

            case EndingSceneCue.SpawnColon:
                _stage.SpawnSprite(
                    EndingActorDefinitions.ClearTimeColon,
                    EndingSpriteRole.ClearTimeDigit);
                return;

            case EndingSceneCue.SpawnMinutesTens:
                SpawnDigit(gameTimeMinutes / 10, EndingResultTextLayout.MinutesTensX);
                return;

            case EndingSceneCue.SpawnMinutesUnits:
                SpawnDigit(gameTimeMinutes % 10, EndingResultTextLayout.MinutesUnitsX);
                return;

            case EndingSceneCue.EndZebesExplosion:
            case EndingSceneCue.TransitionToCredits:
                throw new InvalidOperationException($"The {cue} cue of the {wrapper.Role} actor changes the ending's phase, which its own update answers.");
        }
    }

    /// <summary>Spawns clear-time <paramref name="digit"/>'s animation in the clear-time column at <paramref name="x"/>.</summary>
    private void SpawnDigit(int digit, int x)
    {
        _stage.SpawnSprite(
            new(x, EndingActorDefinitions.ClearTimeDigitY),
            EndingActorDefinitions.ClearTimeDigitColouring,
            ClearTimeDigitAnimation(digit),
            EndingSpriteRole.ClearTimeDigit);
    }

    /// <summary>The animation showing clear-time <paramref name="digit"/>.</summary>
    private static EndingSceneAnimation ClearTimeDigitAnimation(int digit) => (uint)digit < (uint)EndingCreditsState.ClearTimeDigitAnimations.Length
        ? EndingCreditsState.ClearTimeDigitAnimations[digit]
        : throw new ArgumentOutOfRangeException(nameof(digit), digit, "A clear-time digit is 0..9.");

    /// <summary>Installs the two-piece Samus reward art selected by clear-time thresholds.</summary>
    internal void SpawnEndingRewardActors()
    {
        sprites.Clear();
        foreach (EndingSpriteDefinition actor in EndingRewardCast.Idle(_stage.EndingReward))
            _stage.SpawnSprite(actor, EndingSpriteRole.RewardSamus);
    }

    /// <summary>How the explosion shows: not yet, as its burst, or as its finale, as its cues move it on.</summary>
    internal EndingExplosionDisplay ExplosionDisplay { get; private set; }
}
