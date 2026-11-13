using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

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
    /// <summary>Supplies room events and the bus used by Samus pose setup.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places, where the top hull finds the ship's other pieces when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The room's gunship, its pieces found when its top hull was placed; null until then.</summary>
    private GunshipPieces? _ship;

    /// <summary>Binds the frame, room, and slot owners used by the ship's three-part actor.</summary>
    internal GunshipBehavior(EnemyAudioQueue audio, EnemyProjectileEngine projectiles, EnemyRoomBinding room, EnemySlotTable slotTable, EnemyFrameResultBuilder frameResults)
    {
        _frameResults = frameResults;
        _audio = audio;
        _projectiles = projectiles;
        _room = room;
        _slotTable = slotTable;
    }
    /// <summary>Room load: the placement scenario and the Samus the post-Ceres ship is placed against.</summary>
    internal void BeginRoomLoad(GunshipLoadScenario scenario, SamusState? samus)
    {
        _gunshipLoadScenario = scenario;
        _samusAtEnemyInitialization = samus;
    }
    /// <summary>Selects ordinary Landing Site placement or the high-altitude post-Ceres descent.</summary>
    private GunshipLoadScenario _gunshipLoadScenario;
    /// <summary>Samus actor captured for the post-Ceres ship's initial placement.</summary>
    private SamusState? _samusAtEnemyInitialization;
    /// <summary>Whether gunship restoration has requested the save message box and is awaiting <see cref="GunshipBehavior.AnswerGunshipSavePrompt"/> before its exit animation can resume.</summary>
    public bool GunshipSavePromptPending { get; internal set; }

    /// <summary>
    /// Resumes the ship once the save message box has its answer. The ship plays the same exit animation for either
    /// answer; the message box flow makes the save itself, as a save station's, when the answer is yes.
    /// </summary>
    /// <exception cref="InvalidOperationException">The system is not loaded or no gunship save prompt is awaiting a response.</exception>
    public void AnswerGunshipSavePrompt()
    {
        _room.EnsureLoaded();
        if (!GunshipSavePromptPending)
            throw new InvalidOperationException("The gunship save prompt is not awaiting a response.");

        GunshipPieces ship = _ship ?? throw new InvalidOperationException("The gunship save prompt is pending in a room with no gunship.");
        RoomEnemySlot top = ship.Top;
        RoomEnemySlot pad = ship.EntrancePad;
        GunshipSavePromptPending = false;
        Part(top).Function = GunshipFunction.WaitForExitPadToOpen;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadOpening);
        Part(top).FunctionTimer = 144;
    }

    /// <summary>
    /// Each hull and pad place's typed animation playback (#1600). Initialization sets a place's playback before its animation
    /// plays, so a place's entry always belongs to its current piece.
    /// </summary>
    private readonly Dictionary<RoomEnemySlot, AnimationPlayback<GunshipSegment>> _shipAnimations = [];

    /// <summary>Starts <paramref name="segment"/> on <paramref name="slot"/> from the next update.</summary>
    private void StartGunshipAnimation(RoomEnemySlot slot, GunshipSegment segment)
    {
        slot.AnimationClock.StepOnNextUpdate();
        _shipAnimations[slot] = new AnimationPlayback<GunshipSegment>(GunshipAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs a hull or pad piece's typed program where the shared enemy animation would.</summary>
    internal void AdvanceGunshipAnimation(RoomEnemySlot slot)
    {
        if (!slot.AnimationClock.Expire())
            return;
        AnimationPlayback<GunshipSegment> playback = _shipAnimations[slot];
        EnemyAnimationPlayback.Play(slot, GunshipAnimations.Segments, ref playback,
            cue => throw new InvalidOperationException($"The gunship's programs publish no cue; got {cue}."));
        _shipAnimations[slot] = playback;
    }

    /// <summary>Initializes the top hull and selects ordinary idle or post-Ceres descent.</summary>
    internal void InitializeGunshipTop(RoomEnemySlot slot)
    {
        _ship = new GunshipPieces(slot, _slotTable.ListedAfter(slot, 1), _slotTable.ListedAfter(slot, 2));
        var part = new GunshipPartState();
        slot.SetSpeciesState(part);
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        _shipAnimations[slot] = EnemyAnimationPlayback.Start(slot, GunshipAnimations.StartingWith(GunshipSegment.TopHull));
        if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
        {
            SamusState samus = _samusAtEnemyInitialization
                ?? throw new InvalidOperationException(
                    "Post-Ceres gunship initialization requires the loader's Samus actor.");
            // Post-Ceres initialization attaches the top hull seventeen pixels above Samus and
            // enters function three, the high-altitude descent used only after Ceres.
            slot.Position = slot.Position.WithWrappedWholeY(samus.Position.Y.Whole - 17);
            part.Function = GunshipFunction.DescendAfterCeres;
        }
        else
        {
            slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole - 25);
            part.BaseY = slot.Position.Y.Whole;
            part.Function = GunshipFunction.Idle;
        }
        part.BobTimer = 1;
        part.BobPhase = 0;
    }

    /// <summary>Initializes bottom hull or pad from the enemies its room lists before it.</summary>
    internal void InitializeGunshipBottom(RoomEnemySlot slot)
    {
        var part = new GunshipPartState();
        slot.SetSpeciesState(part);
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        _shipAnimations[slot] = EnemyAnimationPlayback.Start(slot, GunshipAnimations.StartingWith(slot.SpawnSettings<GunshipSpawnSettings>().Part == GunshipBottomPiece.EntrancePad
            ? GunshipSegment.BottomEntrancePad
            : GunshipSegment.BottomHull));

        if (slot.SpawnSettings<GunshipSpawnSettings>().Part == GunshipBottomPiece.EntrancePad)
        {
            slot.Position = slot.Position.WithWrappedWholeY(GunshipPadBorrowedHeight.Start(
                distance => _slotTable.ListedBefore(slot, distance).Position.Y.Whole));
        }
        else
        {
            if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
            {
                SamusState samus = _samusAtEnemyInitialization
                    ?? throw new InvalidOperationException(
                        "Post-Ceres gunship bottom initialization requires Samus.");
                slot.Position = slot.Position.WithWrappedWholeY(samus.Position.Y.Whole + 23);
            }
            else
            {
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + 15);
                part.EngineSoundTimer = 71;
            }
        }
        part.Function = GunshipFunction.NoOperation;
    }

    /// <summary>Runs the top hull's sound timer and landing, boarding, save, or takeoff state machine.</summary>
    internal void RunGunshipTopMain(
        RoomEnemySlot top,
        SamusState? samus,
        ControllerButton newlyPressedControllerInput)
    {
        if (!_slotTable.IsFilled(Pieces(top).EntrancePad))
            throw new InvalidDataException("Gunship top is missing its two following component slots.");

        RoomEnemySlot bottom = Pieces(top).Bottom;

        // The solid bottom decrements its engine-sound timer and reloads 70 on
        // one/underflow. The engine sound is requested before the reload, exactly as it is in
        // the gunship top's main behaviour; this periodic producer continues while the ship is idling/bobbing.
        if (--Part(bottom).EngineSoundTimer <= 0)
        {
            _audio.QueueEnemySound(EnemySound.GunshipHover, priority: SoundRequestPriority.Normal);
            Part(bottom).EngineSoundTimer = 70;
        }

        // The landed ship continuously performs its four-phase bob before dispatching the function itself, in every
        // function from waiting for the landing entrance through loading the liftoff dust.
        GunshipFunction function = Part(top).Function;
        if (BobsWhileLanded(function))
            StepGunshipBob(top);

        switch (function)
        {
            case GunshipFunction.DescendAfterCeres:
                DescendPostCeresGunship(top, samus);
                return;
            case GunshipFunction.ApplyLandingBrakes:
                BouncePostCeresGunship(top, samus);
                return;
            case GunshipFunction.WaitForLandingEntranceToOpen:
                if (TickGunshipFunctionTimer(top))
                    Part(top).Function = GunshipFunction.EjectSamus;
                return;
            case GunshipFunction.EjectSamus:
                RaisePostCeresSamus(top, samus);
                return;
            case GunshipFunction.FinishLanding:
                if (TickGunshipFunctionTimer(top))
                {
                    Part(top).Function = GunshipFunction.Idle;
                    if (samus is not null)
                        samus.Control.Unlock();
                    _frameResults.Gunship = GunshipFrameEvent.LandingCompleted;
                }
                return;
            case GunshipFunction.Idle:
                HandleIdleGunshipEntrance(top, samus, newlyPressedControllerInput);
                return;
            case GunshipFunction.WaitForEntranceToOpen:
                if (TickGunshipFunctionTimer(top))
                    Part(top).Function = GunshipFunction.LowerSamus;
                return;
            case GunshipFunction.LowerSamus:
                LowerSamusIntoGunship(top, samus);
                return;
            case GunshipFunction.WaitForEntranceToClose:
                if (TickGunshipFunctionTimer(top))
                    Part(top).Function = GunshipFunction.BeginLiftoffOrRestoreSamus;
                return;
            case GunshipFunction.BeginLiftoffOrRestoreSamus:
                RestoreSamusInGunship(top, samus);
                return;
            case GunshipFunction.HandleSaveConfirmation:
                GunshipSavePromptPending = true;
                _frameResults.Gunship = GunshipFrameEvent.SavePromptRequested;
                return;
            case GunshipFunction.WaitForExitPadToOpen:
                if (TickGunshipFunctionTimer(top))
                    Part(top).Function = GunshipFunction.RaiseSamus;
                return;
            case GunshipFunction.RaiseSamus:
                RaiseSamusOutOfGunship(top, samus);
                return;
            case GunshipFunction.FinishSamusExit:
                if (TickGunshipFunctionTimer(top))
                {
                    Part(top).Function = GunshipFunction.Idle;
                    if (samus is not null)
                        samus.Control.Unlock();
                    _frameResults.Gunship = GunshipFrameEvent.ExitCompleted;
                }
                return;
            case GunshipFunction.ReadyTakeoffEngines:
                ReadyGunshipTakeoffEngines(top);
                return;
            case GunshipFunction.FireUpEngines:
                FireUpGunshipEngines(top, samus);
                return;
            case GunshipFunction.SteadyLiftoff:
                LiftGunshipAtConstantSpeed(top, samus);
                return;
            case GunshipFunction.AcceleratingLiftoff:
                AccelerateEscapingGunship(top, samus);
                return;
            case GunshipFunction.MoveAccelerating:
                MoveEscapingGunship(top, samus);
                return;
            case GunshipFunction.NoOperation:
                throw new InvalidDataException(
                    $"Gunship function {Part(top).Function} is not translated.");
            case GunshipFunction.None:
            default:
                throw new InvalidOperationException($"Undefined GunshipFunction {function}.");
        }
    }

    /// <summary>Carries Samus and all ship parts downward, slowing below Y=768 before the landing clamp.</summary>
    private void DescendPostCeresGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship descent lost Samus.");

        // Function three carries all four actors as a rigid body. Above Y=768 it moves
        // 4.5 pixels per call; below that threshold it slows to 2.5 and clamps the
        // top hull to 1119 before beginning the game's seventeen-entry bounce table.
        // The clamp writes only whole positions; subpixels keep this call's sum.
        SignedSixteenSixteen delta = top.Position.Y.Whole < GunshipMotionDefinitions.SlowDescentY ? GunshipMotionDefinitions.FastDescentSpeed : GunshipMotionDefinitions.SlowDescentSpeed;
        AddGunshipYFixed(samus, top, delta);
        if (top.Position.Y.Whole < GunshipMotionDefinitions.LandedTopY)
            return;

        RoomEnemySlot bottom = Pieces(top).Bottom;
        RoomEnemySlot pad = Pieces(top).EntrancePad;
        top.Position = top.Position.WithWholeY(GunshipMotionDefinitions.LandedTopY);
        bottom.Position = bottom.Position.WithWholeY(GunshipMotionDefinitions.LandedBottomY);
        pad.Position = pad.Position.WithWholeY(GunshipMotionDefinitions.LandedPadY);
        Part(top).Function = GunshipFunction.ApplyLandingBrakes;
        Part(top).LandingBrakeStep = 0;
    }

    /// <summary>Applies seventeen authored brake offsets, then opens the pad and aligns Samus.</summary>
    internal void BouncePostCeresGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship bounce lost Samus.");

        int yDelta = GunshipMotion.LandingBrakeYDelta(Part(top).LandingBrakeStep);
        samus.Position += new Offset16<RoomPixels>(0, yDelta);
        foreach (RoomEnemySlot slot in Pieces(top).All)
        {
            slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + yDelta);
        }

        Part(top).LandingBrakeStep++;
        if (Part(top).LandingBrakeStep < GunshipMotionDefinitions.LandingBrakeYDeltas.Length)
            return;

        RoomEnemySlot pad = Pieces(top).EntrancePad;
        Part(top).Function = GunshipFunction.WaitForLandingEntranceToOpen;
        Part(top).BaseY = top.Position.Y.Whole;
        Part(top).BobTimer = 1;
        Part(top).BobPhase = 0;
        samus.Position = samus.Position.WithWrappedWholeX(top.Position.X.Whole + 1);
        samus.WritePreviousXPosition(samus.Position.X.Whole);
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadOpening);
        Part(top).FunctionTimer = 144;
        _frameResults.Gunship = GunshipFrameEvent.LandingPadOpened;
        _audio.QueueEnemySound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Raises Samus from the landed ship and closes its pad at the exit height.</summary>
    private void RaisePostCeresSamus(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship exit lost Samus.");

        samus.Position -= new Offset16<RoomPixels>(0, 1);
        if (CoordinateAxis.Difference(samus.Position.Y.Whole, Part(top).BaseY - 30) >= 0)
            return;

        RoomEnemySlot pad = Pieces(top).EntrancePad;
        Part(top).Function = GunshipFunction.FinishLanding;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadClosing);
        Part(top).FunctionTimer = 144;
        _frameResults.Gunship = GunshipFrameEvent.LandingPadClosed;
    }

    /// <summary>Adds the same 16.16 Y displacement to Samus and all three ship components.</summary>
    private void AddGunshipYFixed(SamusState samus, RoomEnemySlot top, SignedSixteenSixteen delta)
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
    /// Whether the landed ship bobs before running <paramref name="function"/>: every function from waiting for the
    /// landing entrance through loading the liftoff dust. The game tests its function against the range of
    /// those functions (a range which also contains Idle); firing up the engines is the first function past it.
    /// </summary>
    private static bool BobsWhileLanded(GunshipFunction function) => function is
        GunshipFunction.WaitForLandingEntranceToOpen or GunshipFunction.EjectSamus or GunshipFunction.FinishLanding or
        GunshipFunction.Idle or GunshipFunction.WaitForEntranceToOpen or GunshipFunction.LowerSamus or
        GunshipFunction.WaitForEntranceToClose or GunshipFunction.BeginLiftoffOrRestoreSamus or
        GunshipFunction.HandleSaveConfirmation or GunshipFunction.WaitForExitPadToOpen or GunshipFunction.RaiseSamus or
        GunshipFunction.FinishSamusExit or GunshipFunction.ReadyTakeoffEngines;

    /// <summary>Cycles the four idle-bob entries while moving the hull and pad together.</summary>
    internal void StepGunshipBob(RoomEnemySlot top)
    {
        if (--Part(top).BobTimer > 0)
            return;

        GunshipIdleBobDefinition bob = GunshipMotion.IdleBob(Part(top).BobPhase);
        Part(top).BobTimer = bob.Timer;
        int yDelta = bob.YDelta;
        foreach (RoomEnemySlot slot in Pieces(top).All)
        {
            slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + yDelta);
        }
        Part(top).BobPhase = (Part(top).BobPhase + 1) % GunshipMotionDefinitions.IdleBobs.Length;
    }

    /// <summary>Accepts a new Down press inside the standing entrance box and opens the pad.</summary>
    private void HandleIdleGunshipEntrance(
        RoomEnemySlot top,
        SamusState? samus,
        ControllerButton newlyPressedControllerInput)
    {
        ControllerButton newlyPressed = newlyPressedControllerInput;
        if (samus is null || !newlyPressed.HasAny(ControllerButton.Down))
            return;

        bool insideEntrance =
            CoordinateAxis.AsSigned(CoordinateAxis.Wrap(top.Position.X.Whole - 8 - samus.Position.X.Whole)) < 0 &&
            CoordinateAxis.AsSigned(CoordinateAxis.Wrap(top.Position.X.Whole + 8 - samus.Position.X.Whole)) >= 0 &&
            CoordinateAxis.AsSigned(CoordinateAxis.Wrap(top.Position.Y.Whole - 64 - samus.Position.Y.Whole)) < 0 &&
            CoordinateAxis.AsSigned(CoordinateAxis.Wrap(top.Position.Y.Whole - samus.Position.Y.Whole)) >= 0;
        if (insideEntrance && SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Standing)
        {
            RoomEnemySlot pad = Pieces(top).EntrancePad;
            Part(top).Function = GunshipFunction.WaitForEntranceToOpen;
            // The alignment stores the ship's X as both Samus's current and previous X, so the camera sees
            // no horizontal motion from this alignment.
            if (samus.Position.X.Whole != GunshipMotionDefinitions.LandedX)
            {
                samus.Position = samus.Position.WithWholeX(top.Position.X.Whole);
                samus.WritePreviousXPosition(samus.Position.X.Whole);
            }
            SamusPoseMetadata.ApplyForwardFacingPoseSetup(samus);
            samus.Control.EnterGunship();
            pad.Position = pad.Position.WithWrappedWholeY(top.Position.Y.Whole - 1);
            StartGunshipAnimation(pad, GunshipSegment.EntrancePadOpening);
            Part(top).FunctionTimer = 144;
            _frameResults.Gunship = GunshipFrameEvent.EntryStarted;
            _audio.QueueEnemySound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
        }
    }

    /// <summary>Decrements the function timer, treating one and signed underflow as expiry.</summary>
    private static bool TickGunshipFunctionTimer(RoomEnemySlot top)
    {
        return --Part(top).FunctionTimer <= 0;
    }

    /// <summary>Lowers Samus two pixels per frame until the closing-pad threshold.</summary>
    private void LowerSamusIntoGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship entry lost its Samus actor.");
        samus.Position += new Offset16<RoomPixels>(0, 2);
        if (CoordinateAxis.Difference(samus.Position.Y.Whole, Part(top).BaseY + 18) < 0)
            return;

        RoomEnemySlot pad = Pieces(top).EntrancePad;
        Part(top).Function = GunshipFunction.WaitForEntranceToClose;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadClosing);
        Part(top).FunctionTimer = 144;
        _frameResults.Gunship = GunshipFrameEvent.EntryPadClosing;
    }

    /// <summary>Refills Samus in two-unit steps or skips directly to escape takeoff after Mother Brain.</summary>
    private void RestoreSamusInGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship restoration lost its Samus actor.");

        // The Zebes-timebomb-set event is set by Mother Brain's death sequence. Bypasses every refill
        // and save-prompt branch here, starts showing the takeoff artwork, and keeps the
        // already hidden/input-locked Samus rigidly attached to the ship.
        if (_room.Services.Events.Has(StoryEvent.ZebesTimebombSet))
        {
            RoomEnemySlot bottom = Pieces(top).Bottom;
            Part(top).Function = GunshipFunction.ReadyTakeoffEngines;
            Part(top).EngineReadyingElapsed = 0;
            Part(bottom).EscapeVelocity = SignedEightEight.Zero;
            Part(bottom).RumbleFrame = 0;
            _frameResults.Gunship = GunshipFrameEvent.EscapeTakeoffStarted;
            return;
        }

        SamusEnergyRestoration.Restore(samus, 2);
        samus.Missiles = RestoreTwo(samus.Missiles, samus.MaxMissiles);
        samus.SuperMissiles = RestoreTwo(samus.SuperMissiles, samus.MaxSuperMissiles);
        samus.PowerBombs = RestoreTwo(samus.PowerBombs, samus.MaxPowerBombs);
        if (samus.ReserveEnergy < samus.MaxReserveEnergy ||
            samus.Health < samus.MaxHealth ||
            samus.Missiles < samus.MaxMissiles ||
            samus.SuperMissiles < samus.MaxSuperMissiles ||
            samus.PowerBombs < samus.MaxPowerBombs)
            return;

        // The prompt itself opens when the save-confirmation behaviour runs on the following frame.
        Part(top).Function = GunshipFunction.HandleSaveConfirmation;
    }

    /// <summary>Raises Samus two pixels per frame, then closes the pad and emits the exit event.</summary>
    private void RaiseSamusOutOfGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship exit lost its Samus actor.");
        samus.Position -= new Offset16<RoomPixels>(0, 2);
        if (CoordinateAxis.Difference(samus.Position.Y.Whole, Part(top).BaseY - 30) >= 0)
            return;

        RoomEnemySlot pad = Pieces(top).EntrancePad;
        Part(top).Function = GunshipFunction.FinishSamusExit;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadClosing);
        Part(top).FunctionTimer = 144;
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
        int elapsed = Part(top).EngineReadyingElapsed;
        if (elapsed >= GunshipTakeoffDefinitions.EngineReadyingDuration)
            throw new InvalidDataException("The gunship's takeoff readied its engines past its duration.");

        Part(top).EngineReadyingElapsed = elapsed + 1;
        if (Part(top).EngineReadyingElapsed >= GunshipTakeoffDefinitions.EngineReadyingDuration)
        {
            _room.Session.Looks.Show(GunshipTakeoffDefinitions.TakeoffLook);
            Part(top).Function = GunshipFunction.FireUpEngines;
            Part(top).EngineReadyingElapsed = 0;
        }
    }

    /// <summary>Runs the 128-frame engine-rumble function.</summary>
    private void FireUpGunshipEngines(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship takeoff lost Samus.");
        RoomEnemySlot bottom = Pieces(top).Bottom;
        RoomEnemySlot pad = Pieces(top).EntrancePad;
        int rumbleFrame = Part(bottom).RumbleFrame;
        int shake = GunshipTakeoffDefinitions.RumbleShakesDown.IsSetIn(rumbleFrame)
            ? (rumbleFrame < 64 ? 1 : 2)
            : (rumbleFrame < 64 ? -1 : -2);
        samus.Position += new Offset16<RoomPixels>(0, shake);
        top.Position = top.Position.WithWrappedWholeY(samus.Position.Y.Whole - 17);
        pad.Position = pad.Position.WithWrappedWholeY(top.Position.Y.Whole - 1);
        bottom.Position = bottom.Position.WithWrappedWholeY(samus.Position.Y.Whole + 23);

        Part(bottom).RumbleFrame++;
        if (Part(bottom).RumbleFrame == 64)
        {
            // The six calls happen after the frame counter reaches
            // exactly 64. Descending projectile allocation means the last cloud, the outer left one, occupies the
            // highest surviving slot, matching the ordinary scheduler.
            foreach (GunshipDustPlacement cloud in GunshipDustDefinitions.LiftoffClouds)
                _projectiles.SpawnGunshipLiftoffDustCloud(cloud, samus);
        }
        if (Part(bottom).RumbleFrame >= GunshipTakeoffDefinitions.EngineRumbleFrames)
        {
            Part(top).Function = GunshipFunction.SteadyLiftoff;
            Part(top).FunctionTimer = 0;
        }
    }

    /// <summary>Runs constant two-pixel liftoff.</summary>
    private void LiftGunshipAtConstantSpeed(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship liftoff lost Samus.");
        MoveGunshipAndSamusToY(top, samus, CoordinateAxis.Wrap(samus.Position.Y.Whole - 2));
        if (CoordinateAxis.Difference(top.Position.Y.Whole, 896) < 0)
        {
            Part(top).Function = GunshipFunction.AcceleratingLiftoff;
            Part(Pieces(top).Bottom).EscapeVelocity = GunshipMotionDefinitions.EscapeStartVelocity;
        }
    }

    /// <summary>Runs the accelerating takeoff function.</summary>
    private void AccelerateEscapingGunship(RoomEnemySlot top, SamusState? samus)
    {
        MoveEscapingGunship(top, samus);
        if (CoordinateAxis.Difference(top.Position.Y.Whole, 256) < 0)
        {
            Part(top).Function = GunshipFunction.MoveAccelerating;
            _frameResults.Gunship = GunshipFrameEvent.EscapeTakeoffCompleted;
        }
    }

    /// <summary>Runs the shared 8.8 velocity integrator.</summary>
    private void MoveEscapingGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Escaping gunship lost Samus.");
        RoomEnemySlot bottom = Pieces(top).Bottom;
        GunshipPartState bottomPart = Part(bottom);
        bottomPart.AccelerateEscape();

        samus.Kinematics.Position = samus.Kinematics.Position.WithY(samus.Kinematics.Position.Y.MovedBy(
            -bottomPart.EscapeVelocity.ToSixteenSixteenDelta()));
        MoveGunshipAndSamusToY(top, samus, samus.Position.Y.Whole);
    }

    /// <summary>Positions hull and pad at fixed whole-pixel offsets from Samus during escape.</summary>
    private void MoveGunshipAndSamusToY(
        RoomEnemySlot top,
        SamusState samus,
        int samusY)
    {
        samus.Position = samus.Position.WithWholeY(samusY);
        RoomEnemySlot bottom = Pieces(top).Bottom;
        RoomEnemySlot pad = Pieces(top).EntrancePad;
        top.Position = top.Position.WithWrappedWholeY(samusY - 17);
        pad.Position = pad.Position.WithWrappedWholeY(top.Position.Y.Whole - 1);
        bottom.Position = bottom.Position.WithWrappedWholeY(samusY + 23);
    }

    /// <summary>Restores a resource by two without exceeding its maximum.</summary>
    private static int RestoreTwo(int current, int maximum)
    {
        if (current >= maximum)
            return current;
        return Math.Min(current + 2, maximum);
    }

    /// <summary>Runs Gunship's main AI.</summary>
    internal void RunGunshipMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunGunshipTopMain(
            slot,
            frame.Samus,
            frame.NewlyPressedControllerInput);
    }

    /// <summary>The state of a gunship hull or pad slot.</summary>
    private static GunshipPartState Part(RoomEnemySlot slot) => GunshipPartState.Of(slot);

    /// <summary>The gunship's pieces, which <paramref name="top"/> found when it was placed.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="top"/> is not the room's placed top hull.</exception>
    private GunshipPieces Pieces(RoomEnemySlot top) =>
        _ship is { } ship && ReferenceEquals(ship.Top, top)
            ? ship
            : throw new InvalidOperationException($"Enemy {top.SlotIndex} runs the gunship but is not its placed top hull.");
}
