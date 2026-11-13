using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's turning-floor getaway and the station's self-destruct countdown that follows.</summary>
[RoomLifetime]
internal sealed class CeresRidleyGetaway
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues Ceres battle and getaway sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Writes Ceres target colors and supplemental composite drawing.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Publishes Ceres escape and self-destruct room status.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;
    /// <summary>The session's unauthored Speed Booster cadence, whose delays the wing frames leave behind.</summary>
    private readonly UnauthoredBoostCadence _unauthoredBoostCadence;



    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyGetaway(CeresRidleyGetawayDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _roomStatus = dependencies.RoomStatus;
        _unauthoredBoostCadence = Ensure.NotNull(dependencies.UnauthoredBoostCadence);
    }

    /// <summary>
    /// The chamber's walls the getaway scene shows around its turning floor (#1545): the Ceres door's pictures of the
    /// chamber's left and right walls, standing at their room points in the doors' escaping colours, behind the floor
    /// and the actors.
    /// </summary>
    internal ImmutableArray<PerspectiveFloorScenery> ChamberWalls() =>
    [
        ChamberWall(EnemyFrameId.CeresDoorTurningFloorLeftWall, CeresGetawayChamberWallDefinitions.LeftWall),
        ChamberWall(EnemyFrameId.CeresDoorTurningFloorRightWall, CeresGetawayChamberWallDefinitions.RightWall),
    ];

    /// <summary>The chamber wall <paramref name="frame"/> pictures, standing at <paramref name="point"/>.</summary>
    private PerspectiveFloorScenery ChamberWall(EnemyFrameId frame, Position16<RoomPixels> point) =>
        new(_presentation.SheetPicture(EnemyDefinitionId.CeresDoor, frame), point, CeresDoorShades.InEscapingColours);

    /// <summary>Sets up the getaway floor: its angle, scroll, zoom and centre before the first getaway-table entry.</summary>
    internal static void SetupCeresRidleyGetawayFloor(RidleyEnemyState state)
    {
        state.GetawayActive = true;
        state.GetawayFinished = false;
        state.GetawayFrameIndex = 0;
        state.FloorAngle = RotationAngle.FromSteps(0, 1);
        state.FloorOffset = new Offset16<FloorPixels>(-128, 32);
        state.FloorZoom = 2048;
        state.FloorCenter = new Position16<FloorPixels>(64, 64);
        state.GetawayBabyFrame = 0;
        state.GetawayWingFrame = 0;
    }

    /// <summary>
    /// Runs the room's Ceres Ridley getaway cutscene while it is active.
    /// It runs after the
    /// frame's enemy AI and Samus processing, so the getaway's first step shares the frame
    /// in which Ridley's escape handoff publishes Ceres status one.
    /// </summary>
    public void RunCeresRidleyGetawayRoomMain(SamusState? samus, int frameCounter)
    {
        if (_ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyEscaped)
            return;
        RidleyEnemyState state = _ridleyEncounter.Ridley ?? throw new InvalidOperationException(
            "Ceres status one requires the Ceres Ridley getaway state.");
        if (state.GetawayFinished)
            return;
        TickCeresRidleyGetawayFloor(state, samus, frameCounter);
    }

    /// <summary>
    /// Runs the getaway room main until its motion ends. This routine
    /// owns presentation only: ordinary Ridley has already been hidden, while Samus remains
    /// in room-world coordinates exactly as she does during the boss-room getaway.
    /// </summary>
    internal void TickCeresRidleyGetawayFloor(
        RidleyEnemyState state,
        SamusState? samus,
        int frameCounter)
    {
        int frameIndex = state.GetawayFrameIndex;
        state.GetawayFrameIndex = frameIndex + 1;

        // The getaway requests the departure sound on its first frame only. Tying the request to the
        // frame preserves the same one-shot behavior after saves/replays and pauses.
        if (frameIndex == 0)
            _audio.QueueEnemySound(EnemySound.CeresRidleyGetaway, priority: SoundRequestPriority.Normal);

        // At frame 104, Samus's grab handler replaces Samus's movement/hack handlers so the
        // rotating boss image cannot overlap her from the following frame onward.
        if (frameIndex == 104 && samus is not null)
            samus.CeresRidleyEjection.Request(samus);

        var frame = CeresRidleyGetawayCurve.FromIndex(frameIndex);
        int zoom = frame.Zoom;
        if (frame.IsFinished)
        {
            // The getaway's end restores the ordinary view and clears all rotation and scroll values.
            // Preserve that clean handoff explicitly so the renderer cannot accidentally
            // keep sampling the rotating floor for the self-destruct text scene.
            state.GetawayFinished = true;
            state.GetawayActive = false;
            state.FloorOffset = Offset16<FloorPixels>.Zero;
            state.FloorCenter = Position16<FloorPixels>.Zero;
            // The getaway's end hands the same extended actor workspace to the escape-text sequence. Samus's
            // special push/fall handler remains responsible for restoring normal input;
            // ending the rotating view itself does not unlock her in the original game.
            state.Function = RidleyAiFunction.CeresActivateSelfDestruct;
            state.CeresSelfDestructPhase = CeresSelfDestructPhase.Start;
            return;
        }

        state.FloorZoom = zoom;
        // The getaway frame's velocity is added to the vertical word and subtracted from the
        // horizontal word.
        state.FloorOffset = Offset16<FloorPixels>.Wrapped(
            state.FloorOffset.X - frame.Velocity.X,
            state.FloorOffset.Y + frame.Velocity.Y);

        UpdateCeresRidleyFloorZoomColours(state, zoom);
        state.FloorAngle = state.FloorAngle.AddRaw(48);

        // The Baby capsule advances on every fourth frame of the frame counter using 0,1,2,1.
        if (RidleyCadenceDefinitions.GetawayCapsuleStepEveryFourthUpdate.IsDue(frameCounter))
        {
            state.GetawayBabyFrame = (state.GetawayBabyFrame + 1) % 4;
            _room.Services.ShowFloorFrame(CeresFloorAnimation.BabyFrameForPhase(state.GetawayBabyFrame));
        }

        // The wings beat on every eighth frame of the frame counter. Each wing frame leaves behind the delays
        // Samus's unauthored Speed Booster cadence runs on from then on.
        if (RidleyCadenceDefinitions.GetawayWingStepEveryEighthUpdate.IsDue(frameCounter))
        {
            state.GetawayWingFrame = (state.GetawayWingFrame + 1) % 2;
            CeresFloorAnimationFrame wing = state.GetawayWingFrame == 0
                ? CeresFloorAnimationFrame.WingFrame0
                : CeresFloorAnimationFrame.WingFrame1;
            _room.Services.ShowFloorFrame(wing);
            CeresRidleyGetawayBoostDelays delays = _bossArtwork.CeresRidleyGetawayBoostDelays ?? throw new InvalidOperationException(
                "The Ceres Ridley getaway requires the installed delays its wing frames leave behind.");
            _unauthoredBoostCadence.TakeDelaysLeftByWings(delays.LeftBy(wing));
        }
    }

    /// <summary>
    /// How the perspective floor appears as the getaway last turned, zoomed and moved it, before room shake, or null while
    /// the getaway does not show the floor: turned by the getaway's angle at its zoom about the floor's centre, and
    /// scrolled by the getaway's offset across the floor.
    /// </summary>
    internal static FloorView? FloorTransform(RidleyEnemyState? ridley) => ridley is { GetawayActive: true } getaway
        ? FloorView.Turned(getaway.FloorAngle, getaway.FloorZoom, getaway.FloorCenter.AsSignedOffset(), getaway.FloorOffset)
        : null;

    /// <summary>Selects the zoom-dependent fifteen-color Ceres getaway palette without replacing transparent color zero.</summary>
    /// <param name="state">The getaway's Ridley, whose room colours show the floor.</param>
    /// <param name="zoom">The floor's zoom, in 256ths of a floor pixel per screen pixel.</param>
    internal static void UpdateCeresRidleyFloorZoomColours(RidleyEnemyState state, int zoom)
    {
        // The floor's shade is chosen by the zoom's whole floor pixels per screen pixel.
        // There are fifteen colors; palette entry zero remains the shared transparent color.
        int zoomStep = UnsignedEightEight.Whole(zoom);
        if (zoomStep < CeresRidleyPaletteDefinitions.GetawayZoomStepCount)
        {
            Ensure.NotNull(state).Colours.ShowFloorZoom(zoomStep);
            return;
        }
        throw new InvalidOperationException(
            $"Ceres Ridley turning-floor zoom step {zoomStep} is not in the installed palette catalog.");
    }

    /// <summary>
    /// Runs the shared Ridley function for the Ceres branch: the warning runs through its phases
    /// (<see cref="CeresSelfDestructPhase"/>) in order. English skips
    /// only the Japanese subtitle phase and types the game's three-line English warning. Japanese runs the
    /// subtitle phase first, shows its subtitle, and then uses the same typewriter.
    /// </summary>
    internal void TickCeresRidleySelfDestruct(RidleyEnemyState state)
    {
        switch (state.CeresSelfDestructPhase)
        {
            case CeresSelfDestructPhase.Start:
                state.CeresEscapeReadying = CeresEscapeReadying.Warning;
                state.CeresSelfDestructPhase = CeresSelfDestructPhase.ReadyingWarning;
                goto case CeresSelfDestructPhase.ReadyingWarning;

            case CeresSelfDestructPhase.ReadyingWarning:
                if (ReadyCeresEscapeWarning(state))
                {
                    state.CeresEscapeReadying = CeresEscapeReadying.Backgrounds;
                    state.CeresSelfDestructPhase = CeresSelfDestructPhase.ReadyingBackgrounds;
                    goto case CeresSelfDestructPhase.ReadyingBackgrounds;
                }
                return;

            case CeresSelfDestructPhase.ReadyingBackgrounds:
                if (ReadyCeresEscapeWarning(state))
                {
                    // The warning setup draws the emergency text immediately after the final
                    // graphics record. The call was missing from the earlier
                    // translation, so the letters' artwork was ready but the text never
                    // appeared on screen.
                    DrawCeresEmergencyText();
                    // The warning setup also queues the set's third song after showing the final
                    // static warning picture. The frontend consumes this typed request
                    // through the same queue used by every other music producer.
                    _audio.RequestRidleyMusic(MusicRequest.SelectTrack(MusicTrack.ThirdSetSong));
                    state.CeresSelfDestructPhase = CeresSelfDestructPhase.HoldingTitle;
                    state.CeresEscapeTextDelayTimer = 128;
                }
                return;

            case CeresSelfDestructPhase.HoldingTitle:
                UpdateCeresSelfDestructAlarm(state, _ridleyEncounter.Parts.Body.FrameCounter);
                if (--state.CeresEscapeTextDelayTimer != 0)
                    return;

                state.CeresEscapeTypewriter = _bossArtwork.EscapeTypewriterPresentation is { } presentation
                    ? new(presentation.Get(EscapeTypewriterProgramId.Ceres))
                    : null;

                // English goes straight to typing the warning; Japanese
                // first runs the subtitle phase. An earlier translation skipped the typing
                // phase for English, suppressing every character below "EMERGENCY".
                state.CeresSelfDestructPhase = _roomStatus.JapaneseText
                    ? CeresSelfDestructPhase.JapaneseSubtitle
                    : CeresSelfDestructPhase.TypingWarning;
                return;

            case CeresSelfDestructPhase.JapaneseSubtitle:
                UpdateCeresSelfDestructAlarm(state, _ridleyEncounter.Parts.Body.FrameCounter);
                if (--state.CeresEscapeTextDelayTimer == 0)
                {
                    state.CeresSelfDestructPhase = CeresSelfDestructPhase.TypingWarning;
                    DrawCeresJapaneseOverlay();
                }
                // The typewriter finishing moves the warning one phase on from wherever the delay left it: the
                // subtitle phase to typing, or typing to finishing.
                if (StepCeresEscapeTypewriter(state))
                {
                    state.CeresSelfDestructPhase = state.CeresSelfDestructPhase == CeresSelfDestructPhase.JapaneseSubtitle
                        ? CeresSelfDestructPhase.TypingWarning
                        : CeresSelfDestructPhase.Finishing;
                }
                return;

            case CeresSelfDestructPhase.TypingWarning:
                UpdateCeresSelfDestructAlarm(state, _ridleyEncounter.Parts.Body.FrameCounter);
                if (StepCeresEscapeTypewriter(state))
                    state.CeresSelfDestructPhase = CeresSelfDestructPhase.Finishing;
                return;

            case CeresSelfDestructPhase.Finishing:
                UpdateCeresSelfDestructAlarm(state, _ridleyEncounter.Parts.Body.FrameCounter);
                state.Velocity = new EightEightVector<RoomPixels>(0, 0);
                state.Function = RidleyAiFunction.CeresSelfDestructPaletteOnly;
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.SelfDestructStarted;
                // The escape-text sequence sets the area-boss bit inside Ridley's AI, so later slots (the
                // room's exit door) observe it in this same enemy update.
                _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
                _frameResults.CeresEscapeStarted = true;
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresSelfDestructPhase)} {state.CeresSelfDestructPhase}.");
        }
    }

    /// <summary>Shows the warning's EMERGENCY title over the room's foreground.</summary>
    internal void DrawCeresEmergencyText()
    {
        DrawCeresOverlay(CeresEscapeWarningPictures.Emergency);
    }

    /// <summary>
    /// Writes one warning picture over the room's foreground with its top-left corner at its pixel of the foreground
    /// (#1466, #1555, #1601).
    /// </summary>
    private void DrawCeresOverlay(CeresEscapeWarningPicture page) =>
        _ridleyEncounter.ShowCeresWarning(new WrittenPicture(page.Picture, page.Corner));

    /// <summary>
    /// Runs one update of the self-destruct's current readying stage (#1406, #1538): on the last update of bringing up the
    /// escape timer and warning, the arena's backgrounds show them from this update's frame on; readying the backgrounds
    /// shows nothing new but still takes its updates.
    /// </summary>
    /// <returns>True once the stage has spent all its updates.</returns>
    /// <exception cref="InvalidDataException">No stage was started.</exception>
    internal bool ReadyCeresEscapeWarning(RidleyEnemyState state)
    {
        CeresEscapeReadying readying = state.CeresEscapeReadying;
        if (!readying.IsStarted)
            throw new InvalidDataException("The Ceres self-destruct readied its warning without starting a stage.");
        if (readying.IsFinished)
            return true;
        state.CeresEscapeReadying = readying.Advance();
        if (state.CeresEscapeReadying is { Stage: CeresEscapeReadyingStage.Warning, IsFinished: true })
            _presentation.ShowLook(CeresEscapeReadyingDefinitions.WarningLook);
        return state.CeresEscapeReadying.IsFinished;
    }

    /// <summary>Shows every row half of the one-shot Japanese overlay list over the room's foreground.</summary>
    internal void DrawCeresJapaneseOverlay()
    {
        foreach (CeresEscapeWarningPicture page in CeresEscapeWarningPictures.Japanese)
            DrawCeresOverlay(page);
    }

    /// <summary>Runs the byte-oriented escape-text command stream.</summary>
    private bool StepCeresEscapeTypewriter(RidleyEnemyState state)
    {
        var installed = state.CeresEscapeTypewriter ?? throw new InvalidDataException(
            "Ceres warning text requires installed typewriter artwork.");
        bool completed = installed.Step(out EscapeTypewriterGlyph? glyph);
        // The Ceres program types over the room's foreground (#1348), as part of the warning (#1555).
        if (glyph is { } typed)
            _ridleyEncounter.ShowCeresWarning(new WrittenPicture(typed.Picture, typed.Corner));
        state.CeresEscapeTextDelayTimer = installed.DelayTimer;
        if (installed.ClickRequested)
            _audio.QueueEnemySound(EscapeTypewriterDefinitions.CeresClick, priority: SoundRequestPriority.Low);
        return completed;
    }

    /// <summary>Steps the three-colour alarm cycle of the warning's EMERGENCY title every fourth frame.</summary>
    internal static void UpdateCeresSelfDestructAlarm(RidleyEnemyState state, int frameCounter)
    {
        if (!RidleyCadenceDefinitions.SelfDestructAlarmStepEveryFourthUpdate.IsDue(frameCounter))
            return;

        state.SelfDestructAlarmStep = (state.SelfDestructAlarmStep + 1) % 16;
        state.Colours.ShowAlarm(state.SelfDestructAlarmStep);
    }
}
