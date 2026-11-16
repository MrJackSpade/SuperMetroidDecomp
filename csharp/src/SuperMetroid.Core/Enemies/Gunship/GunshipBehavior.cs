using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship: its top and bottom initialization, landing, save prompt, Samus boarding and exit, and the
/// post-Ceres and escape lift-offs, with the frame events it publishes.
/// </summary>
[RoomLifetime]
internal sealed class GunshipBehavior : IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. GunshipPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new GunshipEntity(this, slot, common);

    /// <summary>Queues engine and entrance-pad sounds at the frame boundaries.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Creates gunship dust during takeoff.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The loaded room's bindings: its events and the game-state services that set up Samus's pose.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places, where the top hull finds the ship's other pieces when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>
    /// The top hull of the ship whose save prompt awaits its answer (<see cref="AnswerGunshipSavePrompt"/>), or null when
    /// no prompt is pending.
    /// </summary>
    private RoomEnemySlot? _topAwaitingSaveAnswer;

    /// <summary>Binds the frame, room, and slot owners used by the ship's three-part actor.</summary>
    internal GunshipBehavior(EnemyAudioQueue audio, EnemyProjectileEngine projectiles, EnemyRoomBinding room, EnemySlotTable slotTable, EnemyFrameResultBuilder frameResults)
    {
        _frameResults = frameResults;
        _audio = audio;
        _projectiles = projectiles;
        _room = room;
        _slotTable = slotTable;
    }
    /// <summary>Room load: the placement scenario.</summary>
    internal void BeginRoomLoad(GunshipLoadScenario scenario) => _gunshipLoadScenario = scenario;
    /// <summary>Selects ordinary Landing Site placement or the high-altitude post-Ceres descent.</summary>
    private GunshipLoadScenario _gunshipLoadScenario;
    /// <summary>Whether gunship restoration has requested the save message box and is awaiting <see cref="GunshipBehavior.AnswerGunshipSavePrompt"/> before its exit animation can resume.</summary>
    public bool GunshipSavePromptPending => _topAwaitingSaveAnswer is not null;

    /// <summary>
    /// Resumes the ship once the save message box has its answer. The ship plays the same exit animation for either
    /// answer; the message box flow makes the save itself, as a save station's, when the answer is yes.
    /// </summary>
    /// <exception cref="InvalidOperationException">The system is not loaded or no gunship save prompt is awaiting a response.</exception>
    public void AnswerGunshipSavePrompt()
    {
        RoomEnemySlot top = _topAwaitingSaveAnswer
            ?? throw new InvalidOperationException("The gunship save prompt is not awaiting a response.");
        RoomEnemySlot pad = Pieces(top).EntrancePad;
        _topAwaitingSaveAnswer = null;
        State(top).Phase = GunshipPhase.WaitForExitPadToOpen;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadOpening);
        State(top).PhaseTimer = GunshipBoardingDefinitions.PadMovementDuration;
    }

    /// <summary>Starts <paramref name="segment"/> on <paramref name="slot"/> from the next update.</summary>
    private static void StartGunshipAnimation(RoomEnemySlot slot, GunshipSegment segment)
    {
        slot.AnimationClock.StepOnNextUpdate();
        slot.RequireSpeciesState<GunshipPieceState>().Playback =
            new AnimationPlayback<GunshipSegment>(GunshipAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs a hull or pad piece's typed animation where the shared enemy animation would.</summary>
    internal static void AdvanceGunshipAnimation(RoomEnemySlot slot)
    {
        if (!slot.AnimationClock.Expire())
            return;
        GunshipPieceState piece = slot.RequireSpeciesState<GunshipPieceState>();
        AnimationPlayback<GunshipSegment> playback = piece.Playback;
        EnemyAnimationPlayback.Play(slot, GunshipAnimations.Segments, ref playback,
            cue => throw new InvalidOperationException($"The gunship's animations publish no cue; got {cue}."));
        piece.Playback = playback;
    }

    /// <summary>
    /// Initializes the top hull and selects ordinary idle or post-Ceres descent, which places it against the loading
    /// <paramref name="samus"/>.
    /// </summary>
    internal void InitializeGunshipTop(RoomEnemySlot slot, SamusState samus)
    {
        var pieces = new GunshipPieces(slot, _slotTable.ListedAfter(slot, 1), _slotTable.ListedAfter(slot, 2));
        slot.SetSpeciesState(pieces);
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        slot.SetSpeciesState(new GunshipPieceState(EnemyAnimationPlayback.Start(slot, GunshipAnimations.StartingWith(GunshipSegment.TopHull))));
        GunshipState ship;
        if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
        {
            // Post-Ceres initialization attaches the top hull seventeen pixels above Samus and
            // enters the high-altitude descent used only after Ceres.
            slot.Position = slot.Position.WithWrappedWholeY(samus.Position.Y.Whole - GunshipMotionDefinitions.TopHullAboveSamus);
            ship = new GunshipState(GunshipPhase.DescendAfterCeres);
        }
        else
        {
            slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole - GunshipMotionDefinitions.LandedTopHullRaise);
            ship = new GunshipState(GunshipPhase.Idle) { BaseY = slot.Position.Y.Whole };
        }
        ship.BobTimer = 1;
        ship.BobPhase = 0;
        slot.SetSpeciesState(ship);
        // Every placement, landed or descending after Ceres, starts the lights' glow among the room's colour effects. The
        // glow keeps this gunship (#1783): it runs until the room's colour effects are cleared and shows on these pieces.
        _frameResults.Glow = new EnemyGlowStart(GunshipLightDefinitions.Glow, pieces);
    }

    /// <summary>
    /// Initializes bottom hull or pad from the enemies its room lists before it; after Ceres the bottom hull is placed
    /// against the loading <paramref name="samus"/>.
    /// </summary>
    internal void InitializeGunshipBottom(RoomEnemySlot slot, SamusState samus)
    {
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        slot.SetSpeciesState(new GunshipPieceState(EnemyAnimationPlayback.Start(slot, GunshipAnimations.StartingWith(slot.SpawnSettings<GunshipSpawnSettings>().Part == GunshipBottomPiece.EntrancePad
            ? GunshipSegment.BottomEntrancePad
            : GunshipSegment.BottomHull))));

        if (slot.SpawnSettings<GunshipSpawnSettings>().Part == GunshipBottomPiece.EntrancePad)
        {
            slot.Position = slot.Position.WithWrappedWholeY(GunshipPadBorrowedHeight.Start(
                distance => _slotTable.ListedBefore(slot, distance).Position.Y.Whole));
        }
        else
        {
            if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
            {
                slot.Position = slot.Position.WithWrappedWholeY(samus.Position.Y.Whole + GunshipMotionDefinitions.BottomHullBelowSamus);
            }
            else
            {
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + GunshipMotionDefinitions.LandedBottomHullDrop);
                // The ship's engine sound starts with its bottom hull, which the room lists right after the top hull that
                // holds the ship.
                State(_slotTable.ListedBefore(slot, 1)).EngineSoundTimer = GunshipHoverSoundDefinitions.FirstDelay;
            }
        }
    }

    /// <summary>Runs the top hull's sound timer and landing, boarding, save, or takeoff state machine.</summary>
    internal void RunGunshipTopMain(
        RoomEnemySlot top,
        SamusState samus,
        GameplayAction newlyPressedControllerInput)
    {
        // The ship decrements its engine-sound timer and reloads the hover sound's interval on one or past zero. The engine
        // sound is requested before the reload; this periodic producer continues while the ship is idling or bobbing.
        if (--State(top).EngineSoundTimer <= 0)
        {
            _audio.QueueEnemySound(EnemySound.GunshipHover, priority: SoundRequestPriority.Normal);
            State(top).EngineSoundTimer = GunshipHoverSoundDefinitions.Interval;
        }

        // The landed ship continuously performs its four-phase bob before running the phase itself, in every
        // phase from waiting for the landing entrance through loading the liftoff dust.
        GunshipPhase phase = State(top).Phase;
        if (BobsWhileLanded(phase))
            StepGunshipBob(top);

        switch (phase)
        {
            case GunshipPhase.DescendAfterCeres:
                DescendPostCeresGunship(top, samus);
                return;
            case GunshipPhase.ApplyLandingBrakes:
                BouncePostCeresGunship(top, samus);
                return;
            case GunshipPhase.WaitForLandingEntranceToOpen:
                if (TickGunshipPhaseTimer(top))
                    State(top).Phase = GunshipPhase.EjectSamus;
                return;
            case GunshipPhase.EjectSamus:
                RaisePostCeresSamus(top, samus);
                return;
            case GunshipPhase.FinishLanding:
                if (TickGunshipPhaseTimer(top))
                {
                    State(top).Phase = GunshipPhase.Idle;
                    samus.Control.Unlock();
                    _frameResults.Gunship = GunshipFrameEvent.LandingCompleted;
                }
                return;
            case GunshipPhase.Idle:
                HandleIdleGunshipEntrance(top, samus, newlyPressedControllerInput);
                return;
            case GunshipPhase.WaitForEntranceToOpen:
                if (TickGunshipPhaseTimer(top))
                    State(top).Phase = GunshipPhase.LowerSamus;
                return;
            case GunshipPhase.LowerSamus:
                LowerSamusIntoGunship(top, samus);
                return;
            case GunshipPhase.WaitForEntranceToClose:
                if (TickGunshipPhaseTimer(top))
                    State(top).Phase = GunshipPhase.BeginLiftoffOrRestoreSamus;
                return;
            case GunshipPhase.BeginLiftoffOrRestoreSamus:
                RestoreSamusInGunship(top, samus);
                return;
            case GunshipPhase.HandleSaveConfirmation:
                _topAwaitingSaveAnswer = top;
                _frameResults.Gunship = GunshipFrameEvent.SavePromptRequested;
                return;
            case GunshipPhase.WaitForExitPadToOpen:
                if (TickGunshipPhaseTimer(top))
                    State(top).Phase = GunshipPhase.RaiseSamus;
                return;
            case GunshipPhase.RaiseSamus:
                RaiseSamusOutOfGunship(top, samus);
                return;
            case GunshipPhase.FinishSamusExit:
                if (TickGunshipPhaseTimer(top))
                {
                    State(top).Phase = GunshipPhase.Idle;
                    samus.Control.Unlock();
                    _frameResults.Gunship = GunshipFrameEvent.ExitCompleted;
                }
                return;
            case GunshipPhase.ReadyTakeoffEngines:
                ReadyGunshipTakeoffEngines(top);
                return;
            case GunshipPhase.FireUpEngines:
                FireUpGunshipEngines(top, samus);
                return;
            case GunshipPhase.SteadyLiftoff:
                LiftGunshipAtConstantSpeed(top, samus);
                return;
            case GunshipPhase.AcceleratingLiftoff:
                AccelerateEscapingGunship(top, samus);
                return;
            case GunshipPhase.MoveAccelerating:
                MoveEscapingGunship(top, samus);
                return;
            default:
                throw new InvalidOperationException($"The gunship has no behaviour for phase {phase}.");
        }
    }

    /// <summary>Carries Samus and all ship parts downward, slowing below Y=768 before the landing clamp.</summary>
    private static void DescendPostCeresGunship(RoomEnemySlot top, SamusState samus)
    {
        // The after-Ceres descent carries all four actors as a rigid body. Above Y=768 it moves
        // 4.5 pixels an update; below that threshold it slows to 2.5 and clamps the
        // top hull to 1119 before beginning its seventeen-frame landing bounce.
        // The clamp writes only whole positions; subpixels keep this update's sum.
        SignedSixteenSixteen delta = top.Position.Y.Whole < GunshipMotionDefinitions.SlowDescentY ? GunshipMotionDefinitions.FastDescentSpeed : GunshipMotionDefinitions.SlowDescentSpeed;
        AddGunshipYFixed(samus, top, delta);
        if (top.Position.Y.Whole < GunshipMotionDefinitions.LandedTopY)
            return;

        RoomEnemySlot bottom = Pieces(top).Bottom;
        RoomEnemySlot pad = Pieces(top).EntrancePad;
        top.Position = top.Position.WithWholeY(GunshipMotionDefinitions.LandedTopY);
        bottom.Position = bottom.Position.WithWholeY(GunshipMotionDefinitions.LandedBottomY);
        pad.Position = pad.Position.WithWholeY(GunshipMotionDefinitions.LandedPadY);
        State(top).Phase = GunshipPhase.ApplyLandingBrakes;
        State(top).LandingBrakeStep = 0;
    }

    /// <summary>Applies seventeen authored brake offsets, then opens the pad and aligns Samus.</summary>
    internal void BouncePostCeresGunship(RoomEnemySlot top, SamusState samus)
    {
        int yDelta = GunshipMotion.LandingBrakeYDelta(State(top).LandingBrakeStep);
        samus.Position += new Offset16<RoomPixels>(0, yDelta);
        foreach (RoomEnemySlot slot in Pieces(top).All)
        {
            slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + yDelta);
        }

        State(top).LandingBrakeStep++;
        if (State(top).LandingBrakeStep < GunshipMotionDefinitions.LandingBrakeYDeltas.Length)
            return;

        RoomEnemySlot pad = Pieces(top).EntrancePad;
        State(top).Phase = GunshipPhase.WaitForLandingEntranceToOpen;
        State(top).BaseY = top.Position.Y.Whole;
        State(top).BobTimer = 1;
        State(top).BobPhase = 0;
        samus.Position = samus.Position.WithWrappedWholeX(top.Position.X.Whole + 1);
        samus.WritePreviousXPosition(samus.Position.X.Whole);
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadOpening);
        State(top).PhaseTimer = GunshipBoardingDefinitions.PadMovementDuration;
        _frameResults.Gunship = GunshipFrameEvent.LandingPadOpened;
        _audio.QueueEnemySound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Raises Samus from the landed ship and closes its pad at the exit height.</summary>
    private void RaisePostCeresSamus(RoomEnemySlot top, SamusState samus)
    {
        samus.Position -= new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.ArrivalRiseStep);
        if (CoordinateAxis.Difference(samus.Position.Y.Whole, State(top).BaseY - GunshipBoardingDefinitions.ExitHeight) >= 0)
            return;

        RoomEnemySlot pad = Pieces(top).EntrancePad;
        State(top).Phase = GunshipPhase.FinishLanding;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadClosing);
        State(top).PhaseTimer = GunshipBoardingDefinitions.PadMovementDuration;
        _frameResults.Gunship = GunshipFrameEvent.LandingPadClosed;
    }

    /// <summary>Adds the same 1/65536-pixel Y displacement to Samus and all three ship components.</summary>
    private static void AddGunshipYFixed(SamusState samus, RoomEnemySlot top, SignedSixteenSixteen delta)
    {
        FixedCoordinate samusY = new FixedCoordinate(samus.Position.Y.Whole, samus.Kinematics.Position.Y.Fraction)
            .MovedBy(delta);
        samus.Position = samus.Position.WithWholeY(samusY.Whole);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(samusY.Fraction);

        foreach (RoomEnemySlot slot in Pieces(top).All)
        {
            slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(delta));
        }
    }

    /// <summary>
    /// Whether the landed ship bobs before running <paramref name="phase"/>: every phase from waiting for the
    /// landing entrance through loading the liftoff dust. The game tests its phase against the range of
    /// those phases (a range which also contains Idle); firing up the engines is the first phase past it.
    /// </summary>
    private static bool BobsWhileLanded(GunshipPhase phase) => phase is
        GunshipPhase.WaitForLandingEntranceToOpen or GunshipPhase.EjectSamus or GunshipPhase.FinishLanding or
        GunshipPhase.Idle or GunshipPhase.WaitForEntranceToOpen or GunshipPhase.LowerSamus or
        GunshipPhase.WaitForEntranceToClose or GunshipPhase.BeginLiftoffOrRestoreSamus or
        GunshipPhase.HandleSaveConfirmation or GunshipPhase.WaitForExitPadToOpen or GunshipPhase.RaiseSamus or
        GunshipPhase.FinishSamusExit or GunshipPhase.ReadyTakeoffEngines;

    /// <summary>Cycles the four idle-bob phases while moving the hull and pad together.</summary>
    internal static void StepGunshipBob(RoomEnemySlot top)
    {
        if (--State(top).BobTimer > 0)
            return;

        GunshipIdleBobDefinition bob = GunshipMotion.IdleBob(State(top).BobPhase);
        State(top).BobTimer = bob.Timer;
        int yDelta = bob.YDelta;
        foreach (RoomEnemySlot slot in Pieces(top).All)
        {
            slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + yDelta);
        }
        State(top).BobPhase = (State(top).BobPhase + 1) % GunshipMotionDefinitions.IdleBobs.Length;
    }

    /// <summary>Accepts a new Down press inside the standing entrance box and opens the pad.</summary>
    private void HandleIdleGunshipEntrance(
        RoomEnemySlot top,
        SamusState samus,
        GameplayAction newlyPressedControllerInput)
    {
        GameplayAction newlyPressed = newlyPressedControllerInput;
        if (!newlyPressed.HasAny(GameplayAction.Down))
            return;

        bool insideEntrance =
            CoordinateAxis.AsSigned(CoordinateAxis.Wrap(top.Position.X.Whole - 8 - samus.Position.X.Whole)) < 0 &&
            CoordinateAxis.AsSigned(CoordinateAxis.Wrap(top.Position.X.Whole + 8 - samus.Position.X.Whole)) >= 0 &&
            CoordinateAxis.AsSigned(CoordinateAxis.Wrap(top.Position.Y.Whole - 64 - samus.Position.Y.Whole)) < 0 &&
            CoordinateAxis.AsSigned(CoordinateAxis.Wrap(top.Position.Y.Whole - samus.Position.Y.Whole)) >= 0;
        if (insideEntrance && SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Standing)
        {
            RoomEnemySlot pad = Pieces(top).EntrancePad;
            State(top).Phase = GunshipPhase.WaitForEntranceToOpen;
            // The alignment stores the ship's X as both Samus's current and previous X, so the camera sees
            // no horizontal motion from this alignment.
            if (samus.Position.X.Whole != GunshipMotionDefinitions.LandedX)
            {
                samus.Position = samus.Position.WithWholeX(top.Position.X.Whole);
                samus.WritePreviousXPosition(samus.Position.X.Whole);
            }
            SamusPoseMetadata.ApplyForwardFacingPoseSetup(samus);
            samus.Control.EnterGunship();
            pad.Position = pad.Position.WithWrappedWholeY(top.Position.Y.Whole - GunshipMotionDefinitions.PadAboveTopHull);
            StartGunshipAnimation(pad, GunshipSegment.EntrancePadOpening);
            State(top).PhaseTimer = GunshipBoardingDefinitions.PadMovementDuration;
            _frameResults.Gunship = GunshipFrameEvent.EntryStarted;
            _audio.QueueEnemySound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
        }
    }

    /// <summary>Decrements the phase timer, treating one and a step past zero as expiry.</summary>
    private static bool TickGunshipPhaseTimer(RoomEnemySlot top)
    {
        return --State(top).PhaseTimer <= 0;
    }

    /// <summary>Lowers Samus into the ship until she is aboard, then closes the pad.</summary>
    private void LowerSamusIntoGunship(RoomEnemySlot top, SamusState samus)
    {
        samus.Position += new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.BoardingStep);
        if (CoordinateAxis.Difference(samus.Position.Y.Whole, State(top).BaseY + GunshipBoardingDefinitions.BoardedDepth) < 0)
            return;

        RoomEnemySlot pad = Pieces(top).EntrancePad;
        State(top).Phase = GunshipPhase.WaitForEntranceToClose;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadClosing);
        State(top).PhaseTimer = GunshipBoardingDefinitions.PadMovementDuration;
        _frameResults.Gunship = GunshipFrameEvent.EntryPadClosing;
    }

    /// <summary>Refills Samus by the ship's refill step or skips directly to escape takeoff after Mother Brain.</summary>
    private void RestoreSamusInGunship(RoomEnemySlot top, SamusState samus)
    {
        // The Zebes-timebomb-set event is set by Mother Brain's death sequence. Bypasses every refill
        // and save-prompt branch here, starts showing the takeoff artwork, and keeps the
        // already hidden/input-locked Samus rigidly attached to the ship.
        if (_room.Services.Events.Has(StoryEvent.ZebesTimebombSet))
        {
            State(top).Phase = GunshipPhase.ReadyTakeoffEngines;
            State(top).EngineReadyingElapsed = 0;
            State(top).EscapeVelocity = SignedEightEight.Zero;
            State(top).RumbleFrame = 0;
            _frameResults.Gunship = GunshipFrameEvent.EscapeTakeoffStarted;
            return;
        }

        SamusEnergyRestoration.Restore(samus, GunshipBoardingDefinitions.RefillStep);
        samus.Missiles = RestoreStep(samus.Missiles, samus.MaxMissiles);
        samus.SuperMissiles = RestoreStep(samus.SuperMissiles, samus.MaxSuperMissiles);
        samus.PowerBombs = RestoreStep(samus.PowerBombs, samus.MaxPowerBombs);
        if (samus.ReserveEnergy < samus.MaxReserveEnergy ||
            samus.Health < samus.MaxHealth ||
            samus.Missiles < samus.MaxMissiles ||
            samus.SuperMissiles < samus.MaxSuperMissiles ||
            samus.PowerBombs < samus.MaxPowerBombs)
            return;

        // The prompt itself opens when the save-confirmation behaviour runs on the following frame.
        State(top).Phase = GunshipPhase.HandleSaveConfirmation;
    }

    /// <summary>Raises Samus out of the ship, then closes the pad and emits the exit event.</summary>
    private void RaiseSamusOutOfGunship(RoomEnemySlot top, SamusState samus)
    {
        samus.Position -= new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.LeavingStep);
        if (CoordinateAxis.Difference(samus.Position.Y.Whole, State(top).BaseY - GunshipBoardingDefinitions.ExitHeight) >= 0)
            return;

        RoomEnemySlot pad = Pieces(top).EntrancePad;
        State(top).Phase = GunshipPhase.FinishSamusExit;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadClosing);
        State(top).PhaseTimer = GunshipBoardingDefinitions.PadMovementDuration;
        _frameResults.Gunship = GunshipFrameEvent.ExitPadClosing;
        _audio.QueueEnemySound(RoomSound.GunshipPadClosing, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Runs one update of the takeoff readying the engines (#1406, #1538): on the last of
    /// <see cref="GunshipTakeoffDefinitions.EngineReadyingDuration"/> updates the gunship shows its takeoff look from this
    /// update's frame on, and the engines fire up.
    /// </summary>
    /// <exception cref="InvalidDataException">The readying ran past its duration.</exception>
    internal void ReadyGunshipTakeoffEngines(RoomEnemySlot top)
    {
        int elapsed = State(top).EngineReadyingElapsed;
        if (elapsed >= GunshipTakeoffDefinitions.EngineReadyingDuration)
            throw new InvalidDataException("The gunship's takeoff readied its engines past its duration.");

        State(top).EngineReadyingElapsed = elapsed + 1;
        if (State(top).EngineReadyingElapsed >= GunshipTakeoffDefinitions.EngineReadyingDuration)
        {
            _room.Session.Looks.Show(GunshipTakeoffDefinitions.TakeoffLook);
            State(top).Phase = GunshipPhase.FireUpEngines;
            State(top).EngineReadyingElapsed = 0;
        }
    }

    /// <summary>Runs the 128-frame engine-rumble phase.</summary>
    private void FireUpGunshipEngines(RoomEnemySlot top, SamusState samus)
    {
        int rumbleFrame = State(top).RumbleFrame;
        bool strong = rumbleFrame >= GunshipTakeoffDefinitions.RumbleStrengthensFrame;
        int shake = GunshipTakeoffDefinitions.RumbleShakesDown.IsSetIn(rumbleFrame)
            ? (strong ? 2 : 1)
            : (strong ? -2 : -1);
        samus.Position += new Offset16<RoomPixels>(0, shake);
        PlaceGunshipAroundSamus(top, samus.Position.Y.Whole);

        State(top).RumbleFrame++;
        if (State(top).RumbleFrame == GunshipTakeoffDefinitions.RumbleStrengthensFrame)
        {
            // The six calls happen after the frame counter reaches
            // exactly 64. Descending projectile allocation means the last cloud, the outer left one, occupies the
            // highest surviving slot, matching the ordinary scheduler.
            foreach (GunshipDustPlacement cloud in GunshipDustDefinitions.LiftoffClouds)
                GunshipLiftoffDust.SpawnCloud(_projectiles, cloud, samus);
        }
        if (State(top).RumbleFrame >= GunshipTakeoffDefinitions.EngineRumbleFrames)
        {
            State(top).Phase = GunshipPhase.SteadyLiftoff;
            State(top).PhaseTimer = 0;
        }
    }

    /// <summary>Runs constant two-pixel liftoff.</summary>
    private static void LiftGunshipAtConstantSpeed(RoomEnemySlot top, SamusState samus)
    {
        MoveGunshipAndSamusToY(top, samus, CoordinateAxis.Wrap(samus.Position.Y.Whole - GunshipTakeoffDefinitions.SteadyLiftoffRise));
        if (CoordinateAxis.Difference(top.Position.Y.Whole, GunshipTakeoffDefinitions.AcceleratingLiftoffY) < 0)
        {
            State(top).Phase = GunshipPhase.AcceleratingLiftoff;
            State(top).EscapeVelocity = GunshipMotionDefinitions.EscapeStartVelocity;
        }
    }

    /// <summary>Runs the accelerating takeoff phase.</summary>
    private void AccelerateEscapingGunship(RoomEnemySlot top, SamusState samus)
    {
        MoveEscapingGunship(top, samus);
        if (CoordinateAxis.Difference(top.Position.Y.Whole, GunshipTakeoffDefinitions.TakeoffCompleteY) < 0)
        {
            State(top).Phase = GunshipPhase.MoveAccelerating;
            _frameResults.Gunship = GunshipFrameEvent.EscapeTakeoffCompleted;
        }
    }

    /// <summary>Carries the escaping ship and Samus on up, faster each update.</summary>
    private static void MoveEscapingGunship(RoomEnemySlot top, SamusState samus)
    {
        GunshipState ship = State(top);
        ship.AccelerateEscape();

        samus.Kinematics.Position = samus.Kinematics.Position.WithY(samus.Kinematics.Position.Y.MovedBy(
            -ship.EscapeVelocity.ToSixteenSixteenDelta()));
        MoveGunshipAndSamusToY(top, samus, samus.Position.Y.Whole);
    }

    /// <summary>Positions hull and pad at fixed whole-pixel offsets from Samus during escape.</summary>
    private static void MoveGunshipAndSamusToY(
        RoomEnemySlot top,
        SamusState samus,
        int samusY)
    {
        samus.Position = samus.Position.WithWholeY(samusY);
        PlaceGunshipAroundSamus(top, samusY);
    }

    /// <summary>Places the top hull, the entrance pad and the bottom hull where they sit around Samus at <paramref name="samusY"/>.</summary>
    private static void PlaceGunshipAroundSamus(RoomEnemySlot top, int samusY)
    {
        RoomEnemySlot bottom = Pieces(top).Bottom;
        RoomEnemySlot pad = Pieces(top).EntrancePad;
        top.Position = top.Position.WithWrappedWholeY(samusY - GunshipMotionDefinitions.TopHullAboveSamus);
        pad.Position = pad.Position.WithWrappedWholeY(top.Position.Y.Whole - GunshipMotionDefinitions.PadAboveTopHull);
        bottom.Position = bottom.Position.WithWrappedWholeY(samusY + GunshipMotionDefinitions.BottomHullBelowSamus);
    }

    /// <summary>Restores a resource by the ship's refill step without exceeding its maximum.</summary>
    private static int RestoreStep(int current, int maximum)
    {
        if (current >= maximum)
            return current;
        return Math.Min(current + GunshipBoardingDefinitions.RefillStep, maximum);
    }

    /// <summary>Runs Gunship's main AI.</summary>
    internal void RunGunshipMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunGunshipTopMain(
            slot,
            frame.Samus,
            frame.NewlyPressedControllerInput);
    }

    /// <summary>The state of the ship <paramref name="top"/> runs.</summary>
    private static GunshipState State(RoomEnemySlot top) => GunshipState.Of(top);

    /// <summary>The gunship's pieces, which <paramref name="top"/> found when it was placed.</summary>
    private static GunshipPieces Pieces(RoomEnemySlot top) => top.RequireSpeciesState<GunshipPieces>();
}
