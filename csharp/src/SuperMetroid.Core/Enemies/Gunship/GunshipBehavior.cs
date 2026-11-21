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
    private readonly EnemyPassEvents _passEvents;

    /// <summary>
    /// The gunship's top hull, which moves the ship, and its bottom hull with the entrance pad (#2146): only the room's
    /// population load places them, as each reads the load's surroundings. The top hull offers the pieces its room lists
    /// after it the creation that gives the entrance pad the animation the hull opens and closes (#2554); a pad no top hull
    /// was listed before plays its own, as the game's does.
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Loads<EnemySpawnSettings>([EnemyDefinitionId.ShipTop], (occupant, _, place, common, surroundings) =>
        {
            var top = new GunshipTopEntity(this, occupant, place, common, surroundings);
            _partOffers.OfferLoaded<GunshipSpawnSettings>(EnemyDefinitionId.ShipBottomEntrance, (piece, settings, piecePlace, pieceCommon, pieceSurroundings) =>
                CreateBottom(piece, settings, piecePlace, pieceCommon, pieceSurroundings, top.State.Pieces.EntrancePadPiece));
            return EnemyOccupant.Running(top);
        });
        registrar.Loads<GunshipSpawnSettings>([EnemyDefinitionId.ShipBottomEntrance], (occupant, settings, place, common, surroundings) =>
            CreateBottom(occupant, settings, place, common, surroundings, StartingEntrancePadPiece()));
    }

    /// <summary>
    /// The bottom hull, or the entrance pad playing <paramref name="padPiece"/>, the animation the top hull listed before it
    /// made for it, or, with no top hull before it, its own.
    /// </summary>
    private EnemyOccupant CreateBottom(RoomEnemyOccupant occupant, GunshipSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings, GunshipPieceState padPiece) =>
        EnemyOccupant.PlacedOnly(new GunshipBottomEntity(occupant, place, common, settings, Ensure.NotNull(settings).Part == GunshipBottomPiece.EntrancePad
            ? InitializeGunshipEntrancePad(place, padPiece)
            : InitializeGunshipBottomHull(place, Ensure.NotNull(surroundings).Samus)));

    /// <summary>The entrance pad's animation as the top hull makes it, which starts as the pad's own setup starts its clock.</summary>
    private static GunshipPieceState StartingEntrancePadPiece() =>
        new(new AnimationPlayback<GunshipSegment>(GunshipAnimations.StartingWith(GunshipSegment.BottomEntrancePad), AnimationPlace.StartOfAnimation));

    /// <summary>Where the top hull offers the pieces listed after it the creation that gives the pad its animation.</summary>
    private readonly EnemyPartOffers _partOffers;

    /// <summary>Queues engine and entrance-pad sounds at the frame boundaries.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Creates gunship dust during takeoff.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>The dust clouds the gunship's liftoff raises, its own (#2571).</summary>
    private GunshipLiftoffDust.GunshipDustBehavior Dust { get; } = new();
    /// <summary>The loaded room's bindings: its events and the game-state services that set up Samus's pose.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places, where the top hull finds the ship's other pieces when it is placed.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>The room's enemy presentation, which shows the takeoff look once the engines fire up.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Binds the frame, room, place and presentation owners used by the ship's three-part actor.</summary>
    internal GunshipBehavior(EnemyAudioQueue audio, EnemyProjectileEngine projectiles, EnemyRoomBinding room, EnemyPlaces places, EnemyPassEvents passEvents,
        EnemyPresentation presentation, EnemyPartOffers partOffers)
    {
        _partOffers = partOffers;
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
    /// Resumes <paramref name="top"/>'s ship once its save message box has its answer (<see cref="GunshipSavePrompt"/>).
    /// The ship plays the same exit animation for either answer; the message box flow makes the save itself, as a save
    /// station's, when the answer is yes.
    /// </summary>
    internal static void AnswerGunshipSavePrompt(GunshipTop top)
    {
        RoomEnemyPlace pad = Ensure.NotNull(top).Pieces.EntrancePad;
        top.Ship.Phase = GunshipPhase.WaitForExitPadToOpen;
        StartGunshipAnimation(pad, top.Pieces.EntrancePadPiece, GunshipSegment.EntrancePadOpening);
        top.Ship.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
    }

    /// <summary>Starts <paramref name="segment"/> on <paramref name="place"/> from the next update.</summary>
    private static void StartGunshipAnimation(RoomEnemyPlace place, GunshipPieceState gunshipPieceState, GunshipSegment segment)
    {
        place.AnimationClock.StepOnNextUpdate();
        gunshipPieceState.Playback =
            new AnimationPlayback<GunshipSegment>(GunshipAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs a hull or pad piece's typed animation where the shared enemy animation would.</summary>
    internal static void AdvanceGunshipAnimation(RoomEnemyPlace place, GunshipPieceState piece)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<GunshipSegment> playback = piece.Playback;
        EnemyAnimationPlayback.PlaySilently(place, GunshipAnimations.Segments, ref playback);
        piece.Playback = playback;
    }

    /// <summary>
    /// Initializes the top hull and selects ordinary idle or post-Ceres descent, which places it against the loading
    /// <paramref name="samus"/>.
    /// </summary>
    /// <returns>The ship the top hull holds from its setup.</returns>
    internal GunshipTop InitializeGunshipTop(RoomEnemyPlace place, SamusState samus)
    {
        // The pad's animation starts as the pad is set up, after the top hull, which keeps it to open and close the pad.
        var pieces = new GunshipPieces(place,
            _places.Member(place, GunshipPartListingDefinitions.Listing, GunshipMember.TopHull, GunshipMember.BottomHull),
            _places.Member(place, GunshipPartListingDefinitions.Listing, GunshipMember.TopHull, GunshipMember.EntrancePad),
            StartingEntrancePadPiece());
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        var hull = new GunshipPieceState(EnemyAnimationPlayback.Start(place, GunshipAnimations.StartingWith(GunshipSegment.TopHull)));
        GunshipState ship;
        if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
        {
            // Post-Ceres initialization attaches the top hull above Samus and
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
        return new GunshipTop(pieces, hull, ship);
    }

    /// <summary>Readies a bottom hull's or pad's place to animate, out of Samus's reach.</summary>
    private static void PrepareGunshipBottomPlace(RoomEnemyPlace place)
    {
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>
    /// Initializes the entrance pad, which plays <paramref name="piece"/>, the animation the top hull listed before it made
    /// for it, from the height of the enemies its room lists before it.
    /// </summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    private GunshipPieceState InitializeGunshipEntrancePad(RoomEnemyPlace place, GunshipPieceState piece)
    {
        PrepareGunshipBottomPlace(place);
        // The pad holds the animation the top hull made for it, which starts as the pad's own setup starts its clock.
        place.AnimationClock.Restart();
        // The hull the pad takes its height from can stand above the room's top, after Ceres.
        Func<int, RoomEnemyPlace> listedBefore = _places.PlacesBefore(place);
        place.Position = OffRoomPlaces.WithWholeY(place.Position, GunshipPadBorrowedHeight.Start(
            distance => listedBefore(distance).Position.Y.Whole));
        return Ensure.NotNull(piece);
    }

    /// <summary>Initializes the bottom hull; after Ceres it is placed against the loading <paramref name="samus"/>.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    private GunshipPieceState InitializeGunshipBottomHull(RoomEnemyPlace place, SamusState samus)
    {
        PrepareGunshipBottomPlace(place);
        var state = new GunshipPieceState(EnemyAnimationPlayback.Start(place, GunshipAnimations.StartingWith(GunshipSegment.BottomHull)));
        if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
        {
            place.Position = OffRoomPlaces.WithWholeY(place.Position, samus.Position.Y.Whole + GunshipMotionDefinitions.BottomHullBelowSamus);
        }
        else
        {
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole + GunshipMotionDefinitions.LandedBottomHullDrop);
        }
        return state;
    }

    /// <summary>What <see cref="RunGunshipTopMain"/> does for each <see cref="GunshipPhase"/> member, each member's own arm.</summary>
    private IdentityTable<GunshipPhase, Action<RoomEnemyPlace, GunshipTop, SamusState, GameplayAction>> RunGunshipTopMainForGunshipPhase => field ??= new(new Dictionary<GunshipPhase, Action<RoomEnemyPlace, GunshipTop, SamusState, GameplayAction>>
    {
        [GunshipPhase.DescendAfterCeres] = (top, ship, samus, _) => DescendPostCeresGunship(top, ship.Ship, ship.Pieces, samus),
        [GunshipPhase.ApplyLandingBrakes] = (top, ship, samus, _) => BouncePostCeresGunship(top, ship.Ship, ship.Pieces, samus),
        [GunshipPhase.WaitForLandingEntranceToOpen] = (_, ship, _, _) =>
        {
            if (TickGunshipPhaseTimer(ship.Ship))
                ship.Ship.Phase = GunshipPhase.EjectSamus;
        },
        [GunshipPhase.EjectSamus] = (_, ship, samus, _) => RaisePostCeresSamus(ship.Ship, ship.Pieces, samus),
        [GunshipPhase.FinishLanding] = (_, ship, samus, _) =>
        {
            if (TickGunshipPhaseTimer(ship.Ship))
            {
                ship.Ship.Phase = GunshipPhase.Idle;
                samus.Control.Unlock();
                _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.LandingCompleted));
            }
        },
        [GunshipPhase.Idle] = (top, ship, samus, newlyPressedControllerInput) => HandleIdleGunshipEntrance(top, ship.Ship, ship.Pieces, samus, newlyPressedControllerInput),
        [GunshipPhase.WaitForEntranceToOpen] = (_, ship, _, _) =>
        {
            if (TickGunshipPhaseTimer(ship.Ship))
                ship.Ship.Phase = GunshipPhase.LowerSamus;
        },
        [GunshipPhase.LowerSamus] = (_, ship, samus, _) => LowerSamusIntoGunship(ship.Ship, ship.Pieces, samus),
        [GunshipPhase.WaitForEntranceToClose] = (_, ship, _, _) =>
        {
            if (TickGunshipPhaseTimer(ship.Ship))
                ship.Ship.Phase = GunshipPhase.BeginLiftoffOrRestoreSamus;
        },
        [GunshipPhase.BeginLiftoffOrRestoreSamus] = (_, ship, samus, _) => RestoreSamusInGunship(ship.Ship, samus),
        [GunshipPhase.HandleSaveConfirmation] = (_, ship, _, _) => _passEvents.Publish(new GunshipSavePromptRequested(new GunshipSavePrompt(ship))),
        [GunshipPhase.WaitForExitPadToOpen] = (_, ship, _, _) =>
        {
            if (TickGunshipPhaseTimer(ship.Ship))
                ship.Ship.Phase = GunshipPhase.RaiseSamus;
        },
        [GunshipPhase.RaiseSamus] = (_, ship, samus, _) => RaiseSamusOutOfGunship(ship.Ship, ship.Pieces, samus),
        [GunshipPhase.FinishSamusExit] = (_, ship, samus, _) =>
        {
            if (TickGunshipPhaseTimer(ship.Ship))
            {
                ship.Ship.Phase = GunshipPhase.Idle;
                samus.Control.Unlock();
                _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.ExitCompleted));
            }
        },
        [GunshipPhase.WaitForEngines] = (_, ship, _, _) => WaitForGunshipEngines(ship.Ship),
        [GunshipPhase.FireUpEngines] = (top, ship, samus, _) => FireUpGunshipEngines(top, ship.Ship, ship.Pieces, samus),
        [GunshipPhase.SteadyLiftoff] = (top, ship, samus, _) => LiftGunshipAtConstantSpeed(top, ship.Pieces, ship.Ship, samus),
        [GunshipPhase.AcceleratingLiftoff] = (top, ship, samus, _) => AccelerateEscapingGunship(top, ship.Pieces, ship.Ship, samus),
        [GunshipPhase.MoveAccelerating] = (top, ship, samus, _) => MoveEscapingGunship(top, ship.Pieces, ship.Ship, samus),
    });

    /// <summary>Runs the top hull's sound timer and landing, boarding, save, or takeoff state machine.</summary>
    internal void RunGunshipTopMain(
        RoomEnemyPlace top, GunshipTop ship,
        SamusState samus,
        GameplayAction newlyPressedControllerInput)
    {
        GunshipState gunshipState = ship.Ship;
        GunshipPieces gunshipPieces = ship.Pieces;
        // The engine sound plays on the update its timer runs out, which restarts it, in every phase of the ship.
        if (gunshipState.EngineSoundTimer.StepEnds())
        {
            _audio.QueueEnemySound(EnemySound.GunshipHover, priority: SoundRequestPriority.Normal);
            gunshipState.EngineSoundTimer = UpdateCountdown.For(GunshipHoverSoundDefinitions.Interval);
        }

        // The landed ship bobs before running its phase, from waiting for the landing entrance through waiting for the
        // engines to fire up for takeoff.
        GunshipPhase phase = gunshipState.Phase;
        if (BobsWhileLanded(phase))
            StepGunshipBob(gunshipState, gunshipPieces);

        RunGunshipTopMainForGunshipPhase[phase](top, ship, samus, newlyPressedControllerInput);
    }

    /// <summary>Carries Samus and all ship parts downward, slowing below <see cref="GunshipMotionDefinitions.SlowDescentY"/> before the landing clamp.</summary>
    private static void DescendPostCeresGunship(RoomEnemyPlace top, GunshipState gunshipState, GunshipPieces gunshipPieces, SamusState samus)
    {
        // The after-Ceres descent carries all the ship's actors as a rigid body. Above its slowing height it
        // moves fast; below that threshold it slows and clamps the top hull to its landing height
        // before beginning its landing bounce.
        // The clamp writes only whole positions; the fractions of a pixel keep this update's sum.
        SignedSixteenSixteen delta = top.Position.Y.Whole < GunshipMotionDefinitions.SlowDescentY ? GunshipMotionDefinitions.FastDescentSpeed : GunshipMotionDefinitions.SlowDescentSpeed;
        MoveGunshipAndSamusVertically(samus, gunshipPieces, delta);
        if (top.Position.Y.Whole < GunshipMotionDefinitions.LandedTopY)
            return;

        RoomEnemyPlace bottom = Pieces(gunshipPieces).Bottom;
        RoomEnemyPlace pad = Pieces(gunshipPieces).EntrancePad;
        top.Position = top.Position.WithWholeY(GunshipMotionDefinitions.LandedTopY);
        bottom.Position = bottom.Position.WithWholeY(GunshipMotionDefinitions.LandedBottomY);
        pad.Position = pad.Position.WithWholeY(GunshipMotionDefinitions.LandedPadY);
        gunshipState.Phase = GunshipPhase.ApplyLandingBrakes;
        gunshipState.LandingBrake = GunshipLandingBrake.First;
    }

    /// <summary>Applies the authored brake offsets, then opens the pad and aligns Samus.</summary>
    private void BouncePostCeresGunship(RoomEnemyPlace top, GunshipState gunshipState, GunshipPieces gunshipPieces, SamusState samus)
    {
        GunshipLandingBrake brake = gunshipState.LandingBrake;
        int yDelta = brake.YDelta;
        samus.Position += new Offset16<RoomPixels>(0, yDelta);
        foreach (RoomEnemyPlace place in Pieces(gunshipPieces).All)
        {
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole + yDelta);
        }

        if (brake.Next is { } next)
        {
            gunshipState.LandingBrake = next;
            return;
        }

        RoomEnemyPlace pad = Pieces(gunshipPieces).EntrancePad;
        gunshipState.Phase = GunshipPhase.WaitForLandingEntranceToOpen;
        gunshipState.BaseY = top.Position.Y.Whole;
        gunshipState.BobTimer = UpdateCountdown.LastingOneUpdate();
        gunshipState.Bob = GunshipIdleBob.First;
        samus.Position = samus.Position.WithWholeX(top.Position.X.Whole + GunshipMotionDefinitions.LandedSamusRightOfShip);
        samus.WritePreviousXPosition(samus.Position.X.Whole);
        StartGunshipAnimation(pad, gunshipPieces.EntrancePadPiece, GunshipSegment.EntrancePadOpening);
        gunshipState.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.LandingPadOpened));
        _audio.QueueEnemySound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Raises Samus from the landed ship and closes its pad at the exit height.</summary>
    private void RaisePostCeresSamus(GunshipState gunshipState, GunshipPieces gunshipPieces, SamusState samus)
    {
        samus.Position -= new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.ArrivalRiseStep);
        // The landed ship and Samus rising out of it lie inside the landing site, so her height is compared plainly.
        if (samus.Position.Y.Whole >= gunshipState.BaseY - GunshipBoardingDefinitions.ExitHeight)
            return;

        RoomEnemyPlace pad = Pieces(gunshipPieces).EntrancePad;
        gunshipState.Phase = GunshipPhase.FinishLanding;
        StartGunshipAnimation(pad, gunshipPieces.EntrancePadPiece, GunshipSegment.EntrancePadClosing);
        gunshipState.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.LandingPadClosed));
    }

    /// <summary>Moves Samus and the ship's three pieces down by <paramref name="delta"/> together.</summary>
    private static void MoveGunshipAndSamusVertically(SamusState samus, GunshipPieces gunshipPieces, SignedSixteenSixteen delta)
    {
        samus.Position = samus.Position.WithY(samus.Position.Y.MovedBy(delta));

        foreach (RoomEnemyPlace place in Pieces(gunshipPieces).All)
        {
            place.Position = place.Position.WithY(place.Position.Y.MovedBy(delta));
        }
    }

    /// <summary>
    /// Whether the landed ship bobs before running <paramref name="phase"/>: every phase from waiting for the
    /// landing entrance through waiting for the engines, Idle among them; firing up the engines is the first that does not.
    /// </summary>
    private static bool BobsWhileLanded(GunshipPhase phase) => GunshipPhaseDefinitions.BobbingWhileLanded.Contains(phase);

    /// <summary>Cycles the four idle-bob phases while moving the hull and pad together.</summary>
    private static void StepGunshipBob(GunshipState gunshipState, GunshipPieces gunshipPieces)
    {
        if (!gunshipState.BobTimer.Step())
            return;

        GunshipIdleBob bob = gunshipState.Bob;
        gunshipState.BobTimer = UpdateCountdown.For(bob.Timer);
        int yDelta = bob.YDelta;
        foreach (RoomEnemyPlace place in Pieces(gunshipPieces).All)
        {
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole + yDelta);
        }
        gunshipState.Bob = bob.Next;
    }

    /// <summary>Accepts a new Down press inside the standing entrance box and opens the pad.</summary>
    private void HandleIdleGunshipEntrance(
        RoomEnemyPlace top, GunshipState gunshipState, GunshipPieces gunshipPieces,
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
            RoomEnemyPlace pad = Pieces(gunshipPieces).EntrancePad;
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
            StartGunshipAnimation(pad, gunshipPieces.EntrancePadPiece, GunshipSegment.EntrancePadOpening);
            gunshipState.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
            _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.EntryStarted));
            _audio.QueueEnemySound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
        }
    }

    /// <summary>Counts the phase's wait: the update it ends on ends the waiting phase (#2340).</summary>
    private static bool TickGunshipPhaseTimer(GunshipState gunshipState) => gunshipState.PhaseTimer.StepEnds();

    /// <summary>Lowers Samus into the ship until she is aboard, then closes the pad.</summary>
    private void LowerSamusIntoGunship(GunshipState gunshipState, GunshipPieces gunshipPieces, SamusState samus)
    {
        samus.Position += new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.BoardingStep);
        // Samus sinks into the landed ship, inside the landing site, so her height is compared plainly.
        if (samus.Position.Y.Whole < gunshipState.BaseY + GunshipBoardingDefinitions.BoardedDepth)
            return;

        RoomEnemyPlace pad = Pieces(gunshipPieces).EntrancePad;
        gunshipState.Phase = GunshipPhase.WaitForEntranceToClose;
        StartGunshipAnimation(pad, gunshipPieces.EntrancePadPiece, GunshipSegment.EntrancePadClosing);
        gunshipState.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.EntryPadClosing));
    }

    /// <summary>Refills Samus by the ship's refill step or skips directly to escape takeoff after Mother Brain.</summary>
    private void RestoreSamusInGunship(GunshipState gunshipState, SamusState samus)
    {
        // Once Mother Brain's death has set the Zebes timebomb, the ship neither refills Samus nor offers to save: it
        // waits for its engines to fire up for the escape at once, carrying the hidden Samus with it.
        if (_room.Services.Events.Has(StoryEvent.ZebesTimebombSet))
        {
            gunshipState.Phase = GunshipPhase.WaitForEngines;
            gunshipState.PhaseTimer = UpdateCountdown.For(GunshipTakeoffDefinitions.EngineStartUpdates);
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
        gunshipState.Phase = GunshipPhase.HandleSaveConfirmation;
    }

    /// <summary>Raises Samus out of the ship, then closes the pad and emits the exit event.</summary>
    private void RaiseSamusOutOfGunship(GunshipState gunshipState, GunshipPieces gunshipPieces, SamusState samus)
    {
        samus.Position -= new Offset16<RoomPixels>(0, GunshipBoardingDefinitions.LeavingStep);
        // Samus rises out of the landed ship, inside the landing site, so her height is compared plainly.
        if (samus.Position.Y.Whole >= gunshipState.BaseY - GunshipBoardingDefinitions.ExitHeight)
            return;

        RoomEnemyPlace pad = Pieces(gunshipPieces).EntrancePad;
        gunshipState.Phase = GunshipPhase.FinishSamusExit;
        StartGunshipAnimation(pad, gunshipPieces.EntrancePadPiece, GunshipSegment.EntrancePadClosing);
        gunshipState.PhaseTimer = UpdateCountdown.For(GunshipBoardingDefinitions.PadMovementDuration);
        _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.ExitPadClosing));
        _audio.QueueEnemySound(RoomSound.GunshipPadClosing, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Waits for the engines to fire up: when the <see cref="GunshipTakeoffDefinitions.EngineStartUpdates"/> wait runs out the
    /// gunship shows its takeoff look from this update's frame on, and the engines fire up.
    /// </summary>
    private void WaitForGunshipEngines(GunshipState gunshipState)
    {
        if (!TickGunshipPhaseTimer(gunshipState))
            return;

        _presentation.ShowLook(GunshipTakeoffDefinitions.TakeoffLook);
        gunshipState.Phase = GunshipPhase.FireUpEngines;
    }

    /// <summary>Runs the engine-rumble phase (<see cref="GunshipTakeoffDefinitions.EngineRumbleFrames"/>).</summary>
    private void FireUpGunshipEngines(RoomEnemyPlace top, GunshipState gunshipState, GunshipPieces gunshipPieces, SamusState samus)
    {
        UpdateCountdown rumble = gunshipState.Rumble;
        int rumbleFrame = rumble.Age;
        bool strong = rumbleFrame >= GunshipTakeoffDefinitions.RumbleStrengthensFrame;
        // The rumble shakes down and up by turns, one shake each update (#2339).
        int shakeSize = strong ? GunshipTakeoffDefinitions.StrongRumbleShake : GunshipTakeoffDefinitions.RumbleShake;
        int shake = shakeSize.Toward(GunshipTakeoffDefinitions.RumbleShakesDown.IsInSecondHalfAt(rumbleFrame) ? VerticalDirection.Down : VerticalDirection.Up);
        samus.Position += new Offset16<RoomPixels>(0, shake);
        PlaceGunshipAroundSamus(top, gunshipPieces, samus.Position.Y.Whole);

        bool rumbled = rumble.Step();
        if (rumble.Age == GunshipTakeoffDefinitions.RumbleStrengthensFrame)
        {
            // The six clouds rise as the rumble strengthens. Each takes the highest projectile place still free, so
            // the last cloud, the outer left one, takes the highest place the other five left.
            foreach (GunshipDustPlacement cloud in GunshipDustDefinitions.LiftoffClouds)
                GunshipLiftoffDust.SpawnCloud(_projectiles, Dust, cloud, samus);
        }
        if (rumbled)
            gunshipState.Phase = GunshipPhase.SteadyLiftoff;
    }

    /// <summary>Runs the constant-speed liftoff (<see cref="GunshipTakeoffDefinitions.SteadyLiftoffRise"/>).</summary>
    private static void LiftGunshipAtConstantSpeed(RoomEnemyPlace top, GunshipPieces gunshipPieces, GunshipState gunshipState, SamusState samus)
    {
        // The steady liftoff rises two pixels an update from the landed ship until the hull passes 896, deep inside the
        // landing site, so it moves and compares plainly.
        MoveGunshipAndSamusToY(top, gunshipPieces, samus, samus.Position.Y.Whole - GunshipTakeoffDefinitions.SteadyLiftoffRise);
        if (top.Position.Y.Whole < GunshipTakeoffDefinitions.AcceleratingLiftoffY)
        {
            gunshipState.Phase = GunshipPhase.AcceleratingLiftoff;
            gunshipState.EscapeVelocity = GunshipMotionDefinitions.EscapeStartVelocity;
        }
    }

    /// <summary>Runs the accelerating takeoff phase.</summary>
    private void AccelerateEscapingGunship(RoomEnemyPlace top, GunshipPieces gunshipPieces, GunshipState gunshipState, SamusState samus)
    {
        MoveEscapingGunship(top, gunshipPieces, gunshipState, samus);
        // The accelerating takeoff ends once the hull passes 256, long before its speed could carry it past the room's top
        // in one update, so it compares plainly.
        if (top.Position.Y.Whole < GunshipTakeoffDefinitions.TakeoffCompleteY)
        {
            gunshipState.Phase = GunshipPhase.MoveAccelerating;
            _passEvents.Publish(new GunshipEvent(GunshipFrameEvent.EscapeTakeoffCompleted));
        }
    }

    /// <summary>Carries the escaping ship and Samus on up, faster each update.</summary>
    private static void MoveEscapingGunship(RoomEnemyPlace top, GunshipPieces gunshipPieces, GunshipState gunshipState, SamusState samus)
    {
        GunshipState ship = gunshipState;
        ship.AccelerateEscape();

        samus.Kinematics.Position = samus.Kinematics.Position.WithY(samus.Kinematics.Position.Y.MovedBy(
            -ship.EscapeVelocity.ToSixteenSixteenDelta()));
        MoveGunshipAndSamusToY(top, gunshipPieces, samus, samus.Position.Y.Whole);
    }

    /// <summary>Positions hull and pad at fixed whole-pixel offsets from Samus during escape.</summary>
    private static void MoveGunshipAndSamusToY(
        RoomEnemyPlace top, GunshipPieces gunshipPieces,
        SamusState samus,
        int samusY)
    {
        samus.Position = samus.Position.WithWholeY(samusY);
        PlaceGunshipAroundSamus(top, gunshipPieces, samusY);
    }

    /// <summary>
    /// Places the top hull, the entrance pad and the bottom hull where they sit around Samus at <paramref name="samusY"/>: the
    /// escaping ship climbs past the room's top, and arrives from above it after Ceres, so each piece keeps its place as
    /// <see cref="OffRoomPlaces"/> does.
    /// </summary>
    private static void PlaceGunshipAroundSamus(RoomEnemyPlace top, GunshipPieces gunshipPieces, int samusY)
    {
        RoomEnemyPlace bottom = Pieces(gunshipPieces).Bottom;
        RoomEnemyPlace pad = Pieces(gunshipPieces).EntrancePad;
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
    internal void RunGunshipMainAi(RoomEnemyPlace place, GunshipTop ship, IEnemyAiFrame frame)
    {
        RunGunshipTopMain(
            place, ship,
            frame.Samus,
            frame.NewlyPressedControllerInput);
    }

    /// <summary>The gunship's pieces, which <paramref name="top"/> found when it was placed.</summary>
    private static GunshipPieces Pieces(GunshipPieces gunshipPieces) => gunshipPieces;
}
