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
internal sealed class GunshipBehavior : IEnemyEntityFactory, IEnemySettingsFactory<GunshipSpawnSettings>
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyPassEvents _passEvents;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.ShipBottomEntrance, EnemyDefinitionId.ShipTop];

    /// <summary>The gunship's bottom hull or its entrance pad, as its placement names (#2146).</summary>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, GunshipSpawnSettings settings) =>
        new GunshipBottomEntity(this, place, common, setup, settings);

    /// <inheritdoc/>
    void IEnemySpeciesFactory.RegisterIn(EnemySpeciesRegistry registry)
    {
        IEnemyEntityFactory untold = this;
        Ensure.NotNull(registry).Add(untold);
        registry.Add<GunshipSpawnSettings>(this);
    }

    /// <summary>The gunship's top hull, which moves the ship, or its bottom hull with the entrance pad (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no gunship definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.ShipTop)
            return new GunshipTopEntity(this, place, common, setup);
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

    /// <summary>The state each top hull made, as it was placed, for its bottom hull and entrance pad (#2554).</summary>
    private readonly EnemyPartLinks<GunshipPieceState> _pieceStates = new();

    /// <summary>The room's enemy presentation, which shows the takeoff look once the engines fire up.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Binds the frame, room, place and presentation owners used by the ship's three-part actor.</summary>
    internal GunshipBehavior(EnemyAudioQueue audio, EnemyProjectileEngine projectiles, EnemyRoomBinding room, EnemyPlaces places, EnemyPassEvents passEvents,
        EnemyPresentation presentation)
    {
        _presentation = presentation;
        _passEvents = passEvents;
        _audio = audio;
        _projectiles = projectiles;
        _room = room;
        _places = places;
    }
    /// <summary>Room load: the placement scenario.</summary>
    internal void BeginRoomLoad(GunshipLoadScenario scenario) => _gunshipLoadScenario = scenario;
    /// <summary>Selects ordinary Landing Site placement or the high-altitude post-Ceres descent.</summary>
    private GunshipLoadScenario _gunshipLoadScenario;
    /// <summary>
    /// Resumes the ship once the save message box has its answer. The ship plays the same exit animation for either
    /// answer; the message box flow makes the save itself, as a save station's, when the answer is yes.
    /// </summary>
    /// <param name="pieces">The pieces of the ship that asked.</param>
    /// <param name="ship">The state of the ship that asked.</param>
    internal static void AnswerGunshipSavePrompt(GunshipPieces pieces, GunshipState ship)
    {
        ship.Phase = GunshipPhase.WaitForExitPadToOpen;
        StartGunshipAnimation(pieces.EntrancePad, pieces.EntrancePadState, GunshipSegment.EntrancePadOpening);
        ship.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
    }

    /// <summary>Starts <paramref name="segment"/> on <paramref name="place"/> from the next update.</summary>
    private static void StartGunshipAnimation(RoomEnemyPlace place, GunshipPieceState state, GunshipSegment segment)
    {
        place.AnimationClock.StepOnNextUpdate();
        state.Playback =
            new AnimationPlayback<GunshipSegment>(GunshipAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs a hull or pad piece's typed animation where the shared enemy animation would.</summary>
    internal static void AdvanceGunshipAnimation(RoomEnemyPlace place, GunshipPieceState piece)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<GunshipSegment> playback = piece.Playback;
        EnemyAnimationPlayback.Play(place, GunshipAnimations.Segments, ref playback,
            cue => throw new InvalidOperationException($"The gunship's animations publish no cue; got {cue}."));
        piece.Playback = playback;
    }

    /// <summary>
    /// Initializes the top hull and selects ordinary idle or post-Ceres descent, which places it against the loading
    /// <paramref name="samus"/>.
    /// </summary>
    internal GunshipTop InitializeGunshipTop(RoomEnemyPlace place, SamusState samus)
    {
        // The top hull makes its bottom hull's and entrance pad's states, which each takes as it is placed after it (#2554).
        var pieces = new GunshipPieces(
            place,
            _places.ListedAfter(place, 1),
            _places.ListedAfter(place, 2),
            new GunshipPieceState(new(GunshipAnimations.StartingWith(GunshipSegment.BottomHull), AnimationPlace.StartOfAnimation)),
            new GunshipPieceState(new(GunshipAnimations.StartingWith(GunshipSegment.BottomEntrancePad), AnimationPlace.StartOfAnimation)));
        _pieceStates.Place(pieces.Bottom, pieces.BottomState);
        _pieceStates.Place(pieces.EntrancePad, pieces.EntrancePadState);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        var piece = new GunshipPieceState(EnemyAnimationPlayback.Start(place, GunshipAnimations.StartingWith(GunshipSegment.TopHull)));
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
        ship.BobTimer = UpdateCountdown.LastingOneUpdate();
        ship.Bob = GunshipIdleBob.First;
        // Every placement, landed or descending after Ceres, starts the lights' glow among the room's colour effects. The
        // glow keeps this gunship (#1783): it runs until the room's colour effects are cleared and shows on these pieces.
        _passEvents.Publish(new EnemyGlowStarted(new EnemyGlowStart(GunshipLightDefinitions.Glow, pieces)));
        return new GunshipTop(pieces, piece, ship);
    }

    /// <summary>
    /// Initializes bottom hull or pad from the enemies its room lists before it; after Ceres the bottom hull is placed
    /// against the loading <paramref name="samus"/>.
    /// </summary>
    internal GunshipPieceState InitializeGunshipBottom(RoomEnemyPlace place, SamusState samus, GunshipSpawnSettings settings)
    {
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        // The piece takes the state the top hull, listed before it, made for it as it was placed.
        GunshipPieceState state = _pieceStates.Of(place);
        state.Playback = EnemyAnimationPlayback.Start(place, GunshipAnimations.StartingWith(settings.Part == GunshipBottomPiece.EntrancePad
            ? GunshipSegment.BottomEntrancePad
            : GunshipSegment.BottomHull));

        if (settings.Part == GunshipBottomPiece.EntrancePad)
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
        return state;
    }

    /// <summary>Runs the top hull's sound timer and landing, boarding, save, or takeoff state machine.</summary>
    internal void RunGunshipTopMain(
        RoomEnemyPlace top, GunshipState state, GunshipPieces pieces,
        SamusState samus,
        GameplayAction newlyPressedControllerInput)
    {
        // The engine sound plays on the update its timer runs out, which restarts it, in every phase of the ship.
        if (state.EngineSoundTimer.StepEnds())
        {
            _audio.QueueEnemySound(EnemySound.GunshipHover, priority: SoundRequestPriority.Normal);
            state.EngineSoundTimer = UpdateCountdown.For(GunshipHoverSoundDefinitions.Interval);
        }

        // The landed ship bobs before running its phase, from waiting for the landing entrance through waiting for the
        // engines to fire up for takeoff.
        GunshipPhase phase = state.Phase;
        if (BobsWhileLanded(phase))
            StepGunshipBob(pieces, state);

        switch (phase)
        {
            case GunshipPhase.DescendAfterCeres:
                DescendPostCeresGunship(top, state, pieces, samus);
                return;
            case GunshipPhase.ApplyLandingBrakes:
                BouncePostCeresGunship(top, pieces, state, samus);
                return;
            case GunshipPhase.WaitForLandingEntranceToOpen:
                if (TickGunshipPhaseTimer(state))
                    state.Phase = GunshipPhase.EjectSamus;
                return;
            case GunshipPhase.EjectSamus:
                RaisePostCeresSamus(pieces, state, samus);
                return;
            case GunshipPhase.FinishLanding:
                if (TickGunshipPhaseTimer(state))
                {
                    state.Phase = GunshipPhase.Idle;
                    samus.Control.Unlock();
                    _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.LandingCompleted));
                }
                return;
            case GunshipPhase.Idle:
                HandleIdleGunshipEntrance(top, state, pieces, samus, newlyPressedControllerInput);
                return;
            case GunshipPhase.WaitForEntranceToOpen:
                if (TickGunshipPhaseTimer(state))
                    state.Phase = GunshipPhase.LowerSamus;
                return;
            case GunshipPhase.LowerSamus:
                LowerSamusIntoGunship(pieces, state, samus);
                return;
            case GunshipPhase.WaitForEntranceToClose:
                if (TickGunshipPhaseTimer(state))
                    state.Phase = GunshipPhase.BeginLiftoffOrRestoreSamus;
                return;
            case GunshipPhase.BeginLiftoffOrRestoreSamus:
                RestoreSamusInGunship(state, samus);
                return;
            case GunshipPhase.HandleSaveConfirmation:
                _passEvents.Publish(new GunshipSavePromptRequested(new GunshipSavePrompt(pieces, state)));
                return;
            case GunshipPhase.WaitForExitPadToOpen:
                if (TickGunshipPhaseTimer(state))
                    state.Phase = GunshipPhase.RaiseSamus;
                return;
            case GunshipPhase.RaiseSamus:
                RaiseSamusOutOfGunship(pieces, state, samus);
                return;
            case GunshipPhase.FinishSamusExit:
                if (TickGunshipPhaseTimer(state))
                {
                    state.Phase = GunshipPhase.Idle;
                    samus.Control.Unlock();
                    _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.ExitCompleted));
                }
                return;
            case GunshipPhase.WaitForEngines:
                WaitForGunshipEngines(state);
                return;
            case GunshipPhase.FireUpEngines:
                FireUpGunshipEngines(top, state, pieces, samus);
                return;
            case GunshipPhase.SteadyLiftoff:
                LiftGunshipAtConstantSpeed(top, state, pieces, samus);
                return;
            case GunshipPhase.AcceleratingLiftoff:
                AccelerateEscapingGunship(top, state, pieces, samus);
                return;
            case GunshipPhase.MoveAccelerating:
                MoveEscapingGunship(top, state, pieces, samus);
                return;
            default:
                throw new InvalidOperationException($"The gunship has no behaviour for phase {phase}.");
        }
    }

    /// <summary>Carries Samus and all ship parts downward, slowing below Y=768 before the landing clamp.</summary>
    private static void DescendPostCeresGunship(RoomEnemyPlace top, GunshipState gunshipState, GunshipPieces state, SamusState samus)
    {
        // The after-Ceres descent carries all four actors as a rigid body. Above Y=768 it moves
        // 4.5 pixels an update; below that threshold it slows to 2.5 and clamps the
        // top hull to 1119 before beginning its seventeen-frame landing bounce.
        // The clamp writes only whole positions; the fractions of a pixel keep this update's sum.
        SignedSixteenSixteen delta = top.Position.Y.Whole < GunshipMotionDefinitions.SlowDescentY ? GunshipMotionDefinitions.FastDescentSpeed : GunshipMotionDefinitions.SlowDescentSpeed;
        MoveGunshipAndSamusVertically(samus, state, delta);
        if (top.Position.Y.Whole < GunshipMotionDefinitions.LandedTopY)
            return;

        RoomEnemyPlace bottom = state.Bottom;
        RoomEnemyPlace pad = state.EntrancePad;
        top.Position = top.Position.WithWholeY(GunshipMotionDefinitions.LandedTopY);
        bottom.Position = bottom.Position.WithWholeY(GunshipMotionDefinitions.LandedBottomY);
        pad.Position = pad.Position.WithWholeY(GunshipMotionDefinitions.LandedPadY);
        gunshipState.Phase = GunshipPhase.ApplyLandingBrakes;
        gunshipState.LandingBrake = GunshipLandingBrake.First;
    }

    /// <summary>Applies seventeen authored brake offsets, then opens the pad and aligns Samus.</summary>
    internal void BouncePostCeresGunship(RoomEnemyPlace top, GunshipPieces gunshipPieces, GunshipState state, SamusState samus)
    {
        GunshipLandingBrake brake = state.LandingBrake;
        int yDelta = brake.YDelta;
        samus.Position += new Offset16<RoomPixels>(0, yDelta);
        foreach (RoomEnemyPlace place in gunshipPieces.All)
        {
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole + yDelta);
        }

        if (brake.Next is { } next)
        {
            state.LandingBrake = next;
            return;
        }

        RoomEnemyPlace pad = gunshipPieces.EntrancePad;
        state.Phase = GunshipPhase.WaitForLandingEntranceToOpen;
        state.BaseY = top.Position.Y.Whole;
        state.BobTimer = UpdateCountdown.LastingOneUpdate();
        state.Bob = GunshipIdleBob.First;
        samus.Position = samus.Position.WithWholeX(top.Position.X.Whole + GunshipMotionDefinitions.LandedSamusRightOfShip);
        samus.WritePreviousXPosition(samus.Position.X.Whole);
        StartGunshipAnimation(pad, gunshipPieces.EntrancePadState, GunshipSegment.EntrancePadOpening);
        state.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.LandingPadOpened));
        _audio.QueueEnemySound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Raises Samus from the landed ship and closes its pad at the exit height.</summary>
    private void RaisePostCeresSamus(GunshipPieces gunshipPieces, GunshipState state, SamusState samus)
    {
        samus.Position -= new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.ArrivalRiseStep);
        // The landed ship and Samus rising out of it lie inside the landing site, so her height is compared plainly.
        if (samus.Position.Y.Whole >= state.BaseY - GunshipBoardingDefinitions.ExitHeight)
            return;

        RoomEnemyPlace pad = gunshipPieces.EntrancePad;
        state.Phase = GunshipPhase.FinishLanding;
        StartGunshipAnimation(pad, gunshipPieces.EntrancePadState, GunshipSegment.EntrancePadClosing);
        state.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.LandingPadClosed));
    }

    /// <summary>Moves Samus and the ship's three pieces down by <paramref name="delta"/> together.</summary>
    private static void MoveGunshipAndSamusVertically(SamusState samus, GunshipPieces state, SignedSixteenSixteen delta)
    {
        samus.Position = samus.Position.WithY(samus.Position.Y.MovedBy(delta));

        foreach (RoomEnemyPlace place in state.All)
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
    internal static void StepGunshipBob(GunshipPieces gunshipPieces, GunshipState state)
    {
        if (!state.BobTimer.Step())
            return;

        GunshipIdleBob bob = state.Bob;
        state.BobTimer = UpdateCountdown.For(bob.Timer);
        int yDelta = bob.YDelta;
        foreach (RoomEnemyPlace place in gunshipPieces.All)
        {
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole + yDelta);
        }
        state.Bob = bob.Next;
    }

    /// <summary>Accepts a new Down press inside the standing entrance box and opens the pad.</summary>
    private void HandleIdleGunshipEntrance(
        RoomEnemyPlace top, GunshipState gunshipState, GunshipPieces state,
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
            RoomEnemyPlace pad = state.EntrancePad;
            gunshipState.Phase = GunshipPhase.WaitForEntranceToOpen;
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
            StartGunshipAnimation(pad, state.EntrancePadState, GunshipSegment.EntrancePadOpening);
            gunshipState.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
            _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.EntryStarted));
            _audio.QueueEnemySound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
        }
    }

    /// <summary>Counts the phase's wait: the update it ends on ends the waiting phase (#2340).</summary>
    private static bool TickGunshipPhaseTimer(GunshipState state) => state.PhaseTimer.StepEnds();

    /// <summary>Lowers Samus into the ship until she is aboard, then closes the pad.</summary>
    private void LowerSamusIntoGunship(GunshipPieces gunshipPieces, GunshipState state, SamusState samus)
    {
        samus.Position += new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.BoardingStep);
        // Samus sinks into the landed ship, inside the landing site, so her height is compared plainly.
        if (samus.Position.Y.Whole < state.BaseY + GunshipBoardingDefinitions.BoardedDepth)
            return;

        RoomEnemyPlace pad = gunshipPieces.EntrancePad;
        state.Phase = GunshipPhase.WaitForEntranceToClose;
        StartGunshipAnimation(pad, gunshipPieces.EntrancePadState, GunshipSegment.EntrancePadClosing);
        state.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.EntryPadClosing));
    }

    /// <summary>Refills Samus by the ship's refill step or skips directly to escape takeoff after Mother Brain.</summary>
    private void RestoreSamusInGunship(GunshipState state, SamusState samus)
    {
        // Once Mother Brain's death has set the Zebes timebomb, the ship neither refills Samus nor offers to save: it
        // waits for its engines to fire up for the escape at once, carrying the hidden Samus with it.
        if (_room.Services.Events.Has(StoryEvent.ZebesTimebombSet))
        {
            state.Phase = GunshipPhase.WaitForEngines;
            state.PhaseTimer = UpdateCountdown.For(GunshipTakeoffDefinitions.EngineStartUpdates);
            _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.EscapeTakeoffStarted));
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
        state.Phase = GunshipPhase.HandleSaveConfirmation;
    }

    /// <summary>Raises Samus out of the ship, then closes the pad and emits the exit event.</summary>
    private void RaiseSamusOutOfGunship(GunshipPieces gunshipPieces, GunshipState state, SamusState samus)
    {
        samus.Position -= new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.LeavingStep);
        // Samus rises out of the landed ship, inside the landing site, so her height is compared plainly.
        if (samus.Position.Y.Whole >= state.BaseY - GunshipBoardingDefinitions.ExitHeight)
            return;

        RoomEnemyPlace pad = gunshipPieces.EntrancePad;
        state.Phase = GunshipPhase.FinishSamusExit;
        StartGunshipAnimation(pad, gunshipPieces.EntrancePadState, GunshipSegment.EntrancePadClosing);
        state.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.ExitPadClosing));
        _audio.QueueEnemySound(RoomSound.GunshipPadClosing, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Waits one update for the engines to fire up: on the wait's last update the gunship shows its takeoff look from this
    /// update's frame on, and the engines fire up.
    /// </summary>
    internal void WaitForGunshipEngines(GunshipState state)
    {
        if (!TickGunshipPhaseTimer(state))
            return;

        _presentation.ShowLook(GunshipTakeoffDefinitions.TakeoffLook);
        state.Phase = GunshipPhase.FireUpEngines;
    }

    /// <summary>Runs the 128-frame engine-rumble phase.</summary>
    private void FireUpGunshipEngines(RoomEnemyPlace top, GunshipState state, GunshipPieces pieces, SamusState samus)
    {
        UpdateCountdown rumble = state.Rumble;
        int rumbleFrame = rumble.Age;
        bool strong = rumbleFrame >= GunshipTakeoffDefinitions.RumbleStrengthensFrame;
        // The rumble shakes down and up by turns, one shake each update (#2339).
        int shakeSize = strong ? GunshipTakeoffDefinitions.StrongRumbleShake : GunshipTakeoffDefinitions.RumbleShake;
        int shake = shakeSize.Toward(GunshipTakeoffDefinitions.RumbleShakesDown.IsSetIn(rumbleFrame) ? VerticalDirection.Down : VerticalDirection.Up);
        samus.Position += new Offset16<RoomPixels>(0, shake);
        PlaceGunshipAroundSamus(top, pieces, samus.Position.Y.Whole);

        bool rumbled = rumble.Step();
        if (rumble.Age == GunshipTakeoffDefinitions.RumbleStrengthensFrame)
        {
            // The six clouds rise as the rumble strengthens. Each takes the highest projectile place still free, so
            // the last cloud, the outer left one, takes the highest place the other five left.
            foreach (GunshipDustPlacement cloud in GunshipDustDefinitions.LiftoffClouds)
                GunshipLiftoffDust.SpawnCloud(_projectiles, cloud, samus);
        }
        if (rumbled)
            state.Phase = GunshipPhase.SteadyLiftoff;
    }

    /// <summary>Runs constant two-pixel liftoff.</summary>
    private static void LiftGunshipAtConstantSpeed(RoomEnemyPlace top, GunshipState state, GunshipPieces pieces, SamusState samus)
    {
        // The steady liftoff rises two pixels an update from the landed ship until the hull passes 896, deep inside the
        // landing site, so it moves and compares plainly.
        MoveGunshipAndSamusToY(top, pieces, samus, samus.Position.Y.Whole - GunshipTakeoffDefinitions.SteadyLiftoffRise);
        if (top.Position.Y.Whole < GunshipTakeoffDefinitions.AcceleratingLiftoffY)
        {
            state.Phase = GunshipPhase.AcceleratingLiftoff;
            state.EscapeVelocity = GunshipMotionDefinitions.EscapeStartVelocity;
        }
    }

    /// <summary>Runs the accelerating takeoff phase.</summary>
    private void AccelerateEscapingGunship(RoomEnemyPlace top, GunshipState state, GunshipPieces pieces, SamusState samus)
    {
        MoveEscapingGunship(top, state, pieces, samus);
        // The accelerating takeoff ends once the hull passes 256, long before its speed could carry it past the room's top
        // in one update, so it compares plainly.
        if (top.Position.Y.Whole < GunshipTakeoffDefinitions.TakeoffCompleteY)
        {
            state.Phase = GunshipPhase.MoveAccelerating;
            _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.EscapeTakeoffCompleted));
        }
    }

    /// <summary>Carries the escaping ship and Samus on up, faster each update.</summary>
    private static void MoveEscapingGunship(RoomEnemyPlace top, GunshipState ship, GunshipPieces pieces, SamusState samus)
    {
        ship.AccelerateEscape();

        samus.Kinematics.Position = samus.Kinematics.Position.WithY(samus.Kinematics.Position.Y.MovedBy(
            -ship.EscapeVelocity.ToSixteenSixteenDelta()));
        MoveGunshipAndSamusToY(top, pieces, samus, samus.Position.Y.Whole);
    }

    /// <summary>Positions hull and pad at fixed whole-pixel offsets from Samus during escape.</summary>
    private static void MoveGunshipAndSamusToY(
        RoomEnemyPlace top, GunshipPieces state,
        SamusState samus,
        int samusY)
    {
        samus.Position = samus.Position.WithWholeY(samusY);
        PlaceGunshipAroundSamus(top, state, samusY);
    }

    /// <summary>
    /// Places the top hull, the entrance pad and the bottom hull where they sit around Samus at <paramref name="samusY"/>: the
    /// escaping ship climbs past the room's top, and arrives from above it after Ceres, so each piece keeps its place as
    /// <see cref="OffRoomPlaces"/> does.
    /// </summary>
    private static void PlaceGunshipAroundSamus(RoomEnemyPlace top, GunshipPieces state, int samusY)
    {
        RoomEnemyPlace bottom = state.Bottom;
        RoomEnemyPlace pad = state.EntrancePad;
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
    internal void RunGunshipMainAi(RoomEnemyPlace place, GunshipState state, GunshipPieces pieces, IEnemyAiFrame frame)
    {
        RunGunshipTopMain(
            place, state, pieces,
            frame.Samus,
            frame.NewlyPressedControllerInput);
    }


}
