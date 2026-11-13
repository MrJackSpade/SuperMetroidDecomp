using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Installed-asset implementation of the Ceres-destruction game state: Ceres explodes and the
/// camera subsequently follows Samus's gunship toward Zebes.
/// </summary>
/// <remarks>
/// The cinematic owns this sequence. The state deliberately retains the
/// game's phase boundaries, counters, 16.16 camera motion, turning-floor rotation values, and
/// compiled sprite animation scripts, so the scene's progress is game state:
/// there is no host-authored video or timer that merely happens to end at the
/// same gameplay room.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class CeresDestructionCinematicState : IFrontendScreen
{
    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork, SuperMetroidGameState state)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.MapPresentation, artwork.MapPresentation))
            BindGlowColors(artwork.MapPresentation?.PowerBombGlowColors);
        if (!ReferenceEquals(previous.IntroCinematicArt, artwork.IntroCinematicArt))
            BindArtwork(artwork.IntroCinematicArt);
        BindSpriteSheets(artwork.SpriteSheets);
    }

    /// <summary>Binds the installed sprite sheets the scene's actors draw from.</summary>
    [MemberNotNull(nameof(spriteSheets))]
    internal void BindSpriteSheets(InstalledSpriteSheets? sheets)
    {
        spriteSheets = sheets ?? throw new InvalidOperationException("Ceres destruction requires the installed sprite sheets.");
    }

    /// <summary>Shared queue for Ceres music and the final station blast sound.</summary>
    private readonly AudioQueues? audio;
    /// <summary>
    /// What the turning floor shows (#1467), or null while it shows nothing: before the station is set up, and once the
    /// final explosion with Samus still aboard has cleared the whole floor (#1513).
    /// </summary>
    internal CeresFloorShot? FloorShot { get; private set; }
    /// <summary>Whether the Zebes reveal has loaded its characters and planet map over the station's (#1467).</summary>
    internal bool ZebesRevealed { get; private set; }
    /// <summary>
    /// The colour the scene shows behind every layer (#1512): black until the scene loads the flight's colours, then the
    /// installed flight's backdrop, which the engine flicker never recolours.
    /// </summary>
    internal RgbColor Backdrop { get; private set; }
    /// <summary>The live actors' sprite entries, drawn from the scene's current sheet (#1358).</summary>
    internal readonly ScreenSprites objects;
    /// <summary>The installed backdrops and actor placements, bound when the game is bound to its installed content (#1616).</summary>
    internal IntroCinematicArtwork? Artwork { get; private set; }
    /// <summary>The installed sprite sheets the Ceres and Zebes actors draw from; artwork, never saved.</summary>
    private InstalledSpriteSheets spriteSheets;

    /// <summary>
    /// The actors' sheet: the Ceres scene's until the Zebes reveal loads its own artwork in its place, then the
    /// reveal's.
    /// </summary>
    private CeresSceneSpriteSheet Sprites => new(spriteSheets.Get(ZebesRevealed
        ? SpriteSheetId.ZebesRevealSprites : SpriteSheetId.CeresDestructionSprites));
    /// <summary>Live asteroid, blast, planet, star, and title objects in update order.</summary>
    internal readonly List<IntroDiscoverySprite> actors = [];
    /// <summary>Each sliding reveal actor's upward speed, from rest when the slide starts; a deleted actor's is dropped.</summary>
    private readonly Dictionary<IntroDiscoverySprite, SignedEightEight> slideSpeeds = [];
    /// <summary>
    /// The small asteroids and the vortex, which the Ceres scene draws after every other actor, so the large asteroid and
    /// the blasts overlap them.
    /// </summary>
    private readonly List<IntroDiscoverySprite> ceresActorsDrawnLast = [];
    /// <summary>Runs the final station blast through the ordinary power-bomb color curve.</summary>
    private readonly SamusPowerBombExplosionState stationExplosion = new();
    /// <summary>Engine-flicker palette program created at the Zebes reveal boundary.</summary>
    internal RoomPaletteFxSystem? PaletteFx { get; set; }
    /// <summary>Planet actor whose sliding frame behaviour uses the larger acceleration.</summary>
    internal IntroDiscoverySprite? ZebesPlanetActor { get; private set; }
    /// <summary>Star actor whose offscreen deletion completes the cinematic.</summary>
    private IntroDiscoverySprite? zebesCompletionStarActor;
    /// <summary>Title actor whose cue starts the turning-floor flight toward Zebes.</summary>
    private IntroDiscoverySprite? zebesTitleActor;

    /// <summary>Signed 16.16 camera offset applied to the turning-floor Ceres or Zebes scene.</summary>
    internal FixedPosition<FloorPixels> Background { get; private set; } = new(new(CoordinateAxis.Wrap(-44), 0), new(CoordinateAxis.Wrap(-112), 0));
    /// <summary>
    /// The floor's zoom. Every producer keeps it within zero through
    /// <see cref="CeresDestructionSceneDefinitions.Motion.FarZebesZoomLimit"/> plus one fast step, and the only decrease
    /// is guarded by the minimum-zoom test, so it never falls below zero.
    /// </summary>
    internal FloorZoom Zoom { get; private set; } = FloorZoom.OwnSize;
    /// <summary>How far the scene has turned its floor clockwise.</summary>
    private RotationAngle angle;
    /// <summary>Master brightness from zero (black) to fifteen (full); the slow fades clamp every step to that range.</summary>
    [IntegerQuantity("screen brightness level, zero through fifteen")]
    private int brightness;
    /// <summary>Alternating-call delay between brightness changes.</summary>
    private int fadeCounter = 1;
    /// <summary>Phase-specific hold or mosaic size countdown.</summary>
    private int phaseTimer;
    /// <summary>Fallback music wait when no shared audio queue is present.</summary>
    private int musicQueueTimer = 14;
    /// <summary>Completed waiting updates before the scene is set up; setup follows the last wait.</summary>
    private int initialFrameWaits;
    /// <summary>
    /// Cinematic updates elapsed, incremented after phase dispatch and before actor handling. Only its phase
    /// within a four-update cycle is read, which a wider counter preserves exactly.
    /// </summary>
    private int cinematicFrameCounter;
    /// <summary>Calls elapsed for the staggered and repeating Ceres explosion spawner.</summary>
    private int explosionSpawnerFrame;
    /// <summary>Cycles the authored offsets for repeating small explosions.</summary>
    private int explosionOffsetIndex;
    /// <summary>Countdown carried between repeating explosion spawn calls; it fires on reaching zero or below.</summary>
    private int explosionRepeatCountdown;
    /// <summary>Selects the turning floor or the planet-reveal page.</summary>
    private bool usesFloor = true;
    /// <summary>Whether the turning floor shows during the turning-floor scenes, or only the sprites do.</summary>
    private ScreenLayers shownLayers = ScreenLayers.Foreground | ScreenLayers.Sprites;
    /// <summary>The route when the escape timer expires while Samus remains aboard Ceres.</summary>
    internal bool WithSamus { get; }

    /// <param name="audio">The shared music and sound queue.</param>
    /// <param name="glowColors">Installed Power Bomb glow colours for the station explosion.</param>
    /// <param name="artwork">Installed cinematic artwork; the scene cannot start without it.</param>
    /// <param name="outcome">
    /// The Ceres-destruction state with Samus aboard: the escape timer expired before Samus left, so the
    /// gunship never appears and the fade-out ends the game instead of flying to Zebes.
    /// </param>
    /// <param name="spriteSheets">Required installed sprite sheets the scene's actors draw from.</param>
    public CeresDestructionCinematicState(
        AudioQueues? audio = null,
        InstalledPowerBombGlowColors? glowColors = null,
        IntroCinematicArtwork? artwork = null,
        CeresDestructionOutcome outcome = CeresDestructionOutcome.SamusEscaped,
        InstalledSpriteSheets? spriteSheets = null)
    {
        BindSpriteSheets(spriteSheets);
        WithSamus = outcome == CeresDestructionOutcome.SamusAboard;
        this.audio = audio;
        Artwork = artwork ?? throw new InvalidOperationException(
            "Ceres destruction requires installed cinematic artwork.");
        stationExplosion.PresentationColors = glowColors;
        objects = new ScreenSprites();
    }

    /// <summary>
    /// True while the scene's opening step waits out frames of the display: the next update finishes that wait instead of
    /// starting a new update of the cinematic (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    internal bool IsWaitingOutDisplayFrame =>
        initialFrameWaits is > 0 and <= CeresDestructionSceneDefinitions.InitialFrameWaits;

    /// <summary>Binds the installed station-blast colors when the game is bound to its installed content.</summary>
    internal void BindGlowColors(InstalledPowerBombGlowColors? colors) =>
        stationExplosion.PresentationColors = colors;

    /// <summary>
    /// Binds installed artwork when the game is bound to its installed content. Only the
    /// graphics already reached by the current phase are refreshed; counters,
    /// actors, audio and gameplay timeline remain untouched. The backdrop is refreshed only
    /// before the later engine-flicker palette program starts, as the scene reloads the
    /// flight's colours only until then (#1512).
    /// </summary>
    internal void BindArtwork(IntroCinematicArtwork? value)
    {
        Artwork = value;
        if (value is null) return;
        if (CeresDestructionPhaseGroups.ThroughFlightToZebesStart.Contains(Phase))
            Backdrop = value.CeresFlight.Backdrop;
    }

    /// <summary>Current dispatcher phase, including the alternate game-over route.</summary>
    public CeresDestructionPhase Phase { get; internal set; }

    /// <summary>True after the completion star leaves the Zebes screen or the terminal route ends.</summary>
    public bool Finished => Phase == CeresDestructionPhase.Finished;

    /// <summary>Executes one call through the destruction cinematic's dispatcher.</summary>
    public void Step()
    {
        // The cinematic first waits nine updates with the screen black; each wait is one update
        // that continues it. After the last it sets up the scene and returns to the cinematic
        // phase, whose object handling and frame-counter tail then run once.
        if (initialFrameWaits < CeresDestructionSceneDefinitions.InitialFrameWaits)
        {
            initialFrameWaits++;
            return;
        }
        if (initialFrameWaits == CeresDestructionSceneDefinitions.InitialFrameWaits)
        {
            initialFrameWaits++;
            SetupCeresDestruction();
            // Setup queues the cinematic music and the state's own track last.
            audio?.QueueMusic(MusicRequest.Stop);
            audio?.QueueMusic(
                MusicRequest.LoadDataSet(MusicDataSet.Ceres));
            audio?.ScheduleMusic(
                MusicRequest.SelectTrack(WithSamus
                    ? CeresDestructionSceneDefinitions.Music.CeresWithSamusTrack
                    : CeresDestructionSceneDefinitions.Music.CeresTrack),
                MusicQueueDelay.AtLeastShortest(CeresDestructionSceneDefinitions.Music.QueueDelay));
        }
        else
        {
            StepPhase();
        }
        StepWrapperTail();
    }

    /// <summary>Runs one cinematic step after the opening waits and advances the station blast.</summary>
    private void StepPhase()
    {
        // The update's per-line effects run before the cinematic. A
        // newly spawned blast therefore cannot advance until the following call.
        stationExplosion.StepFrame();
        switch (Phase)
        {
            case CeresDestructionPhase.WaitForMusicQueue:
                if (MusicQueueFinished())
                    Phase = CeresDestructionPhase.FadeInAndDrift;
                break;

            case CeresDestructionPhase.FlyToZebesInitial:
                // The Zebes scene is set up and the fade-in installed.
                SetupZebesReveal();
                Phase = CeresDestructionPhase.FadeInZebes;
                break;

            case CeresDestructionPhase.FadeInAndDrift:
                StepInitialDrift();
                Zoom = Zoom.Widened(CeresDestructionSceneDefinitions.Motion.CeresZoomStep);
                StepSlowFadeIn();
                if (brightness == 15)
                {
                    explosionRepeatCountdown = CeresExplosionDefinitions.RepeatingCountdownSeed;
                    Phase = CeresDestructionPhase.ApproachExplosion;
                }
                break;

            case CeresDestructionPhase.ApproachExplosion:
                StepInitialDrift();
                if (Zoom < CeresDestructionSceneDefinitions.Motion.CeresZoomLimit)
                {
                    Zoom = Zoom.Widened(CeresDestructionSceneDefinitions.Motion.CeresZoomStep);
                }
                else
                {
                    // The final blast snaps the zoom to the explosion's, creates the final blast, and
                    // installs the flying-away function on this exact handler call.
                    Zoom = CeresDestructionSceneDefinitions.Motion.CeresExplosionZoom;
                    SpawnFinalCeresExplosion();
                    Phase = CeresDestructionPhase.FlyingAwayFromExplosion;
                }
                break;

            case CeresDestructionPhase.FlyingAwayFromExplosion:
                Background = Background.WithX(Background.X.WithWhole(CoordinateAxis.Wrap(Background.X.Whole + 2)));
                angle = angle.TurnedBySteps(-1);
                if (Zoom < CeresDestructionSceneDefinitions.Motion.MinimumZoom)
                {
                    phaseTimer = CeresDestructionSceneDefinitions.Timing.ExplosionHoldFrames;
                    Phase = CeresDestructionPhase.HoldAfterExplosion;
                }
                else
                {
                    Zoom = Zoom.Narrowed(CeresDestructionSceneDefinitions.Motion.SlowZoomStep);
                }
                break;

            case CeresDestructionPhase.HoldAfterExplosion:
                if (--phaseTimer <= 0)
                {
                    fadeCounter = 1;
                    Phase = CeresDestructionPhase.FadeOutCeres;
                }
                break;

            case CeresDestructionPhase.FadeOutCeres:
                // Once the screen is black the next step is installed; it runs next update.
                // The game-over route instead stops the music and ends at the death black-out.
                if (StepSlowFadeOut())
                {
                    if (WithSamus)
                    {
                        audio?.QueueMusic(MusicRequest.Stop);
                        Phase = CeresDestructionPhase.Finished;
                    }
                    else
                    {
                        Phase = CeresDestructionPhase.FlyToZebesInitial;
                    }
                }
                break;

            case CeresDestructionPhase.FadeInZebes:
                StepZebesMosaic();
                StepSlowFadeIn();
                if (brightness == 15)
                    Phase = CeresDestructionPhase.RemoveZebesMosaic;
                break;

            case CeresDestructionPhase.RemoveZebesMosaic:
                if (StepZebesMosaic())
                    SetupZebesFloorActors();
                break;

            case CeresDestructionPhase.PlanetZebesTitle:
                StepZebesActors(motion: ZebesActorsMotion.Holding);
                break;

            case CeresDestructionPhase.FlyingTowardZebesA:
                Background = Background.WithY(Background.Y.MovedBy(CeresDestructionSceneDefinitions.Motion.EighthPixel));
                Background = Background.WithX(Background.X.MovedBy(CeresDestructionSceneDefinitions.Motion.NegativeHalfPixel));
                if (Zoom < CeresDestructionSceneDefinitions.Motion.ZebesApproachZoomLimit)
                    Zoom = Zoom.Widened(CeresDestructionSceneDefinitions.Motion.NearZebesZoomStep);
                else
                    Phase = CeresDestructionPhase.FlyingTowardZebesB;
                StepZebesActors(motion: ZebesActorsMotion.Holding);
                break;

            case CeresDestructionPhase.FlyingTowardZebesB:
                Background = Background.WithY(Background.Y.MovedBy(CeresDestructionSceneDefinitions.Motion.EighthPixel));
                Background = Background.WithX(Background.X.MovedBy(CeresDestructionSceneDefinitions.Motion.NegativeHalfPixel));
                if (CoordinateAxis.AsSigned(Background.X.Whole) < -128)
                {
                    Phase = CeresDestructionPhase.FlyingTowardZebesC;
                }
                else
                {
                    Zoom = Zoom.Widened(CeresDestructionSceneDefinitions.Motion.SlowZoomStep);
                    angle = angle.TurnedBySteps(-1);
                }
                StepZebesActors(motion: ZebesActorsMotion.Holding);
                break;

            case CeresDestructionPhase.FlyingTowardZebesC:
                Background = Background.WithY(Background.Y.MovedBy(CeresDestructionSceneDefinitions.Motion.EighthPixel));
                Background = Background.WithX(Background.X.MovedBy(CeresDestructionSceneDefinitions.Motion.EighthPixel));
                if (Zoom < CeresDestructionSceneDefinitions.Motion.FarZebesZoomLimit)
                {
                    Zoom = Zoom.Widened(CeresDestructionSceneDefinitions.Motion.FastZoomStep);
                }
                else
                {
                    // The game hides the ship's turning floor before the hold, not
                    // when the later pan starts. The planet and star sprites stay shown.
                    shownLayers = ScreenLayers.Sprites;
                    phaseTimer = CeresDestructionSceneDefinitions.Timing.ZebesHoldFrames;
                    Phase = CeresDestructionPhase.HoldCloseZebes;
                }
                StepZebesActors(motion: ZebesActorsMotion.Holding);
                break;

            case CeresDestructionPhase.HoldCloseZebes:
                StepZebesActors(motion: ZebesActorsMotion.Holding);
                if (--phaseTimer <= 0)
                    Phase = CeresDestructionPhase.SlideZebesSceneAway;
                break;

            case CeresDestructionPhase.SlideZebesSceneAway:
                StepZebesActors(motion: ZebesActorsMotion.SlidingAway);
                break;
            // The cinematic has ended; its owner advances the game state.
            case CeresDestructionPhase.Finished:
                break;
            default:
                throw new InvalidOperationException($"Undefined CeresDestructionPhase {Phase}.");
        }
    }

    /// <summary>The cinematic state's work after the cinematic function returns.</summary>
    private void StepWrapperTail()
    {
        if (CeresDestructionPhaseGroups.CeresScene.Contains(Phase))
            StepCeresActors();

        // The cinematic's game state runs the palette effects after cinematic objects, including the
        // frame which spawns the program.
        if (!CeresDestructionPhaseGroups.ThroughFlightToZebesStart.Contains(Phase))
        {
            if (PaletteFx is null)
            {
                PaletteFx = new RoomPaletteFxSystem(new PaletteFxSessionState());
                PaletteFx.Start(CeresDestructionSceneDefinitions.GunshipEngineFlicker, 0);
            }
            // The flicker's records are shades the import pre-colours the scene's pictures in (#1512); the scene names the
            // record its running engine program shows (#1597).
            PaletteFx.StepScreen(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, TorizoIsDead = false, AreaMiniBossDefeated = false });
            if (PaletteFx.AudioRequests.SoundRequests.Count != 0)
                throw new InvalidDataException("Ceres engine palette program unexpectedly requested audio.");
        }

        // The cinematic's game state increments the shared cinematic frame counter after calling the
        // current function but before object handling. The mosaic test therefore observes
        // this pre-increment value on its following dispatcher call.
        cinematicFrameCounter++;
    }

    /// <summary>Sets up the Ceres floor, sprites, colours, and three initial actors after the ninth opening wait.</summary>
    private void SetupCeresDestruction()
    {
        IntroCinematicArtwork content = Artwork ?? throw new InvalidOperationException(
            "Ceres destruction requires installed cinematic artwork.");

        // The scene setup loads the station's two views; the floor shows the station's installed picture (#1467).
        FloorShot = CeresFloorShot.Station;
        Backdrop = content.CeresFlight.Backdrop;

        actors.Clear();
        for (int index = 0; index < CeresDestructionActorDefinitions.InitialActorCount; index++)
        {
            // Each actor starts at its installed placement (#1596).
            actors.Add(CreateActor(CeresDestructionActors.InitialActor(index), content.CeresDestruction.DestructionActors[index].Position));
        }
        ceresActorsDrawnLast.Clear();
        ceresActorsDrawnLast.Add(actors[1]);
        ceresActorsDrawnLast.Add(actors[2]);
        Phase = CeresDestructionPhase.WaitForMusicQueue;
    }

    /// <summary>Stages the planet-reveal map and resets camera, angle, fade, and mosaic state.</summary>
    private void SetupZebesReveal()
    {
        if (Artwork is null)
            throw new InvalidOperationException("Zebes reveal requires installed cinematic artwork.");

        // The reveal starts on the planet-reveal page. It also stages the rear-gunship view, which becomes visible
        // only when the gunship step later selects the floor again.
        ZebesRevealed = true;
        FloorShot = CeresFloorShot.ZebesApproach;

        actors.Clear();
        Background = FixedPosition<FloorPixels>.Zero;
        angle = RotationAngle.Zero;
        Zoom = FloorZoom.OwnSize;
        brightness = 0;
        fadeCounter = 1;
        phaseTimer = CeresDestructionSceneDefinitions.Timing.InitialMosaicSize;
        usesFloor = false;
    }

    /// <summary>Creates the planet, stars, and title sprites for the later turning-floor flight.</summary>
    private void SetupZebesFloorActors()
    {
        usesFloor = true;
        Background = new(Background.X.WithWhole(CeresDestructionSceneDefinitions.Sprites.ZebesInitialBackgroundX),
            Background.Y.WithWrappedWhole(-104));
        angle = CeresDestructionSceneDefinitions.Motion.ApproachAngle;
        Zoom = FloorZoom.OwnSize;
        actors.Clear();
        ZebesPlanetActor = CreateZebesActor(0);
        actors.Add(ZebesPlanetActor);
        actors.Add(CreateZebesActor(1));
        actors.Add(CreateZebesActor(2));
        actors.Add(CreateZebesActor(3));
        zebesCompletionStarActor = CreateZebesActor(4);
        actors.Add(zebesCompletionStarActor);
        zebesTitleActor = CreateZebesActor(5);
        actors.Add(zebesTitleActor);
        Phase = CeresDestructionPhase.PlanetZebesTitle;
    }

    /// <summary>Drifts the Ceres camera by one sixteenth pixel down and one quarter pixel left.</summary>
    private void StepInitialDrift()
    {
        Background = Background.WithY(Background.Y.MovedBy(CeresDestructionSceneDefinitions.Motion.SixteenthPixel));
        Background = Background.WithX(Background.X.MovedBy(CeresDestructionSceneDefinitions.Motion.NegativeQuarterPixel));
    }

    /// <summary>Raises master brightness one level on alternating calls, capped at fifteen.</summary>
    private void StepSlowFadeIn()
    {
        if (fadeCounter-- > 0)
            return;
        fadeCounter = 1;
        brightness = Math.Min(15, brightness + 1);
    }

    /// <summary>Lowers brightness on alternating calls and reports the first fully black frame.</summary>
    private bool StepSlowFadeOut()
    {
        if (fadeCounter-- > 0)
            return false;
        fadeCounter = 1;
        brightness = Math.Max(0, brightness - 1);
        return brightness == 0;
    }

    /// <summary>
    /// Reduces the planet reveal's mosaic size every fourth cinematic frame until clear. The fade-in already steps it,
    /// so it passes zero before the removal phase tests it and wraps from the smallest size to the largest.
    /// </summary>
    private bool StepZebesMosaic()
    {
        if (!CeresCinematicCadenceDefinitions.ZebesMosaicStepEveryFourthUpdate.IsDue(cinematicFrameCounter))
            return false;
        phaseTimer = (phaseTimer + CeresDestructionSceneDefinitions.Timing.MosaicSizeCount - 1) %
            CeresDestructionSceneDefinitions.Timing.MosaicSizeCount;
        return phaseTimer == 0;
    }

    /// <summary>Waits for queued music, using a fixed fallback when audio is absent.</summary>
    private bool MusicQueueFinished() =>
        audio is null ? --musicQueueTimer <= 0 : !audio.HasQueuedMusic;

    /// <summary>Installs an authored sprite at <paramref name="start"/> with its attributes, program, and frame behaviour.</summary>
    private static IntroDiscoverySprite CreateActor(
        CeresDestructionActorDefinition definition, Position16<ScreenPixels> start)
    {
        var actor = new IntroDiscoverySprite(
            start,
            CeresShades.Drawn(definition.Colouring),
            definition.Program)
        {
            HorizontalLoopWidth = definition.WrapX ? CeresDestructionSceneDefinitions.WorldWidth : null,
        };
        actor.InstallFrameBehavior(definition.ActiveFrameBehavior);
        return actor;
    }

    /// <summary>Creates one Zebes reveal actor at its installed placement (#1596).</summary>
    private IntroDiscoverySprite CreateZebesActor(int index)
    {
        IntroCinematicArtwork content = Artwork ?? throw new InvalidOperationException(
            "The Zebes reveal requires installed cinematic artwork.");
        return CreateActor(CeresDestructionActors.ZebesActor(index), content.CeresDestruction.RevealActors[index].Position);
    }


    /// <summary>Spawns authored blast waves and advances live Ceres actors in descending slot order.</summary>
    private void StepCeresActors()
    {
        explosionSpawnerFrame++;

        // The explosion spawner remains invisible for 128 frames, spawns five staggered small blasts,
        // waits 80 BEFORE installing the repeating-blast frame behaviour, then runs
        // it during the final 64-frame wait. The frame behaviour stops on departure;
        // the program still reaches its four terminal blasts afterward.
        CeresExplosionSpawnEvents events = CeresExplosions.EventsAtFrame(explosionSpawnerFrame,
            CeresDestructionPhaseGroups.BeforeFlyingAway.Contains(Phase), ref explosionRepeatCountdown);
        if (events.Initial)
        {
            for (int index = 0; index < CeresExplosionDefinitions.InitialExplosionCount; index++)
                SpawnCeresExplosion(
                    CeresExplosionDefinitions.InitialActor,
                    CeresExplosions.InitialExplosion(index));
        }
        if (events.Repeating)
        {
            int offset = explosionOffsetIndex++ &
                (CeresExplosionDefinitions.RepeatingExplosionCount - 1);
            SpawnCeresExplosion(
                CeresExplosionDefinitions.RepeatingActor,
                CeresExplosions.RepeatingExplosion(offset));
        }
        if (events.Final)
        {
            for (int index = 0; index < CeresExplosionDefinitions.FinalExplosionCount; index++)
                SpawnCeresExplosion(
                    CeresExplosionDefinitions.FinalWaveActor,
                    CeresExplosions.FinalExplosion(index));
        }

        for (int index = actors.Count - 1; index >= 0; index--)
        {
            IntroDiscoverySprite actor = actors[index];
            if (index < CeresDestructionActorDefinitions.InitialActorCount)
            {
                CeresDestructionActorDefinition definition =
                    CeresDestructionActors.InitialActor(index);
                if (actor.FrameBehavior != definition.ActiveFrameBehavior)
                {
                    throw new InvalidDataException(
                        $"Ceres destruction actor {index} names invalid frame behaviour " +
                        $"{actor.FrameBehavior}.");
                }
                if (definition.WrapX)
                    AddWrappedX(actor, definition.HorizontalDelta);
            }
            else
                MoveExplosion(actor);

            actor.Step();
            if (!actor.IsActive)
            {
                ceresActorsDrawnLast.Remove(actor);
                actors.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// The final blast replaces the floor on the same dispatcher call that creates the final explosion. With Samus gone
    /// the upper half becomes the gunship viewed from the front over a cleared lower half; with Samus still aboard no
    /// gunship leaves and both halves are cleared, so the floor shows nothing (#1513). Leaving the station on either half
    /// would make the station itself flee the explosion and later carry it into the Zebes approach.
    /// </summary>
    private void LoadFinalExplosionMaps() =>
        FloorShot = WithSamus ? null : CeresFloorShot.Explosion;

    /// <summary>Starts the power-bomb blast, queues sound, adds its sprite, and replaces the station maps.</summary>
    private void SpawnFinalCeresExplosion()
    {
        // The final blast calls the ordinary enemy explosion with screen-space coordinates,
        // before replacing the station picture. Keep its authored curves/colors/lifetime.
        stationExplosion.Spawn(new(
            CoordinateAxis.Wrap(CeresDestructionSceneDefinitions.CeresFloorCenter.X - Background.X.Whole),
            CoordinateAxis.Wrap(CeresDestructionSceneDefinitions.CeresFloorCenter.Y - Background.Y.Whole)));
        audio?.QueueSound(SuperMetroid.Core.Audio.SamusSound.PowerBombExplosion,
            priority: SoundRequestPriority.Essential);
        var cinematicExplosion = new IntroDiscoverySprite(
            new(CoordinateAxis.Wrap(CeresDestructionSceneDefinitions.CeresFloorCenter.X - Background.X.Whole),
                CoordinateAxis.Wrap(CeresDestructionSceneDefinitions.CeresFloorCenter.Y - Background.Y.Whole)),
            CeresShades.Drawn(CeresSpriteColouring.Explosions),
            CeresExplosionDefinitions.StationBlastActor.Program);
        cinematicExplosion.InstallFrameBehavior(
            CeresExplosionDefinitions.StationBlastActor.FrameBehavior);
        actors.Add(cinematicExplosion);

        LoadFinalExplosionMaps();
    }

    /// <summary>Creates a small blast at a camera-relative authored offset with its initial animation delay.</summary>
    private void SpawnCeresExplosion(
        CinematicActorDefinition definition,
        CeresExplosionPlacement placement)
    {
        var actor = new IntroDiscoverySprite(
            new(CoordinateAxis.Wrap(CeresDestructionSceneDefinitions.CeresFloorCenter.X -
                    CoordinateAxis.AsSigned(Background.X.Whole) + placement.Offset.X),
                CoordinateAxis.Wrap(CeresDestructionSceneDefinitions.CeresFloorCenter.Y -
                    CoordinateAxis.AsSigned(Background.Y.Whole) + placement.Offset.Y)),
            CeresShades.Drawn(CeresSpriteColouring.Explosions),
            definition.Program);
        actor.InstallFrameBehavior(definition.FrameBehavior);

        // The initializer writes its stagger directly to the frame hold. The loop countdown is a different count, used
        // by the program's counted loops; using it here would make all five blasts appear immediately despite distinct
        // delays.
        actor.DelayFirstInstruction(placement.DelayFrames);
        actors.Add(actor);
    }

    /// <summary>Applies the fleeing gunship's X subvelocity and signed Y velocity to an explosion sprite.</summary>
    private static void MoveExplosion(IntroDiscoverySprite actor)
    {
        actor.Position = new(
            IntroCinematicMotion.AddFraction(actor.Position.X, CeresDestructionSceneDefinitions.Motion.GunshipXSubvelocity),
            actor.Position.Y.MovedBy(CeresDestructionSceneDefinitions.Motion.GunshipYVelocity));
    }

    /// <summary>Advances the planet and title objects, applying slide acceleration and finishing on star deletion.</summary>
    private void StepZebesActors(ZebesActorsMotion motion)
    {
        for (int index = actors.Count - 1; index >= 0; index--)
        {
            IntroDiscoverySprite actor = actors[index];
            if (motion == ZebesActorsMotion.SlidingAway)
            {
                CeresDestructionActorDefinition definition = ReferenceEquals(actor, ZebesPlanetActor)
                    ? CeresDestructionActors.ZebesActor(0)
                    : ReferenceEquals(actor, zebesCompletionStarActor)
                        ? CeresDestructionActors.ZebesActor(4)
                        : ReferenceEquals(actor, zebesTitleActor)
                            ? CeresDestructionActors.ZebesActor(5)
                            : CeresDestructionActors.ZebesActor(1);
                if (definition.SlideFrameBehavior is null)
                {
                    throw new InvalidDataException(
                        "PLANET ZEBES title actor remained active when scene sliding began.");
                }
                actor.InstallFrameBehavior(definition.SlideFrameBehavior);
                // Zebes accelerates by 64 in 8.8; all four star sheets use 32. Star
                // sheet five owns the reveal's completion. Identify the actors
                // by ownership, not their mutable list index: actors delete as they
                // cross -128, so indexes necessarily shift during this loop.
                // The slide speed grows by at most 64 a frame and the actor is deleted within a few dozen frames, once
                // it crosses -128, so the speed stays far inside its range.
                SignedEightEight slideSpeed = slideSpeeds.GetValueOrDefault(actor) + definition.SlideAcceleration;
                slideSpeeds[actor] = slideSpeed;
                actor.Position = actor.Position.WithY(actor.Position.Y.MovedBy(-slideSpeed.ToSixteenSixteenDelta()));
                if (CoordinateAxis.AsSigned(actor.WholePosition.Y) < -128)
                {
                    // Each reveal object deletes itself before its screen Y,
                    // 0 through 255, can wrap below the screen. Only star sheet five
                    // additionally hands the cinematic back to game load.
                    if (definition.CompletesScene)
                    {
                        Phase = CeresDestructionPhase.Finished;
                        return;
                    }

                    actor.Delete();
                    actors.RemoveAt(index);
                    slideSpeeds.Remove(actor);
                    continue;
                }
            }

            actor.Step(HandleZebesInstruction);
            if (!actor.IsActive)
                actors.RemoveAt(index);
        }
    }

    /// <summary>Lets the title actor start the camera flight at its authored cue boundary.</summary>
    private void HandleZebesInstruction(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.FadeInZebesText:
            case CinematicSpriteCueKind.SpawnJapaneseZebesTextIfNeeded:
            case CinematicSpriteCueKind.FadeOutZebesText:
                return;

            case CinematicSpriteCueKind.StartFlyingToZebes:
                // The title actor, not a host timer, publishes the camera flight exactly
                // where its animation script reaches the flight step.
                Background = new(Background.X.WithWhole(CeresDestructionSceneDefinitions.Motion.FloorExitX),
                    Background.Y.WithWrappedWhole(-112));
                angle = CeresDestructionSceneDefinitions.Motion.ApproachAngle;
                Zoom = CeresDestructionSceneDefinitions.Motion.MinimumZoom;
                Phase = CeresDestructionPhase.FlyingTowardZebesA;
                return;

            case CinematicSpriteCueKind.SetFrameBehavior:
            case CinematicSpriteCueKind.EndZebesExplosion:
            case CinematicSpriteCueKind.ExplosionFinale:
            case CinematicSpriteCueKind.FadeExplosionPalette:
            case CinematicSpriteCueKind.GreyOut:
            case CinematicSpriteCueKind.Launch:
            case CinematicSpriteCueKind.LaunchHead:
            case CinematicSpriteCueKind.PrepareHead:
            case CinematicSpriteCueKind.Shoot:
            case CinematicSpriteCueKind.SpawnClearTime:
            case CinematicSpriteCueKind.SpawnColon:
            case CinematicSpriteCueKind.SpawnCompletedText:
            case CinematicSpriteCueKind.SpawnExplosionSilhouette:
            case CinematicSpriteCueKind.SpawnFinalExplosionWave:
            case CinematicSpriteCueKind.SpawnHoursTens:
            case CinematicSpriteCueKind.SpawnHoursUnits:
            case CinematicSpriteCueKind.SpawnInitialExplosionWave:
            case CinematicSpriteCueKind.SpawnMinutesTens:
            case CinematicSpriteCueKind.SpawnMinutesUnits:
            case CinematicSpriteCueKind.SpawnParticles:
            case CinematicSpriteCueKind.SpawnRinkas0And1:
            case CinematicSpriteCueKind.SpawnRinkas2And3:
            case CinematicSpriteCueKind.SpawnSuitedJump:
            case CinematicSpriteCueKind.SpawnSuitlessJump:
            case CinematicSpriteCueKind.StartIntroPage2:
            case CinematicSpriteCueKind.StartIntroPage3:
            case CinematicSpriteCueKind.StartIntroPage4:
            case CinematicSpriteCueKind.StartIntroPage5:
            case CinematicSpriteCueKind.StartRinkaMoving:
            case CinematicSpriteCueKind.StartZebesExplosion:
            case CinematicSpriteCueKind.TransitionToCredits:
            default:
                throw new InvalidOperationException($"The Zebes title programs publish no {cue} cue.");
        }
    }

    /// <summary>Adds a fractional asteroid X step and wraps its whole coordinate around the world's width.</summary>
    private static void AddWrappedX(IntroDiscoverySprite actor, SignedSixteenSixteen horizontalDelta)
    {
        // The wrapping field adds only the delta's fraction, with nothing in the whole pixel: a leftward delta,
        // whose fraction is the rest of a pixel, carries the actor right instead.
        FixedCoordinate x = IntroCinematicMotion.AddFraction(actor.Position.X, horizontalDelta);
        actor.Position = actor.Position.WithX(x.WithWhole(CoordinateAxis.Wrap(x.Whole % CeresDestructionSceneDefinitions.WorldWidth)));
    }


    /// <summary>
    /// Captures the scene's ordered layers (#1514): over Ceres, the station's turning floor with the asteroids and blasts
    /// in front of it; on the approach to Zebes, the planet and its stars behind the gunship's floor; or the Zebes reveal's
    /// planet picture. The per-sprite-part priority tiers that once interleaved the sprites with the floor are a
    /// presentation quirk the model does not keep.
    /// </summary>
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        var sprites = new SpritesLayer(objects.Draws);
        DisplayLayer[] layers;
        if (usesFloor && (shownLayers & ScreenLayers.Foreground) == ScreenLayers.None)
        {
            layers = [sprites];
        }
        else if (usesFloor)
        {
            FloorView floor = FloorView.Turned(angle, Zoom,
                CeresDestructionPhaseGroups.CeresScene.Contains(Phase)
                    ? CeresDestructionSceneDefinitions.CeresFloorCenter
                    : CeresDestructionSceneDefinitions.ZebesFloorCenter,
                Background.Whole.AsSignedOffset());
            // Before the station is set up, and after the explosion with Samus aboard clears it, the floor shows nothing.
            if (FloorShot is not { } shot)
                layers = [sprites];
            else
            {
                var turning = new TurningFloorLayer(floor, CeresFloors.Of(spriteSheets, shot).Shaded(EngineShading));
                layers = CeresDestructionPhaseGroups.CeresScene.Contains(Phase) ? [turning, sprites] : [sprites, turning];
            }
        }
        else
        {
            // Preserve the existing non-mosaicked path. Mosaic implementation
            // is a game-correctness change, not an implicit part of this extraction.
            // The Zebes reveal shows the planet's installed picture once it is loaded (#1467).
            layers = [new PicturedPlaneLayer(CeresCinematicPictures.Zebes(spriteSheets, ZebesRevealed), Offset16<PlanePixels>.Zero)];
        }
        if (CaptureStationExplosion() is { } blast)
            layers = [.. layers, blast];
        return new(new FramePaint(Backdrop), layers, brightness);
    }

    /// <summary>
    /// The shading of the gunship's floor (#1512, #1544, #1597): the shade of the frame the gunship engine's flicker
    /// program shows, which recolours the engine's colour, or the floor's standard colours while no program has shown one.
    /// </summary>
    internal PictureShading EngineShading => PaletteFx?.FrameOf(PaletteFxColours.CutsceneGunshipEngine) is { } frame
        ? PictureShading.Shaded(PaletteFxShades.Of(PaletteFxColours.CutsceneGunshipEngine, frame))
        : PictureShading.Standard;

    /// <summary>Captures the active station blast's added colour as a render layer, over the whole screen.</summary>
    private ColourAddWindowLayer? CaptureStationExplosion() =>
        GameplayEffectCapture.CapturePowerBombGlow(PowerBombRenderSnapshot.Capture(stationExplosion),
            cameraPosition: Position16<RoomPixels>.Zero, firstLine: 0);

    /// <summary>
    /// Places live actors as sprite entries in the order they spawned, the Ceres scene's small asteroids and vortex after
    /// every other actor.
    /// </summary>
    private void PrepareRenderSprites()
    {
        objects.BeginFrame();
        // Same-priority sprite overlap is decided by drawing order.
        IEnumerable<IntroDiscoverySprite> drawActors = CeresDestructionPhaseGroups.CeresScene.Contains(Phase)
            ? actors.Where(actor => !ceresActorsDrawnLast.Contains(actor)).Concat(actors.Where(ceresActorsDrawnLast.Contains)) : actors;
        foreach (IntroDiscoverySprite actor in drawActors)
            actor.Draw(objects, installedArt: Sprites);
        objects.FinishFrame();
    }

}
