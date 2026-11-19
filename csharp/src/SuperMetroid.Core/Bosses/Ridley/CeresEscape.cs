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

/// <summary>
/// The station's escape from Ceres Ridley's room (#2228): his getaway over the turning floor, which the room main runs,
/// then the self-destruct warning, which runs in his place in the enemy update. It owns its own floor, warning and colour
/// effects for the room; Ridley's state plays no part once he is hidden.
/// </summary>
[RoomLifetime]
internal sealed class CeresEscape : IGameplayShadingSource
{
    /// <summary>The enemy pass's results, which the self-destruct writes.</summary>
    private readonly EnemyPassEvents _passEvents;
    /// <summary>Provides room services and the floor's frames.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues the getaway's and the warning's sounds and music.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Shows the warning's look, the chamber's walls and the escape's colour effects.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The station's escape progress, which the getaway and the self-destruct advance.</summary>
    private readonly ICeresEscapeStatus _status;
    /// <summary>Says whether the warning types its Japanese text.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;
    /// <summary>The session's unauthored Speed Booster cadence, whose delays the wing frames leave behind.</summary>
    private readonly UnauthoredBoostCadence _unauthoredBoostCadence;

    /// <summary>The shade the getaway floor's zoom shows, or null before the getaway shows one.</summary>
    private RidleyFadeImage? floorZoom;

    /// <summary>Builds the escape from the collaborators it uses.</summary>
    internal CeresEscape(CeresEscapeDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _passEvents = dependencies.PassEvents;
        _room = dependencies.Room;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _status = dependencies.Status;
        _roomStatus = dependencies.RoomStatus;
        _unauthoredBoostCadence = Ensure.NotNull(dependencies.UnauthoredBoostCadence);
    }

    /// <summary>Where the escape stands.</summary>
    internal CeresEscapeStage Stage { get; private set; } = CeresEscapeStage.AwaitingGetaway;

    /// <summary>The getaway's turning floor.</summary>
    internal CeresTurningFloor Floor { get; } = new();

    /// <summary>The self-destruct's warning.</summary>
    internal CeresSelfDestructWarning Warning { get; } = new();

    /// <summary>
    /// The chamber's walls the getaway scene shows around its turning floor (#1545, #1989): the chamber wall's pictures of the
    /// chamber's left and right walls, standing at their room points in the doors' escaping colours, behind the floor
    /// and the actors.
    /// </summary>
    internal ImmutableArray<PerspectiveFloorScenery> ChamberWalls() =>
    [
        ChamberWall(EnemyFrameId.CeresGetawayChamberLeftWall, CeresGetawayChamberWallDefinitions.LeftWall),
        ChamberWall(EnemyFrameId.CeresGetawayChamberRightWall, CeresGetawayChamberWallDefinitions.RightWall),
    ];

    /// <summary>The chamber wall <paramref name="frame"/> pictures, standing at <paramref name="point"/>.</summary>
    private PerspectiveFloorScenery ChamberWall(EnemyFrameId frame, Position16<RoomPixels> point) =>
        new(_presentation.SheetPicture(EnemyDefinitionId.CeresGetawayChamberWall, frame), point, CeresDoorShades.InEscapingColours);

    /// <summary>
    /// Begins the getaway as Ridley hands the room to it: publishes his escape, sets up the floor's angle, scroll, zoom and
    /// centre before its first step, and shows the escape's colour effects.
    /// </summary>
    internal void BeginGetaway()
    {
        _status.CeresStatus = CeresEscapeStatus.RidleyEscaped;
        Stage = CeresEscapeStage.Getaway;
        Floor.Active = true;
        Floor.Flight = UpdateCountdown.Lasting(CeresRidleyGetawayCurve.Duration);
        Floor.Angle = CeresRidleyGetawayDefinitions.InitialAngle;
        Floor.Offset = CeresRidleyGetawayDefinitions.InitialFloorOffset;
        Floor.Zoom = CeresRidleyGetawayDefinitions.InitialZoom;
        Floor.Center = CeresRidleyGetawayDefinitions.InitialFloorCentre;
        Floor.Capsule = CeresTurningFloor.FirstCapsulePicture;
        Floor.Wings = CeresTurningFloor.FirstWingPicture;
        _presentation.BossShadings.Show(BossShadingLayer.CeresEscape, this);
    }

    /// <summary>
    /// Runs the getaway while it flies, until its motion ends. It runs after the frame's enemy AI and Samus processing, so
    /// the getaway's first step shares the frame in which Ridley's escape handoff publishes
    /// <see cref="CeresEscapeStatus.RidleyEscaped"/>. This step owns presentation only: ordinary Ridley has already been
    /// hidden, while Samus remains in room-world coordinates exactly as she does during the boss-room getaway.
    /// </summary>
    /// <param name="samus">Samus, whom Ridley's departure shoves.</param>
    /// <param name="floor">The arena's turning floor, whose capsule and wings the getaway animates.</param>
    /// <param name="frameCounter">The game's frame counter, which beats the capsule and the wings.</param>
    public void RunGetawayRoomMain(SamusState samus, RoomFloor floor, int frameCounter)
    {
        if (Stage != CeresEscapeStage.Getaway)
            return;
        int update = Floor.Flight.Age;
        bool flown = Floor.Flight.IsOver;
        Floor.Flight.Step();

        // The getaway requests the departure sound on its first update only.
        if (update == 0)
            _audio.QueueEnemySound(EnemySound.CeresRidleyGetaway, priority: SoundRequestPriority.Normal);

        // Ridley's shove takes Samus's movement so the rotating boss image cannot overlap her from the following update
        // onward.
        if (update == CeresRidleyGetawayDefinitions.SamusShoveUpdate)
            samus.CeresRidleyEjection.Request(samus);

        // The update after the curves' last ends the getaway.
        if (flown)
        {
            // The getaway's end restores the ordinary view and clears all rotation and scroll values, so the
            // self-destruct warning shows over the room, not the turning floor.
            Floor.Active = false;
            Floor.Offset = Offset16<FloorPixels>.Zero;
            Floor.Center = Offset16<FloorPixels>.Zero;
            // The getaway's end hands on to the self-destruct warning, which Ridley's AI runs. Samus's
            // special push and fall movement remains responsible for restoring normal input;
            // ending the rotating view itself does not unlock her in the game.
            BeginSelfDestruct();
            return;
        }

        FloorZoom zoom = CeresRidleyGetawayCurve.ZoomAt(update);
        Offset16<FloorPixels> drift = CeresRidleyGetawayCurve.DriftAt(update);
        Floor.Zoom = zoom;
        // The update's drift is added to the vertical offset and subtracted from the horizontal offset. Over the whole
        // getaway the drift curve takes the offset from its start, (-128, 32), no further than -268 across and between
        // -110 and 1019 down, so the sums always lie within the offset's range and need no wrap (#2471).
        Floor.Offset = new Offset16<FloorPixels>(
            Floor.Offset.X - drift.X,
            Floor.Offset.Y + drift.Y);

        ShowFloorZoom(zoom);
        Floor.Angle += CeresRidleyGetawayDefinitions.TurnPerFrame;

        // The Baby capsule advances on every fourth frame of the frame counter using 0,1,2,1.
        if (RidleyCadenceDefinitions.GetawayCapsuleStepEveryFourthUpdate.IsDue(frameCounter))
        {
            Floor.Capsule = Floor.Capsule.NextInCycle;
            floor.Show(Floor.Capsule.Entry);
        }

        // The wings beat on every eighth frame of the frame counter. Each wing frame leaves behind the delays
        // Samus's unauthored Speed Booster cadence runs on from then on.
        if (RidleyCadenceDefinitions.GetawayWingStepEveryEighthUpdate.IsDue(frameCounter))
        {
            Floor.Wings = Floor.Wings.NextInCycle;
            CeresFloorActorPicture wing = Floor.Wings.Entry;
            floor.Show(wing);
            _unauthoredBoostCadence.TakeDelaysLeftByWings(_bossArtwork.CeresRidleyGetawayBoostDelays.LeftBy(wing));
        }
    }

    /// <summary>
    /// How the perspective floor appears as the getaway last turned, zoomed and moved it, before room shake, or null while
    /// the getaway does not show the floor: turned by the getaway's angle at its zoom about the floor's centre, and
    /// scrolled by the getaway's offset across the floor.
    /// </summary>
    internal FloorView? ShownFloor => Floor.Active
        ? FloorView.Turned(Floor.Angle, Floor.Zoom, Floor.Center, Floor.Offset)
        : null;

    /// <summary>Shows the zoom-dependent shade of the floor's fifteen colours, which leaves its transparent colour as it is.</summary>
    /// <param name="zoom">The floor's zoom.</param>
    /// <exception cref="ArgumentOutOfRangeException">The floor has no shade for the zoom's whole floor pixels per screen pixel.</exception>
    internal void ShowFloorZoom(FloorZoom zoom)
    {
        // The floor's shade is chosen by the zoom's whole floor pixels per screen pixel.
        int wholeZoom = zoom.WholeFloorPixelsPerScreenPixel;
        floorZoom = RidleyFadeDefinitions.GetawayFloorZoom.ImageAt(wholeZoom);
    }

    /// <summary>Begins the self-destruct warning from its first step, as the getaway ends.</summary>
    internal void BeginSelfDestruct()
    {
        Stage = CeresEscapeStage.SelfDestruct;
        Warning.Phase = CeresSelfDestructPhase.Start;
    }

    /// <summary>
    /// Runs the escape's step in Ridley's place in the enemy update, which his AI hands it once he has got away (#2228):
    /// nothing while the room main flies the getaway, then the self-destruct warning's step, then only its alarm.
    /// </summary>
    /// <param name="ridleyFrameCounter">Ridley's body's update count, which paces the alarm.</param>
    internal void RunRidleysStep(int ridleyFrameCounter)
    {
        switch (Stage)
        {
            case CeresEscapeStage.Getaway:
                // The room main owns the getaway; his step is empty.
                return;
            case CeresEscapeStage.SelfDestruct:
                TickSelfDestruct(ridleyFrameCounter);
                return;
            case CeresEscapeStage.AlarmOnly:
                StepAlarm(ridleyFrameCounter);
                return;
            case CeresEscapeStage.AwaitingGetaway:
                throw new InvalidOperationException("Ceres Ridley handed his step to an escape that has not begun.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresEscapeStage)} {Stage}.");
        }
    }

    /// <summary>
    /// Runs the self-destruct warning through its phases (<see cref="CeresSelfDestructPhase"/>) in order. English skips
    /// only the Japanese subtitle phase and types the game's three-line English warning. Japanese runs the subtitle phase
    /// first, shows its subtitle, and then uses the same typewriter.
    /// </summary>
    private void TickSelfDestruct(int ridleyFrameCounter)
    {
        // A phase that ends by entering the next phase in the same update runs that phase too.
        bool entersNext;
        do
            entersNext = Warning.Phase.Run(this, ridleyFrameCounter);
        while (entersNext);
    }

    /// <summary>Runs one update of the warning's <see cref="CeresSelfDestructPhase.Start"/> phase.</summary>
    /// <returns>Whether the warning goes on to the phase it entered in this same update.</returns>
    internal bool RunStart()
    {
        Warning.Phase = CeresSelfDestructPhase.WaitingForWarning;
        Warning.PhaseWait.LastFor(CeresSelfDestructWarningDefinitions.BeforeWarningUpdates);
        return true;
    }

    /// <summary>Runs one update of the warning's <see cref="CeresSelfDestructPhase.WaitingForWarning"/> phase.</summary>
    /// <returns>Whether the warning goes on to the phase it entered in this same update.</returns>
    internal bool RunWaitingForWarning()
    {
        if (Warning.PhaseWait.StepKeepsRunning())
            return false;

        // The escape timer and warning show from this update's frame on, and the wait for the title starts on it.
        _presentation.ShowLook(CeresEscapeWarningLookDefinitions.WarningLook);
        Warning.Phase = CeresSelfDestructPhase.WaitingForTitle;
        Warning.PhaseWait.LastFor(CeresSelfDestructWarningDefinitions.BeforeTitleUpdates);
        return true;
    }

    /// <summary>Runs one update of the warning's <see cref="CeresSelfDestructPhase.WaitingForTitle"/> phase.</summary>
    /// <returns>Whether the warning goes on to the phase it entered in this same update.</returns>
    internal bool RunWaitingForTitle()
    {
        if (Warning.PhaseWait.StepKeepsRunning())
            return false;

        ShowWarningPicture(CeresEscapeWarningPictures.Emergency);
        // The escape music is requested after the title shows.
        _audio.RequestRidleyMusic(MusicRequest.SelectTrack(MusicTrack.Escape));
        Warning.Phase = CeresSelfDestructPhase.HoldingTitle;
        Warning.PhaseWait.LastFor(CeresSelfDestructWarningDefinitions.TitleHoldUpdates);
        return false;
    }

    /// <summary>Runs one update of the warning's <see cref="CeresSelfDestructPhase.HoldingTitle"/> phase.</summary>
    /// <returns>Whether the warning goes on to the phase it entered in this same update.</returns>
    internal bool RunHoldingTitle(int ridleyFrameCounter)
    {
        StepAlarm(ridleyFrameCounter);
        if (Warning.PhaseWait.StepKeepsRunning())
            return false;

        Warning.Typewriter = new(_bossArtwork.EscapeTypewriterPresentation.Get(EscapeTypewriterMessageId.Ceres));

        // English goes straight to typing the warning; Japanese first runs the subtitle phase.
        if (_roomStatus.JapaneseText)
        {
            Warning.Phase = CeresSelfDestructPhase.JapaneseSubtitle;
            Warning.PhaseWait.LastFor(CeresSelfDestructWarningDefinitions.JapaneseSubtitleWaitUpdates);
        }
        else
            Warning.Phase = CeresSelfDestructPhase.TypingWarning;
        return false;
    }

    /// <summary>Runs one update of the warning's <see cref="CeresSelfDestructPhase.JapaneseSubtitle"/> phase.</summary>
    /// <returns>Whether the warning goes on to the phase it entered in this same update.</returns>
    internal bool RunJapaneseSubtitle(int ridleyFrameCounter)
    {
        StepAlarm(ridleyFrameCounter);
        if (Warning.PhaseWait.StepRunsOut())
        {
            Warning.Phase = CeresSelfDestructPhase.TypingWarning;
            foreach (CeresEscapeWarningPicture line in CeresEscapeWarningPictures.JapaneseSubtitle)
                ShowWarningPicture(line);
        }
        // The typewriter finishing moves the warning one phase on from wherever the wait left it: the
        // subtitle phase to typing, or typing to finishing.
        if (StepTypewriter())
        {
            Warning.Phase = Warning.Phase == CeresSelfDestructPhase.JapaneseSubtitle
                ? CeresSelfDestructPhase.TypingWarning
                : CeresSelfDestructPhase.Finishing;
        }
        return false;
    }

    /// <summary>Runs one update of the warning's <see cref="CeresSelfDestructPhase.TypingWarning"/> phase.</summary>
    /// <returns>Whether the warning goes on to the phase it entered in this same update.</returns>
    internal bool RunTypingWarning(int ridleyFrameCounter)
    {
        StepAlarm(ridleyFrameCounter);
        if (StepTypewriter())
            Warning.Phase = CeresSelfDestructPhase.Finishing;
        return false;
    }

    /// <summary>Runs one update of the warning's <see cref="CeresSelfDestructPhase.Finishing"/> phase.</summary>
    /// <returns>Whether the warning goes on to the phase it entered in this same update.</returns>
    internal bool RunFinishing(int ridleyFrameCounter)
    {
        StepAlarm(ridleyFrameCounter);
        Stage = CeresEscapeStage.AlarmOnly;
        _status.CeresStatus = CeresEscapeStatus.SelfDestructStarted;
        // The escape-text sequence records the area boss's defeat inside Ridley's AI, so enemies listed after
        // him (the room's exit door) observe it in this same enemy update.
        _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaBoss);
        _passEvents.Publish(new CeresEscapeStarted());
        return false;
    }

    /// <summary>
    /// Writes one warning picture over the room's foreground with its top-left corner at its pixel of the foreground
    /// (#1466, #1555, #1601).
    /// </summary>
    internal void ShowWarningPicture(CeresEscapeWarningPicture page) =>
        Warning.Show(new WrittenPicture(page.Picture, page.Corner));

    /// <summary>Types the next step of the escape text.</summary>
    private bool StepTypewriter()
    {
        var installed = Warning.Typewriter ?? throw new InvalidDataException(
            "Ceres warning text requires installed typewriter artwork.");
        EscapeTypewriterStep typing = installed.Step();
        // The Ceres message types over the room's foreground (#1348), as part of the warning (#1555).
        if (typing is EscapeTypewriterStep.Typed typed)
            Warning.Show(new WrittenPicture(typed.Glyph.Picture, typed.Glyph.Corner));
        if (typing is EscapeTypewriterStep.TypedWithClick)
            _audio.QueueEnemySound(EscapeTypewriterDefinitions.CeresClick, priority: SoundRequestPriority.Low);
        return typing is EscapeTypewriterStep.Complete;
    }

    /// <summary>Plays the three-colour alarm cycle of the warning's EMERGENCY title on to its next colouring every fourth update.</summary>
    internal void StepAlarm(int frameCounter)
    {
        if (!RidleyCadenceDefinitions.SelfDestructAlarmStepEveryFourthUpdate.IsDue(frameCounter))
            return;

        Warning.Alarm.Step();
    }

    /// <summary>The shading the room's pictures show: the getaway floor's zoom, once the getaway has shown one.</summary>
    internal PictureShading RoomShading => floorZoom is { } zoom ? PictureShading.Shaded(RidleyShades.Fade(zoom)) : PictureShading.Standard;

    /// <summary>
    /// The shading the warning's written pictures show: its alarm's step, once the alarm has shown one. They show their own
    /// colours and alarm, never the room's (#1544).
    /// </summary>
    internal PictureShading WarningShading => Warning.Alarm.Shown is { } step ? PictureShading.Shaded(RidleyShades.Fade(step)) : PictureShading.Standard;

    /// <summary>The getaway floor's zoom over the room's pictures, and the alarm over the warning's written pictures (#1544).</summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (PictureShade shade in RoomShading.Shades)
            frame.Shade(GameplayPictures.Room, shade);
        foreach (PictureShade shade in WarningShading.Shades)
            frame.Shade(GameplayPictures.WrittenOverRoom, shade);
    }
}
