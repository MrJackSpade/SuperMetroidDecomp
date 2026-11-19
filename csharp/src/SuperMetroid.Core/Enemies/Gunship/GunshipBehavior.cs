using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.ShipBottomEntrance, EnemyDefinitionId.ShipTop];

    /// <summary>The gunship's top hull, which moves the ship, or its bottom hull with the entrance pad (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no gunship definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.ShipTop)
            return new GunshipTopEntity(this, place, common);
        if (definition == EnemyDefinitionId.ShipBottomEntrance)
            return new GunshipBottomEntity(this, place, common);
        throw new InvalidOperationException($"The gunship species creates no {place.DefinitionId}.");
    }

    /// <summary>Queues engine and entrance-pad sounds at the frame boundaries.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Creates gunship dust during takeoff.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The loaded room's bindings: its events and the game-state services that set up Samus's pose.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places, where the top hull finds the ship's other pieces when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>
    /// The top hull of the ship whose save prompt awaits its answer (<see cref="AnswerGunshipSavePrompt"/>), or null when
    /// no prompt is pending.
    /// </summary>
    private RoomEnemyPlace? _topAwaitingSaveAnswer;

    /// <summary>The room's enemy presentation, which shows the takeoff look once the engines fire up.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Binds the frame, room, place and presentation owners used by the ship's three-part actor.</summary>
    internal GunshipBehavior(EnemyAudioQueue audio, EnemyProjectileEngine projectiles, EnemyRoomBinding room, EnemyPlaces places, EnemyFrameResultBuilder frameResults,
        EnemyPresentation presentation)
    {
        _presentation = presentation;
        _frameResults = frameResults;
        _audio = audio;
        _projectiles = projectiles;
        _room = room;
        _places = places;
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
        RoomEnemyPlace top = _topAwaitingSaveAnswer
            ?? throw new InvalidOperationException("The gunship save prompt is not awaiting a response.");
        RoomEnemyPlace pad = Pieces(top).EntrancePad;
        _topAwaitingSaveAnswer = null;
        State(top).Phase = GunshipPhase.WaitForExitPadToOpen;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadOpening);
        State(top).PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
    }

    /// <summary>Starts <paramref name="segment"/> on <paramref name="place"/> from the next update.</summary>
    private static void StartGunshipAnimation(RoomEnemyPlace place, GunshipSegment segment)
    {
        place.AnimationClock.StepOnNextUpdate();
        place.RequireSpeciesState<GunshipPieceState>().Playback =
            new AnimationPlayback<GunshipSegment>(GunshipAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs a hull or pad piece's typed animation where the shared enemy animation would.</summary>
    internal static void AdvanceGunshipAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        GunshipPieceState piece = place.RequireSpeciesState<GunshipPieceState>();
        AnimationPlayback<GunshipSegment> playback = piece.Playback;
        EnemyAnimationPlayback.Play(place, GunshipAnimations.Segments, ref playback,
            cue => throw new InvalidOperationException($"The gunship's animations publish no cue; got {cue}."));
        piece.Playback = playback;
    }

    /// <summary>
    /// Initializes the top hull and selects ordinary idle or post-Ceres descent, which places it against the loading
    /// <paramref name="samus"/>.
    /// </summary>
    internal void InitializeGunshipTop(RoomEnemyPlace place, SamusState samus)
    {
        var pieces = new GunshipPieces(place, _places.ListedAfter(place, 1), _places.ListedAfter(place, 2));
        place.SetSpeciesState(pieces);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.SetSpeciesState(new GunshipPieceState(EnemyAnimationPlayback.Start(place, GunshipAnimations.StartingWith(GunshipSegment.TopHull))));
        GunshipState ship;
        if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
        {
            // Post-Ceres initialization attaches the top hull seventeen pixels above Samus and
            // enters the high-altitude descent used only after Ceres, which starts the ship above the room's top.
            place.Position = OffRoomPlaces.WithWholeY(place.Position, samus.Position.Y.Whole - GunshipMotionDefinitions.TopHullAboveSamus);
            ship = new GunshipState(GunshipPhase.DescendAfterCeres) { EngineSoundTimer = UpdateCountdown.For(GunshipHoverSoundDefinitions.FirstDelayAfterCeres) };
        }
        else
        {
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole - GunshipMotionDefinitions.LandedTopHullRaise);
            // The landed ship's engine sound starts as the ship is placed.
            ship = new GunshipState(GunshipPhase.Idle) { BaseY = place.Position.Y.Whole, EngineSoundTimer = UpdateCountdown.For(GunshipHoverSoundDefinitions.FirstDelay) };
        }
        ship.BobTimer = UpdateCountdown.For(1);
        ship.Bob = GunshipIdleBob.First;
        place.SetSpeciesState(ship);
        // Every placement, landed or descending after Ceres, starts the lights' glow among the room's colour effects. The
        // glow keeps this gunship (#1783): it runs until the room's colour effects are cleared and shows on these pieces.
        _frameResults.Glow = new EnemyGlowStart(GunshipLightDefinitions.Glow, pieces);
    }

    /// <summary>
    /// Initializes bottom hull or pad from the enemies its room lists before it; after Ceres the bottom hull is placed
    /// against the loading <paramref name="samus"/>.
    /// </summary>
    internal void InitializeGunshipBottom(RoomEnemyPlace place, SamusState samus)
    {
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.SetSpeciesState(new GunshipPieceState(EnemyAnimationPlayback.Start(place, GunshipAnimations.StartingWith(place.SpawnSettings<GunshipSpawnSettings>().Part == GunshipBottomPiece.EntrancePad
            ? GunshipSegment.BottomEntrancePad
            : GunshipSegment.BottomHull))));

        if (place.SpawnSettings<GunshipSpawnSettings>().Part == GunshipBottomPiece.EntrancePad)
        {
            // The hull the pad takes its height from can stand above the room's top, after Ceres.
            place.Position = OffRoomPlaces.WithWholeY(place.Position, GunshipPadBorrowedHeight.Start(
                distance => _places.ListedBefore(place, distance).Position.Y.Whole));
        }
        else
        {
            if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
            {
                place.Position = OffRoomPlaces.WithWholeY(place.Position, samus.Position.Y.Whole + GunshipMotionDefinitions.BottomHullBelowSamus);
            }
            else
            {
                place.Position = place.Position.WithWholeY(place.Position.Y.Whole + GunshipMotionDefinitions.LandedBottomHullDrop);
            }
        }
    }

    /// <summary>Runs the top hull's sound timer and landing, boarding, save, or takeoff state machine.</summary>
    internal void RunGunshipTopMain(
        RoomEnemyPlace top,
        SamusState samus,
        GameplayAction newlyPressedControllerInput)
    {
        // The engine sound plays on the update its timer runs out, which restarts it, in every phase of the ship.
        if (State(top).EngineSoundTimer.StepEnds())
        {
            _audio.QueueEnemySound(EnemySound.GunshipHover, priority: SoundRequestPriority.Normal);
            State(top).EngineSoundTimer = UpdateCountdown.For(GunshipHoverSoundDefinitions.Interval);
        }

        // The landed ship bobs before running its phase, from waiting for the landing entrance through waiting for the
        // engines to fire up for takeoff.
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
            case GunshipPhase.WaitForEngines:
                WaitForGunshipEngines(top);
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
    private static void DescendPostCeresGunship(RoomEnemyPlace top, SamusState samus)
    {
        // The after-Ceres descent carries all four actors as a rigid body. Above Y=768 it moves
        // 4.5 pixels an update; below that threshold it slows to 2.5 and clamps the
        // top hull to 1119 before beginning its seventeen-frame landing bounce.
        // The clamp writes only whole positions; the fractions of a pixel keep this update's sum.
        SignedSixteenSixteen delta = top.Position.Y.Whole < GunshipMotionDefinitions.SlowDescentY ? GunshipMotionDefinitions.FastDescentSpeed : GunshipMotionDefinitions.SlowDescentSpeed;
        MoveGunshipAndSamusVertically(samus, top, delta);
        if (top.Position.Y.Whole < GunshipMotionDefinitions.LandedTopY)
            return;

        RoomEnemyPlace bottom = Pieces(top).Bottom;
        RoomEnemyPlace pad = Pieces(top).EntrancePad;
        top.Position = top.Position.WithWholeY(GunshipMotionDefinitions.LandedTopY);
        bottom.Position = bottom.Position.WithWholeY(GunshipMotionDefinitions.LandedBottomY);
        pad.Position = pad.Position.WithWholeY(GunshipMotionDefinitions.LandedPadY);
        State(top).Phase = GunshipPhase.ApplyLandingBrakes;
        State(top).LandingBrake = GunshipLandingBrake.First;
    }

    /// <summary>Applies seventeen authored brake offsets, then opens the pad and aligns Samus.</summary>
    internal void BouncePostCeresGunship(RoomEnemyPlace top, SamusState samus)
    {
        GunshipLandingBrake brake = State(top).LandingBrake;
        int yDelta = brake.YDelta;
        samus.Position += new Offset16<RoomPixels>(0, yDelta);
        foreach (RoomEnemyPlace place in Pieces(top).All)
        {
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole + yDelta);
        }

        if (brake.Next is { } next)
        {
            State(top).LandingBrake = next;
            return;
        }

        RoomEnemyPlace pad = Pieces(top).EntrancePad;
        State(top).Phase = GunshipPhase.WaitForLandingEntranceToOpen;
        State(top).BaseY = top.Position.Y.Whole;
        State(top).BobTimer = UpdateCountdown.For(1);
        State(top).Bob = GunshipIdleBob.First;
        samus.Position = samus.Position.WithWholeX(top.Position.X.Whole + GunshipMotionDefinitions.LandedSamusRightOfShip);
        samus.WritePreviousXPosition(samus.Position.X.Whole);
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadOpening);
        State(top).PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _frameResults.Gunship = GunshipFrameEvent.LandingPadOpened;
        _audio.QueueEnemySound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Raises Samus from the landed ship and closes its pad at the exit height.</summary>
    private void RaisePostCeresSamus(RoomEnemyPlace top, SamusState samus)
    {
        samus.Position -= new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.ArrivalRiseStep);
        // The landed ship and Samus rising out of it lie inside the landing site, so her height is compared plainly.
        if (samus.Position.Y.Whole >= State(top).BaseY - GunshipBoardingDefinitions.ExitHeight)
            return;

        RoomEnemyPlace pad = Pieces(top).EntrancePad;
        State(top).Phase = GunshipPhase.FinishLanding;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadClosing);
        State(top).PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _frameResults.Gunship = GunshipFrameEvent.LandingPadClosed;
    }

    /// <summary>Moves Samus and the ship's three pieces down by <paramref name="delta"/> together.</summary>
    private static void MoveGunshipAndSamusVertically(SamusState samus, RoomEnemyPlace top, SignedSixteenSixteen delta)
    {
        FixedCoordinate samusY = new FixedCoordinate(samus.Position.Y.Whole, samus.Kinematics.Position.Y.Fraction)
            .MovedBy(delta);
        samus.Position = samus.Position.WithWholeY(samusY.Whole);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(samusY.Fraction);

        foreach (RoomEnemyPlace place in Pieces(top).All)
        {
            place.Position = place.Position.WithY(place.Position.Y.MovedBy(delta));
        }
    }

    /// <summary>
    /// Whether the landed ship bobs before running <paramref name="phase"/>: every phase from waiting for the
    /// landing entrance through waiting for the engines, Idle among them; firing up the engines is the first that does not.
    /// </summary>
    private static bool BobsWhileLanded(GunshipPhase phase) => phase is
        GunshipPhase.WaitForLandingEntranceToOpen or GunshipPhase.EjectSamus or GunshipPhase.FinishLanding or
        GunshipPhase.Idle or GunshipPhase.WaitForEntranceToOpen or GunshipPhase.LowerSamus or
        GunshipPhase.WaitForEntranceToClose or GunshipPhase.BeginLiftoffOrRestoreSamus or
        GunshipPhase.HandleSaveConfirmation or GunshipPhase.WaitForExitPadToOpen or GunshipPhase.RaiseSamus or
        GunshipPhase.FinishSamusExit or GunshipPhase.WaitForEngines;

    /// <summary>Cycles the four idle-bob phases while moving the hull and pad together.</summary>
    internal static void StepGunshipBob(RoomEnemyPlace top)
    {
        if (!State(top).BobTimer.Step())
            return;

        GunshipIdleBob bob = State(top).Bob;
        State(top).BobTimer = UpdateCountdown.For(bob.Timer);
        int yDelta = bob.YDelta;
        foreach (RoomEnemyPlace place in Pieces(top).All)
        {
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole + yDelta);
        }
        State(top).Bob = bob.Next;
    }

    /// <summary>Accepts a new Down press inside the standing entrance box and opens the pad.</summary>
    private void HandleIdleGunshipEntrance(
        RoomEnemyPlace top,
        SamusState samus,
        GameplayAction newlyPressedControllerInput)
    {
        GameplayAction newlyPressed = newlyPressedControllerInput;
        if (!newlyPressed.HasAny(GameplayAction.Down))
            return;

        // Samus stands within the entrance's half width of the ship's middle, the left edge excluded, and less than the
        // entrance's height above its top. She stands on the landed ship, inside the landing site, so her offset from it is
        // a plain one.
        int samusFromShipX = samus.Position.X.Whole - top.Position.X.Whole;
        int samusFromShipY = samus.Position.Y.Whole - top.Position.Y.Whole;
        bool insideEntrance =
            samusFromShipX > -GunshipBoardingDefinitions.EntranceHalfWidth && samusFromShipX <= GunshipBoardingDefinitions.EntranceHalfWidth &&
            samusFromShipY > -GunshipBoardingDefinitions.EntranceHeight && samusFromShipY <= 0;
        if (insideEntrance && SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Standing)
        {
            RoomEnemyPlace pad = Pieces(top).EntrancePad;
            State(top).Phase = GunshipPhase.WaitForEntranceToOpen;
            // Samus is set at the ship's X, her previous X with her, so the camera follows no horizontal motion
            // from this alignment.
            if (samus.Position.X.Whole != GunshipMotionDefinitions.LandedX)
            {
                samus.Position = samus.Position.WithWholeX(top.Position.X.Whole);
                samus.WritePreviousXPosition(samus.Position.X.Whole);
            }
            SamusPoseMetadata.ApplyForwardFacingPoseSetup(samus);
            samus.Control.EnterGunship();
            pad.Position = pad.Position.WithWholeY(top.Position.Y.Whole - GunshipMotionDefinitions.PadAboveTopHull);
            StartGunshipAnimation(pad, GunshipSegment.EntrancePadOpening);
            State(top).PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
            _frameResults.Gunship = GunshipFrameEvent.EntryStarted;
            _audio.QueueEnemySound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
        }
    }

    /// <summary>Counts the phase's wait: the update it ends on ends the waiting phase (#2340).</summary>
    private static bool TickGunshipPhaseTimer(RoomEnemyPlace top) => State(top).PhaseTimer.StepEnds();

    /// <summary>Lowers Samus into the ship until she is aboard, then closes the pad.</summary>
    private void LowerSamusIntoGunship(RoomEnemyPlace top, SamusState samus)
    {
        samus.Position += new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.BoardingStep);
        // Samus sinks into the landed ship, inside the landing site, so her height is compared plainly.
        if (samus.Position.Y.Whole < State(top).BaseY + GunshipBoardingDefinitions.BoardedDepth)
            return;

        RoomEnemyPlace pad = Pieces(top).EntrancePad;
        State(top).Phase = GunshipPhase.WaitForEntranceToClose;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadClosing);
        State(top).PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _frameResults.Gunship = GunshipFrameEvent.EntryPadClosing;
    }

    /// <summary>Refills Samus by the ship's refill step or skips directly to escape takeoff after Mother Brain.</summary>
    private void RestoreSamusInGunship(RoomEnemyPlace top, SamusState samus)
    {
        // Once Mother Brain's death has set the Zebes timebomb, the ship neither refills Samus nor offers to save: it
        // waits for its engines to fire up for the escape at once, carrying the hidden Samus with it.
        if (_room.Services.Events.Has(StoryEvent.ZebesTimebombSet))
        {
            State(top).Phase = GunshipPhase.WaitForEngines;
            State(top).PhaseTimer = UpdateCountdown.For(GunshipTakeoffDefinitions.EngineStartUpdates);
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
    private void RaiseSamusOutOfGunship(RoomEnemyPlace top, SamusState samus)
    {
        samus.Position -= new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.LeavingStep);
        // Samus rises out of the landed ship, inside the landing site, so her height is compared plainly.
        if (samus.Position.Y.Whole >= State(top).BaseY - GunshipBoardingDefinitions.ExitHeight)
            return;

        RoomEnemyPlace pad = Pieces(top).EntrancePad;
        State(top).Phase = GunshipPhase.FinishSamusExit;
        StartGunshipAnimation(pad, GunshipSegment.EntrancePadClosing);
        State(top).PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _frameResults.Gunship = GunshipFrameEvent.ExitPadClosing;
        _audio.QueueEnemySound(RoomSound.GunshipPadClosing, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Waits one update for the engines to fire up: on the wait's last update the gunship shows its takeoff look from this
    /// update's frame on, and the engines fire up.
    /// </summary>
    internal void WaitForGunshipEngines(RoomEnemyPlace top)
    {
        if (!TickGunshipPhaseTimer(top))
            return;

        _presentation.ShowLook(GunshipTakeoffDefinitions.TakeoffLook);
        State(top).Phase = GunshipPhase.FireUpEngines;
    }

    /// <summary>Runs the 128-frame engine-rumble phase.</summary>
    private void FireUpGunshipEngines(RoomEnemyPlace top, SamusState samus)
    {
        UpdateCountdown rumble = State(top).Rumble;
        int rumbleFrame = rumble.Age;
        bool strong = rumbleFrame >= GunshipTakeoffDefinitions.RumbleStrengthensFrame;
        // The rumble shakes down and up by turns, one shake each update (#2339).
        int shakeSize = strong ? GunshipTakeoffDefinitions.StrongRumbleShake : GunshipTakeoffDefinitions.RumbleShake;
        int shake = GunshipTakeoffDefinitions.RumbleShakesDown.IsSetIn(rumbleFrame) ? shakeSize : -shakeSize;
        samus.Position += new Offset16<RoomPixels>(0, shake);
        PlaceGunshipAroundSamus(top, samus.Position.Y.Whole);

        bool rumbled = rumble.Step();
        if (rumble.Age == GunshipTakeoffDefinitions.RumbleStrengthensFrame)
        {
            // The six clouds rise as the rumble strengthens. Each takes the highest projectile place still free, so
            // the last cloud, the outer left one, takes the highest place the other five left.
            foreach (GunshipDustPlacement cloud in GunshipDustDefinitions.LiftoffClouds)
                GunshipLiftoffDust.SpawnCloud(_projectiles, cloud, samus);
        }
        if (rumbled)
            State(top).Phase = GunshipPhase.SteadyLiftoff;
    }

    /// <summary>Runs constant two-pixel liftoff.</summary>
    private static void LiftGunshipAtConstantSpeed(RoomEnemyPlace top, SamusState samus)
    {
        // The steady liftoff rises two pixels an update from the landed ship until the hull passes 896, deep inside the
        // landing site, so it moves and compares plainly.
        MoveGunshipAndSamusToY(top, samus, samus.Position.Y.Whole - GunshipTakeoffDefinitions.SteadyLiftoffRise);
        if (top.Position.Y.Whole < GunshipTakeoffDefinitions.AcceleratingLiftoffY)
        {
            State(top).Phase = GunshipPhase.AcceleratingLiftoff;
            State(top).EscapeVelocity = GunshipMotionDefinitions.EscapeStartVelocity;
        }
    }

    /// <summary>Runs the accelerating takeoff phase.</summary>
    private void AccelerateEscapingGunship(RoomEnemyPlace top, SamusState samus)
    {
        MoveEscapingGunship(top, samus);
        // The accelerating takeoff ends once the hull passes 256, long before its speed could carry it past the room's top
        // in one update, so it compares plainly.
        if (top.Position.Y.Whole < GunshipTakeoffDefinitions.TakeoffCompleteY)
        {
            State(top).Phase = GunshipPhase.MoveAccelerating;
            _frameResults.Gunship = GunshipFrameEvent.EscapeTakeoffCompleted;
        }
    }

    /// <summary>Carries the escaping ship and Samus on up, faster each update.</summary>
    private static void MoveEscapingGunship(RoomEnemyPlace top, SamusState samus)
    {
        GunshipState ship = State(top);
        ship.AccelerateEscape();

        samus.Kinematics.Position = samus.Kinematics.Position.WithY(samus.Kinematics.Position.Y.MovedBy(
            -ship.EscapeVelocity.ToSixteenSixteenDelta()));
        MoveGunshipAndSamusToY(top, samus, samus.Position.Y.Whole);
    }

    /// <summary>Positions hull and pad at fixed whole-pixel offsets from Samus during escape.</summary>
    private static void MoveGunshipAndSamusToY(
        RoomEnemyPlace top,
        SamusState samus,
        int samusY)
    {
        samus.Position = samus.Position.WithWholeY(samusY);
        PlaceGunshipAroundSamus(top, samusY);
    }

    /// <summary>
    /// Places the top hull, the entrance pad and the bottom hull where they sit around Samus at <paramref name="samusY"/>: the
    /// escaping ship climbs past the room's top, and arrives from above it after Ceres, so each piece keeps its place as
    /// <see cref="OffRoomPlaces"/> does.
    /// </summary>
    private static void PlaceGunshipAroundSamus(RoomEnemyPlace top, int samusY)
    {
        RoomEnemyPlace bottom = Pieces(top).Bottom;
        RoomEnemyPlace pad = Pieces(top).EntrancePad;
        top.Position = OffRoomPlaces.WithWholeY(top.Position, samusY - GunshipMotionDefinitions.TopHullAboveSamus);
        pad.Position = OffRoomPlaces.WithWholeY(pad.Position, top.Position.Y.Whole - GunshipMotionDefinitions.PadAboveTopHull);
        bottom.Position = OffRoomPlaces.WithWholeY(bottom.Position, samusY + GunshipMotionDefinitions.BottomHullBelowSamus);
    }

    /// <summary>Restores a resource by the ship's refill step without exceeding its maximum.</summary>
    private static int RestoreStep(int current, int maximum)
    {
        if (current >= maximum)
            return current;
        return Math.Min(current + GunshipBoardingDefinitions.RefillStep, maximum);
    }

    /// <summary>Runs Gunship's main AI.</summary>
    internal void RunGunshipMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunGunshipTopMain(
            place,
            frame.Samus,
            frame.NewlyPressedControllerInput);
    }

    /// <summary>The state of the ship <paramref name="top"/> runs.</summary>
    private static GunshipState State(RoomEnemyPlace top) => GunshipState.Of(top);

    /// <summary>The gunship's pieces, which <paramref name="top"/> found when it was placed.</summary>
    private static GunshipPieces Pieces(RoomEnemyPlace top) => top.RequireSpeciesState<GunshipPieces>();
}
