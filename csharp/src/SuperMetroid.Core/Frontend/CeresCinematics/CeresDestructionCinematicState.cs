using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Installed-asset implementation of the Ceres-destruction game state: Ceres explodes and the
/// camera subsequently follows Samus's gunship toward Zebes.
/// </summary>
/// <remarks>
/// The cinematic owns this sequence. The state deliberately retains the
/// game's phase boundaries, counters, camera motion, turning-floor rotation values, and
/// sprite animations, so the scene's progress is game state:
/// there is no separately authored video or timer that merely happens to end at the
/// same gameplay room.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class CeresDestructionCinematicState : IFrontendScreen
{
    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.ScreenContent, artwork.ScreenContent))
            BindGlowColors(artwork.ScreenContent.PowerBombGlowColors);
        if (!ReferenceEquals(previous.IntroCinematicArt, artwork.IntroCinematicArt))
            BindArtwork(artwork.IntroCinematicArt);
        BindSpriteSheets(artwork.SpriteSheets);
    }

    /// <summary>Binds the installed sprite sheets the scene's actors draw from.</summary>
    [MemberNotNull(nameof(spriteSheets))]
    internal void BindSpriteSheets(InstalledSpriteSheets sheets)
    {
        spriteSheets = Ensure.NotNull(sheets);
    }

    /// <summary>Shared queue for Ceres music and the final station blast sound.</summary>
    private readonly AudioQueues audio;
    /// <summary>What the scene shows behind and among its actors (#2541), which the scene moves on from one stage to the next.</summary>
    internal CeresDestructionView View { get; private set; }
    /// <summary>Whether the Zebes reveal's pictures and planet show in place of the station's (#1467).</summary>
    internal bool ZebesRevealed => View is CeresDestructionView.ZebesReveal or CeresDestructionView.ZebesApproach or CeresDestructionView.ZebesClose;
    /// <summary>
    /// The colour the scene shows behind every layer (#1512): black until the scene loads the flight's colours, then the
    /// installed flight's backdrop, which the engine flicker never recolours.
    /// </summary>
    internal RgbColor Backdrop { get; private set; }
    /// <summary>The live actors' sprite entries, drawn from the scene's current sheet (#1358).</summary>
    internal readonly ScreenSprites screenSprites;
    /// <summary>The installed backdrops and actor placements, bound when the game is bound to its installed content (#1616).</summary>
    internal IntroCinematicArtwork Artwork { get; private set; }
    /// <summary>The installed sprite sheets the Ceres and Zebes actors draw from; artwork, never saved.</summary>
    private InstalledSpriteSheets spriteSheets;

    /// <summary>
    /// The actors' sheet: the Ceres scene's until the Zebes reveal loads its own artwork in its place, then the
    /// reveal's.
    /// </summary>
    private CeresSceneSpriteSheet Sprites => new(spriteSheets.Get(ZebesRevealed
        ? SpriteSheetId.ZebesRevealSprites : SpriteSheetId.CeresDestructionSprites), ActorShading);
    /// <summary>
    /// The live actors over Ceres in the order they spawned (#2489): the persistent asteroids and vortex, then the blasts.
    /// They update from the last to the first.
    /// </summary>
    private readonly List<CeresSceneActor> ceresActors = [];
    /// <summary>The live Zebes reveal actors in the order they spawned (#2489); they update from the last to the first.</summary>
    private readonly List<ZebesRevealActor> revealActors = [];
    /// <summary>
    /// The invisible actor whose animation spawns the station's waves of blasts (#2489). The scene's setup update is its
    /// first; it runs before the other actors over Ceres, so a blast it spawns runs in the same update.
    /// </summary>
    private readonly CinematicActor<CeresAnimation, CinematicSpritePose, CeresExplosionSpawnerCue> explosionSpawner;
    /// <summary>
    /// The places of the repeating blasts the spawner has still to spawn (#2541), in the order it spawns them: each play of
    /// its repeating blast takes the next.
    /// </summary>
    private readonly Queue<Offset16<ScreenPixels>> repeatingBlastPlacesLeft = new(CeresExplosionDefinitions.RepeatingBlastPlaces);
    /// <summary>The blast that destroys the station (#1981), centred on it in screen pixels.</summary>
    private readonly CeresStationBlast stationExplosion;

    /// <summary>The installed Power Bomb glow colours the station blast glows in, bound again with the scene's artwork.</summary>
    private InstalledPowerBombGlowColors glowColors;
    /// <summary>
    /// The scene's colour effects (#1733): none before the station is set up; from its setup the station's navigation
    /// lights; once the Zebes reveal's setup clears them, the gunship engine's flicker and the PLANET ZEBES caption's fades.
    /// </summary>
    internal ColourEffectSystem ColourEffects { get; private set; } = ColourEffectSystem.ForScreen();
    /// <summary>
    /// The Japanese subtitle the title's cue has just spawned, which runs on the same update, after the title, before it
    /// joins the reveal's actors.
    /// </summary>
    private ZebesRevealActor? spawnedSubtitle;
    /// <summary>The Japanese text option, which makes the PLANET ZEBES title spawn its Japanese subtitle (#1733).</summary>
    private readonly bool japaneseText;

    /// <summary>The camera offset applied to the turning-floor Ceres or Zebes scene.</summary>
    internal FixedVector<FloorPixels> FloorScroll { get; private set; } = new(
        SignedSixteenSixteen.FromWholePixels(CeresDestructionSceneDefinitions.Places.CeresInitialFloorScrollX),
        SignedSixteenSixteen.FromWholePixels(CeresDestructionSceneDefinitions.Places.CeresAndFlightFloorScrollY));
    /// <summary>
    /// The floor's zoom. Every producer keeps it within zero through
    /// <see cref="CeresDestructionSceneDefinitions.Motion.FarZebesZoomLimit"/> plus one fast step, and the only decrease
    /// is guarded by the minimum-zoom test, so it never falls below zero.
    /// </summary>
    internal FloorZoom Zoom { get; private set; } = FloorZoom.OwnSize;
    /// <summary>How far the scene has turned its floor clockwise.</summary>
    private RotationAngle angle;
    /// <summary>The screen's brightness, black at first, and its slow fades, one level on every second update.</summary>
    private readonly ScreenFade fade = new(0, stepDelay: CeresDestructionSceneDefinitions.Timing.FirstFadeWaitBeforeEachStep,
        firstWait: CeresDestructionSceneDefinitions.Timing.FirstFadeWaitBeforeEachStep);
    /// <summary>The hold of the phase holding: after the explosion, then on close Zebes.</summary>
    private UpdateCountdown phaseHold = UpdateCountdown.Ended();
    /// <summary>
    /// The width of the square blocks the Zebes reveal draws its planet picture in (#1774), which also counts the reveal
    /// down: every fourth update of the reveal's fade-in and of the removal that follows, the blocks narrow by one pixel,
    /// and the removal ends on the step that leaves them one pixel wide, the picture unpixelated. A step from one-pixel
    /// blocks starts again from the coarsest (<see cref="CeresDestructionSceneDefinitions.RevealPixelation.CoarsestBlockWidth"/>).
    /// That restart is load-bearing: the fade-in clears the picture once before the removal's first step, so the reveal
    /// pixelates a second time from the coarsest blocks, and lasts that much longer, before it ends.
    /// </summary>
    [IntegerQuantity("pixelation block width in screen pixels, one through sixteen")]
    private int revealBlockWidth = 1;
    /// <summary>
    /// The scene's wait, black, before it sets the station up, of <see cref="CeresDestructionSceneDefinitions.InitialFrameWaits"/>
    /// updates (#2488): the update after it is over sets the scene up; null once it is set up.
    /// </summary>
    private UpdateCountdown? openingWait = UpdateCountdown.Lasting(CeresDestructionSceneDefinitions.InitialFrameWaits);
    /// <summary>
    /// Cinematic updates elapsed, incremented after the cinematic phase runs and before the actors step. Only its phase
    /// within a four-update cycle is read, which a wider counter preserves exactly.
    /// </summary>
    private int cinematicFrameCounter;
    /// <summary>The route when the escape timer expires while Samus remains aboard Ceres.</summary>
    internal bool WithSamus { get; }

    /// <param name="audio">The shared music and sound queue, whose music the scene waits on.</param>
    /// <param name="glowColors">Installed Power Bomb glow colours for the station explosion.</param>
    /// <param name="artwork">Installed cinematic artwork.</param>
    /// <param name="spriteSheets">Installed sprite sheets the scene's actors draw from.</param>
    /// <param name="outcome">
    /// The Ceres-destruction state with Samus aboard: the escape timer expired before Samus left, so the
    /// gunship never appears and the fade-out ends the game instead of flying to Zebes.
    /// </param>
    /// <param name="japaneseText">The Japanese text option, which adds the PLANET ZEBES title's Japanese subtitle.</param>
    public CeresDestructionCinematicState(
        AudioQueues audio,
        InstalledPowerBombGlowColors glowColors,
        IntroCinematicArtwork artwork,
        InstalledSpriteSheets spriteSheets,
        CeresDestructionOutcome outcome,
        bool japaneseText)
    {
        this.japaneseText = japaneseText;
        BindSpriteSheets(Ensure.NotNull(spriteSheets));
        WithSamus = outcome == CeresDestructionOutcome.SamusAboard;
        this.audio = Ensure.NotNull(audio);
        Artwork = Ensure.NotNull(artwork);
        stationExplosion = new CeresStationBlast();
        this.glowColors = Ensure.NotNull(glowColors);
        screenSprites = new ScreenSprites();
        explosionSpawner = new(Offset16<ScreenPixels>.Of(0, 0), CeresShades.Drawn(CeresSpriteColouring.Explosions),
            CeresAnimations.ExplosionSpawner, CeresExplosionDefinitions.SpawnerActor.Animation);
    }

    /// <summary>
    /// True while the scene's opening step waits out frames of the display: the next update finishes that wait instead of
    /// starting a new update of the cinematic (<see cref="DisplayFrameWaitContinuation"/>). That is from the scene's first
    /// update, which takes its first wait, until the update that sets it up.
    /// </summary>
    internal bool IsWaitingOutDisplayFrame =>
        openingWait is { IsUncounted: false };

    /// <summary>Binds the installed station-blast colors when the game is bound to its installed content.</summary>
    internal void BindGlowColors(InstalledPowerBombGlowColors colors) => glowColors = Ensure.NotNull(colors);

    /// <summary>
    /// Binds installed artwork when the game is bound to its installed content. Only the
    /// graphics already reached by the current phase are refreshed; counters,
    /// actors, audio and gameplay timeline remain untouched. The backdrop is refreshed only
    /// before the later engine-flicker colour effect starts, as the scene reloads the
    /// flight's colours only until then (#1512).
    /// </summary>
    internal void BindArtwork(IntroCinematicArtwork value)
    {
        Artwork = Ensure.NotNull(value);
        if (CeresDestructionPhaseGroups.ThroughFlightToZebesStart.Contains(Phase))
            Backdrop = value.CeresFlight.Backdrop;
    }

    /// <summary>Current phase, including the alternate game-over route.</summary>
    public CeresDestructionPhase Phase { get; private set; }

    /// <summary>True after the completion star leaves the Zebes screen or the terminal route ends.</summary>
    public bool Finished => Phase == CeresDestructionPhase.Finished;

    /// <summary>Runs one update of the destruction cinematic.</summary>
    public void Step()
    {
        // The cinematic first waits out its opening waits (CeresDestructionSceneDefinitions.InitialFrameWaits) with the
        // screen black, each one update that continues it. After the last it sets up the scene and goes on with the cinematic
        // phase, whose objects and update count then advance once.
        if (openingWait is { } wait)
        {
            if (wait.IsRunning)
            {
                wait.Count();
                return;
            }
            openingWait = null;
            SetupCeresDestruction();
            // Setup queues the cinematic music and the state's own track last.
            audio.QueueMusic(MusicRequest.Stop);
            audio.QueueMusic(
                MusicRequest.LoadDataSet(MusicDataSet.Ceres));
            audio.ScheduleMusic(
                MusicRequest.SelectTrack(WithSamus
                    ? CeresDestructionSceneDefinitions.Music.CeresWithSamusTrack
                    : CeresDestructionSceneDefinitions.Music.CeresTrack),
                CeresDestructionSceneDefinitions.Music.QueueDelay);
        }
        else
        {
            StepPhase();
        }
        StepActorsColoursAndCount();
    }

    /// <summary>Runs one cinematic step after the opening waits and advances the station blast.</summary>
    private void StepPhase()
    {
        // The station blast steps before the cinematic's phase, so a blast the phase spawns does not advance until
        // the following update.
        stationExplosion.StepFrame(glowColors);
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
                if (fade.Brighten())
                    Phase = CeresDestructionPhase.ApproachExplosion;
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
                    // enters the flying-away phase on this same update.
                    Zoom = CeresDestructionSceneDefinitions.Motion.CeresExplosionZoom;
                    SpawnFinalCeresExplosion();
                    Phase = CeresDestructionPhase.FlyingAwayFromExplosion;
                }
                break;

            case CeresDestructionPhase.FlyingAwayFromExplosion:
                FloorScroll = FloorScroll.WithX(FloorScroll.X + CeresDestructionSceneDefinitions.Motion.FlyAwayScroll);
                angle = angle.TurnedBySteps(CeresDestructionSceneDefinitions.Motion.FlyAwayTurnSteps);
                if (Zoom < CeresDestructionSceneDefinitions.Motion.MinimumZoom)
                {
                    phaseHold = UpdateCountdown.Lasting(CeresDestructionSceneDefinitions.Timing.ExplosionHoldFrames);
                    Phase = CeresDestructionPhase.HoldAfterExplosion;
                }
                else
                {
                    Zoom = Zoom.Narrowed(CeresDestructionSceneDefinitions.Motion.SlowZoomStep);
                }
                break;

            case CeresDestructionPhase.HoldAfterExplosion:
                if (phaseHold.Step())
                {
                    fade.StepEvery(CeresDestructionSceneDefinitions.Timing.FadeUpdatesPerStep);
                    Phase = CeresDestructionPhase.FadeOutCeres;
                }
                break;

            case CeresDestructionPhase.FadeOutCeres:
                // Once the screen is black the next step is installed; it runs next update.
                // The game-over route instead stops the music and ends at the death black-out.
                if (fade.Darken())
                {
                    if (WithSamus)
                    {
                        audio.QueueMusic(MusicRequest.Stop);
                        Phase = CeresDestructionPhase.Finished;
                    }
                    else
                    {
                        Phase = CeresDestructionPhase.FlyToZebesInitial;
                    }
                }
                break;

            case CeresDestructionPhase.FadeInZebes:
                StepRevealPixelation();
                if (fade.Brighten())
                    Phase = CeresDestructionPhase.RemoveZebesPixelation;
                break;

            case CeresDestructionPhase.RemoveZebesPixelation:
                if (StepRevealPixelation())
                    SetupZebesFloorActors();
                break;

            case CeresDestructionPhase.PlanetZebesTitle:
                StepZebesActors(motion: ZebesActorsMotion.Holding);
                break;

            case CeresDestructionPhase.FlyingTowardZebes:
                FloorScroll = FloorScroll.WithY(FloorScroll.Y + CeresDestructionSceneDefinitions.Motion.EighthPixel);
                FloorScroll = FloorScroll.WithX(FloorScroll.X + CeresDestructionSceneDefinitions.Motion.NegativeHalfPixel);
                if (Zoom < CeresDestructionSceneDefinitions.Motion.ZebesApproachZoomLimit)
                    Zoom = Zoom.Widened(CeresDestructionSceneDefinitions.Motion.NearZebesZoomStep);
                else
                    Phase = CeresDestructionPhase.TurningTowardZebes;
                StepZebesActors(motion: ZebesActorsMotion.Holding);
                break;

            case CeresDestructionPhase.TurningTowardZebes:
                FloorScroll = FloorScroll.WithY(FloorScroll.Y + CeresDestructionSceneDefinitions.Motion.EighthPixel);
                FloorScroll = FloorScroll.WithX(FloorScroll.X + CeresDestructionSceneDefinitions.Motion.NegativeHalfPixel);
                if (FloorScroll.X.WholePixels < CeresDestructionSceneDefinitions.Places.TurnTowardZebesEndX)
                {
                    Phase = CeresDestructionPhase.ClosingInOnZebes;
                }
                else
                {
                    Zoom = Zoom.Widened(CeresDestructionSceneDefinitions.Motion.SlowZoomStep);
                    angle = angle.TurnedBySteps(CeresDestructionSceneDefinitions.Motion.TowardZebesTurnSteps);
                }
                StepZebesActors(motion: ZebesActorsMotion.Holding);
                break;

            case CeresDestructionPhase.ClosingInOnZebes:
                FloorScroll = FloorScroll.WithY(FloorScroll.Y + CeresDestructionSceneDefinitions.Motion.EighthPixel);
                FloorScroll = FloorScroll.WithX(FloorScroll.X + CeresDestructionSceneDefinitions.Motion.EighthPixel);
                if (Zoom < CeresDestructionSceneDefinitions.Motion.FarZebesZoomLimit)
                {
                    Zoom = Zoom.Widened(CeresDestructionSceneDefinitions.Motion.FastZoomStep);
                }
                else
                {
                    // The game hides the ship's turning floor before the hold, not
                    // when the later pan starts. The planet and star sprites stay shown.
                    View = CeresDestructionView.ZebesClose;
                    phaseHold = UpdateCountdown.Lasting(CeresDestructionSceneDefinitions.Timing.ZebesHoldFrames);
                    Phase = CeresDestructionPhase.HoldCloseZebes;
                }
                StepZebesActors(motion: ZebesActorsMotion.Holding);
                break;

            case CeresDestructionPhase.HoldCloseZebes:
                StepZebesActors(motion: ZebesActorsMotion.Holding);
                if (phaseHold.Step())
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

    /// <summary>The cinematic state's work after the cinematic phase returns.</summary>
    private void StepActorsColoursAndCount()
    {
        if (CeresDestructionPhaseGroups.CeresScene.Contains(Phase))
            StepCeresActors();

        // The cinematic's game state runs the colour effects after cinematic actors, every update from the setup on,
        // including the update which starts an effect. Their frames are shades the import pre-colours the scene's pictures
        // in (#1512); the scene names the frame each running effect shows (#1597). The scene binds no audio requests, so an
        // effect that made a sound would fail loudly (#2057).
        ColourEffects.StepScreen();

        // The cinematic's update count advances after its phase runs but before its actors. The pixelation's cadence
        // therefore reads this update's count on its following update.
        cinematicFrameCounter++;
    }

    /// <summary>Sets up the Ceres floor, sprites, colours, and three initial actors after the last opening wait.</summary>
    private void SetupCeresDestruction()
    {
        IntroCinematicArtwork content = Artwork;

        // The scene setup loads the station's two views; the floor shows the station's installed picture (#1467).
        View = CeresDestructionView.Station;
        Backdrop = content.CeresFlight.Backdrop;
        // The setup clears the colour effects and starts the station's navigation lights (#1733).
        ColourEffects = ColourEffectSystem.ForScreen();
        ColourEffects.StartScreen(CeresDestructionSceneDefinitions.NavigationLights);

        ceresActors.Clear();
        foreach (CeresDestructionActorRole role in CeresDestructionActorDefinitions.DestructionSpawnOrder)
        {
            // Each actor starts at its role's installed placement (#1596).
            CeresDestructionActorDefinition definition = CeresDestructionActors.InitialActor(role);
            ceresActors.Add(new CeresDestructionActor(role, definition,
                CreateActor(definition, content.CeresDestruction.DestructionActors[role].Place)));
        }
        Phase = CeresDestructionPhase.WaitForMusicQueue;
    }

    /// <summary>Stages the planet-reveal map and resets camera, angle, fade, and pixelation.</summary>
    private void SetupZebesReveal()
    {
        // The reveal starts on the planet-reveal page. It also stages the rear-gunship view, which becomes visible
        // only when the gunship step later selects the floor again.
        View = CeresDestructionView.ZebesReveal;
        // The reveal clears the colour effects, ending the station's navigation lights, and starts the engine's flicker.
        ColourEffects = ColourEffectSystem.ForScreen();
        ColourEffects.StartScreen(CeresDestructionSceneDefinitions.GunshipEngineFlicker);

        ceresActors.Clear();
        FloorScroll = FixedVector<FloorPixels>.Zero;
        angle = RotationAngle.Zero;
        Zoom = FloorZoom.OwnSize;
        fade.Show(0);
        fade.StepEvery(CeresDestructionSceneDefinitions.Timing.FadeUpdatesPerStep);
        revealBlockWidth = CeresDestructionSceneDefinitions.RevealPixelation.StartingBlockWidth;
    }

    /// <summary>Creates the planet, stars, and title sprites for the later turning-floor flight.</summary>
    private void SetupZebesFloorActors()
    {
        View = CeresDestructionView.ZebesApproach;
        FloorScroll = new(FloorScroll.X.WithWholePixels(CeresDestructionSceneDefinitions.Places.ZebesInitialFloorScrollX),
            FloorScroll.Y.WithWholePixels(CeresDestructionSceneDefinitions.Places.ZebesInitialFloorScrollY));
        angle = CeresDestructionSceneDefinitions.Motion.ApproachAngle;
        Zoom = FloorZoom.OwnSize;
        revealActors.Clear();
        foreach (ZebesRevealActorRole role in CeresDestructionActorDefinitions.RevealSpawnOrder)
            revealActors.Add(CreateZebesActor(role));
        Phase = CeresDestructionPhase.PlanetZebesTitle;
    }

    /// <summary>Drifts the Ceres camera by one sixteenth pixel down and one quarter pixel left.</summary>
    private void StepInitialDrift()
    {
        FloorScroll = FloorScroll.WithY(FloorScroll.Y + CeresDestructionSceneDefinitions.Motion.SixteenthPixel);
        FloorScroll = FloorScroll.WithX(FloorScroll.X + CeresDestructionSceneDefinitions.Motion.NegativeQuarterPixel);
    }


    /// <summary>
    /// Narrows the Zebes reveal's pixelation blocks by one pixel every fourth cinematic frame, from one-pixel blocks
    /// starting again from the coarsest (<see cref="revealBlockWidth"/>), and reports whether this step left the planet
    /// picture unpixelated.
    /// </summary>
    private bool StepRevealPixelation()
    {
        if (!CeresCinematicCadenceDefinitions.RevealPixelationStepEveryFourthUpdate.IsDue(cinematicFrameCounter))
            return false;
        revealBlockWidth = revealBlockWidth == 1
            ? CeresDestructionSceneDefinitions.RevealPixelation.CoarsestBlockWidth
            : revealBlockWidth - 1;
        return revealBlockWidth == 1;
    }

    /// <summary>Whether the music queue has sent every request the scene queued.</summary>
    private bool MusicQueueFinished() => !audio.HasQueuedMusic;

    /// <summary>Installs an authored sprite at <paramref name="start"/> with its colouring and animation.</summary>
    private static CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> CreateActor(
        CeresDestructionActorDefinition definition, Offset16<ScreenPixels> start)
    {
        var actor = new CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue>(
            start,
            CeresShades.Drawn(definition.Colouring),
            CeresAnimations.Scenes,
            definition.Animation)
        {
            DriftLoopWidth = definition.DriftsRoundLoop ? CeresDestructionSceneDefinitions.DriftLoopWidth : null,
        };
        return actor;
    }

    /// <summary>Creates the Zebes reveal actor that shows <paramref name="role"/> at its installed placement (#1596, #1733).</summary>
    private ZebesRevealActor CreateZebesActor(ZebesRevealActorRole role)
    {
        CeresDestructionActorDefinition definition = CeresDestructionActors.ZebesActor(role);
        return new ZebesRevealActor(role, definition, CreateActor(definition, Artwork.CeresDestruction.RevealActors[role].Place));
    }


    /// <summary>
    /// Runs the explosion spawner, whose cues spawn the station's waves of blasts, then advances the live actors over Ceres,
    /// from the last in the list to the first.
    /// </summary>
    private void StepCeresActors()
    {
        if (explosionSpawner.IsActive)
            explosionSpawner.Step(HandleExplosionSpawnerCue);

        for (int index = ceresActors.Count - 1; index >= 0; index--)
        {
            CeresSceneActor actor = ceresActors[index];
            actor.Move();
            actor.Sprite.Step();
            if (!actor.Sprite.IsActive)
                ceresActors.RemoveAt(index);
        }
    }

    /// <summary>Spawns the waves of blasts the explosion spawner's animation cues (#2489).</summary>
    private void HandleExplosionSpawnerCue(CeresExplosionSpawnerCue cue)
    {
        switch (cue)
        {
            case CeresExplosionSpawnerCue.SpawnInitialWave:
                foreach (CeresExplosionPlacement blast in CeresExplosionDefinitions.InitialWave)
                    SpawnCeresExplosion(CeresExplosionDefinitions.InitialActor, blast);
                return;

            case CeresExplosionSpawnerCue.SpawnRepeatingBlast:
                // Each play of the spawner's repeating blast spawns one at the next of their places.
                SpawnCeresExplosion(CeresExplosionDefinitions.RepeatingActor, new CeresExplosionPlacement(
                    repeatingBlastPlacesLeft.Dequeue(), CeresExplosionDefinitions.RepeatingBlastDelayFrames));
                return;

            case CeresExplosionSpawnerCue.SpawnFinalWave:
                foreach (CeresExplosionPlacement blast in CeresExplosionDefinitions.FinalWave)
                    SpawnCeresExplosion(CeresExplosionDefinitions.FinalWaveActor, blast);
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresExplosionSpawnerCue)} {cue}.");
        }
    }

    /// <summary>
    /// The final blast replaces the floor on the same update that creates the final explosion. With Samus gone
    /// the upper half becomes the gunship viewed from the front over a cleared lower half; with Samus still aboard no
    /// gunship leaves and both halves are cleared, so the floor shows nothing (#1513). No half keeps the station.
    /// </summary>
    private void LoadFinalExplosionMaps() =>
        View = WithSamus ? CeresDestructionView.StationCleared : CeresDestructionView.GunshipLeavingExplosion;

    /// <summary>Starts the station blast, queues its sound, adds its sprite, and replaces the station maps.</summary>
    private void SpawnFinalCeresExplosion()
    {
        // The final blast burns through the Power Bomb blast's stages before the station picture is replaced. It centres
        // on the station's floor centre as the moved floor shows it on the screen.
        int blastX = CeresDestructionSceneDefinitions.CeresFloorCenter.X - FloorScroll.X.WholePixels;
        int blastY = CeresDestructionSceneDefinitions.CeresFloorCenter.Y - FloorScroll.Y.WholePixels;
        stationExplosion.Spawn(Offset16<ScreenPixels>.Of(blastX, blastY));
        audio.QueueSound(SuperMetroid.Core.Audio.SamusSound.PowerBombExplosion,
            priority: SoundRequestPriority.Essential);
        ceresActors.Add(new CeresBlast(new CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue>(
            Offset16<ScreenPixels>.Of(blastX, blastY),
            CeresShades.Drawn(CeresSpriteColouring.Explosions),
            CeresAnimations.Scenes,
            CeresExplosionDefinitions.StationBlastActor.Animation)));

        LoadFinalExplosionMaps();
    }

    /// <summary>Creates a small blast at a camera-relative authored offset with its initial animation delay.</summary>
    private void SpawnCeresExplosion(
        CinematicActorDefinition<CeresAnimation> definition,
        CeresExplosionPlacement placement)
    {
        var actor = new CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue>(
            Offset16<ScreenPixels>.Of(
                CeresDestructionSceneDefinitions.CeresFloorCenter.X - FloorScroll.X.WholePixels + placement.Offset.X,
                CeresDestructionSceneDefinitions.CeresFloorCenter.Y - FloorScroll.Y.WholePixels + placement.Offset.Y),
            CeresShades.Drawn(CeresSpriteColouring.Explosions),
            CeresAnimations.Scenes,
            definition.Animation);

        // Each explosion holds its first frame for its own stagger, so the five blasts appear at their own delays.
        actor.DelayFirstStep(placement.DelayFrames);
        ceresActors.Add(new CeresBlast(actor));
    }

    /// <summary>Advances the planet and title objects, applying slide acceleration and finishing on star deletion.</summary>
    private void StepZebesActors(ZebesActorsMotion motion)
    {
        for (int index = revealActors.Count - 1; index >= 0; index--)
        {
            ZebesRevealActor actor = revealActors[index];
            if (motion == ZebesActorsMotion.SlidingAway)
            {
                // The reveal ends when the lower-right stars leave.
                actor.Slide();
                if (actor.Sprite.ScreenPlace.Y < CeresDestructionSceneDefinitions.Places.RevealActorExitY)
                {
                    // Each reveal object leaves before it slides off the top of the screen. Only the
                    // lower-right stars also hand the cinematic back to game load.
                    if (actor.Definition.CompletesScene)
                    {
                        Phase = CeresDestructionPhase.Finished;
                        return;
                    }

                    actor.Sprite.Delete();
                    revealActors.RemoveAt(index);
                    continue;
                }
            }

            actor.Sprite.Step(HandleZebesCue);
            if (!actor.Sprite.IsActive)
                revealActors.RemoveAt(index);
            // A subtitle the title has just spawned comes after the title in the cinematic's list of objects, so it runs on
            // the same update and shows its first pose.
            if (spawnedSubtitle is { } subtitle)
            {
                spawnedSubtitle = null;
                subtitle.Sprite.Step(HandleZebesCue);
                revealActors.Add(subtitle);
            }
        }
    }

    /// <summary>Lets the title actor start the camera flight at its authored cue boundary.</summary>
    private void HandleZebesCue(CeresCue cue)
    {
        switch (cue)
        {
            case CeresCue.FadeInZebesTitle:
                // The caption starts fading in from black as it first shows (#1733).
                ColourEffects.StartScreen(CeresDestructionSceneDefinitions.ZebesTextFadeIn);
                return;

            case CeresCue.FadeOutZebesTitle:
                // The caption fades out to black as its hold ends, and stays black (#1733).
                ColourEffects.StartScreen(CeresDestructionSceneDefinitions.ZebesTextFadeOut);
                return;

            case CeresCue.SpawnJapaneseTitleIfNeeded:
                // With Japanese text the title spawns its Japanese subtitle as its first hold ends (#1733).
                if (japaneseText)
                    spawnedSubtitle = CreateZebesActor(ZebesRevealActorRole.JapaneseSubtitle);
                return;

            case CeresCue.StartFlyingToZebes:
                // The title actor, not a separate timer, publishes the camera flight exactly
                // where its animation reaches the flight step.
                FloorScroll = new(FloorScroll.X.WithWholePixels(CeresDestructionSceneDefinitions.Motion.FloorExitX),
                    FloorScroll.Y.WithWholePixels(CeresDestructionSceneDefinitions.Places.CeresAndFlightFloorScrollY));
                angle = CeresDestructionSceneDefinitions.Motion.ApproachAngle;
                Zoom = CeresDestructionSceneDefinitions.Motion.MinimumZoom;
                Phase = CeresDestructionPhase.FlyingTowardZebes;
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresCue)} {cue}.");
        }
    }


    /// <summary>
    /// Captures the scene's ordered layers (#1514): over Ceres, the station's turning floor with the asteroids and blasts
    /// in front of it; on the approach to Zebes, the planet and its stars behind the gunship's floor; or the Zebes reveal's
    /// planet picture. Each picture and sprite is drawn whole, in its layer's order.
    /// </summary>
    /// <exception cref="InvalidOperationException">The view is undefined.</exception>
    public LayeredScene ComposeScene()
    {
        PrepareRenderSprites();
        var sprites = new SpritesLayer(screenSprites.Draws);
        DisplayLayer[] layers = View switch
        {
            // Before the station is set up, after the explosion with Samus aboard clears the floor, and once the gunship's
            // floor is hidden near Zebes, the scene shows its sprites alone.
            CeresDestructionView.BeforeStation or CeresDestructionView.StationCleared or CeresDestructionView.ZebesClose => [sprites],
            CeresDestructionView.Station => TurningFloorLayers(CeresFloorShot.Station, sprites),
            CeresDestructionView.GunshipLeavingExplosion => TurningFloorLayers(CeresFloorShot.Explosion, sprites),
            CeresDestructionView.ZebesApproach => TurningFloorLayers(CeresFloorShot.ZebesApproach, sprites),
            // The Zebes reveal shows the planet's installed picture (#1467), pixelated in the reveal's blocks (#1774); its
            // sprites are not pixelated.
            CeresDestructionView.ZebesReveal =>
            [
                new PicturedPlaneLayer(CeresCinematicPictures.Zebes(spriteSheets), Offset16<PlanePixels>.Zero)
                {
                    Pixelation = new BackgroundPixelation(revealBlockWidth),
                },
            ],
            _ => throw new InvalidOperationException($"Undefined {nameof(CeresDestructionView)} {View}."),
        };
        if (CaptureStationExplosion() is { } blast)
            layers = [.. layers, blast];
        return new(new FramePaint(Backdrop), layers, fade.Level);
    }

    /// <summary>
    /// The turning floor showing <paramref name="shot"/> with <paramref name="sprites"/>: over Ceres the asteroids and blasts
    /// in front of the floor, on the approach to Zebes the planet and its stars behind it.
    /// </summary>
    private DisplayLayer[] TurningFloorLayers(CeresFloorShot shot, SpritesLayer sprites)
    {
        bool overCeres = CeresDestructionPhaseGroups.CeresScene.Contains(Phase);
        FloorView floor = FloorView.Turned(angle, Zoom,
            overCeres ? CeresDestructionSceneDefinitions.CeresFloorCenter : CeresDestructionSceneDefinitions.ZebesFloorCenter,
            FloorScroll.Whole);
        var turning = new TurningFloorLayer(floor, CeresFloors.Of(spriteSheets, shot).Shaded(FloorShading));
        return overCeres ? [turning, sprites] : [sprites, turning];
    }

    /// <summary>
    /// The shading of the turning floor (#1512, #1544, #1597, #1733): the shade of the frame each of the floor's colour
    /// effects shows, the station's navigation lights over Ceres and the gunship engine's flicker on the approach to Zebes,
    /// or the floor's standard colours while no effect has shown one.
    /// </summary>
    internal PictureShading FloorShading
    {
        get
        {
            PictureShading shading = PictureShading.Standard;
            foreach (ColourEffectColours colours in CeresDestructionSceneDefinitions.FloorColours)
            {
                if (ColourEffects.FrameOf(colours) is { } frame)
                    shading = shading.With(ColourEffectShades.Of(colours, frame));
            }
            return shading;
        }
    }

    /// <summary>
    /// An actor's shading (#1733): <paramref name="drawn"/>, with the frame a colour effect animating its colouring last
    /// showed laid over it; a fade that has ended keeps showing its last frame.
    /// </summary>
    private PictureShading ActorShading(PictureShade drawn)
    {
        PictureShading shading = PictureShading.Shaded(drawn);
        if (CeresDestructionSceneDefinitions.FadedColours.Where(pair => CeresShades.Drawn(pair.Key) == drawn)
                .Select(pair => (ColourEffectColours?)pair.Value).SingleOrDefault() is { } colours &&
            ColourEffects.FrameOf(colours) is { } frame)
            shading = shading.With(ColourEffectShades.Of(colours, frame));
        return shading;
    }

    /// <summary>Captures the active station blast's added colour as a render layer, over the whole screen.</summary>
    private ColourAddWindowLayer? CaptureStationExplosion() => stationExplosion.Capture();

    /// <summary>
    /// Places live actors as sprite entries in the order they spawned, the Ceres scene's small asteroids and vortex after
    /// every other actor.
    /// </summary>
    private void PrepareRenderSprites()
    {
        screenSprites.BeginFrame();
        // Same-priority sprite overlap is decided by drawing order.
        IEnumerable<CeresSceneActor> overCeres = CeresDestructionPhaseGroups.CeresScene.Contains(Phase)
            ? ceresActors.Where(actor => !actor.DrawnLast).Concat(ceresActors.Where(actor => actor.DrawnLast)) : ceresActors;
        foreach (CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> actor in overCeres.Select(actor => actor.Sprite).Concat(revealActors.Select(actor => actor.Sprite)))
            actor.Draw(screenSprites, installedArt: Sprites);
        screenSprites.FinishFrame();
    }

}
