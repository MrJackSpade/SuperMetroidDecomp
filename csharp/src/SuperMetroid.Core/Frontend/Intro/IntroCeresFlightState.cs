using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;
using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Initial turning-floor leg of Samus's flight toward Ceres station.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class IntroCeresFlightState
{
    /// <summary>Holds the music queue whose drain gates the initial display.</summary>
    private readonly AudioQueues audio;
    /// <summary>The colour the flight shows behind every layer, the installed flight's backdrop (#1512).</summary>
    internal RgbColor Backdrop { get; private set; }
    /// <summary>The front-star or rear-view actors' sprite entries over the flight sprite page (#1358).</summary>
    internal readonly ScreenSprites screenSprites;
    /// <summary>The installed Ceres flight sheet the stars and rear-view actors draw from, rebound without restarting the scene.</summary>
    private SpriteSheet flightSprites;
    /// <summary>
    /// The flight's colour effects (#1733): the scene's setup clears them, and the rear view's setup starts the gunship
    /// engine's flicker and the station's navigation lights.
    /// </summary>
    private ColourEffectSystem ColourEffects { get; } = ColourEffectSystem.ForScreen();
    /// <summary>The rear view's actors' installed starting places (#1596).</summary>
    private CeresFlightActorPlacements actorLayout;
    /// <summary>Front-view star actor, whose rush also drives the diagonal background rush.</summary>
    private readonly CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> stars =
        CreateActor(CeresFlightActorDefinitions.FrontStars, CeresFlightActorDefinitions.FrontStarsStart);

    /// <summary>The front stars' speed on each axis, which the background rush shares.</summary>
    private SignedEightEight starsVelocity = CeresFlightActorDefinitions.FrontStars.InitialVelocity;

    /// <summary>The floor point the scene places at the screen's top-left, advanced with the star actor or rear-view drift.</summary>
    private FixedVector<FloorPixels> background = new(
        SignedSixteenSixteen.FromWholePixels(CeresFlightSceneDefinitions.FrontInitialFloorScrollX),
        SignedSixteenSixteen.FromWholePixels(CeresFlightSceneDefinitions.FrontInitialFloorScrollY));
    /// <summary>
    /// The floor's zoom: its front zoom at the start, narrowed by the slow step per update to its narrowest for the front
    /// approach, then widened by the slow step and then the fast step for the rear view, to its widest
    /// (<see cref="CeresFlightRenderDefinitions"/>).
    /// </summary>
    private FloorZoom zoom = CeresFlightRenderDefinitions.FrontInitialZoom;
    /// <summary>How far the scene has turned its floor clockwise.</summary>
    private RotationAngle angle = CeresFlightSceneDefinitions.FrontInitialAngle;
    /// <summary>
    /// The screen's brightness: held black until music is ready, then fully lit, and lowered a level at the fade-out's pace
    /// in the final fade, stopping at black.
    /// </summary>
    private readonly ScreenFade fade = new(0);
    /// <summary>The rear view's five station, asteroid, vortex and star actors, in spawn order, created when the camera switches to it.</summary>
    internal IReadOnlyList<CeresRearViewActor> RearViewActors { get; private set; } = [];
    /// <summary>
    /// The rear-view flash's white wash (<see cref="WhiteWash"/>): full white when the rear view starts, one step fewer each
    /// update, stopping at none; none before the rear view.
    /// </summary>
    private WhiteWash? rearViewFlash;
    /// <summary>
    /// The SPACE COLONY caption's typing (#2488), played by the shared player from the title's setup: its letters, with
    /// Japanese text its katakana caption, its hold and the fade-out cue; none before the title.
    /// </summary>
    private AnimationPlayer<SpaceColonyCaptionText, SpaceColonyCaptionTyping, SpaceColonyCaptionCue>? caption;
    /// <summary>How many of the caption's letters are shown, in order; none before the title.</summary>
    private int CaptionLettersShown => caption?.Shown?.Letters ?? 0;
    /// <summary>Whether the Japanese text's katakana caption has shown, from the update it shows until the flight ends.</summary>
    private bool JapaneseCaptionShown => caption?.Shown?.Shows == SpaceColonyCaptionText.WithKatakana;
    /// <summary>Whether the Japanese text is selected, which shows the katakana caption.</summary>
    private readonly bool japaneseText;

    /// <summary>Loads installed flight art and leaves the scene black for the setup update.</summary>
    /// <param name="audio">The shared music queue the approach waits on.</param>
    /// <param name="artwork">Installed cinematic artwork.</param>
    /// <param name="sheets">The installed sheets: the Ceres flight sheet the stars and rear-view actors draw from, and the caption's pictures.</param>
    /// <param name="japaneseText">The Japanese text option, which adds the katakana SPACE COLONY caption.</param>
    public IntroCeresFlightState(AudioQueues audio, CeresFlightArtwork artwork, InstalledSpriteSheets sheets,
        bool japaneseText)
    {
        this.japaneseText = japaneseText;
        this.audio = Ensure.NotNull(audio);
        BindSheets(sheets);
        CeresFlightArtwork content = Ensure.NotNull(artwork);
        actorLayout = content.Actors;

        // Setup loads the floor's front view and the actors' artwork; the scene shows the front view's installed floor (#1467).
        screenSprites = new ScreenSprites();
        Backdrop = content.Backdrop;
        // The loads above are invisible: the screen stays black until the music queue drains.
        // The display setup itself runs as the next update's cinematic phase.
        Phase = IntroCeresFlightPhase.Initial;
    }

    /// <summary>Rebinds the installed sheets: the Ceres flight sheet the stars and rear-view actors draw from, and the caption's pictures.</summary>
    [MemberNotNull(nameof(flightSprites), nameof(floorSheets))]
    public void BindSheets(InstalledSpriteSheets sheets)
    {
        floorSheets = Ensure.NotNull(sheets);
        flightSprites = sheets.Get(SpriteSheetId.CeresFlightSprites);
    }

    /// <summary>The installed sheets the floor's views and the caption are pictured in (#1467); artwork, never saved.</summary>
    private InstalledSpriteSheets floorSheets;

    /// <summary>Reapplies current external art and backdrop without resetting the live flight phase.</summary>
    public void BindArtwork(CeresFlightArtwork artwork)
    {
        actorLayout = Ensure.NotNull(artwork).Actors;
        Backdrop = artwork.Backdrop;
    }

    /// <summary>Current cinematic phase, including the final handoff to Ceres gameplay.</summary>
    private IntroCeresFlightPhase Phase { get; set; }

    /// <summary>True after the SPACE COLONY hold and final fade reach black.</summary>
    public bool Finished => Phase == IntroCeresFlightPhase.Finished;

    /// <summary>What <c>Step</c> does for each <see cref="IntroCeresFlightPhase"/> member, each member's own arm.</summary>
    private IdentityTable<IntroCeresFlightPhase, Action> StepForIntroCeresFlightPhase => field ??= new(new Dictionary<IntroCeresFlightPhase, Action>
    {
        [IntroCeresFlightPhase.Initial] = () =>
        {
            // The flight's music data, then its track, on the shared queue.
            audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.Ceres));
            audio.ScheduleMusic(
                MusicRequest.SelectTrack(CeresFlightSceneDefinitions.FlightSong),
                CeresFlightSceneDefinitions.MusicDelay);
            Phase = IntroCeresFlightPhase.WaitForMusicQueue;
        },
        [IntroCeresFlightPhase.WaitForMusicQueue] = () =>
        {
            // The approach enables the display once the music queue is empty.
            if (!audio.HasQueuedMusic)
            {
                fade.Show(DisplayBrightness.Full);
                Phase = IntroCeresFlightPhase.FlyingIntoCamera;
            }
        },
        [IntroCeresFlightPhase.FlyingIntoCamera] = () =>
        {
            if (zoom >= CeresFlightRenderDefinitions.FrontNarrowingFloor)
            {
                zoom = zoom.Narrowed(CeresFlightRenderDefinitions.SlowZoomStep);
            }
            else
            {
                SetupRearView();
            }
        },
        [IntroCeresFlightPhase.FlyingTowardCeres] = () => StepFlyingTowardCeres(),
        [IntroCeresFlightPhase.SpaceColonyTitle] = () => StepSpaceColonyTitle(),
        [IntroCeresFlightPhase.FadeOut] = () => StepFadeOut(),
        [IntroCeresFlightPhase.StartGameAtCeres] = () =>
        {
            // The handoff runs as the update after the screen goes black; it hands control to
            // the new-game setup state, whose owner also performs this phase's Ceres checkpoint save.
            Phase = IntroCeresFlightPhase.Finished;
        },
        [IntroCeresFlightPhase.Finished] = () =>
        {
            // The flight has ended; its owner moves to the new-game setup state.
        },
    });

    /// <summary>Advances the music gate, camera, rear-view actors, title cadence, or final fade for one update.</summary>
    public void Step()
    {
        StepForIntroCeresFlightPhase[Phase]();

        if (Phase == IntroCeresFlightPhase.FlyingIntoCamera)
            StepStarsAndBackground();

        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or
            IntroCeresFlightPhase.SpaceColonyTitle or
            IntroCeresFlightPhase.FadeOut)
            StepRearViewActors();
        else if (Phase is not (IntroCeresFlightPhase.StartGameAtCeres or IntroCeresFlightPhase.Finished))
            stars.Step();

        // The cinematic's game state runs the colour effects after its actors, including on the update that
        // starts one; their frames are shades the import pre-colours the flight's sheet in (#1733).
        ColourEffects.StepScreen();
    }

    /// <summary>
    /// Captures the flight's ordered layers without rasterizing (#1514): the scene's sprites, then the gunship's turning
    /// floor in front of them, or, once the caption is typed, the sprites with the caption over them; and the rear view's
    /// white flash over everything.
    /// </summary>
    public LayeredScene ComposeScene()
    {
        PrepareRenderSprites();
        // The stars, the station, its asteroids and the vortex lie beyond the gunship: they are drawn behind its floor,
        // each whole.
        var sprites = new SpritesLayer(screenSprites.Draws);
        var layers = new List<DisplayLayer> { sprites };
        if (Phase is IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut or
            IntroCeresFlightPhase.StartGameAtCeres or IntroCeresFlightPhase.Finished)
        {
            // The caption shows the pictures of the letters typed so far and, once drawn, the katakana caption (#1467),
            // over the station it names.
            layers.Add(new PicturedPlaneLayer(CeresCinematicPictures.Caption(
                floorSheets,
                CaptionLettersShown, JapaneseCaptionShown), Offset16<PlanePixels>.Zero));
        }
        else
        {
            layers.Add(new TurningFloorLayer(FloorView.Turned(angle, zoom,
                CeresFlightRenderDefinitions.FloorCenter, background.Whole),
                CeresFloors.Of(floorSheets,
                    ShowsRearView ? CeresFloorShot.FlightRear : CeresFloorShot.FlightFront).Shaded(FloorShading)));
        }
        if (Phase == IntroCeresFlightPhase.FlyingTowardCeres)
        {
            layers.Add((rearViewFlash ?? throw new InvalidOperationException("The rear view shows no flash before its setup.")).Layer);
        }
        return new(new FramePaint(Backdrop), layers.ToArray(), fade.Level);
    }

    /// <summary>
    /// The shading of the turning floor (#1733): the shade of the frame the gunship engine's flicker shows once the rear
    /// view has started it, or the floor's standard colours before; the floor fades with the display's brightness (#1512).
    /// </summary>
    private PictureShading FloorShading => ColourEffects.FrameOf(CeresFlightSceneDefinitions.EngineColours) is { } frame
        ? PictureShading.Standard.With(ColourEffectShades.Of(CeresFlightSceneDefinitions.EngineColours, frame))
        : PictureShading.Standard;

    /// <summary>Places the front stars or surviving rear-view actors as this frame's sprite entries.</summary>
    private void PrepareRenderSprites()
    {
        screenSprites.BeginFrame();
        var spriteArtwork = new CinematicSpriteSheet(flightSprites, CinematicSpriteFamily.CeresFlight, ActorShading);
        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut)
            foreach (CeresRearViewActor actor in RearViewActors)
                actor.Sprite.Draw(screenSprites, installedArt: spriteArtwork);
        else stars.Draw(screenSprites, installedArt: spriteArtwork);
        screenSprites.FinishFrame();
    }

    /// <summary>
    /// An actor's shading (#1733): <paramref name="drawn"/>, with the frame a colour effect animating its colouring last
    /// showed laid over it: the station's navigation lights over the station's colours.
    /// </summary>
    private PictureShading ActorShading(PictureShade drawn)
    {
        PictureShading shading = PictureShading.Shaded(drawn);
        if (CeresFlightSceneDefinitions.LitColours.Where(pair => CeresShades.Drawn(pair.Key) == drawn)
                .Select(pair => (ColourEffectColours?)pair.Value).SingleOrDefault() is { } colours &&
            ColourEffects.FrameOf(colours) is { } frame)
            shading = shading.With(ColourEffectShades.Of(colours, frame));
        return shading;
    }

    /// <summary>Whether the scene shows its rear view: once the rear-view map has been queued.</summary>
    private bool ShowsRearView => Phase is not (IntroCeresFlightPhase.Initial or IntroCeresFlightPhase.WaitForMusicQueue or
        IntroCeresFlightPhase.FlyingIntoCamera);

    /// <summary>Loads the rear map, places five actors, and starts the white flash.</summary>
    private void SetupRearView()
    {
        // The rear view's map loads; the rear view's phase selects its installed floor (#1467).
        background = new(SignedSixteenSixteen.FromWholePixels(CeresFlightSceneDefinitions.RearViewFloorScrollX),
            SignedSixteenSixteen.FromWholePixels(CeresFlightSceneDefinitions.RearViewFloorScrollY));
        angle = CeresFlightSceneDefinitions.RearViewAngle;

        // The actors spawn, update and draw in their spawn order. Their visual animations
        // remain authored scripts; the flight's definitions supply behaviour identity,
        // each actor's start and the physical motion applied by StepRearViewActors.
        // Each actor starts at its role's installed placement (#1596).
        CeresFlightActorPlacements layout = actorLayout;
        RearViewActors = [.. CeresFlightActorDefinitions.RearViewSpawnOrder
            .Select(role => new CeresRearViewActor(role, CreateActor(CeresFlightActors.RearViewActor(role), layout[role].Place)))];

        // The flash washes the rear view, sprites and backdrop toward white. It begins fully white and takes one step
        // off the wash per frame, from the next, until none is left.
        rearViewFlash = WhiteWash.FullWhite(CeresFlightSceneDefinitions.RearViewFlashUpdatesPerStep, CeresFlightSceneDefinitions.RearViewFlashFirstStepUpdate);
        Phase = IntroCeresFlightPhase.FlyingTowardCeres;
        // The rear view's setup starts the gunship engine's flicker, then the station's blinking navigation lights (#1733).
        ColourEffects.StartScreen(CeresFlightSceneDefinitions.GunshipEngineFlicker);
        ColourEffects.StartScreen(CeresFlightSceneDefinitions.NavigationLights);
    }

    /// <summary>Creates one cinematic actor at <paramref name="start"/> with its authored colouring and animation.</summary>
    private static CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> CreateActor(CeresFlightActorDefinition definition, Offset16<ScreenPixels> start)
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

    /// <summary>Fades the white flash and enlarges the rear-view turning image until the title begins.</summary>
    private void StepFlyingTowardCeres()
    {
        // The white flash fades by one step per frame, stopping at none.
        (rearViewFlash ?? throw new InvalidOperationException("The rear view flies with no flash set up.")).Fade();

        // The rear view drifts left by its drift and grows the rear-of-gunship turning
        // image in two bands: the slow step and then the fast step, to the rear view's widest zoom.
        background = background.WithX(background.X - CeresFlightRenderDefinitions.RearViewDrift);
        if (zoom < CeresFlightRenderDefinitions.RearSlowWideningLimit)
            zoom = zoom.Widened(CeresFlightRenderDefinitions.SlowZoomStep);
        else if (zoom < CeresFlightRenderDefinitions.RearFastWideningLimit)
            zoom = zoom.Widened(CeresFlightRenderDefinitions.FastZoomStep);
        else
            SetupSpaceColonyTitle();
    }

    /// <summary>Switches to the caption page and starts typing the caption, its first glyph shown at once.</summary>
    private void SetupSpaceColonyTitle()
    {
        // The title replaces the turning image with the caption page
        // and starts the caption's background text object. The rear-view cinematic actors are not
        // deleted, so Ceres and both asteroid layers remain behind the title text.
        // English text shows no katakana caption; Japanese text shows it for one frame before the hold.
        caption = AnimationPlayer<SpaceColonyCaptionText, SpaceColonyCaptionTyping, SpaceColonyCaptionCue>.StartShowing(
            SpaceColonyCaptionAnimations.Segments,
            SpaceColonyCaptionAnimations.Typing(japaneseText ? SpaceColonyCaptionText.WithKatakana : SpaceColonyCaptionText.English));
        Phase = IntroCeresFlightPhase.SpaceColonyTitle;
    }

    /// <summary>
    /// Types the title's letters at the title's typing pace, optionally adds katakana, then holds the completed caption
    /// before the fade step hands over to the ordinary fade owner.
    /// </summary>
    private void StepSpaceColonyTitle() =>
        (caption ?? throw new InvalidOperationException("The SPACE COLONY title runs with no caption set up.")).Step(CarryOut);

    /// <summary>Carries out <paramref name="cue"/> of the caption's typing.</summary>
    /// <exception cref="InvalidOperationException">The cue is not one the caption's typing raises.</exception>
    private void CarryOut(SpaceColonyCaptionCue cue)
    {
        if (cue != SpaceColonyCaptionCue.StartFadeOut)
            throw new InvalidOperationException($"Undefined SPACE COLONY caption cue {cue}.");
        fade.StepEvery(CeresFlightSceneDefinitions.FadeOutUpdatesPerStep);
        Phase = IntroCeresFlightPhase.FadeOut;
    }

    /// <summary>Lowers brightness every other update until the black screen permits the Ceres handoff.</summary>
    private void StepFadeOut()
    {
        // The fade-out removes a brightness level at its pace and ends with the screen black.
        if (fade.Darken())
            Phase = IntroCeresFlightPhase.StartGameAtCeres;
    }

    /// <summary>Moves each rear-view actor according to its defined motion before stepping its sprite animation.</summary>
    private void StepRearViewActors()
    {
        foreach ((CeresRearViewActorRole role, CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> actor) in RearViewActors)
        {
            CeresFlightActorDefinition definition = CeresFlightActors.RearViewActor(role);
            if (definition.DriftsRoundLoop)
                actor.DriftRoundLoop(definition.HorizontalDelta);
            else
                actor.Position = actor.Position.WithX(actor.Position.X + definition.HorizontalDelta);
            actor.Step();
        }
    }

    /// <summary>Accelerates the front stars and both turning-floor scroll offsets along the same diagonal.</summary>
    private void StepStarsAndBackground()
    {
        // The star field's speed grows by its acceleration each update, and that same growing distance moves the star
        // actor and both turning-floor offsets, so the ship's rush looks diagonal.
        // The rush lasts while the zoom falls to its narrowest, so the speed reverses and grows only a few pixels an update
        // the other way.
        starsVelocity += CeresFlightActorDefinitions.FrontStarAcceleration;
        SignedSixteenSixteen rush = starsVelocity.ToSixteenSixteenDelta();
        stars.Position = new(stars.Position.X + rush, stars.Position.Y + rush);
        background = new(background.X + rush, background.Y + rush);
    }

}
