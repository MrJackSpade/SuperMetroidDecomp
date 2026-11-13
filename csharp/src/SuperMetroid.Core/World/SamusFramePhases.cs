using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// Samus's phases of a gameplay frame: grounded movement, animation transitions, uninitialized-pose movement,
/// the grapple beam, X-ray from the HUD, load appearance and beam tile loading.
/// </summary>
internal sealed class SamusFramePhases
{
    /// <summary>Supplies Samus's installed death steps.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Opens the attract demo's Samus input window and selects the demo's new-input state.</summary>
    private readonly IAttractSamusInput _attractDemo;
    /// <summary>Supplies held and newly pressed controller bits to Samus phase handlers.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Gates movement and animation branches during gameplay time freeze.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Supplies elapsed gameplay frames to the Shinespark movement handler.</summary>
    private readonly GameTimeState _gameTime;
    /// <summary>Provides frame-counter cadence for movement and animation behavior.</summary>
    private readonly FrameClock _clock;
    /// <summary>The runtime's assists; a shinespark reads whether Samus cannot die.</summary>
    private readonly ActiveAssists _assists;
    /// <summary>Owns the live Samus state and phase-specific handler selection.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Provides active level data, camera, and room state for movement scans.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Supplies the shared game state consulted by movement handlers.</summary>
    private readonly SharedGameState _system;
    /// <summary>The pose update after normal beta's animation.</summary>
    private readonly SamusPoseTransitionPhase _transitions;

    /// <summary>Binds the frame owners required to preserve Samus's alpha, beta, and presentation ordering.</summary>
    internal SamusFramePhases(SamusFramePhasesDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _attractDemo = dependencies.AttractDemo;
        _controller1 = dependencies.Controller1;
        _freeze = dependencies.Freeze;
        _gameTime = dependencies.GameTime;
        _clock = dependencies.Clock;
        _assists = dependencies.Assists;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _system = dependencies.System;
        _transitions = new SamusPoseTransitionPhase(new SamusPoseTransitionPhaseDependencies { Controller1 = dependencies.Controller1, Freeze = dependencies.Freeze, Clock = dependencies.Clock, Options = dependencies.Options, Player = dependencies.Player, Room = dependencies.Room });
    }

    /// <summary>
    /// Shows the colours of the beams pause teardown supplies. Unpausing does this while the screen is dark, so the
    /// first resumed gameplay frame shows the newly equipped combination's colours.
    /// </summary>
    internal void ShowEquippedBeamColours(SamusBeamFlags equippedBeams) =>
        SamusProjectileSystem.LoadBeamPalette(_player.Colours, equippedBeams);

    /// <summary>
    /// The grapple beam's step, reached from the HUD-selection dispatch of
    /// Samus's HUD-specific behaviour and projectiles in her alpha pass.
    /// It therefore runs before Samus's projectiles and before the enemies' main step,
    /// so enemies see the beam endpoint and Samus position it leaves this frame. Only
    /// connected and release functions own Samus's movement; an extending or cancelling
    /// beam coexists with the current pose's ordinary movement.
    /// </summary>
    private bool RunGrappleBeamHandler(SamusBetaHandler beta, ISamusRoomObjects roomObjects, SamusProjectileSystem projectiles, RoomScope room)
    {
        if (_player.Samus is null || _room.LevelData is null)
            throw new InvalidOperationException("The grapple handler requires Samus and room collision data.");
        _player.LastGrappleMovement = null;
        bool grappleOwnsMovement = false;
        if (beta is SamusBetaHandler.Fatal or SamusBetaHandler.SuitPickup)
        {
            // The death sequence's game states do not call the ordinary Samus alpha/beta
            // handlers. The death state advances later at the animation seam.
        }
        else if (_player.Samus.Grapple.Phase == GrapplePhase.Firing)
        {
            _player.LastGrappleMovement = SamusGrappleMovement.StepFiring(

                _room.LevelData,
                _player.Samus,
                _controller1.Current,
                roomObjects,
                room.Enemies.GrappleTargets.ResolveGrappleEndpoint,
                connectionPoseChange: GrapplePoseChange.Deferred);
            grappleOwnsMovement = _player.LastGrappleMovement.Value.OwnsMovement;
        }
        else if (_player.Samus.Grapple.Phase == GrapplePhase.CancelPending)
        {
            _player.LastGrappleMovement =
                SamusGrappleMovement.CompleteFiringCancellation(_room.LevelData, _player.Samus);
            grappleOwnsMovement = _player.LastGrappleMovement.Value.OwnsMovement;
        }
        else if (_player.Samus.Grapple.Phase is
            GrapplePhase.ConnectedSwinging or GrapplePhase.ReleaseFromSwing or
            GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab or
            GrapplePhase.WallGrabRelease or GrapplePhase.WallJumping or
            GrapplePhase.Dropped)
        {
            _player.LastGrappleMovement = SamusGrappleMovement.Step(new SamusTerrain { Level = _room.LevelData, RoomObjects = roomObjects }, _player.Samus, _controller1.Current, _controller1.NewlyPressed, _clock.Counter, room.Enemies.GrappleTargets.ResolveGrappleEndpoint, poseChange: GrapplePoseChange.Deferred);
            grappleOwnsMovement = _player.LastGrappleMovement.Value.OwnsMovement;
        }
        else if (!_freeze.TimeIsFrozen &&
                 SamusGrappleHudInput.IsSelectedAndAdmitted(_player.Samus, projectiles.PostureAdmission) &&
                 ((_controller1.NewlyPressed | _player.Samus.InputSnapshot.PreviousDrawNewInput) & ControllerButton.X) != ControllerButton.None)
        {
            // Selecting the grapple on the HUD fires it once the pose's movement type admits it.
            SamusGrappleMovement.BeginFiring(_player.Samus, _controller1.Current);
            _player.LastGrappleMovement = new GrappleMovementResult(
                _player.Samus.Grapple.Phase,
                Fired: _player.Samus.Grapple.Phase == GrapplePhase.Firing,
                OwnsMovement: false);
        }

        if (_player.LastGrappleMovement is { Fired: true })
            LoadGrapplePalette();
        else if (_player.LastGrappleMovement is { Phase: GrapplePhase.Inactive })
        {
            // Cancellation, drop, wall-jump and swing-release tails all
            // reload the equipped beam palette before returning to inactive.
            _player.Colours.ShowBeams(SuperMetroid.Core.Samus.Palettes.SamusShades.Beam(SamusBeamCombinations.Of(_player.Samus.EquippedBeams)));
        }

        // The grapple's bottom-boundary check runs after every grapple function, including inactive.
        // Swing calculations above therefore consumed last frame's bit; this write
        // publishes the current bottom-boundary result for the next frame exactly
        // where the handler does.
        SamusGrappleMovement.RefreshLiquidPhysicsFlag(_player.Samus);
        return grappleOwnsMovement;
    }

    /// <summary>Shows the grapple's firing beam shade and its flare's shade as Samus's colours (#1512).</summary>
    internal void LoadGrapplePalette()
    {
        // Both ordinary HUD firing and debug entry select the same shades.
        _player.Colours.ShowBeams(SuperMetroid.Core.Samus.Palettes.SamusShades.Beam(GrappleBeamDefinitions.Palettes.FiringSelection));
        _player.Colours.ShowGrappleFlare(SuperMetroid.Core.Samus.Palettes.SamusShades.GrappleFiringFlare);
    }

    /// <summary>
    /// Runs the complete Samus-side X-ray admission path after the HUD has selected the
    /// equipped scope and observed held Dash, matching the HUD's X-ray handler, its setup, and the X-ray spawn.
    /// </summary>
    /// <remarks>
    /// HUD cursor movement is not yet a general runtime subsystem, so this narrow entry
    /// point represents only the already-selected item. It still requires equipped bit
    /// for X-ray and passes the live dispatch game state (the pause darkening and unpause brightening states
    /// also run this alpha pass but setup rejects them), bomb cooldown/count, power-bomb phase, current/previous
    /// movement types, vertical velocity, landing-pose gate, and speed divisor to the X-ray
    /// setup logic. The debug runner uses it without substituting an invented pose or art frame.
    /// </remarks>
    public bool TryBeginXrayFromSelectedHudItem(SuperMetroid.Core.Progression.SuperMetroidGameState gameState, SamusBombProjectileSystem bombProjectiles)
    {
        if (_player.Samus is null || !_player.GroundedSamusMovementEnabled || _room.LevelData is null)
        {
            throw new InvalidOperationException(
                "X-ray requires initialized gameplay Samus and room level data.");
        }

        // The HUD's selection prevents selecting the HUD item without the scope. The lower-level
        // X-ray spawn assumes that selector has already succeeded.
        if (!_player.Samus.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope))
            return false;

        return _player.Samus.Xray.TryBegin(_player.Samus, _player.PreviousMovementTypeForXray, new XrayBeginConditions { GameState = gameState, PowerBombExplosionActive = bombProjectiles.PowerBombExplosion.IsActive, ProjectileCooldownTimer = bombProjectiles.CooldownTimer, BombCounter = bombProjectiles.BombCounter }, XrayActivation.Deferred);
    }

    /// <summary>
    /// Runs Samus's movement-type table for a pose stored without
    /// initializing it: the cached movement type, not the pose, selects the mover.
    /// </summary>
    private void StepUninitializedPoseMovement(ISamusRoomObjects roomObjects)
    {
        RoomLevelData level = _room.LevelData ?? throw new InvalidOperationException(
            "Uninitialized-pose movement requires active room level data.");
        switch (SamusPoseMetadata.ReadMovementType(_player.IntroducedSamus))
        {
            case SamusMovementType.Special:
                // The momentum reset only clears the momentum-transition selector.
                break;
            case SamusMovementType.SpinJumping:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepSpinJump(
level, _player.IntroducedSamus, _controller1.Current, _clock.Counter,
                    _controller1.NewlyPressed, roomObjects);
                break;
            case SamusMovementType.NormalJumping:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepNormalJump(
level, _player.IntroducedSamus, _controller1.Current, _clock.Counter, roomObjects);
                break;
            case SamusMovementType.Standing:
            case SamusMovementType.Running:
            case SamusMovementType.MorphBallGround:
            case SamusMovementType.Crouching:
            case SamusMovementType.Falling:
            case SamusMovementType.UnusedGlitchBall:
            case SamusMovementType.MorphBallFalling:
            case SamusMovementType.UnusedGlitchBallAlternate:
            case SamusMovementType.Knockback:
            case SamusMovementType.CompactAirborne:
            case SamusMovementType.TurningOnGround:
            case SamusMovementType.PostureTransition:
            case SamusMovementType.Moonwalking:
            case SamusMovementType.SpringBallGround:
            case SamusMovementType.SpringBallInAir:
            case SamusMovementType.SpringBallFalling:
            case SamusMovementType.WallJumping:
            case SamusMovementType.RanIntoWall:
            case SamusMovementType.Grappling:
            case SamusMovementType.TurningWhileJumping:
            case SamusMovementType.TurningWhileFalling:
            case SamusMovementType.DamageBoost:
            case SamusMovementType.DraygonHeld:
            default:
                throw new NotSupportedException(
                    $"Cached movement type {SamusPoseMetadata.ReadMovementType(_player.IntroducedSamus)} under " +
                    $"uninitialized pose {_player.IntroducedSamus.Pose} has no translated movement-type mover.");
        }
    }

    /// <summary>
    /// The beta handler installed for this beta: the first of death, the suit's empty beta, command zero, command six,
    /// the attract demo, the gunship's handler and another lock that holds, otherwise the normal handler.
    /// </summary>
    private SamusBetaHandler DispatchedBetaHandler(SamusHandlerFrame frame, SamusState samus)
    {
        if (frame.DeathOwnsSamus)
            return SamusBetaHandler.Fatal;
        SamusBetaHandler installed = samus.Control.Mode.Beta;
        if (installed is SamusBetaHandler.SuitPickup or SamusBetaHandler.ScriptLocked or SamusBetaHandler.RefillStation)
            return installed;
        return _attractDemo.IsAttractDemo ? SamusBetaHandler.TitleDemo : installed;
    }

    /// <summary>
    /// Runs the frame handler's alpha: the head, then, when Samus movement is enabled,
    /// the tail. The enable flag is captured once here for every later gate of the frame.
    /// </summary>
    internal void RunAlpha(SamusHandlerFrame frame, SuperMetroid.Core.Progression.SuperMetroidGameState gameState, SamusState samus,
        ScrollBoundaryCamera camera, FixedPosition<RoomPixels>? cameraPointAtFrameStart, RoomScope room)
    {
        RunAlphaHead(frame, samus, camera, cameraPointAtFrameStart, room);
        frame.SampleMovementEnabled(_player.GroundedSamusMovementEnabled);
        if (frame.MovementEnabled)
            RunAlphaTail(frame, gameState, samus, camera, room);
    }

    /// <summary>
    /// Runs the frame handler's beta: beta movement when Samus movement is enabled, then
    /// the tail.
    /// </summary>
    internal void RunBeta(SamusHandlerFrame frame, SamusState samus, ScrollBoundaryCamera camera, ISamusFrameAudio audio, RoomScope room)
    {
        if (frame.MovementEnabled)
            RunBetaMovement(frame, samus, camera, room);
        RunBetaTail(frame, samus, audio, room.RoomObjects, room);
    }

    /// <summary>
    /// Runs alpha's tail once movement is enabled: environment gravity, terrain hazards, inside-block reactions, HUD
    /// selection, X-ray admission, bombs, grapple and projectiles, then the end-of-alpha movement snapshot.
    /// </summary>
    private void RunAlphaTail(SamusHandlerFrame frame, SuperMetroid.Core.Progression.SuperMetroidGameState gameState, SamusState samus,
        ScrollBoundaryCamera camera, RoomScope room)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        // Frame-handler alpha refreshes the liquid-gravity values after pose input/radius setup and
        // before beta movement. Refreshing the split gravity values every frame is
        // essential at a liquid surface: a jump can cross from water to air without
        // any pose transition that would otherwise reinitialize acceleration.
        SamusAerialMovement.ConfigureEnvironmentGravity(samus);

        if (_room.ActiveRoom is null)
            throw new InvalidOperationException("Live Samus terrain reactions require an active room.");
        // Block-inside detection samples the body after alpha's radius update: a pose whose
        // radius grew in last frame's beta reaches air spikes with its new body now.
        // Samus command zero replaces the ordinary alpha handler with a locked one,
        // which never dispatches block-inside detection. Automatic Reserve recovery
        // uses this exact lock; re-sampling spikes while it owns Samus changes the
        // post-recovery knockback window used by Shinespark Suit.
        if (samus.Control.Mode.Beta != SamusBetaHandler.ScriptLocked)
        {
            SamusTerrainHazardCollision.PrepareFrame(
                _room.LevelData,
                samus,
                _room.ActiveRoom.AreaIndex,
                _system.HasAnyBossBits(_room.ActiveRoom.AreaIndex, BossBits.AreaBoss));
        }

        if (!_freeze.TimeIsFrozen && !frame.AlphaInputLocked(samus) && _room.ActiveRoom is { } insideRoom)
            SamusInsideBlockReactions.PrepareFrame(_room.LevelData, samus, insideRoom.AreaIndex,
                _system.HasAnyBossBits(insideRoom.AreaIndex, BossBits.AreaBoss), room.RoomObjects);

        // Alpha order is cooldown -> movement-type HUD projectile producer ->
        // projectile handling. The outer gameplay loop then runs overlap
        // before beta movement. A newly placed bomb therefore counts 60 -> 59 and
        // selects its first art record in the placement frame itself.
        // The door fade's enemy and draw pass does not execute normal alpha.
        // In particular its draw-only zero input must not release a held charge.
        // X-ray's freeze leaves the normal state handler installed, so the normal handler
        // still runs cooldown, HUD switching and the HUD handler; only its
        // projectile-handling tail stops, which both projectile owners
        // apply themselves. A selection cancel during X-ray therefore takes effect.
        frame.SampleAlphaRuns(!_freeze.GameplayTimeFrozen && !frame.DeathOwnsSamus && !room.Enemies.Elevator.ElevatorDoorTransitionActive);
        if (frame.AlphaRuns)
        {
            // The HUD selection update runs before the movement-type HUD projectile producer.
            // Consequently a Select edge can choose missiles and an X edge can
            // fire one during this same alpha pass. Input-locked message/elevator
            // handlers do not execute the normal selection owner.
            // The switched-to handler of the item the loop settles on runs on every
            // cancel or select edge, even one that leaves the selection unchanged.
            // Each clears the charge and restores suit colors, except grapple's
            // while a grapple is already active, which changes nothing.
            if (!frame.AlphaInputLocked(samus) && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
                samus.HudSelection.HandleHudSelection(samus, _controller1.Current, _controller1.NewlyPressed).ItemHandlerAccepted &&
                (samus.SelectedHudItem != HudItem.Grapple ||
                 samus.Grapple.Phase == GrapplePhase.Inactive))
            {
                room.Projectiles.Firing.CancelChargeForHudSelection();
                SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.Load(_player.Colours, samus.EquippedItems);
            }

            // The selected scope uses held Run, not Fire. Setup installs its
            // own pose handlers, superseding alpha's ordinary pending pose.
            if (!frame.AlphaInputLocked(samus) && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
                samus.SelectedHudItem == HudItem.XRay &&
                samus.Grapple.Phase == GrapplePhase.Inactive &&
                (_controller1.Current & ControllerButton.B) != ControllerButton.None &&
                TryBeginXrayFromSelectedHudItem(gameState, room.BombProjectiles))
            {
                frame.MarkXrayActivated();
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }

            // X-ray's freeze disables enemy room.Projectiles, room objects, animated tiles,
            // and palette FX while time is frozen. Normal bombs cannot be placed or
            // advanced through this producer during the X-ray interval.
            // The bombs' share of the HUD handler runs the shared cooldown, then the morph ball's producer.
            if (room.BombProjectiles.RunHudProducer(samus, _controller1.Current, _controller1.NewlyPressed))
            {
                room.Projectiles.Firing.CancelChargeForBombSpread();
                SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.Load(_player.Colours, samus.EquippedItems);
            }

        }

        frame.SampleGrappleOwnership(RunGrappleBeamHandler(frame.Mode.Beta, room.RoomObjects, room.Projectiles, room));

        if (frame.AlphaRuns)
        {
            // The bomb half above has already decremented the shared projectile cooldown. The humanoid handler then
            // dispatches the HUD producer, which fires and queues its sound, before the game handles any projectile: the
            // bombs, then the shots (#1620). Passing the live camera position also gives the projectile deletion check
            // its exact off-screen deletion window. The Spacetime beam's malformed frame-behaviour entry writes straight into
            // Samus's progression owners during this pass, so later consumers this frame, and saves, observe them.
            var shotTerrain = new SamusTerrain { Level = _room.LevelData, RoomObjects = room.RoomObjects, Progression = _system };
            room.Projectiles.FireFromHud(shotTerrain, samus, new ProjectileButtonInput { ControllerInput = _controller1.Current, ControllerNewInput = _controller1.NewlyPressed, ControllerPreviousNewInput = samus.InputSnapshot.PreviousDrawNewInput }, room.BombProjectiles, projectileProducerEnabled: !frame.AlphaInputLocked(samus));
            _ = room.BombProjectiles.HandleBombs(new SamusTerrain { Level = _room.LevelData, RoomObjects = room.RoomObjects }, samus, overlap: BombJumpOverlapPublication.Deferred);
            room.Projectiles.HandleShots(shotTerrain, samus, camera.PixelPosition, room.BombProjectiles);

            if (!_freeze.TimeIsFrozen)
                room.BombProjectiles.ResolveSamusOverlap(samus, room.Projectiles.Firing.ProjectileInvincibilityTimer);

            // The new-state handler dispatches the selected HUD producer and immediately
            // clears the pose bridge. Pose initialization occurs later in the
            // new-state handler, so a bridge published below survives precisely
            // until this point in the following gameplay frame.
            SamusAerialPoseTransitions.DropPendingTransitionShot(samus);
            samus.ProjectileInheritance.ClearMovement();
        }

        // Samus's end-of-alpha step snapshots movement,
        // before beta applies animation/input transitions. Admission reads the
        // preceding snapshot; a newly queued X-ray pose is not installed yet.
        _player.PreviousMovementTypeForXray = frame.XrayActivatedThisFrame
            ? frame.MovementBeforeXrayAdmission : SamusPoseMetadata.ReadMovementType(samus);
    }

    /// <summary>
    /// Re-samples what the enemy pass can change before beta dispatches: a cancelled prospective pose, the
    /// stationary-script lock, updated beam hits and the contact-damage index beta clears.
    /// </summary>
    internal void RunPostEnemyMainResample(SamusHandlerFrame frame, SamusState samus, SamusBombProjectileSystem bombProjectiles, SamusProjectileSystem projectiles, RoomScope room)
    {
        if (!frame.MovementEnabled)
            return;
        // A Samus command issued by enemy AI that cancels pending transitions cleared alpha's prospective
        // poses and commands, so nothing pending is committed this frame.
        if (samus.ConsumePendingPoseTransitionCancellation())
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
        }
        // Actor commands replace beta before its dispatch in this same update.
        frame.RedispatchBeta(DispatchedBetaHandler(frame, samus));
        if (!_freeze.TimeIsFrozen && !frame.DeathOwnsSamus)
            // Ceres Ridley's dedicated updated-beam adapter; ordinary projectiles collide before each enemy's AI.
            room.Enemies.Ceres.ResolveCeresRidleyProjectileHits(projectiles, bombProjectiles);

        // The enemy pass consumes the preceding frame's contact-damage index.
        // Only beta clears it, before movement republishes the current value.
        if (frame.Mode.Beta != SamusBetaHandler.SuitPickup)
            samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.None;
    }

    /// <summary>
    /// Runs beta's movement: clears the frame's movement results, runs the grapple-release mover, publishes
    /// grapple's corrected camera point and the dispatch pose, then runs the installed movement owner.
    /// </summary>
    private void RunBetaMovement(SamusHandlerFrame frame, SamusState samus, ScrollBoundaryCamera camera, RoomScope room)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        // The normal beta handler dispatches movement type before animation. Every admitted pose
        // below has its own verified direction/mode path; a newly reachable pose
        // cannot accidentally inherit generic standing or running physics.
        _player.LastGroundedSamusMovement = null;
        _player.LastAerialSamusMovement = null;
        _player.LastMorphBallMovement = null;
        _player.LastBombJumpMovement = null;
        _player.LastKnockbackMovement = null;
        _player.LastShinesparkMovement = null;
        _player.LastCrystalFlashMovement = null;
        // X-ray publishes its pose-input and beam results above because both run
        // outside the normal movement-type dispatcher. Its movement result is the
        // directly inspectable frame returned by `StepMovement` below.
        _player.LastDrainedSamusMovement = null;
        _player.LastDraygonEscape = null;

        frame.SampleGrappleReleasePoseInput(false);
        if (samus.Grapple.ReleasedMovementActive)
        {
            // The replacement beta mover does not replace normal airborne alpha
            // input. Preserve its pending aim/turn transition after release cleanup;
            // the connection/release frames that themselves replace the pose still
            // invalidate a lookup sampled from their previous grapple body.
            frame.SampleGrappleReleasePoseInput(
                frame.MovementTypeAtFrameStart != SamusMovementType.Grappling &&
                samus.Pose == frame.PoseAtFrameStart);
            // The grapple release replaces the normal beta movement handler on the
            // release-input frame. It remains independent of the beam function, so
            // it must also run after the grapple cancel has made that function inactive.
            SamusAerialMovement.ConfigureEnvironmentGravity(samus);
            _player.LastAerialSamusMovement = SamusAerialMovement.StepReleasedFromGrapple(

                _room.LevelData,
                samus,
                _controller1.Current,
                _clock.Counter,
                room.RoomObjects);
            frame.SampleGrappleOwnership(true);
        }

        if (_player.LastGrappleMovement is { CameraPrevious: { } grapplePrevious })
        {
            // The grapple's close-collision snap clamps previous-position values to twelve
            // pixels after a close-collision snap. Feed those corrected whole pixels
            // to tracking while preserving the pre-frame subpixels.
            frame.SnapCameraPoint(grapplePrevious);
        }

        frame.SampleBetaMovementPose(samus.Pose);

        RunBetaMovementOwner(frame, samus, camera, room);
    }

    /// <summary>
    /// Runs the installed movement owner in priority order: the empty and command betas, grapple, Draygon's grab,
    /// X-ray, Ceres Ridley's ejection, knockback, bomb jump, Crystal Flash, drained, Shinespark, the locked beta, frozen
    /// time and the special type, and otherwise the pose's normal movement-table entry.
    /// </summary>
    private void RunBetaMovementOwner(SamusHandlerFrame frame, SamusState samus, ScrollBoundaryCamera camera, RoomScope room)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        if (frame.Mode.Beta is SamusBetaHandler.Fatal or SamusBetaHandler.SuitPickup)
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
        }
        // Command zero installs an empty beta handler independently of the
        // movement handler that was active beforehand. Automatic Reserve can
        // therefore freeze an in-progress hurt launch; special owners such as
        // knockback's handler must not outrank this explicitly installed handler.
        else if (frame.Mode.Beta == SamusBetaHandler.ScriptLocked)
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
        }
        else if (frame.GrappleOwnsMovement)
        {
            // The grappling movement type's beta handler is empty. Discard input transitions
            // sampled from the pre-grapple pose once connection installs the grapple swing poses.
            if (!frame.GrappleReleaseAcceptsPoseInput)
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
        }
        // The real grabbed route uses the explicit empty handler installed at the grab,
        // not the otherwise reachable Draygon-held movement type's normal dispatcher. Keep
        // the latter in the pose switch below for direct/diagnostic dispatcher use.
        else if (samus.DraygonGrabbed.OwnsMovement)
        {
            // Deliberately empty: Draygon's actor owns coordinates.
        }
        // Special prospective-pose command five installs the X-ray movement handler instead of the
        // normal movement-type dispatcher. Stable X-ray bodies select one of five
        // angle frames; turning on the ground deliberately performs no beta movement.
        else if (samus.Xray.OwnsSamusControl)
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            samus.Xray.StepMovement(samus);
        }
        // Ceres Ridley's grab installs a separate movement/gamma-handler pair for Ceres
        // Ridley's retreat. It must precede ordinary damage knockback because the knockback
        // poses are shared while ownership, duration, and termination are not.
        else if (samus.CeresRidleyEjection.IsActive)
        {
            // The hurt setup installs the hurt pose and clears every prospective pose and
            // command. Afterwards the damage-boost check suppresses only a
            // damage-boost result selected from a knockback pose; it does not
            // replace the complete input handler. A lookup failure there keeps the
            // knockback pose and its Stop command, which clears the X acceleration
            // mode when the pose is committed.
            if (samus.CeresRidleyEjection.InitializationPending)
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
            else
            {
                if (_player.ProspectiveSamusPose?.ProspectivePose is
                    SamusPoseId.DamageBoostLeftPose or SamusPoseId.DamageBoostRightPose)
                    _player.ProspectiveSamusPose = null;
                if (_player.ProspectiveSamusFallbackPose is
                    SamusPoseId.DamageBoostLeftPose or SamusPoseId.DamageBoostRightPose)
                    _player.ProspectiveSamusFallbackPose = null;
            }
            samus.CeresRidleyEjection.Step(

                _room.LevelData,
                samus,
                camera.PixelPosition.X,
                _clock.Counter);
        }
        // Knockback's handler takes precedence over the normal movement-
        // type dispatcher. Unlike bomb jump, normal pose input remains active so
        // the knockback poses can still select the retail damage-boost escape chord.
        else if (samus.KnockbackActive)
        {
            SamusAerialMovement.ConfigureEnvironmentGravity(samus);
            _player.LastKnockbackMovement = SamusKnockbackMovement.Step(

                _room.LevelData,
                samus,
                _clock.Counter,
                room.RoomObjects);
        }
        // Special command three replaces both handlers. Preserve the retained
        // pose while input is locked; the late-rise helper can restore input
        // before the movement handler ends, allowing a matched cancellation.
        else if (samus.BombJumpStarting || samus.BombJumpActive)
        {
            if (frame.BombJumpLocksPoseInput)
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }

            // The shared alpha environment pass above already selected these values;
            // repeat the explicit special-handler publication here to preserve the
            // old diagnostic seam while allowing water/lava instead of forcing air.
            SamusAerialMovement.ConfigureEnvironmentGravity(samus);
            _player.LastBombJumpMovement = samus.BombJumpStarting
                ? SamusBombJumpMovement.Start(samus)
                : SamusBombJumpMovement.Step(

                    _room.LevelData,
                    samus,
                    _clock.Counter,
                    room.RoomObjects);
        }
        // Crystal Flash installs one of three Crystal Flash movement handlers and an
        // empty pose-input handler. Mother Brain's rainbow command keeps those handlers,
        // but temporarily replaces frame-handler beta with a lock that calls no
        // movement at all. Once command one restores the normal beta, Flash resumes under
        // the drained pose. The Super Metroid never installs that beta lock, so its
        // interruption continues Flash immediately. Model the physical beta owner,
        // not merely the fact that a cinematic pose exists.
        else if (samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive &&
            !(samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked && samus.Control.AlphaLocked))
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.LastCrystalFlashMovement = samus.CrystalFlash.Step(
                samus,
                _clock.Counter);
            if (_player.LastCrystalFlashMovement.Value.PhaseAtStart == CrystalFlashPhase.Raising &&
                _player.LastCrystalFlashMovement.Value.BubbleEffectRequested)
            {
                // Crystal Flash's ending clears its state, copies Samus's raised center into the
                // shared explosion position, then spawns the Crystal Flash explosion. This beta
                // pass occurs after bomb alpha, so the retained Crystal Flash slot
                // observes the released flag and deletes on the following frame.
                room.BombProjectiles.PowerBombExplosion.BeginCrystalFlash(samus.Position.Whole);
            }
        }
        // Drained controller functions install special-movement-type poses whose normal
        // beta dispatcher is empty. Only the drained-fall animation cue replaces the handler
        // with the drained-falling mover; the other phases stay motionless while enemy AI controls
        // pose/timing. This branch is reached only when no independently retained
        // Crystal Flash handler still owns Samus's movement handler.
        // Controller zero changes only the pose, so a crashing spark keeps
        // its handler and finishes below; the drained-fall cue relinquishes the spark itself.
        // The rainbow-beam lock is the beta lock itself: once command one restores
        // the normal beta during enemy AI, this same frame's beta dispatches the knockback
        // pose's knockback mover below, as the movement table does.
        // The controller-held standing/crouching phases last only while Samus keeps a
        // drained body: a pose the movement installs (a wall jump out of a drained
        // spin) returns her to the ordinary movement and input dispatch. So does the landing of a drained fall that an
        // ordinary pose's animation installed by reading into a drained crouch's animation (#1535): the landing restores the
        // normal movement handler, which only a drained pose's special movement leaves empty.
        else if (samus.Drained.Phase != DrainedSamusPhase.Inactive &&
            !(samus.Drained.Phase is DrainedSamusPhase.Standing or DrainedSamusPhase.Crouching or DrainedSamusPhase.OnFloor &&
              !SamusPoseMetadata.IsDrainedPose(samus.Pose)) &&
            !(samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked && !samus.Control.AlphaLocked) &&
            (samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked || !samus.Shinespark.OwnsMovementHandler))
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            if (samus.Drained.Phase == DrainedSamusPhase.Falling)
            {
                _player.LastDrainedSamusMovement = samus.Drained.StepFalling(

                    _room.LevelData,
                    samus,
                    _clock.Counter);
            }
            else if (samus.InitializedPose != samus.Pose)
            {
                // The drained-pose controllers store a drained pose without
                // initializing it, and the movement table keeps dispatching the cached
                // movement type. An airborne Samus therefore keeps her jump
                // physics in the drained pose until a real initializer runs.
                StepUninitializedPoseMovement(room.RoomObjects);
            }
        }
        // The shinespark replaces the normal movement handler. Windup, active
        // launch, and crash therefore own beta movement regardless of the pose's
        // table index; ordinary special-movement-type physics does not exist to fall back to.
        else if (samus.Shinespark.OwnsMovementHandler)
        {
            // Samus's environment-selected vertical acceleration pair is used
            // as spark acceleration. The projectile step does not replace it.
            SamusAerialMovement.ConfigureEnvironmentGravity(samus);
            _player.LastShinesparkMovement = samus.Shinespark.Step(new SamusTerrain { Level = _room.LevelData, RoomObjects = room.RoomObjects }, samus, new ShinesparkFrame { FrameCounter = _clock.Counter, SamusCannotDie = _assists.Current.SamusCannotDie, Projectiles = room.Projectiles, GameTimeFrames = _gameTime.Frames, DeferTimeoutPoseChange = true, PreviousCheckpoint = frame.PreviousCameraPoint });
            if (_player.LastShinesparkMovement.Value.WindupTimedOut)
            {
                // The movement handler publishes an interrupted vertical pose. That
                // priority beats an ordinary input record sampled from the shinespark windup poses
                // earlier in alpha during the timeout frame.
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
        }
        // Samus's lock command installs an alpha handler which advances existing
        // room.Projectiles but a beta handler which performs no movement at all. All
        // special movement owners above take priority; an otherwise
        // locked standing body must not run generic ground collision or drift while
        // the game waits at the Ceres elevator.
        else if (samus.Control.AlphaLocked &&
            !(SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
              room.Enemies.Elevator.ElevatorRunsSamusMovement))
        {
            // Elevator command seven installs the moving beta handler for departure.
            // Finishing the door's opening scroll reinstalls it only on downward arrival. Upward arrival
            // retains command zero until the actor reaches rest; terrain probes there
            // would incorrectly wake the scroll triggers the platform passes.
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
        }
        // The movement dispatch returns before the movement table and echo update while time is
        // frozen. X-ray setup freezes time in alpha, so a running Samus stops on
        // the activation frame before her pose changes.
        else if (_freeze.TimeIsFrozen)
        {
            // No positional movement.
        }
        // The special movement type also has a normal movement-table entry.
        // It only clears the momentum-transition selector. This path is reachable
        // when hurt-expiry command one replaces a freshly installed Shinespark
        // handler while retaining its shinespark windup body and palette (Shinespark Suit).
        else if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Special)
        {
            // No positional movement.
        }
        else
            RunPoseMovement(samus, room.RoomObjects, room);
    }

    /// <summary>
    /// Runs the pose's normal movement-table entry. Every pose the retail producers reach has a branch; any
    /// other pose is corrupt state.
    /// </summary>
    private void RunPoseMovement(SamusState samus, ISamusRoomObjects roomObjects, RoomScope room)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        switch (samus.Pose)
        {
            case SamusPoseId.ForwardFacingPowerSuitPose:
            case SamusPoseId.ForwardFacingSuitedPose:
                StepForwardFacingMovement(samus, roomObjects, room);
                break;
            case SamusPoseId.FacingRightNormalPose:
            case SamusPoseId.StandingAimUpRightPose:
            case SamusPoseId.StandingAimDiagonalUpRightPose:
            case SamusPoseId.StandingAimDiagonalDownRightPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepStanding(_room.LevelData, samus, _clock.Counter, _controller1.Current, roomObjects);
                break;
            case SamusPoseId.MovingRightNormalPose:
            case SamusPoseId.MovingRightGunExtendedPose:
            case SamusPoseId.RunningAimUpRightPose:
            case SamusPoseId.RunningAimDiagonalUpRightPose:
            case SamusPoseId.RunningAimDiagonalDownRightPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepRunning(_room.LevelData, samus, _clock.Counter, _controller1.Current, roomObjects);
                break;
            case SamusPoseId.FacingLeftNormalPose:
            case SamusPoseId.StandingAimUpLeftPose:
            case SamusPoseId.StandingAimDiagonalUpLeftPose:
            case SamusPoseId.StandingAimDiagonalDownLeftPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepStanding(_room.LevelData, samus, _clock.Counter, _controller1.Current, roomObjects);
                break;
            case SamusPoseId.MovingLeftNormalPose:
            case SamusPoseId.MovingLeftGunExtendedPose:
            case SamusPoseId.RunningAimUpLeftPose:
            case SamusPoseId.RunningAimDiagonalUpLeftPose:
            case SamusPoseId.RunningAimDiagonalDownLeftPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepRunning(_room.LevelData, samus, _clock.Counter, _controller1.Current, roomObjects);
                break;
            case SamusPoseId.MoonwalkFacingLeftPose:
            case SamusPoseId.MoonwalkFacingRightPose:
            case SamusPoseId.MoonwalkAimUpLeftPose:
            case SamusPoseId.MoonwalkAimUpRightPose:
            case SamusPoseId.MoonwalkAimDownLeftPose:
            case SamusPoseId.MoonwalkAimDownRightPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepMoonwalking(_room.LevelData, samus, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.RanIntoWallRightPose:
            case SamusPoseId.RanIntoWallLeftPose:
            case SamusPoseId.RanIntoWallAimUpRightPose:
            case SamusPoseId.RanIntoWallAimUpLeftPose:
            case SamusPoseId.RanIntoWallAimDownRightPose:
            case SamusPoseId.RanIntoWallAimDownLeftPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepRanIntoWall(_room.LevelData, samus, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.TurningRightToLeftPose:
            case SamusPoseId.TurningLeftToRightPose:
            case SamusPoseId.TurningRightToLeftAimUpPose:
            case SamusPoseId.TurningLeftToRightAimUpPose:
            case SamusPoseId.TurningRightToLeftAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightAimDiagonalUpPose:
            case SamusPoseId.TurningRightToLeftAimDiagonalDownPose:
            case SamusPoseId.TurningLeftToRightAimDiagonalDownPose:
            case SamusPoseId.TurningRightToLeftCrouchingPose:
            case SamusPoseId.TurningLeftToRightCrouchingPose:
            case SamusPoseId.TurningRightToLeftCrouchingAimUpPose:
            case SamusPoseId.TurningLeftToRightCrouchingAimUpPose:
            case SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose:
            case SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose:
            case SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose:
            case SamusPoseId.MoonwalkTurnJumpLeftPose:
            case SamusPoseId.MoonwalkTurnJumpRightPose:
            case SamusPoseId.MoonwalkTurnJumpAimUpLeftPose:
            case SamusPoseId.MoonwalkTurnJumpAimUpRightPose:
            case SamusPoseId.MoonwalkTurnJumpAimDownLeftPose:
            case SamusPoseId.MoonwalkTurnJumpAimDownRightPose:
                StepTurningPoseMovement(samus, roomObjects);
                break;
            case SamusPoseId.NormalLandingRightPose:
            case SamusPoseId.NormalLandingLeftPose:
            case SamusPoseId.SpinLandingRightPose:
            case SamusPoseId.SpinLandingLeftPose:
            case SamusPoseId.LandingAimUpRightPose:
            case SamusPoseId.LandingAimUpLeftPose:
            case SamusPoseId.LandingAimDiagonalUpRightPose:
            case SamusPoseId.LandingAimDiagonalUpLeftPose:
            case SamusPoseId.LandingAimDiagonalDownRightPose:
            case SamusPoseId.LandingAimDiagonalDownLeftPose:
            case SamusPoseId.FiringLandingRightPose:
            case SamusPoseId.FiringLandingLeftPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepLanding(_room.LevelData, samus, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.MorphBallGroundRightPose:
            case SamusPoseId.MorphBallGroundLeftPose:
            case SamusPoseId.MorphBallMovingRightPose:
            case SamusPoseId.MorphBallMovingLeftPose:
            case SamusPoseId.SpringBallGroundRightPose:
            case SamusPoseId.SpringBallGroundLeftPose:
            case SamusPoseId.SpringBallMovingRightPose:
            case SamusPoseId.SpringBallMovingLeftPose:
                _player.LastMorphBallMovement = SamusMorphBallMovement.StepGrounded(_room.LevelData, samus, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.MorphBallFallingRightPose:
            case SamusPoseId.MorphBallFallingLeftPose:
            case SamusPoseId.SpringBallFallingRightPose:
            case SamusPoseId.SpringBallFallingLeftPose:
                _player.LastMorphBallMovement = SamusMorphBallMovement.StepFalling(_room.LevelData, samus, _controller1.Current, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.SpringBallJumpRightPose:
            case SamusPoseId.SpringBallJumpLeftPose:
                _player.LastMorphBallMovement = SamusMorphBallMovement.StepSpringBallInAir(_room.LevelData, samus, _controller1.Current, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.NeutralJumpTransitionRightPose:
            case SamusPoseId.NeutralJumpTransitionLeftPose:
            case SamusPoseId.NeutralJumpRightPose:
            case SamusPoseId.NeutralJumpLeftPose:
            case SamusPoseId.NormalJumpGunExtendedRightPose:
            case SamusPoseId.NormalJumpGunExtendedLeftPose:
            case SamusPoseId.NormalJumpForwardRightPose:
            case SamusPoseId.NormalJumpForwardLeftPose:
            case SamusPoseId.NormalJumpAimUpRightPose:
            case SamusPoseId.NormalJumpAimUpLeftPose:
            case SamusPoseId.NormalJumpTransitionAimUpRightPose:
            case SamusPoseId.NormalJumpTransitionAimUpLeftPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose:
            case SamusPoseId.NormalJumpAimDiagonalUpRightPose:
            case SamusPoseId.NormalJumpAimDiagonalUpLeftPose:
            case SamusPoseId.NormalJumpAimDiagonalDownRightPose:
            case SamusPoseId.NormalJumpAimDiagonalDownLeftPose:
            case SamusPoseId.NormalJumpAimDownRightPose:
            case SamusPoseId.NormalJumpAimDownLeftPose:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepNormalJump(_room.LevelData, samus, _controller1.Current, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.SpinJumpRightPose:
            case SamusPoseId.SpinJumpLeftPose:
            case SamusPoseId.SpaceJumpRightPose:
            case SamusPoseId.SpaceJumpLeftPose:
            case SamusPoseId.ScrewAttackRightPose:
            case SamusPoseId.ScrewAttackLeftPose:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepSpinJump(_room.LevelData, samus, _controller1.Current, _clock.Counter, _controller1.NewlyPressed, roomObjects);
                break;
            case SamusPoseId.WallJumpRightPose:
            case SamusPoseId.WallJumpLeftPose:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepWallJump(_room.LevelData, samus, _controller1.Current, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.DamageBoostRightPose:
            case SamusPoseId.DamageBoostLeftPose:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepDamageBoost(_room.LevelData, samus, _controller1.Current, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.TurningRightToLeftJumpPose:
            case SamusPoseId.TurningLeftToRightJumpPose:
            case SamusPoseId.TurningRightToLeftJumpAimUpPose:
            case SamusPoseId.TurningLeftToRightJumpAimUpPose:
            case SamusPoseId.TurningRightToLeftJumpAimDownPose:
            case SamusPoseId.TurningLeftToRightJumpAimDownPose:
            case SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose:
            case SamusPoseId.TurningRightToLeftFallingPose:
            case SamusPoseId.TurningLeftToRightFallingPose:
            case SamusPoseId.TurningRightToLeftFallingAimUpPose:
            case SamusPoseId.TurningLeftToRightFallingAimUpPose:
            case SamusPoseId.TurningRightToLeftFallingAimDownPose:
            case SamusPoseId.TurningLeftToRightFallingAimDownPose:
            case SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepTurningInAir(_room.LevelData, samus, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.FallingRightPose:
            case SamusPoseId.FallingLeftPose:
            case SamusPoseId.FallingGunExtendedRightPose:
            case SamusPoseId.FallingGunExtendedLeftPose:
            case SamusPoseId.FallingAimUpRightPose:
            case SamusPoseId.FallingAimUpLeftPose:
            case SamusPoseId.FallingAimDiagonalUpRightPose:
            case SamusPoseId.FallingAimDiagonalUpLeftPose:
            case SamusPoseId.FallingAimDiagonalDownRightPose:
            case SamusPoseId.FallingAimDiagonalDownLeftPose:
            case SamusPoseId.FallingAimDownRightPose:
            case SamusPoseId.FallingAimDownLeftPose:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepFalling(_room.LevelData, samus, _controller1.Current, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.CrouchingRightPose:
            case SamusPoseId.CrouchingLeftPose:
            case SamusPoseId.CrouchingAimUpRightPose:
            case SamusPoseId.CrouchingAimUpLeftPose:
            case SamusPoseId.CrouchingAimDiagonalUpRightPose:
            case SamusPoseId.CrouchingAimDiagonalUpLeftPose:
            case SamusPoseId.CrouchingAimDiagonalDownRightPose:
            case SamusPoseId.CrouchingAimDiagonalDownLeftPose:
                _player.LastGroundedSamusMovement = SamusPostureMovement.StepCrouching(_room.LevelData, samus, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.KnockbackRightPose:
            case SamusPoseId.KnockbackLeftPose:
                StepKnockbackEndingMovement(samus, roomObjects);
                break;
            case SamusPoseId.CrouchingTransitionRightPose:
            case SamusPoseId.CrouchingTransitionLeftPose:
            case SamusPoseId.StandingTransitionRightPose:
            case SamusPoseId.StandingTransitionLeftPose:
            case SamusPoseId.CrouchingTransitionAimUpRightPose:
            case SamusPoseId.CrouchingTransitionAimUpLeftPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose:
            case SamusPoseId.StandingTransitionAimUpRightPose:
            case SamusPoseId.StandingTransitionAimUpLeftPose:
            case SamusPoseId.StandingTransitionAimDiagonalUpRightPose:
            case SamusPoseId.StandingTransitionAimDiagonalUpLeftPose:
            case SamusPoseId.StandingTransitionAimDiagonalDownRightPose:
            case SamusPoseId.StandingTransitionAimDiagonalDownLeftPose:
                _player.LastGroundedSamusMovement = SamusPostureMovement.StepCrouchStandTransition(_room.LevelData, samus, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.MorphingTransitionRightPose:
            case SamusPoseId.MorphingTransitionLeftPose:
            case SamusPoseId.UnmorphingTransitionRightPose:
            case SamusPoseId.UnmorphingTransitionLeftPose:
                _player.LastMorphBallMovement = SamusMorphBallMovement.StepTransition(_room.LevelData, samus, _clock.Counter, roomObjects);
                break;
            case SamusPoseId.DraygonGrabbedNeutralLeftPose:
            case SamusPoseId.DraygonGrabbedAimUpLeftPose:
            case SamusPoseId.DraygonGrabbedFiringLeftPose:
            case SamusPoseId.DraygonGrabbedAimDownLeftPose:
            case SamusPoseId.DraygonGrabbedMovingLeftPose:
            case SamusPoseId.DraygonGrabbedNeutralRightPose:
            case SamusPoseId.DraygonGrabbedAimUpRightPose:
            case SamusPoseId.DraygonGrabbedFiringRightPose:
            case SamusPoseId.DraygonGrabbedAimDownRightPose:
            case SamusPoseId.DraygonGrabbedMovingRightPose:
            case SamusPoseId.DraygonHeldBallLeftPose:
                // This is the normal dispatcher, which only clears
                // the vertical solid-collision result. The actual boss grab above
                // runs its separately installed empty handler instead. The Draygon-held
                // ball the Hyper Beam freeze installs (#1535) has no Draygon, so it stays put.
                SamusDraygonGrabbedState.StepMovement(samus);
                break;
            case SamusPoseId.GlitchBallRightPose:
            case SamusPoseId.GlitchBallLeftPose:
            case SamusPoseId.DraygonHeldStandingUpLeftPose:
                // The glitch-ball movement type's normal entry only clears the vertical solid-collision
                // result, and so does the posture transition's for the compact transitions, which skips
                // their horizontal and vertical movement; nothing lands them, so the landed cleanup never runs.
                // Both hold Samus where the over-read installed them (#1535).
                samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.None;
                break;
            case SamusPoseId.UnusedKnockbackRightPose:
            case SamusPoseId.UnusedKnockbackLeftPose:
                // The glitch-ball knockback's movement type has an empty normal entry: once its knockback
                // ends, Samus holds still in it (#1535).
                break;
            case SamusPoseId.FirstUnauthoredPose:
            case SamusPoseId.SecondUnauthoredPose:
            case SamusPoseId.LastUnauthoredPose:
            case SamusPoseId.CompactAirborneFallingRightPose:
            case SamusPoseId.CompactAirborneFallingLeftPose:
            case SamusPoseId.ShinesparkWindupRightPose:
            case SamusPoseId.ShinesparkWindupLeftPose:
            case SamusPoseId.ShinesparkHorizontalRightPose:
            case SamusPoseId.ShinesparkHorizontalLeftPose:
            case SamusPoseId.ShinesparkVerticalRightPose:
            case SamusPoseId.ShinesparkVerticalLeftPose:
            case SamusPoseId.ShinesparkDiagonalRightPose:
            case SamusPoseId.ShinesparkDiagonalLeftPose:
            case SamusPoseId.CrystalFlashRightPose:
            case SamusPoseId.CrystalFlashLeftPose:
            case SamusPoseId.XrayingStandingRightPose:
            case SamusPoseId.XrayingStandingLeftPose:
            case SamusPoseId.DeathSequenceRightPose:
            case SamusPoseId.DeathSequenceLeftPose:
            case SamusPoseId.XrayingCrouchingRightPose:
            case SamusPoseId.XrayingCrouchingLeftPose:
            case SamusPoseId.CompactMorphingRightPose:
            case SamusPoseId.DraygonHeldMorphingLeftPose:
            case SamusPoseId.CompactStandingUpRightPose:
            case SamusPoseId.DrainedCrouchingRightPose:
            case SamusPoseId.DrainedCrouchingLeftPose:
            case SamusPoseId.DrainedStandingRightPose:
            case SamusPoseId.DrainedStandingLeftPose:
            case SamusPoseId.GrappleSwingRightPose:
            case SamusPoseId.GrappleSwingLeftPose:
            case SamusPoseId.GrappleStandingRightPose:
            case SamusPoseId.GrappleStandingLeftPose:
            case SamusPoseId.GrappleStandingDownRightPose:
            case SamusPoseId.GrappleStandingDownLeftPose:
            case SamusPoseId.GrappleCrouchingRightPose:
            case SamusPoseId.GrappleCrouchingLeftPose:
            case SamusPoseId.GrappleCrouchingDownRightPose:
            case SamusPoseId.GrappleCrouchingDownLeftPose:
            case SamusPoseId.GrappleWallContactLeftPose:
            case SamusPoseId.GrappleWallContactRightPose:
            case SamusPoseId.GlitchBallMorphingRightPose:
            case SamusPoseId.GlitchBallMorphingLeftPose:
            case SamusPoseId.InstantMorphingRightPose:
            case SamusPoseId.InstantMorphingLeftPose:
            case SamusPoseId.CompactAirborneRightPose:
            case SamusPoseId.CompactAirborneLeftPose:
            case SamusPoseId.GrappleAirborneGunExtendedRightPose:
            case SamusPoseId.GrappleAirborneAimDownRightPose:
            case SamusPoseId.GrappleAirborneAimDiagonalDownRightPose:
            case SamusPoseId.DraygonGrabbingLeftPose:
            default:
                // Every pose reachable from the retail input, animation,
                // enemy-grab, death, drained, grapple, Morph Ball, and shinespark
                // producers has an explicit branch above. Reaching this point means
                // the pose no longer agrees with the game-backed producer
                // set; it is corrupt state, not a recoverable movement family.
                throw new InvalidDataException(
                    $"Runtime movement dispatcher received invalid pose {samus.Pose}.");
        }
    }

    /// <summary>Runs the forward-facing poses' movement, including the elevator's one-pixel downward block scan.</summary>
    private void StepForwardFacingMovement(SamusState samus, ISamusRoomObjects roomObjects, RoomScope room)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        // The two front-facing poses enter through controller-locked demo/elevator commands.
        // The actor-owned vertical-collision value remains an explicit producer, while
        // this consumer now includes both the zero-status no-op and the nonzero
        // one-pixel downward block scan.
        SamusGroundedMovement.StepFacingForward(

            _room.LevelData,
            samus,
            _clock.Counter,
            elevatorIsMoving:
                room.Enemies.Elevator.ElevatorStatus != ElevatorActorStatus.Inactive,
            roomObjects: roomObjects,
            publishDoorSideEffects:
                RoomMainRoutines.ShouldPublishFacingForwardElevatorDoorSideEffects(
                    room.Enemies.Elevator.ElevatorStatus));
    }

    /// <summary>Runs a turn pose's movement through its movement type: ground turning, or turning in the air.</summary>
    private void StepTurningPoseMovement(SamusState samus, ISamusRoomObjects roomObjects)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        // The movement table dispatches strictly through the pose definition's
        // movement type. Y direction is data consumed by the selected
        // handler, never a replacement dispatcher. In particular, crouched
        // left-to-right turn can retain downward direction while its type remains
        // turning on the ground; treating it as turning while jumping sent it into the aerial turn's handler
        // and produced the live replay crash from issue 58.
        SamusMovementType turnMovementType = SamusPoseMetadata.ReadMovementKind(samus);
        if (turnMovementType == SamusMovementType.TurningOnGround ||
            (turnMovementType == SamusMovementType.TurningWhileJumping &&
             samus.Kinematics.YDirection == SamusVerticalDirection.None))
        {
            _player.LastGroundedSamusMovement = SamusGroundedMovement.StepTurningOnGround(

                _room.LevelData,
                samus,
                _clock.Counter,
                roomObjects);
        }
        else if (turnMovementType is
            SamusMovementType.TurningWhileJumping or
            SamusMovementType.TurningWhileFalling)
        {
            _player.LastAerialSamusMovement = SamusAerialMovement.StepTurningInAir(

                _room.LevelData,
                samus,
                _clock.Counter,
                roomObjects);
        }
        else
        {
            throw new InvalidDataException(
                $"Turn pose {samus.Pose} has unsupported movement type " +
                $"{turnMovementType}.");
        }
    }

    /// <summary>Runs the knockback movement type's normal entry: the one-frame grounding tail before the knockback ending's target.</summary>
    private void StepKnockbackEndingMovement(SamusState samus, ISamusRoomObjects roomObjects)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        // The knockback movement type's normal movement-table entry is this one. Active
        // knockback and Ceres ejection were intercepted above; this branch
        // is their authentic one-frame normal-dispatch tail before 
        // installs the input/no-input target chosen during alpha.
        SignedSixteenSixteen groundingDisplacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics, samus.HorizontalSpeed);
        BlockMoveResult grounding = SamusGroundedMovement.StepKnockbackOrCrystalFlashEnding(

            _room.LevelData,
            samus,
            _clock.Counter,
            roomObjects);
        _player.LastGroundedSamusMovement = new GroundedMovementResult(default, grounding);
        // The normal knockback entry still publishes collision result one when its
        // downward probe hits the floor. Reuse the ordinary landing seam
        // for presentation, pose selection and command-five speed cleanup.
        if (grounding.Collided && !groundingDisplacement.IsNegative)
            _player.LastAerialSamusMovement = new AerialMovementResult(Landed: true, HitCeiling: false);
    }

    /// <summary>
    /// Runs alpha's head: X-ray's beam object, the frame's camera point, the frame-start captures, the live radius
    /// refresh, pose-input lookup and the lookup-failure pose.
    /// </summary>
    private void RunAlphaHead(SamusHandlerFrame frame, SamusState samus, ScrollBoundaryCamera camera,
        FixedPosition<RoomPixels>? cameraPointAtFrameStart, RoomScope room)
    {
        // X-ray's per-line effect object is not part of the Samus handler. Advance its explicit
        // setup/beam/deactivation state before alpha so state five can restore the
        // ordinary pose and handler pair before this frame samples pose input. The
        // reveal and the beam's window are the frame boundary's to show.
        _player.LastXrayBeamStep = null;
        _player.LastDeathSequenceStep = null;
        if (samus.Xray.IsActive && !samus.DeathSequence.IsActive)
        {
            _player.LastXrayBeamStep = samus.Xray.StepBeam(
                samus,
                _controller1.Current);
        }

        // Preserve movement on either side of the frame boundary. A new camera uses
        // the fallback sampled before enemy-owned motion and the handler.
        frame.SampleCameraPoint(camera.PreviousSamusPoint ?? cameraPointAtFrameStart ??
            throw new InvalidOperationException("A gameplay frame with Samus samples her camera point at its start."));

        // Retain the dispatch pose because the turn-transition animation cue can replace Samus.Pose during
        // animation later in this same frame. Alpha/beta/transition phases all
        // agree on that order; using the mutable value afterward would apply an input
        // match selected for the old pose to the newly installed one.
        _attractDemo.BeginAttractSamusInput();
        frame.SampleFrameStartPose(samus.Pose);
        // Beta dispatches on the pose installed when it runs, which the enemy pass can have
        // replaced since alpha: Draygon's death releases a sparking Samus to facing right normally,
        // and standing movement runs. The pose update then reads that pose's type.
        // Ordinary alpha publishes the current pose's live radius before input
        // and collision. Prospective transitions later in beta can retain the
        // previous radius until this point in the following frame. The locked
        // alpha has no radius update, so e.g. an elevator ride keeps
        // the boarding pose's radius.
        if (_player.GroundedSamusMovementEnabled && !_freeze.TimeIsFrozen && !frame.AlphaInputLocked(samus) &&
            !samus.DeathSequence.IsActive && !room.Enemies.Elevator.ElevatorDoorTransitionActive)
            SamusPoseMetadata.RefreshCollisionRadii(samus);
        // Samus beta precedes room objects. A lock/unlock issued by a room object
        // affects the next beta, not the animation already owned this frame.
        // The suit pickup lock installs an empty beta, not just locked pose input:
        // the suspended movement and all of its timers survive, so the unlock
        // command can resume it after the suit's light-beam transformation (including Blue Suit).
        // The empty beta also skips periodic damage and the energy checks.
        frame.SampleMovementType(SamusPoseMetadata.ReadMovementType(samus));

        // Direction bits already use the transition table's canonical layout. Input
        // matching belongs to frame-handler alpha, before beta moves the CURRENT pose;
        // the winning pose is not installed until after animation below.
        // The front-view poses get one special input rule: a
        // nonzero elevator status returns immediately, before the transition table
        // is even read. Pose zero's transitions otherwise overlap other poses'
        // data and can appear to select nonsense transitions after several frames, so
        // suppressing only their application would be too late and observably wrong.
        frame.SampleDeathOwnership(samus.DeathSequence.IsActive);
        SamusControlMode mode = new(samus.Control.Mode.Alpha, DispatchedBetaHandler(frame, samus));
        // Sound queues persist across the whole game, but this typed control
        // publication is scoped to one frame's Samus handler. It opened before the per-line
        // effect pass, ahead of the landing sounds during collision.
        frame.BeginControl(mode, samus.Xray.OwnsSamusControl && !frame.DeathOwnsSamus, SamusPoseMetadata.ReadMovementType(samus));
        if (frame.XrayOwnsPoseInput)
        {
            samus.Xray.HandlePoseInput(
                samus,
                _controller1.Current);
        }

        // The posture-transition movement type's input dispatcher is empty. Transition
        // poses can still have table entries, but reading them here prematurely
        // interrupts crouch/morph animations before their animation command wins.
        frame.SamplePoseInputLocks(samus,
            ((samus.InputSnapshot.ShinesparkPoseInputLocked || samus.InputSnapshot.CrystalFlashPoseInputLocked) && !samus.InputSnapshot.AutoJumpInputPending) ||
            (SamusPoseMetadata.IsForwardFacingPose(samus.Pose) && room.Enemies.Elevator.ElevatorStatus != ElevatorActorStatus.Inactive));
        SamusPoseTransitionLookup poseLookup =
            frame.DeathOwnsSamus || frame.XrayOwnsPoseInput || frame.ActorLocksPoseInput || frame.PostureTransitionSkipsPoseInput
            ? default
            : SamusPoseTransitionTable.Lookup(
                samus.Pose,
                _controller1.Current,
                _attractDemo.IsAttractDemo ? _controller1.NewlyPressed : samus.InputSnapshot.ConsumeAutoJumpInput(_controller1.NewlyPressed));
        _player.ProspectiveSamusPose = poseLookup.Transition;
        // The retail table matcher clears a pending/active bomb direction as
        // soon as it selects a DIFFERENT pose, before collision and movement.
        if (_player.ProspectiveSamusPose is { } matchedBombCancel &&
            matchedBombCancel.ProspectivePose != samus.Pose)
            samus.BombJumpRequest = SamusBombJumpRequest.None;
        frame.SamplePoseLookup(poseLookup, samus);
        _player.ProspectiveSamusFallbackPose = null;
        _player.ProspectiveSamusWallCollisionPose = null;

        // Zero input and an unmatched nonzero table chord both reach the input fallback; a matched
        // same-pose record does not. Writes the prospective pose slot in alpha and
        // commits it after beta movement and animation, so the selection is captured here.
        // A retained pose still selects a transition slot whose command runs later.
        if (_player.GroundedSamusMovementEnabled && frame.UsePoseDefinitionFallback)
        {
            _player.ProspectiveSamusFallbackPose = SamusLookupFailurePose.Resolve(
                SamusPoseMetadata.ReadMovementType(samus),
                samus.Pose,
                SamusPoseMetadata.ReadNoInputFallbackPose(samus),
                frame.DeceleratingFallbackHasMomentum).ProspectivePose;
        }
    }

    /// <summary>
    /// Runs normal beta's tail after movement: the speed echo, landing effects, the timer/hack handler
    ///  with Draygon's escape, animation or the death sequence, the pose transitions, and periodic damage with
    /// the low-energy check.
    /// </summary>
    private void RunBetaTail(SamusHandlerFrame frame, SamusState samus, ISamusFrameAudio audio, ISamusRoomObjects roomObjects, RoomScope room)
    {
        // Samus's normal movement handler calls the speed-echo update after the selected movement
        // function and before animation/pose transitions. Every fourth gameplay frame
        // at boost stage four, it alternates between two exact world-position snapshots.
        // Keeping this producer here means the draw handler below consumes post-motion
        // coordinates with the same frame ordering as the game.
        if (frame.Mode.Beta != SamusBetaHandler.SuitPickup && _player.LastShinesparkMovement is null)
        {
            samus.HorizontalSpeed.CaptureSpeedEchoPosition(_gameTime.Frames, samus.Position.Whole);
        }

        // Every ordinary downward collision calls the landing-effects update inside movement, before
        // the timer hack, liquid animation, prospective-pose selection, and velocity
        // cleanup. Knockback clears its live vertical speed on landing before this
        // consumer; do not restore its diagnostic pre-impact magnitude for effects.
        // Drained movement retains its separately modeled impact publication.
        bool ordinaryOrBallLanded =
            _player.LastAerialSamusMovement is { Landed: true } ||
            _player.LastMorphBallMovement is { Landed: true };
        bool knockbackLanded = _player.LastKnockbackMovement is { Landed: true };
        DrainedSamusMovementResult? drainedLanding = _player.LastDrainedSamusMovement is { Landed: true } landing ? landing : null;
        bool drainedLanded = drainedLanding is not null;
        if (!frame.DeathOwnsSamus && (ordinaryOrBallLanded || knockbackLanded || drainedLanded))
        {
            UnsignedSixteenSixteen impactYSpeed = drainedLanding is { } drainedSpeed
                ? drainedSpeed.ImpactYSpeed
                : samus.Kinematics.YSpeed;
            samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
                samus,
                frame.MovementTypeAtFrameStart,
                frame.PoseAtFrameStart,
                impactYSpeed);
        }

        // The timer/hack handler runs after movement and before animation.
        // Two of Samus's commands install the Mother Brain-specific Up-edge branches; the
        // method is a cheap no-op for every ordinary gameplay state.
        if (frame.Mode.Beta != SamusBetaHandler.SuitPickup)
            samus.Drained.StepGetUpHandler(samus, _controller1.NewlyPressed);

        if (frame.Mode.Beta != SamusBetaHandler.SuitPickup && samus.DraygonGrabbed.IsActive)
        {
            // The movement handler calls this installed hack handler after movement and before
            // animation. A locked grapple cancels the prospective pose chosen in alpha;
            // an escape release additionally clears every pending route while installing
            // the ordinary facing-normal poses and zeroing the documented velocities.
            _player.LastDraygonEscape = samus.DraygonGrabbed.StepEscapeHandler(
                samus,
                _controller1.NewlyPressed,
                samus.Grapple.Phase == GrapplePhase.ConnectedLocked);
            if (_player.LastDraygonEscape.Value.SuppressProspectivePose ||
                _player.LastDraygonEscape.Value.Released)
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
        }

        // Normal gameplay advances animation during frame-handler beta, before the
        // pose-transition handler, draw handler, and next frame's tile selection.
        // Animation sees the same held input as movement. Ordinary Dash uses B
        // both to accumulate the running delay and to intercept the running animation's
        // cue; passing a reconstructed input later would skew art.
        if (frame.Mode.Beta == SamusBetaHandler.Fatal)
        {
            _player.LastDeathSequenceStep = samus.DeathSequence.Step(
                samus,
                _player.Colours,
                _artwork.SamusBodyArt?.DeathSteps);
        }
        // Command six's empty beta skips animation like the locked beta.
        else if (frame.Mode.Beta is not (SamusBetaHandler.ScriptLocked or SamusBetaHandler.RefillStation or SamusBetaHandler.SuitPickup))
        {
            SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { BossPresent = room.Enemies.RoomStatus.Boss is not null, ControllerInput = _controller1.Current, FrameCounter = _clock.Counter, Random = _system.Random, BeginLiquidSoundRequestFrame = false, ProspectiveInputPose = _player.ProspectiveSamusPose?.ProspectivePose ?? _player.ProspectiveSamusFallbackPose, DemoPoseInput = _attractDemo.IsAttractDemo, QueueEchoSound = audio.QueueEchoSound });
            StepSamusLoadAppearance(room);
        }

        _transitions.Apply(frame, samus, roomObjects);

        if (frame.Mode.Beta is not (SamusBetaHandler.Fatal or SamusBetaHandler.TitleDemo or SamusBetaHandler.SuitPickup))
        {
            // Normal beta ends with the periodic damage, the pause check and
            // the low-energy check, before the enemy-projectile and room-object handlers. Damage those
            // handlers accumulate this frame therefore lands next frame.
            // Demo beta omits these calls, and so does the suit pickup lock's empty beta:
            // damage pending during the transformation stays accumulated
            // until the first ordinary beta after it, which divides it by the new suit.
            // X-ray freezes time and therefore clears
            // rather than applies accumulated damage; fatal zero-energy game-state
            // acquisition remains the outer seam.
            samus.LiquidPhysics.ApplyPeriodicDamage(
                samus,
                timeIsFrozen: _freeze.TimeIsFrozen);
            // Locked/elevator/appearance handlers omit the low-energy check; the gunship command
            // installs a dedicated checker. Automatic reserves invoke their external
            // check in the frontend.
            if (frame.Mode.Beta is SamusBetaHandler.Normal or SamusBetaHandler.Gunship && !samus.Xray.OwnsSamusControl)
                audio.CheckLowHealth?.Invoke();
        }
    }

    /// <summary>
    /// Advances the saved-game appearance owner. The ordinary Samus
    /// animation and palette steps have already run earlier in this frame;
    /// this tail owns only the exact 360-call fanfare lifetime and handler restoration.
    /// </summary>
    internal void StepSamusLoadAppearance(RoomScope room)
    {
        if (!_player.LoadAppearanceActive)
            return;
        SamusState samus = _player.Samus
            ?? throw new InvalidOperationException("Samus load appearance lost its actor.");

        if (!_player.AdvanceLoadAppearance())
            return;
        if (_player.LoadAppearancePaletteFx is { } loadAppearance && room.PaletteFx.IsRunning(loadAppearance))
        {
            throw new InvalidDataException(
                $"Samus load palette FX {loadAppearance} " +
                "outlived the 360-frame load appearance.");
        }

        // The item fanfare publishes its same-pose history shift only on the
        // completion call, before the ordinary handlers regain control.
        samus.CommitPoseHistory();
        samus.Control.Unlock();
        _player.GroundedSamusMovementEnabled = true;
        _player.EndLoadAppearance();
    }
}
