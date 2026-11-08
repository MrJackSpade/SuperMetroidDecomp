using System.Collections.Immutable;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Gameplay;
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
        state.FloorAngle = RotationAngle.FromRaw(1);
        state.FloorOffset = new Offset16<FloorPixels>(-128, 32);
        state.FloorZoom = 2048;
        state.FloorCenter = new Position16<FloorPixels>(64, 64);
        state.GetawayBabyFrame = 0;
        state.GetawayWingFrame = 0;
    }

    /// <summary>
    /// Runs the room's Ceres Ridley getaway cutscene and
    /// <c>HandleCeresRidleyGetawayCutsceneIfActive</c>. It runs after the
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
    /// Runs the getaway room main through its table terminator. This routine
    /// owns presentation only: ordinary Ridley has already been hidden, while Samus remains
    /// in room-world coordinates exactly as she does during the boss-room getaway.
    /// </summary>
    internal void TickCeresRidleyGetawayFloor(
        RidleyEnemyState state,
        SamusState? samus,
        int frameCounter)
    {
        int tableIndex = state.GetawayFrameIndex;
        state.GetawayFrameIndex = tableIndex + 1;

        // HandleCeresRidleyGetawayCutscene requests the departure sound on the first table
        // entry only. Tying the request to the
        // table index preserves the same one-shot behavior after saves/replays and pauses.
        if (tableIndex == 0)
            _audio.QueueEnemySound(Library2Sound.CeresRidleyGetaway, priority: SoundRequestPriority.Normal);

        // At frame 104, Samus's grab handler replaces Samus's movement/hack handlers so the
        // rotating boss image cannot overlap her from the following frame onward.
        if (tableIndex == 104 && samus is not null)
            samus.CeresRidleyEjection.Request(samus);

        var frame = CeresRidleyGetawayCurve.FromIndex(tableIndex);
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

        UpdateCeresRidleyFloorPalette(state, zoom);
        state.FloorAngle = state.FloorAngle.AddRaw(48);

        // The Baby capsule advances on every fourth frame of the frame counter using 0,1,2,1. The transfer
        // lists patch only the two two-byte rows occupied by that animation in the map.
        if ((frameCounter & 3) == 0)
        {
            state.GetawayBabyFrame = (state.GetawayBabyFrame + 1) % 4;
            _room.Services.DrawFloor(CeresFloorAnimation.BabyFrameForPhase(state.GetawayBabyFrame));
        }

        // The six sparse wing rows alternate on every eighth frame of the frame counter. Each wing frame's floor
        // update leaves behind the delays Samus's unauthored Speed Booster cadence runs on from then on.
        if ((frameCounter & 7) == 0)
        {
            state.GetawayWingFrame = (state.GetawayWingFrame + 1) % 2;
            CeresFloorAnimationFrame wing = state.GetawayWingFrame == 0
                ? CeresFloorAnimationFrame.WingFrame0
                : CeresFloorAnimationFrame.WingFrame1;
            _room.Services.DrawFloor(wing);
            CeresRidleyGetawayBoostDelays delays = _bossArtwork.CeresRidleyGetawayBoostDelays ?? throw new InvalidOperationException(
                "The Ceres Ridley getaway requires the installed delays its wing frames leave behind.");
            _room.UnauthoredBoostCadence.TakeDelaysLeftByWings(delays.LeftBy(wing));
        }
    }

    /// <summary>
    /// How the perspective floor appears as the getaway last turned, zoomed and moved it, before room shake, or null while
    /// the getaway does not show the floor: turned by the getaway's angle at its zoom about the floor's centre, and
    /// scrolled by the getaway's offset across the floor.
    /// </summary>
    internal static FloorView? FloorTransform(RidleyEnemyState? ridley) => ridley is { GetawayActive: true } getaway
        ? FloorView.Turned(getaway.FloorAngle, getaway.FloorZoom, getaway.FloorCenter.AsSignedWord(), getaway.FloorOffset)
        : null;

    /// <summary>Selects the zoom-dependent fifteen-color Ceres getaway palette without replacing transparent color zero.</summary>
    /// <param name="state">The getaway's Ridley, whose room colours show the floor.</param>
    /// <param name="zoom">The floor's zoom, 0 through 65535.</param>
    internal static void UpdateCeresRidleyFloorPalette(RidleyEnemyState state, int zoom)
    {
        // SetCeresRidleyPaletteAccordingToZoomLevel uses this wrapped expression.
        // There are fifteen colors; palette entry zero remains the shared transparent color.
        int zoomStep = UnsignedEightEight.Whole(zoom);
        if (zoomStep < CeresRidleyPaletteDefinitions.GetawayZoomStepCount)
        {
            Ensure.NotNull(state).Colours.Room.Lay(RidleyShades.FloorZoom(zoomStep));
            return;
        }
        throw new InvalidOperationException(
            $"Ceres Ridley turning-floor zoom step {zoomStep} is not in the installed palette catalog.");
    }

    /// <summary>
    /// Ports the shared Ridley function for the Ceres branch: the warning runs through its phases
    /// (<see cref="CeresSelfDestructPhase"/>, the C062/C08E/C09F/C0BB/C0F5/C104/C117 entries) in order. English skips
    /// only the Japanese subtitle phase and types the game's three-line English warning. Japanese runs the
    /// subtitle phase first, installs its subtitle tilemap, and then uses the same typewriter.
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
                    // The warning setup calls DrawEmergencyText immediately after the final
                    // graphics record. The call was missing from the earlier
                    // translation, so the newly uploaded letter tiles existed but no
                    // tilemap entries ever referenced them on screen.
                    DrawCeresEmergencyText();
                    // The warning setup also queues music track seven after installing the final
                    // static warning tilemap page. The frontend consumes this typed request
                    // through the same queue used by every other music producer.
                    _audio.RequestRidleyMusic(MusicRequest.SelectTrack(7));
                    state.CeresSelfDestructPhase = CeresSelfDestructPhase.HoldingTitle;
                    state.CeresEscapeTextDelayTimer = 128;
                }
                return;

            case CeresSelfDestructPhase.HoldingTitle:
                UpdateCeresSelfDestructPalette(state, _ridleyEncounter.Parts.Body.FrameCounter);
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
                UpdateCeresSelfDestructPalette(state, _ridleyEncounter.Parts.Body.FrameCounter);
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
                UpdateCeresSelfDestructPalette(state, _ridleyEncounter.Parts.Body.FrameCounter);
                if (StepCeresEscapeTypewriter(state))
                    state.CeresSelfDestructPhase = CeresSelfDestructPhase.Finishing;
                return;

            case CeresSelfDestructPhase.Finishing:
                UpdateCeresSelfDestructPalette(state, _ridleyEncounter.Parts.Body.FrameCounter);
                state.Velocity = new EightEightVector<RoomPixels>(0, 0);
                state.Function = RidleyAiFunction.CeresSelfDestructPaletteOnly;
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.SelfDestructStarted;
                // The escape-text sequence sets the area-boss bit inside Ridley's AI, so later slots (the
                // room's exit door) observe it in this same EnemyMain pass.
                _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
                _frameResults.CeresEscapeStarted = true;
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresSelfDestructPhase)} {state.CeresSelfDestructPhase}.");
        }
    }

    /// <summary>Ports DrawEmergencyText: the EMERGENCY title is written over the room's foreground.</summary>
    internal void DrawCeresEmergencyText()
    {
        DrawCeresOverlay(CeresEscapeOverlayTilemapDefinitions.Emergency);
    }

    /// <summary>Writes the picture of one warning page over the room's foreground at its overlay cell (#1466).</summary>
    private void DrawCeresOverlay(CeresEscapeOverlayTilemapDefinition page) =>
        _room.Services.WriteForegroundPicture(new WrittenPicture(page.Picture, page.Cell));

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

    /// <summary>Writes every row half of the one-shot Japanese overlay list over the room's foreground.</summary>
    internal void DrawCeresJapaneseOverlay()
    {
        foreach (CeresEscapeOverlayTilemapDefinition page in CeresEscapeOverlayTilemapDefinitions.Japanese)
            DrawCeresOverlay(page);
    }

    /// <summary>Runs the byte-oriented escape-text command stream.</summary>
    private bool StepCeresEscapeTypewriter(RidleyEnemyState state)
    {
        var installed = state.CeresEscapeTypewriter ?? throw new InvalidDataException(
            "Ceres warning text requires installed typewriter artwork.");
        bool completed = installed.Step(out EscapeTypewriterGlyph? glyph);
        // The Ceres program types over the room's foreground (#1348).
        if (glyph is { } typed)
            _room.Services.WriteForegroundPicture(new WrittenPicture(typed.Picture, typed.Cell));
        state.CeresEscapeTextDelayTimer = installed.DelayTimer;
        if (installed.ClickRequested)
            _audio.QueueEnemySound(EscapeTypewriterDefinitions.CeresClick, priority: SoundRequestPriority.Low);
        return completed;
    }

    /// <summary>Steps the three-colour alarm cycle of the warning's EMERGENCY title every fourth frame.</summary>
    internal static void UpdateCeresSelfDestructPalette(RidleyEnemyState state, int frameCounter)
    {
        if ((frameCounter & 3) != 0)
            return;

        state.CeresEscapePaletteFrame = (state.CeresEscapePaletteFrame + 1) % 16;
        state.Colours.Warning.Lay(RidleyShades.Alarm(state.CeresEscapePaletteFrame));
    }
}
