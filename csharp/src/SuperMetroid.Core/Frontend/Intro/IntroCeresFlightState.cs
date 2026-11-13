using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Initial turning-floor leg of Samus's flight toward Ceres station.</summary>
internal sealed class IntroCeresFlightState
{
    /// <summary>Holds the music queue whose drain gates the initial display.</summary>
    internal readonly AudioQueues audio;
    /// <summary>The colour the flight shows behind every layer, the installed flight's backdrop (#1512).</summary>
    internal RgbColor Backdrop { get; private set; }
    /// <summary>The front-star or rear-view actors' sprite entries over the flight sprite page (#1358).</summary>
    internal readonly ScreenSprites objects;
    /// <summary>The installed flight's backdrop and placements the scene was bound with, rebound whenever the game is bound to its installed content.</summary>
    internal CeresFlightArtwork? Artwork { get; private set; }
    /// <summary>Installed sprite compositions, rebound without restarting the scene.</summary>
    private CinematicSpriteSheet? spriteArtwork;
    /// <summary>Optional edited rear-view actor positions.</summary>
    private CeresFlightActorPlacements? actorLayout;
    /// <summary>Front-view star actor, whose rush also drives the diagonal background rush.</summary>
    private readonly IntroDiscoverySprite stars =
        CreateActor(CeresFlightActorDefinitions.FrontStars, CeresFlightActorDefinitions.FrontStarsStart);

    /// <summary>The front stars' speed on each axis, which the background rush shares.</summary>
    private SignedEightEight starsVelocity = CeresFlightActorDefinitions.FrontStars.InitialVelocity;

    /// <summary>The floor point the scene places at the screen's top-left, advanced with the star actor or rear-view drift.</summary>
    private FixedPosition<FloorPixels> background = new(new(CoordinateAxis.Wrap(-72), 0), new(CoordinateAxis.Wrap(-104), 0));
    /// <summary>
    /// The floor's zoom: two floor pixels per screen pixel at the start, narrowed by the slow step per update while at
    /// least an eighth (ending at a sixteenth) for the front approach, then widened by the slow step below twelve and by
    /// the fast step below thirty-two for the rear view, ending at thirty-two (<see cref="CeresFlightRenderDefinitions"/>).
    /// </summary>
    private FloorZoom zoom = CeresFlightRenderDefinitions.FrontInitialZoom;
    /// <summary>How far the scene has turned its floor clockwise.</summary>
    private RotationAngle angle = RotationAngle.FromSteps(224);
    /// <summary>
    /// Master brightness from zero (black) to fifteen (full): held at zero until music is ready, set to fifteen, and
    /// lowered one level every other update in the final fade, stopping at zero.
    /// </summary>
    private int brightness;
    /// <summary>Five station and asteroid sprites created when the camera switches to rear view.</summary>
    internal IntroDiscoverySprite[] RearViewActors { get; private set; } = [];
    /// <summary>
    /// The steps left of the rear-view flash's white wash (<see cref="WhiteWash"/>): all of them when the rear view starts,
    /// one fewer each update, stopping at none.
    /// </summary>
    private int flashIntensity;
    /// <summary>How many of the caption's letters are shown, in order; the next letter typed is this one.</summary>
    internal int CaptionLettersShown { get; private set; }
    /// <summary>Sixteen-frame glyph cadence and final 128-frame caption hold.</summary>
    private int spaceColonyTimer;
    /// <summary>Distinguishes the completed caption's final hold from its glyph-writing phase.</summary>
    private bool spaceColonyHoldStarted;
    /// <summary>Whether the optional one-frame katakana caption record has drawn the katakana caption.</summary>
    internal bool JapaneseCaptionShown { get; private set; }
    /// <summary>Selects the game's alternate Japanese caption record.</summary>
    private readonly bool japaneseText;
    /// <summary>Gates one brightness decrement every other frame in the closing fade.</summary>
    private int fadeDelay;

    /// <summary>Loads installed flight art and leaves the scene black for the setup update.</summary>
    /// <param name="audio">The shared music queue the approach waits on.</param>
    /// <param name="artwork">Installed cinematic artwork; the approach cannot start without it.</param>
    /// <param name="japaneseText">The Japanese text option, which adds the katakana SPACE COLONY caption.</param>
    /// <param name="sheets">The installed sheets: the Ceres flight sheet the stars and rear-view actors draw from, and the caption's tiles; required.</param>
    public IntroCeresFlightState(AudioQueues audio,
        CeresFlightArtwork? artwork = null, bool japaneseText = false, InstalledSpriteSheets? sheets = null)
    {
        this.japaneseText = japaneseText;
        this.audio = Ensure.NotNull(audio);
        BindSheets(sheets ?? throw new InvalidOperationException("Ceres approach requires the installed sprite sheets."));
        actorLayout = artwork?.Actors;

        CeresFlightArtwork content = artwork ?? throw new InvalidOperationException(
            "Ceres approach requires installed cinematic artwork.");
        Artwork = content;

        // Setup loads the floor's front view and the actors' artwork; the scene shows the front view's installed floor (#1467).
        objects = new ScreenSprites();
        Backdrop = content.Backdrop;
        // The loads above are invisible: the screen stays black until the music queue drains.
        // The display setup itself runs as the next dispatch's cinematic function.
        Phase = IntroCeresFlightPhase.Initial;
    }

    /// <summary>Rebinds the installed sheets: the Ceres flight sheet the stars and rear-view actors draw from, and the caption's tiles.</summary>
    public void BindSheets(InstalledSpriteSheets? sheets)
    {
        spriteArtwork = sheets is null ? null : new CinematicSpriteSheet(sheets.Get(SpriteSheetId.CeresFlightSprites), CinematicSpriteFamily.CeresFlight);
        floorSheets = sheets;
    }

    /// <summary>The installed sheets the floor's views and the caption are pictured in (#1467); artwork, never saved.</summary>
    private InstalledSpriteSheets? floorSheets;

    /// <summary>Reapplies current external art and backdrop without resetting the live flight phase.</summary>
    public void BindArtwork(CeresFlightArtwork? artwork)
    {
        actorLayout = artwork?.Actors;
        Artwork = artwork;
        if (artwork is null) return;
        Backdrop = artwork.Backdrop;
    }

    /// <summary>Current cinematic dispatch phase, including the final handoff to Ceres gameplay.</summary>
    public IntroCeresFlightPhase Phase { get; private set; }

    /// <summary>True after the SPACE COLONY hold and final fade reach black.</summary>
    public bool Finished => Phase == IntroCeresFlightPhase.Finished;

    /// <summary>Advances the music gate, camera, rear-view actors, title cadence, or final fade for one dispatch.</summary>
    public void Step()
    {
        switch (Phase)
        {
            case IntroCeresFlightPhase.Initial:
                // The flight's music data, then its track, on the shared queue.
                audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.Ceres));
                audio.ScheduleMusic(
                    MusicRequest.SelectTrack(CeresFlightSceneDefinitions.FlightSong),
                    MusicQueueDelay.AtLeastShortest(CeresFlightSceneDefinitions.MusicDelay));
                Phase = IntroCeresFlightPhase.WaitForMusicQueue;
                break;

            case IntroCeresFlightPhase.WaitForMusicQueue:
                // The approach enables the display once the music queue is empty.
                if (!audio.HasQueuedMusic)
                {
                    brightness = 15;
                    Phase = IntroCeresFlightPhase.FlyingIntoCamera;
                }
                break;

            case IntroCeresFlightPhase.FlyingIntoCamera:
                if (zoom >= CeresFlightRenderDefinitions.FrontNarrowingFloor)
                {
                    zoom = zoom.Narrowed(CeresFlightRenderDefinitions.SlowZoomStep);
                }
                else
                {
                    SetupRearView();
                }
                break;

            case IntroCeresFlightPhase.FlyingTowardCeres:
                StepFlyingTowardCeres();
                break;

            case IntroCeresFlightPhase.SpaceColonyTitle:
                StepSpaceColonyTitle();
                break;

            case IntroCeresFlightPhase.FadeOut:
                StepFadeOut();
                break;

            case IntroCeresFlightPhase.StartGameAtCeres:
                // The handoff runs as the update after the screen goes black; it hands control to state
                // the new-game setup state, whose owner also performs this function's area-six checkpoint save.
                Phase = IntroCeresFlightPhase.Finished;
                break;

            case IntroCeresFlightPhase.Finished:
                // The flight has ended; its owner moves to the new-game setup state.
                break;

            default:
                throw new InvalidOperationException($"Undefined Ceres flight phase {Phase}.");
        }

        if (Phase == IntroCeresFlightPhase.FlyingIntoCamera)
            StepStarsAndBackground();

        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or
            IntroCeresFlightPhase.SpaceColonyTitle or
            IntroCeresFlightPhase.FadeOut)
            StepRearViewActors();
        else if (Phase is not (IntroCeresFlightPhase.StartGameAtCeres or IntroCeresFlightPhase.Finished))
            stars.Step();
    }

    /// <summary>
    /// Captures the flight's ordered layers without rasterizing (#1514): the scene's sprites, then the gunship's turning
    /// floor in front of them, or, once the caption is typed, the sprites with the caption over them; and the rear view's
    /// white flash over everything.
    /// </summary>
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        // The stars, the station, its asteroids and the vortex lie beyond the gunship: they are drawn behind its floor.
        // The per-sprite-part priority tiers that once interleaved them with the floor are a presentation quirk the port
        // does not keep.
        var sprites = new SpritesLayer(objects.Draws);
        var layers = new List<DisplayLayer> { sprites };
        if (Phase is IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut or
            IntroCeresFlightPhase.StartGameAtCeres or IntroCeresFlightPhase.Finished)
        {
            // The caption shows the pictures of the letters typed so far and, once drawn, the katakana caption (#1467),
            // over the station it names.
            layers.Add(new PicturedPlaneLayer(CeresCinematicPictures.Caption(
                floorSheets ?? throw new InvalidOperationException("The Ceres flight's caption requires the installed sprite sheets."),
                CaptionLettersShown, JapaneseCaptionShown), Offset16<PlanePixels>.Zero));
        }
        else
        {
            layers.Add(new TurningFloorLayer(FloorView.Turned(angle, zoom,
                CeresFlightRenderDefinitions.FloorCenter, background.Whole.AsSignedOffset()),
                CeresFloors.Of(floorSheets ?? throw new InvalidOperationException("The Ceres flight's floor requires the installed sprite sheets."),
                    ShowsRearView ? CeresFloorShot.FlightRear : CeresFloorShot.FlightFront)
                    // The flight shows no colour effect on its floor; it fades with the display's brightness (#1512).
                    .Shaded(PictureShading.Standard)));
        }
        if (Phase == IntroCeresFlightPhase.FlyingTowardCeres)
        {
            layers.Add(WhiteWash.Layer(flashIntensity));
        }
        return new(new FramePaint(Backdrop), layers.ToArray(), brightness);
    }

    /// <summary>Places the front stars or surviving rear-view actors as this frame's sprite entries.</summary>
    private void PrepareRenderSprites()
    {
        objects.BeginFrame();
        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut)
            foreach (IntroDiscoverySprite actor in RearViewActors)
                actor.Draw(objects, installedArt: spriteArtwork);
        else stars.Draw(objects, installedArt: spriteArtwork);
        objects.FinishFrame();
    }

    /// <summary>Whether the scene shows its rear view: once the rear-view map has been queued.</summary>
    internal bool ShowsRearView => Phase is not (IntroCeresFlightPhase.Initial or IntroCeresFlightPhase.WaitForMusicQueue or
        IntroCeresFlightPhase.FlyingIntoCamera);

    /// <summary>Loads the rear map, places five actors, and primes the white fixed-color flash.</summary>
    private void SetupRearView()
    {
        // The rear view's map loads; the rear view's phase selects its installed floor (#1467).
        background = new(new(CoordinateAxis.Wrap(-32), 0), new(CoordinateAxis.Wrap(-128), 0));
        angle = RotationAngle.FromSteps(32);

        // These five slots are the exact spawn order. Their visual animations
        // remain authored scripts; the compiled definitions supply callback identity,
        // initializer results and the physical motion applied by StepRearViewActors.
        // Each actor starts at its installed placement (#1596).
        CeresFlightActorPlacements layout = actorLayout ?? throw new InvalidOperationException(
            "The Ceres approach's rear view requires the installed actor placements.");
        RearViewActors = Enumerable.Range(0, CeresFlightActorDefinitions.RearViewActorCount)
            .Select(index => CreateActor(CeresFlightActors.RearViewActor(index), layout[index].Position))
            .ToArray();

        // The flash adds the fixed colour to the rear view, sprites, and backdrop. It begins at
        // white and takes one step off the wash per frame until black.
        flashIntensity = WhiteWash.Steps;
        Phase = IntroCeresFlightPhase.FlyingTowardCeres;
    }

    /// <summary>Creates one cinematic actor at <paramref name="start"/> with its authored timing and active frame behaviour.</summary>
    private static IntroDiscoverySprite CreateActor(CeresFlightActorDefinition definition, Position16<ScreenPixels> start)
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

    /// <summary>Fades fixed color and enlarges the rear-view turning image until the title begins.</summary>
    private void StepFlyingTowardCeres()
    {
        // The fixed-colour flash fades by one component step per frame, clamped at zero.
        flashIntensity = Math.Max(0, flashIntensity - 1);

        // The rear view drifts left by an eighth of a pixel and grows the rear-of-gunship turning
        // image in two bands: the slow step until twelve floor pixels per screen pixel, then the fast step until thirty-two.
        background = background.WithX(background.X.MovedBy(-SignedSixteenSixteen.FromSixteenthsOfPixel(2)));
        if (zoom < CeresFlightRenderDefinitions.RearSlowWideningLimit)
            zoom = zoom.Widened(CeresFlightRenderDefinitions.SlowZoomStep);
        else if (zoom < CeresFlightRenderDefinitions.RearFastWideningLimit)
            zoom = zoom.Widened(CeresFlightRenderDefinitions.FastZoomStep);
        else
            SetupSpaceColonyTitle();
    }

    /// <summary>Switches to the caption page and writes its first glyph.</summary>
    private void SetupSpaceColonyTitle()
    {
        // The title replaces the turning image with the caption page
        // and starts the caption's background text object. The rear-view cinematic actors are not
        // deleted, so Ceres and both asteroid layers remain behind the title text.
        CaptionLettersShown = 0;
        spaceColonyHoldStarted = false;
        JapaneseCaptionShown = false;
        CaptionLettersShown++;
        spaceColonyTimer = 16;
        Phase = IntroCeresFlightPhase.SpaceColonyTitle;
    }

    /// <summary>Writes title letters every sixteen frames, optionally adds katakana, then holds before fading.</summary>
    private void StepSpaceColonyTitle()
    {
        if (--spaceColonyTimer > 0)
            return;

        if (CaptionLettersShown < SpaceColonyCaptionDefinitions.LetterCount)
        {
            CaptionLettersShown++;
            spaceColonyTimer = 16;
            return;
        }

        // The caption skips the Japanese caption record for English text. Japanese text draws
        // it on a one-frame record before the hold.
        if (japaneseText && !JapaneseCaptionShown)
        {
            JapaneseCaptionShown = true;
            spaceColonyTimer = 1;
            return;
        }

        // The list's final record holds the completed caption for 128 frames before
        // the fade step installs the ordinary fade owner.
        if (!spaceColonyHoldStarted)
        {
            spaceColonyHoldStarted = true;
            spaceColonyTimer = 128;
            return;
        }

        fadeDelay = 1;
        Phase = IntroCeresFlightPhase.FadeOut;
    }

    /// <summary>Lowers brightness every other dispatch until the black screen permits the Ceres handoff.</summary>
    private void StepFadeOut()
    {
        // The fade step seeds both fade delays with one. The fade-out therefore removes one brightness level every
        // other frame and ends with the screen blanked.
        if (fadeDelay-- > 0)
            return;
        fadeDelay = 1;
        brightness = Math.Max(0, brightness - 1);
        if (brightness == 0)
            Phase = IntroCeresFlightPhase.StartGameAtCeres;
    }

    /// <summary>Moves each rear-view actor according to its compiled motion before stepping its sprite list.</summary>
    private void StepRearViewActors()
    {
        for (int index = 0; index < RearViewActors.Length; index++)
        {
            IntroDiscoverySprite actor = RearViewActors[index];
            CeresFlightActorDefinition definition =
                CeresFlightActors.RearViewActor(index);
            if (actor.FrameBehavior != definition.ActiveFrameBehavior)
            {
                throw new InvalidDataException(
                    $"Ceres flight actor {index} names invalid frame behaviour " +
                    $"{actor.FrameBehavior}.");
            }
            if (definition.WrapX)
                AddWrappedX(actor, definition.HorizontalDelta);
            else
                AddSignedX(actor, definition.HorizontalDelta);
            actor.Step();
        }
    }

    /// <summary>Adds a fractional X delta and wraps the actor within the 512-pixel span.</summary>
    private static void AddWrappedX(IntroDiscoverySprite actor, SignedSixteenSixteen horizontalDelta)
    {
        // The wrapping star field adds only the delta's fraction, with nothing in the whole pixel: a leftward delta,
        // whose fraction is the rest of a pixel, carries the actor right instead.
        FixedCoordinate x = IntroCinematicMotion.AddFraction(actor.Position.X, horizontalDelta);
        actor.Position = actor.Position.WithX(x.WithWhole(CoordinateAxis.Wrap(x.Whole % CeresDestructionSceneDefinitions.WorldWidth)));
    }

    /// <summary>Applies signed horizontal motion without the front-star wrap.</summary>
    private static void AddSignedX(IntroDiscoverySprite actor, SignedSixteenSixteen delta)
    {
        actor.Position = actor.Position.WithX(actor.Position.X.MovedBy(delta));
    }

    /// <summary>Accelerates the front stars and both turning-floor scroll offsets along the same diagonal.</summary>
    private void StepStarsAndBackground()
    {
        // The star field adds 128/256 to a signed 8.8 speed, then applies that same growing delta to
        // the star actor and both turning-floor offsets. This is why the ship rush feels diagonal.
        if (stars.FrameBehavior !=
            CeresFlightActorDefinitions.FrontStars.ActiveFrameBehavior)
        {
            throw new InvalidDataException(
                $"Ceres front stars name invalid frame behaviour " +
                $"{stars.FrameBehavior}.");
        }
        // The rush lasts while the zoom falls from two floor pixels by a sixteenth an update, about 32 updates, so the speed rises from four
        // pixels an update one way to at most twelve the other.
        starsVelocity += CeresFlightActorDefinitions.FrontStarAcceleration;
        SignedEightEight velocity = starsVelocity;
        stars.Position = new(
            IntroCinematicMotion.AddEightEight(stars.Position.X, velocity),
            IntroCinematicMotion.AddEightEight(stars.Position.Y, velocity));
        background = new(
            IntroCinematicMotion.AddEightEight(background.X, velocity),
            IntroCinematicMotion.AddEightEight(background.Y, velocity));
    }

}
