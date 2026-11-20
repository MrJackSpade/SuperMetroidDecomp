using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// Samus's phases of a gameplay frame: grounded movement, animation transitions, uninitialized-pose movement,
/// the grapple beam, X-ray from the HUD, load appearance and showing the equipped beams' colours.
/// </summary>
internal sealed class SamusFramePhases
{
    /// <summary>Opens the attract demo's Samus input window and selects the demo's new-input state.</summary>
    private readonly IAttractSamusInput _attractDemo;
    /// <summary>Supplies the held and newly pressed controller buttons to Samus's frame phases.</summary>
    private readonly GameplayActionInput _playerActions;
    /// <summary>Gates movement and animation branches during gameplay time freeze.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Supplies elapsed gameplay frames to the shinespark while it moves Samus.</summary>
    private readonly GameTimeState _gameTime;
    /// <summary>Provides frame-counter cadence for movement and animation behavior.</summary>
    private readonly FrameClock _clock;
    /// <summary>The runtime's assists; a shinespark reads whether Samus cannot die.</summary>
    private readonly ActiveAssists _assists;
    /// <summary>Owns the live Samus state and the selection of her input and movement modes.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Supplies the shared game state consulted by Samus's movement.</summary>
    private readonly SharedGameState _system;
    /// <summary>Samus's passage through a door, during which her input handling and radius update do not run.</summary>
    private readonly DoorPassage _doorPassage;
    /// <summary>The pose update after the normal movement phase's animation.</summary>
    private readonly SamusPoseTransitionPhase _transitions;

    /// <summary>Binds the frame owners required to preserve Samus's input, movement and presentation ordering.</summary>
    internal SamusFramePhases(SamusFramePhasesDependencies dependencies)
    {
        _attractDemo = dependencies.AttractDemo;
        _playerActions = dependencies.PlayerActions;
        _freeze = dependencies.Freeze;
        _gameTime = dependencies.GameTime;
        _clock = dependencies.Clock;
        _assists = dependencies.Assists;
        _player = dependencies.Player;
        _system = dependencies.System;
        _doorPassage = dependencies.DoorPassage;
        _transitions = new SamusPoseTransitionPhase(new SamusPoseTransitionPhaseDependencies { PlayerActions = dependencies.PlayerActions, Freeze = dependencies.Freeze, Clock = dependencies.Clock, Options = dependencies.Options, Player = dependencies.Player });
    }

    /// <summary>
    /// Shows the colours of the beams pause teardown supplies. Unpausing does this while the screen is dark, so the
    /// first resumed gameplay frame shows the newly equipped combination's colours.
    /// </summary>
    internal void ShowEquippedBeamColours(SamusBeamFlags equippedBeams) =>
        SamusProjectileSystem.ShowBeamShade(_player.Colours, equippedBeams);

    /// <summary>
    /// The grapple beam's step, reached from the HUD selection of
    /// Samus's HUD-specific behaviour and projectiles in her input phase.
    /// It therefore runs before Samus's projectiles and before the enemies' main step,
    /// so enemies see the beam endpoint and Samus position it leaves this frame. Only
    /// connected and release phases own Samus's movement; an extending or cancelling
    /// beam coexists with the current pose's ordinary movement. The step's result is the frame's, for the movement and the
    /// pose update after it.
    /// </summary>
    private bool RunGrappleBeamStep(SamusControlFrame frame, SamusState samus, RoomScope room)
    {
        bool grappleOwnsMovement = false;
        if (frame.Mode.Movement is SamusMovementMode.Fatal or SamusMovementMode.SuitPickup)
        {
            // The death sequence's game states do not run Samus's ordinary input and movement
            // phases. The death state advances later at the animation seam.
        }
        else if (samus.Grapple.Phase == GrapplePhase.Firing)
        {
            frame.Record(SamusGrappleMovement.StepFiring(
                room.LevelData,
                samus,
                _playerActions.Current,
                room.RoomObjects,
                room.Enemies.GrappleTargets.ResolveGrappleEndpoint));
            grappleOwnsMovement = frame.GrappleMovement is { OwnsMovement: true };
        }
        else if (samus.Grapple.Phase == GrapplePhase.CancelPending)
        {
            frame.Record(SamusGrappleMovement.CompleteFiringCancellation(room.LevelData, samus));
            grappleOwnsMovement = frame.GrappleMovement is { OwnsMovement: true };
        }
        else if (samus.Grapple.Phase is
            GrapplePhase.ConnectedSwinging or GrapplePhase.ReleaseFromSwing or
            GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab or
            GrapplePhase.WallGrabRelease or GrapplePhase.WallJumping or
            GrapplePhase.Dropped)
        {
            frame.Record(SamusGrappleMovement.Step(new SamusTerrain { Level = room.LevelData, RoomObjects = room.RoomObjects }, samus, _playerActions.Current, _playerActions.NewlyPressed, room.Enemies.GrappleTargets.ResolveGrappleEndpoint));
            grappleOwnsMovement = frame.GrappleMovement is { OwnsMovement: true };
        }
        else if (!_freeze.TimeIsFrozen &&
                 SamusGrappleHudInput.IsSelectedAndAdmitted(samus, room.Projectiles.PostureAdmission) &&
                 ((_playerActions.NewlyPressed | samus.InputSnapshot.PressesSeenLastUpdate) & GameplayAction.Shoot) != GameplayAction.None)
        {
            // Selecting the grapple on the HUD fires it once the pose's movement type admits it.
            SamusGrappleMovement.BeginFiring(samus, _playerActions.Current);
            frame.Record(new GrappleMovementResult(
                samus.Grapple.Phase,
                Fired: samus.Grapple.Phase == GrapplePhase.Firing,
                OwnsMovement: false));
        }

        if (frame.GrappleMovement is { Fired: true })
            ShowGrappleShades();
        else if (frame.GrappleMovement is { Phase: GrapplePhase.Inactive })
        {
            // A grapple that ends by cancellation, a drop, a wall jump or a release from its swing restores the equipped
            // beam's shade as it becomes inactive.
            _player.Colours.ShowBeams(SuperMetroid.Core.Samus.Colours.SamusShades.Beam(SamusBeamCombinations.Of(samus.EquippedBeams)));
        }

        // Whether the grapple swings submerged is decided after every grapple phase, inactive included, so the swing
        // above used the previous frame's answer and this frame's answer serves the next frame's swing.
        SamusGrappleMovement.RefreshLiquidPhysicsFlag(samus);
        return grappleOwnsMovement;
    }

    /// <summary>Shows the grapple's firing beam shade and its flare's shade as Samus's colours (#1512).</summary>
    private void ShowGrappleShades()
    {
        _player.Colours.ShowBeams(SuperMetroid.Core.Samus.Colours.SamusShades.Beam(GrappleBeamDefinitions.BeamShades.FiringSelection));
        _player.Colours.ShowGrappleFlare(SuperMetroid.Core.Samus.Colours.SamusShades.GrappleFiringFlare);
    }

    /// <summary>
    /// Runs the complete Samus-side X-ray admission path after the HUD has selected the
    /// equipped scope and observed held Dash, matching the HUD's X-ray selection, its setup, and the X-ray spawn.
    /// </summary>
    /// <remarks>
    /// The HUD selection has already settled on X-ray this update. This still requires the X-ray Scope equipped
    /// and passes the live game state (the pause darkening and unpause brightening states
    /// also run this input phase but setup rejects them), bomb cooldown/count, power-bomb phase, current/previous
    /// movement types, vertical velocity, landing-pose gate, and speed divisor to the X-ray
    /// setup logic.
    /// </remarks>
    private bool TryBeginXrayFromSelectedHudItem(SuperMetroid.Core.Progression.SuperMetroidGameState gameState, SamusState samus, SamusBombProjectileSystem bombProjectiles,
        out SamusPoseId? activationPose)
    {
        activationPose = null;
        // The HUD's selection prevents selecting the HUD item without the scope. The lower-level
        // X-ray spawn assumes that selector has already succeeded.
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope))
            return false;

        return samus.Xray.TryBegin(samus, _player.PreviousMovementTypeForXray, new XrayBeginConditions { GameState = gameState, PowerBombExplosionActive = bombProjectiles.PowerBombExplosion.IsActive, ProjectileCooldownTimer = bombProjectiles.CooldownTimer, BombCounter = bombProjectiles.BombCounter },
            out activationPose);
    }

    /// <summary>
    /// Runs Samus's movement for a pose stored without
    /// initializing it: the cached movement type, not the pose, selects the mover.
    /// </summary>
    private void StepUninitializedPoseMovement(SamusControlFrame frame, SamusState samus, RoomScope room)
    {
        RoomLevelData level = room.LevelData;
        ISamusRoomObjects roomObjects = room.RoomObjects;
        // The drained pose is stored uninitialized only over the special movement type, whose momentum reset only clears the
        // momentum-transition selector, and the two jumps (#2291).
        SamusMovementType movement = SamusPoseMetadata.ReadMovementType(samus);
        if (movement == SamusMovementType.SpinJumping)
        {
            frame.Record(SamusAerialMovement.StepSpinJump(
                level, samus, _playerActions.Current, _clock.Counter,
                _playerActions.NewlyPressed, roomObjects));
        }
        else if (movement == SamusMovementType.NormalJumping)
        {
            frame.Record(SamusAerialMovement.StepNormalJump(
                level, samus, _playerActions.Current, _clock.Counter, roomObjects));
        }
        else if (movement != SamusMovementType.Special)
        {
            throw new NotSupportedException(
                $"Cached movement type {movement} has no movement under uninitialized pose {samus.Pose}.");
        }
    }

    /// <summary>
    /// The movement mode installed for this movement phase: the first of death, the suit's empty movement mode, the stationary lock, the station lock,
    /// the attract demo, the gunship's mode and another lock that holds, otherwise the normal mode.
    /// </summary>
    private SamusMovementMode SelectedMovementMode(SamusControlFrame frame, SamusState samus)
    {
        if (frame.DeathOwnsSamus)
            return SamusMovementMode.Fatal;
        SamusMovementMode installed = samus.Control.Mode.Movement;
        if (installed is SamusMovementMode.SuitPickup or SamusMovementMode.ScriptLocked or SamusMovementMode.RefillStation)
            return installed;
        return _attractDemo.IsAttractDemo ? SamusMovementMode.TitleDemo : installed;
    }

    /// <summary>
    /// Runs the input phase: the pose her input chooses, then, when Samus movement is enabled, her HUD, bomb, grapple
    /// and projectile input. The enable flag is captured once here for every later gate of the frame.
    /// </summary>
    internal void RunInputPhase(SamusControlFrame frame, SuperMetroid.Core.Progression.SuperMetroidGameState gameState, SamusState samus,
        FixedPosition<RoomPixels> cameraPointAtFrameStart, RoomScope room)
    {
        ChooseSamusPoseFromInput(frame, samus, cameraPointAtFrameStart, room);
        frame.SampleMovementEnabled(_player.GroundedSamusMovementEnabled);
        if (frame.MovementEnabled)
            RunHudAndProjectileInput(frame, gameState, samus, room);
    }

    /// <summary>
    /// Runs the movement phase: movement when Samus movement is enabled, then her animation and settling.
    /// </summary>
    /// <param name="frame">The frame's Samus control.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="runsLowHealthCheck">Whether the frame's policy runs her low-health check after her periodic damage.</param>
    /// <param name="room">The loaded room.</param>
    internal void RunMovementPhase(SamusControlFrame frame, SamusState samus, bool runsLowHealthCheck, RoomScope room)
    {
        if (frame.MovementEnabled)
            RunMovement(frame, samus, room);
        AnimateAndSettleSamus(frame, samus, runsLowHealthCheck, room);
    }

    /// <summary>
    /// Runs her HUD and projectile input once movement is enabled: environment gravity, terrain hazards, inside-block reactions, HUD
    /// selection, X-ray admission, bombs, grapple and projectiles, then the end-of-input-phase movement snapshot.
    /// </summary>
    private void RunHudAndProjectileInput(SamusControlFrame frame, SuperMetroid.Core.Progression.SuperMetroidGameState gameState, SamusState samus,
        RoomScope room)
    {
        // The input phase refreshes the liquid-gravity values after pose input/radius setup and
        // before movement. Refreshing the split gravity values every frame is
        // essential at a liquid surface: a jump can cross from water to air without
        // any pose transition that would otherwise reinitialize acceleration.
        SamusAerialMovement.ConfigureEnvironmentGravity(samus);

        // Block-inside detection samples the body after the input phase's radius update: a pose whose
        // radius grew in last frame's movement phase reaches air spikes with its new body now.
        // The stationary lock replaces the ordinary input mode with a locked one,
        // which never runs block-inside detection. Automatic Reserve recovery
        // uses this exact lock; re-sampling spikes while it owns Samus changes the
        // post-recovery knockback window used by Shinespark Suit.
        if (samus.Control.Mode.Movement != SamusMovementMode.ScriptLocked)
        {
            SamusTerrainHazardCollision.PrepareFrame(
                room.LevelData,
                samus,
                room.Room.Area,
                _system.HasDefeatedAnyOf(room.Room.Area, DefeatedBosses.AreaBoss));
        }

        // The Wrecked Ship has no power, which its on-foot conveyors need, until its area boss is defeated; every other room
        // has power.
        if (!_freeze.TimeIsFrozen && !frame.InputHandlingLocked(samus))
            SamusInsideBlockReactions.PrepareFrame(room.LevelData, samus,
                AreaIds.HasPower(room.Room.Area, _system.HasDefeatedAnyOf(room.Room.Area, DefeatedBosses.AreaBoss)),
                room.RoomObjects);

        // The input phase order is cooldown -> movement-type HUD projectile producer ->
        // projectile handling. The outer gameplay loop then runs overlap
        // before movement. A newly placed bomb therefore counts its fuse down once and
        // shows its first frame in the placement frame itself.
        // The door fade's enemy and draw pass does not run the normal input phase.
        // In particular its draw-only zero input must not release a held charge.
        // X-ray's freeze leaves the normal input mode installed, so the normal input phase
        // still runs cooldown, HUD switching and the HUD item's step; only its
        // projectile handling stops, which both projectile owners
        // apply themselves. A selection cancel during X-ray therefore takes effect.
        frame.SampleInputHandlingRuns(!_freeze.GameplayTimeFrozen && !frame.DeathOwnsSamus && !_doorPassage.IsUnderWay);
        if (frame.InputHandlingRuns)
        {
            // The HUD selection update runs before the movement-type HUD projectile producer.
            // Consequently an item-selection press can choose missiles and a Shoot press can
            // fire one during this same input phase. Input-locked message and elevator
            // control does not run the normal selection owner.
            // The selection step of the item the loop settles on runs on every
            // cancel or select edge, even one that leaves the selection unchanged.
            // Each clears the charge and restores suit colors, except grapple's
            // while a grapple is already active, which changes nothing.
            if (!frame.InputHandlingLocked(samus) && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
                SamusHudSelection.HandleHudSelection(samus, _playerActions.Current, _playerActions.NewlyPressed).ItemSelectionTookEffect &&
                (samus.SelectedHudItem != HudItem.Grapple ||
                 samus.Grapple.Phase == GrapplePhase.Inactive))
            {
                room.Projectiles.Firing.CancelCharge();
                SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(_player.Colours, samus.EquippedItems);
            }

            // The selected scope uses held Dash, not Shoot. Setup installs its
            // own pose and movement, superseding the input phase's ordinary pending pose.
            if (!frame.InputHandlingLocked(samus) && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
                samus.SelectedHudItem == HudItem.XRay &&
                samus.Grapple.Phase == GrapplePhase.Inactive &&
                (_playerActions.Current & GameplayAction.Dash) != GameplayAction.None &&
                TryBeginXrayFromSelectedHudItem(gameState, samus, room.BombProjectiles, out SamusPoseId? xrayActivationPose))
            {
                frame.RecordXrayActivation(xrayActivationPose);
                _player.ClearProspectivePoses();
            }

            // X-ray's freeze stops the enemy projectiles, room objects, block animations
            // and colour effects while time is frozen. Normal bombs cannot be placed or
            // advanced through this producer during the X-ray interval.
            // The bombs' share of the HUD item's step runs the shared cooldown, then the morph ball's producer.
            if (room.BombProjectiles.TickCooldownAndPlaceBombs(samus, _playerActions.Current, _playerActions.NewlyPressed))
            {
                room.Projectiles.Firing.CancelCharge();
                SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(_player.Colours, samus.EquippedItems);
            }

        }

        frame.SampleGrappleOwnership(RunGrappleBeamStep(frame, samus, room));

        if (frame.InputHandlingRuns)
        {
            // The bomb half above has already decremented the shared projectile cooldown. Her standing form's step then
            // runs the HUD producer, which fires and queues its sound, before the game handles any projectile: the
            // bombs, then the shots (#1620). Passing the live camera position also gives the projectile deletion check
            // its exact off-screen deletion window. A Spacetime beam shot changes Samus's items and progress during this
            // pass, so the rest of this update, and a save, see the change.
            var shotTerrain = new SamusTerrain { Level = room.LevelData, RoomObjects = room.RoomObjects };
            room.Projectiles.FireFromHud(shotTerrain, samus, new ProjectileButtonInput { ControllerInput = _playerActions.Current, ControllerNewInput = _playerActions.NewlyPressed, ControllerPreviousNewInput = samus.InputSnapshot.PressesSeenLastUpdate }, room.BombProjectiles, projectileProducerEnabled: !frame.InputHandlingLocked(samus));
            room.BombProjectiles.HandleBombs(shotTerrain, samus, overlap: BombJumpOverlapPublication.Deferred);
            room.Projectiles.HandleShots(shotTerrain, samus, room.Camera.PixelPosition, room.BombProjectiles, _system);

            if (!_freeze.TimeIsFrozen)
                room.BombProjectiles.ResolveSamusOverlap(samus, room.Projectiles.Firing.SuppressesInteraction);

            // The input phase runs the selected HUD producer and immediately
            // clears the pose bridge. Pose initialization occurs later in the
            // input phase, so a bridge published below survives precisely
            // until this point in the following gameplay frame.
            SamusAerialPoseTransitions.DropTransitionShot(samus);
            samus.ProjectileInheritance.ClearMovement();
        }

        // Samus's end-of-input-phase step snapshots movement,
        // before the movement phase applies animation/input transitions. Admission reads the
        // preceding snapshot; a newly queued X-ray pose is not installed yet.
        _player.PreviousMovementTypeForXray = frame.XrayActivatedThisFrame
            ? frame.MovementBeforeXrayAdmission : SamusPoseMetadata.ReadMovementType(samus);
    }

    /// <summary>
    /// Re-samples what the enemy pass can change before the movement phase runs: a cancelled prospective pose, the
    /// stationary-script lock, updated beam hits and the body attack the movement phase clears.
    /// </summary>
    internal void RunPostEnemyUpdateResample(SamusControlFrame frame, SamusState samus, RoomScope room)
    {
        if (!frame.MovementEnabled)
            return;
        // A control setup issued by enemy AI that cancels pending transitions cleared the input phase's prospective
        // poses and momentum responses, so nothing pending is committed this frame.
        if (samus.PoseTransitionCancellation.Take())
        {
            _player.ClearProspectivePoses();
        }
        // Actors' control setups replace the movement phase before it runs in this same update.
        frame.ReplaceMovementMode(SelectedMovementMode(frame, samus));
        if (!_freeze.TimeIsFrozen && !frame.DeathOwnsSamus)
            // Ceres Ridley's dedicated updated-beam adapter; ordinary projectiles collide before each enemy's AI.
            room.Enemies.Ceres.ResolveCeresRidleyProjectileHits(room.Projectiles, room.BombProjectiles);

        // The enemy pass reads the preceding frame's body attack. Only the movement phase begins without one, before
        // movement leaves the current one.
        if (frame.Mode.Movement != SamusMovementMode.SuitPickup)
            samus.HorizontalSpeed.BeginMovementWithoutBodyAttack();
    }

    /// <summary>
    /// Runs movement: runs the grapple-release mover, publishes
    /// grapple's corrected camera point and the frame-start pose, then runs the installed movement owner.
    /// </summary>
    private void RunMovement(SamusControlFrame frame, SamusState samus, RoomScope room)
    {
        // The normal movement mode runs the movement type's movement before animation. Every admitted pose below has its
        // own direction and mode path, so no pose inherits generic standing or running physics by accident.
        // X-ray publishes its pose-input and beam results above because both run
        // outside the normal movement-type step. Its movement result is the
        // frame `StepMovement` returns below. Whichever mover runs records its result on this update's frame.
        frame.SampleGrappleReleasePoseInput(false);
        if (samus.Grapple.ReleasedMovementActive)
        {
            // The replacement mover does not replace the normal airborne input phase's
            // input. Its pending aim/turn transition survives release cleanup;
            // the connection/release frames that themselves replace the pose still
            // invalidate a lookup sampled from their previous grapple body.
            frame.SampleGrappleReleasePoseInput(
                frame.MovementTypeAtFrameStart != SamusMovementType.Grappling &&
                samus.Pose == frame.PoseAtFrameStart);
            // The flight after letting go of the grapple takes Samus's movement from her pose on the
            // release-input frame. It remains independent of the beam phase, so
            // it must also run after the grapple cancel has made the beam inactive.
            SamusAerialMovement.ConfigureEnvironmentGravity(samus);
            frame.Record(SamusAerialMovement.StepReleasedFromGrapple(
                room.LevelData,
                samus,
                _playerActions.Current,
                _clock.Counter,
                room.RoomObjects));
            frame.SampleGrappleOwnership(true);
        }

        if (frame.GrappleMovement is { CameraPrevious: { } grapplePrevious })
        {
            // The grapple's close-collision snap clamps previous-position values to twelve
            // pixels after a close-collision snap. Feed those corrected whole pixels
            // to tracking while preserving the pre-frame fractions of a pixel.
            frame.SnapCameraPoint(grapplePrevious);
        }

        frame.SampleMovementPose(samus.Pose);

        RunMovementOwner(frame, samus, room);
    }

    /// <summary>
    /// Runs the owner of Samus's movement this frame (<see cref="ResolveMovementOwner"/>): nothing, grapple, Draygon's hold,
    /// X-ray, Ceres Ridley's ejection, knockback, bomb jump, Crystal Flash, the drain, the shinespark, or her pose, which an
    /// elevator ride also runs.
    /// </summary>
    private void RunMovementOwner(SamusControlFrame frame, SamusState samus, RoomScope room)
    {
        frame.SampleMovementOwner(ResolveMovementOwner(frame, samus, room));
        switch (frame.MovementOwner)
        {
            case SamusMovementOwner.None:
                _player.ClearProspectivePoses();
                break;

            case SamusMovementOwner.Grapple:
                // The grappling movement type does not move Samus itself. Discard input transitions
                // sampled from the pre-grapple pose once connection installs the grapple swing poses.
                if (!frame.GrappleReleaseAcceptsPoseInput)
                {
                    _player.ClearProspectivePoses();
                }
                break;

            case SamusMovementOwner.DraygonHold:
                // Deliberately empty: Draygon's actor owns coordinates.
                break;

            case SamusMovementOwner.Xray:
                _player.ClearProspectivePoses();
                samus.Xray.StepMovement(samus);
                break;

            case SamusMovementOwner.CeresRidleyEjection:
                // The hurt setup installs the hurt pose and clears every prospective pose and
                // momentum response. Afterwards the damage-boost check suppresses only a
                // damage-boost result selected from a knockback pose; it does not
                // replace the rest of her pose input. Input choosing no new pose there keeps the
                // knockback pose and its Stop momentum response, which clears the X acceleration
                // mode when the pose is committed.
                if (samus.CeresRidleyEjection.IsStarting)
                {
                    _player.ClearProspectivePoses();
                }
                else
                {
                    if (_player.ProspectiveSamusPose is { } prospective &&
                        SamusPoseMetadata.IsDamageBoostPose(prospective.ProspectivePose))
                        _player.ProspectiveSamusPose = null;
                    if (_player.ProspectiveSamusFallbackPose is { } fallback &&
                        SamusPoseMetadata.IsDamageBoostPose(fallback))
                        _player.ProspectiveSamusFallbackPose = null;
                }
                samus.CeresRidleyEjection.Step(
                    room.LevelData,
                    room.RoomObjects,
                    samus,
                    room.Camera.PixelPosition.X,
                    _clock.Counter);
                break;

            case SamusMovementOwner.Knockback:
                SamusAerialMovement.ConfigureEnvironmentGravity(samus);
                frame.Record(SamusKnockbackMovement.Step(
                    room.LevelData,
                    samus,
                    _clock.Counter,
                    room.RoomObjects));
                break;

            case SamusMovementOwner.BombJump:
                // The retained pose stays while input is locked; the late rise can restore input
                // before the bomb jump lets go of her movement, allowing a matched cancellation.
                if (frame.BombJumpLocksPoseInput)
                {
                    _player.ClearProspectivePoses();
                }

                // The bomb jump's movement chooses its gravity from the medium Samus is in, water, lava or air, as
                // knockback and the shinespark do, from her pose and place as the input phase leaves them.
                SamusAerialMovement.ConfigureEnvironmentGravity(samus);
                frame.Record(samus.BombJumpStarting
                    ? SamusBombJumpMovement.Start(samus)
                    : SamusBombJumpMovement.Step(
                        room.LevelData,
                        samus,
                        _clock.Counter,
                        room.RoomObjects));
                break;

            case SamusMovementOwner.CrystalFlash:
                _player.ClearProspectivePoses();
                frame.Record(samus.CrystalFlash.Step(
                    samus,
                    _clock.Counter));
                if (frame.CrystalFlashMovement is { BubbleStarted: true })
                {
                    // Crystal Flash's ending clears its state, copies Samus's raised center into the
                    // shared explosion position, then spawns the Crystal Flash explosion. This movement
                    // phase runs after the bombs' input phase, so the Crystal Flash's place among the bombs, still held,
                    // observes the released flag and deletes on the following frame.
                    room.BombProjectiles.PowerBombExplosion.BeginCrystalFlash(samus.Position.Whole);
                }
                break;

            case SamusMovementOwner.Drained:
                _player.ClearProspectivePoses();
                if (samus.Drained.Phase == DrainedSamusPhase.Falling)
                {
                    frame.Record(samus.Drained.StepFalling(
                        room.LevelData,
                        room.RoomObjects,
                        samus,
                        _clock.Counter));
                }
                else if (samus.InitializedPose != samus.Pose)
                {
                    // The drained-pose sequences store a drained pose without
                    // initializing it, and the movement keeps running for the cached
                    // movement type. An airborne Samus therefore keeps her jump
                    // physics in the drained pose until a real setup runs.
                    StepUninitializedPoseMovement(frame, samus, room);
                }
                break;

            case SamusMovementOwner.Shinespark:
                // Samus's environment-selected vertical acceleration pair is used
                // as spark acceleration. The projectile step does not replace it.
                SamusAerialMovement.ConfigureEnvironmentGravity(samus);
                frame.Record(samus.Shinespark.Step(new SamusTerrain { Level = room.LevelData, RoomObjects = room.RoomObjects }, samus, new ShinesparkFrame { FrameCounter = _clock.Counter, SamusCannotDie = _assists.Current.SamusCannotDie, Projectiles = room.Projectiles, GameTimeFrames = _gameTime.Frames, PreviousCheckpoint = frame.PreviousCameraPoint }));
                if (frame.ShinesparkMovement is { WindupTimedOut: true })
                {
                    // The spark publishes an interrupted vertical pose. That
                    // priority beats an ordinary input transition sampled from the shinespark windup poses
                    // earlier in the input phase during the timeout frame.
                    _player.ClearProspectivePoses();
                }
                break;

            case SamusMovementOwner.Pose:
            case SamusMovementOwner.Elevator:
                RunPoseOwnedMovement(frame, samus, room);
                break;

            default:
                throw new InvalidOperationException($"Samus's movement owner {frame.MovementOwner} has no movement to run.");
        }
    }

    /// <summary>
    /// Who moves Samus this frame, in priority order: nothing under the death, suit-pickup and stationary-lock movement
    /// modes; then grapple, Draygon's hold, X-ray, Ceres Ridley's ejection, knockback, bomb jump, Crystal Flash, the drain
    /// and the shinespark; then nothing while her control is locked, unless an elevator ride moves her; and otherwise her
    /// pose. Every test reads state and changes none, so resolving the owner before running it keeps the frame's order.
    /// </summary>
    private static SamusMovementOwner ResolveMovementOwner(SamusControlFrame frame, SamusState samus, RoomScope room)
    {
        // The stationary lock installs an empty movement mode independently of whoever
        // moved Samus beforehand. Automatic Reserve can therefore freeze an in-progress
        // hurt launch; special owners such as knockback must not outrank this explicitly installed mode.
        if (frame.Mode.Movement is SamusMovementMode.Fatal or SamusMovementMode.SuitPickup or SamusMovementMode.ScriptLocked)
            return SamusMovementOwner.None;
        if (frame.GrappleOwnsMovement)
            return SamusMovementOwner.Grapple;
        // Draygon's grab keeps Samus still, rather than running her Draygon-held movement type's normal step.
        if (samus.DraygonGrabbed.OwnsMovement)
            return SamusMovementOwner.DraygonHold;
        // X-ray's activation pose moves her by X-ray's own movement instead of the
        // normal movement-type step. Stable X-ray bodies select one of five
        // angle frames; turning on the ground deliberately performs no movement.
        if (samus.Xray.OwnsSamusControl)
            return SamusMovementOwner.Xray;
        // Ceres Ridley's shove must precede ordinary damage knockback because the knockback
        // poses are shared while ownership, duration, and termination are not.
        if (samus.CeresRidleyEjection.IsActive)
            return SamusMovementOwner.CeresRidleyEjection;
        // Knockback takes precedence over the normal movement-type step. Unlike bomb jump,
        // normal pose input remains active so the knockback poses can still select the game's
        // damage-boost escape chord.
        if (samus.KnockbackActive)
            return SamusMovementOwner.Knockback;
        // Arming the bomb jump takes both her pose input and her movement.
        if (samus.BombJumpStarting || samus.BombJumpActive)
            return SamusMovementOwner.BombJump;
        // Crystal Flash moves her through its three phases and locks her pose input. Mother
        // Brain's rainbow-beam setup keeps that hold, but temporarily replaces the movement phase
        // with a lock that moves her not at all. Once Samus's control is restored to normal movement, Flash resumes
        // under the drained pose. The Super Metroid never locks her movement so, and its interruption continues Flash
        // at once.
        if (samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive &&
            !(samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked && samus.Control.InputLocked))
            return SamusMovementOwner.CrystalFlash;
        // The drain's operations install special-movement-type poses that do not move Samus.
        // Only the drained-fall animation cue starts her drained fall; the other phases stay
        // motionless while enemy AI controls pose/timing. This is reached only when Crystal Flash
        // no longer holds her. Controller zero changes only the pose, so a crashing spark keeps
        // its hold and finishes below; the drained-fall cue ends the spark itself.
        // The rainbow-beam lock is the movement lock itself: once Samus's control is restored to normal movement during
        // the enemies' update, this update's movement runs the knockback pose's knockback movement, as the pose's own
        // movement does.
        // The controller-held standing/crouching phases last only while Samus keeps a
        // drained body: a pose the movement installs (a wall jump out of a drained
        // spin) returns her to the ordinary movement and input. So does the landing of a drained fall that an
        // ordinary pose's animation installed by reading into a drained crouch's animation (#1535): the landing returns
        // her to her pose's movement, which only a drained pose's special movement type leaves still.
        if (samus.Drained.Phase != DrainedSamusPhase.Inactive &&
            !(samus.Drained.Phase is DrainedSamusPhase.Standing or DrainedSamusPhase.Crouching or DrainedSamusPhase.OnFloor &&
              !SamusPoseMetadata.IsDrainedPose(samus.Pose)) &&
            !(samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked && !samus.Control.InputLocked) &&
            (samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked || !samus.Shinespark.OwnsMovement))
            return SamusMovementOwner.Drained;
        // The shinespark's windup, active launch, and crash move her regardless of the pose's
        // movement type; ordinary special-movement-type physics does not exist to fall back to.
        if (samus.Shinespark.OwnsMovement)
            return SamusMovementOwner.Shinespark;
        // Samus's control lock installs an input mode which advances existing
        // projectiles but a movement mode which performs no movement at all. All
        // special movement owners above take priority; an otherwise
        // locked standing body must not run generic ground collision or drift while
        // the game waits at the Ceres elevator.
        if (samus.Control.InputLocked)
        {
            // The elevator setup installs the moving movement mode for departure.
            // Finishing the door's opening scroll reinstalls it only on downward arrival. Upward arrival
            // retains the lock until the actor reaches rest, so Samus probes no terrain there and wakes none of the
            // scroll triggers the platform passes.
            return SamusPoseMetadata.IsForwardFacingPose(samus.Pose) && room.Enemies.Elevator.ElevatorRunsSamusMovement
                ? SamusMovementOwner.Elevator
                : SamusMovementOwner.None;
        }
        return SamusMovementOwner.Pose;
    }

    /// <summary>
    /// Runs the movement her pose owns, or an elevator ride's: none while time is frozen or for the special movement type,
    /// and otherwise the pose's normal movement.
    /// </summary>
    private void RunPoseOwnedMovement(SamusControlFrame frame, SamusState samus, RoomScope room)
    {
        // The movement phase returns before the pose's movement and the echo update while time is
        // frozen. X-ray setup freezes time in the input phase, so a running Samus stops on
        // the activation frame before her pose changes.
        if (_freeze.TimeIsFrozen)
            return;
        // The special movement type also has a normal movement.
        // It only clears the momentum-transition selector. This path is reachable
        // when the hit's expiry ends a freshly started shinespark
        // while retaining its shinespark windup body and colouring (Shinespark Suit).
        if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Special)
            return;
        RunPoseMovement(frame, samus, room);
    }

    /// <summary>
    /// Runs the pose's normal movement (#2291): the pose's movement type selects it, as the game's movement for each type
    /// does, with the few poses whose step their own family takes named in their type's movement.
    /// </summary>
    /// <exception cref="InvalidDataException">The pose's movement type has no movement its pose owns.</exception>
    private void RunPoseMovement(SamusControlFrame frame, SamusState samus, RoomScope room)
    {
        SamusMovementType movement = SamusPoseMetadata.ReadMovementType(samus.Pose);
        // The special movement type returns before this, and grappling, a compact airborne pose and the unused movement
        // types are owned by their own movers; any other is corrupt pose state.
        SamusPoseMovement step = SamusMovementTypeDefinitions.Movements.TryGetValue(movement, out SamusMovementTypeDefinition? definition)
            && definition.PoseMovement is { } owned
            ? owned
            : throw new InvalidDataException(
                $"Runtime movement step received pose {samus.Pose}, whose movement type {movement} has no movement its pose owns.");
        step.Step(new PoseMoveSteps(this, new PoseMove(samus, room, frame)));
    }

    /// <summary>The steps of one update's pose movement, against the room and frame it runs in (#2571).</summary>
    /// <param name="phases">The update's Samus phases, whose clock and actions the steps read.</param>
    /// <param name="move">What the movement runs against.</param>
    [RoomScopeBoundary("A per-call input, built for one call and never kept.")]
    private sealed class PoseMoveSteps(SamusFramePhases phases, PoseMove move) : ISamusPoseMovementSteps
    {
        /// <summary>The update's Samus phases.</summary>
        private readonly SamusFramePhases _phases = phases;

        /// <summary>What the movement runs against.</summary>
        private readonly PoseMove _move = move;

        /// <inheritdoc/>
        public void StepStanding() => _phases.StepStandingPose(_move);

        /// <inheritdoc/>
        public void StepRunning() => _move.Frame.Record(SamusGroundedMovement.StepRunning(_move.Level, _move.Samus, _phases._clock.Counter, _phases._playerActions.Current, _move.RoomObjects));

        /// <inheritdoc/>
        public void StepMoonwalking() => _move.Frame.Record(SamusGroundedMovement.StepMoonwalking(_move.Level, _move.Samus, _phases._clock.Counter, _move.RoomObjects));

        /// <inheritdoc/>
        public void StepRanIntoWall() => _move.Frame.Record(SamusGroundedMovement.StepRanIntoWall(_move.Level, _move.Samus, _phases._clock.Counter, _move.RoomObjects));

        /// <inheritdoc/>
        public void StepTurningOnGround() => _phases.StepTurningOnGround(_move);

        /// <inheritdoc/>
        public void StepTurningWhileJumping() => _phases.StepTurningWhileJumpingPose(_move);

        /// <inheritdoc/>
        public void StepTurningInAir() => _phases.StepTurningInAir(_move);

        /// <inheritdoc/>
        public void StepMorphBallGrounded() => _phases.StepMorphBallGrounded(_move);

        /// <inheritdoc/>
        public void StepMorphBallFalling() => _phases.StepMorphBallFalling(_move);

        /// <inheritdoc/>
        public void StepSpringBallInAir() => _move.Frame.Record(SamusMorphBallMovement.StepSpringBallInAir(_move.Level, _move.Samus, _phases._playerActions.Current, _phases._clock.Counter, _move.RoomObjects));

        /// <inheritdoc/>
        public void StepNormalJump() => _move.Frame.Record(SamusAerialMovement.StepNormalJump(_move.Level, _move.Samus, _phases._playerActions.Current, _phases._clock.Counter, _move.RoomObjects));

        /// <inheritdoc/>
        public void StepSpinJump() => _move.Frame.Record(SamusAerialMovement.StepSpinJump(_move.Level, _move.Samus, _phases._playerActions.Current, _phases._clock.Counter, _phases._playerActions.NewlyPressed, _move.RoomObjects));

        /// <inheritdoc/>
        public void StepWallJump() => _move.Frame.Record(SamusAerialMovement.StepWallJump(_move.Level, _move.Samus, _phases._playerActions.Current, _phases._clock.Counter, _move.RoomObjects));

        /// <inheritdoc/>
        public void StepDamageBoost() => _move.Frame.Record(SamusAerialMovement.StepDamageBoost(_move.Level, _move.Samus, _phases._playerActions.Current, _phases._clock.Counter, _move.RoomObjects));

        /// <inheritdoc/>
        public void StepFalling() => _move.Frame.Record(SamusAerialMovement.StepFalling(_move.Level, _move.Samus, _phases._playerActions.Current, _phases._clock.Counter, _move.RoomObjects));

        /// <inheritdoc/>
        public void StepCrouching() => _move.Frame.Record(SamusPostureMovement.StepCrouching(_move.Level, _move.Samus, _phases._clock.Counter, _move.RoomObjects));

        /// <inheritdoc/>
        public void StepKnockbackEnding() => _phases.StepKnockbackEndingMovement(_move);

        /// <inheritdoc/>
        public void StepPostureTransition() => _phases.StepPostureTransitionPose(_move);

        /// <inheritdoc/>
        public void HoldWhereInstalled() => SamusFramePhases.HoldWhereInstalled(_move);

        /// <inheritdoc/>
        public void StepDraygonHeld() => SamusDraygonGrabbedState.StepMovement(_move.Samus);

        /// <inheritdoc/>
        public void HoldStill()
        {
        }
    }

    /// <summary>The standing movement type's movement: the forward-facing poses' and the landings' own steps, otherwise standing's.</summary>
    private void StepStandingPose(PoseMove move)
    {
        if (SamusPoseMetadata.IsForwardFacingPose(move.Samus.Pose))
            StepForwardFacingMovement(move.Samus, move.Room);
        else if (SamusPoseMetadata.IsLandingPose(move.Samus.Pose))
            move.Frame.Record(SamusGroundedMovement.StepLanding(move.Level, move.Samus, _clock.Counter, move.RoomObjects));
        else
            move.Frame.Record(SamusGroundedMovement.StepStanding(move.Level, move.Samus, _clock.Counter, _playerActions.Current, move.RoomObjects));
    }

    /// <summary>
    /// The turning-while-jumping movement type's movement: an aimed crouched turn with no vertical direction turns on the
    /// ground, and every other jumping turn, an aimed crouched one moving up or down among them, turns in the air.
    /// </summary>
    private void StepTurningWhileJumpingPose(PoseMove move)
    {
        // Only these turns read the vertical direction to choose their movement. Elsewhere it is data the chosen movement
        // reads: the plain crouched turns, which turn on the ground, can retain a downward direction and still turn on the
        // ground.
        if (SamusPoseMetadata.IsAimedCrouchingTurnPose(move.Samus.Pose) &&
            move.Samus.Kinematics.VerticalVelocity.HasNoDirection)
            StepTurningOnGround(move);
        else
            StepTurningInAir(move);
    }

    /// <summary>The turn on the ground.</summary>
    private void StepTurningOnGround(PoseMove move) => move.Frame.Record(SamusGroundedMovement.StepTurningOnGround(move.Level, move.Samus, _clock.Counter, move.RoomObjects));

    /// <summary>The turn in the air.</summary>
    private void StepTurningInAir(PoseMove move) => move.Frame.Record(SamusAerialMovement.StepTurningInAir(move.Level, move.Samus, _clock.Counter, move.RoomObjects));

    /// <summary>The Morph Ball's and the Spring Ball's movement on the ground.</summary>
    private void StepMorphBallGrounded(PoseMove move) => move.Frame.Record(SamusMorphBallMovement.StepGrounded(move.Level, move.Samus, _clock.Counter, move.RoomObjects));

    /// <summary>The Morph Ball's and the Spring Ball's fall.</summary>
    private void StepMorphBallFalling(PoseMove move) => move.Frame.Record(SamusMorphBallMovement.StepFalling(move.Level, move.Samus, _playerActions.Current, _clock.Counter, move.RoomObjects));

    /// <summary>
    /// The posture transition movement type's movement: the morph transitions' own step, the Draygon-held stand-up's hold,
    /// otherwise the crouch and stand transitions'.
    /// </summary>
    private void StepPostureTransitionPose(PoseMove move)
    {
        if (SamusPoseMetadata.IsMorphTransitionPose(move.Samus.Pose))
            move.Frame.Record(SamusMorphBallMovement.StepTransition(move.Level, move.Samus, _clock.Counter, move.RoomObjects));
        else if (SamusPoseTraits.IsIn(move.Samus.Pose, SamusPoseFamily.HeldInPlaceStandUp))
            HoldWhereInstalled(move);
        else
            move.Frame.Record(SamusPostureMovement.StepCrouchStandTransition(move.Level, move.Samus, _clock.Counter, move.RoomObjects));
    }

    /// <summary>
    /// The glitch ball's normal movement, which only clears the vertical solid-collision result, as does the posture transition's
    /// for the compact Draygon-held stand-up, which skips its horizontal and vertical movement; nothing lands them, so the
    /// landed cleanup never runs. Both hold Samus where the over-read installed them (#1535).
    /// </summary>
    private static void HoldWhereInstalled(PoseMove move) => move.Samus.ForgetVerticalCollisionResult();

    /// <summary>What a pose's movement runs against on one update (#2291).</summary>
    /// <param name="Samus">Samus.</param>
    /// <param name="Room">The loaded room.</param>
    /// <param name="Frame">The update's Samus control, which keeps the movement's result for the rest of the update.</param>
    [RoomScopeBoundary("A per-call input, built for one call and never kept.")]
    private readonly record struct PoseMove(SamusState Samus, RoomScope Room, SamusControlFrame Frame)
    {
        /// <summary>The room's level data her movement collides with.</summary>
        public RoomLevelData Level => Room.LevelData;

        /// <summary>The room's room objects her movement collides with.</summary>
        public ISamusRoomObjects RoomObjects => Room.RoomObjects;
    }

    /// <summary>Runs the forward-facing poses' movement, including the elevator's one-pixel downward block scan.</summary>
    private void StepForwardFacingMovement(SamusState samus, RoomScope room)
    {
        // The two front-facing poses enter through the controller-locked demo and elevator setups: a forward-facing
        // Samus moves nothing while the elevator is at rest and scans one pixel down while it moves.
        SamusGroundedMovement.StepFacingForward(
            room.LevelData,
            samus,
            _clock.Counter,
            elevatorIsMoving:
                room.Enemies.Elevator.ElevatorStatus != ElevatorActorStatus.Inactive,
            roomObjects: room.RoomObjects,
            doorBlocks: ElevatorDoorBlocks.FacingForward(room.Enemies.Elevator.ElevatorStatus));
    }

    /// <summary>Runs the knockback movement type's normal movement: the one-frame grounding probe before the knockback ending's target.</summary>
    private void StepKnockbackEndingMovement(PoseMove move)
    {
        SamusState samus = move.Samus;
        RoomScope room = move.Room;
        SamusControlFrame frame = move.Frame;
        // The knockback movement type's normal movement is this one. Active
        // knockback and Ceres ejection were intercepted above; this is the knockback
        // movement type's one-frame normal movement before the pose transition
        // installs the input/no-input target chosen during the input phase.
        SignedSixteenSixteen groundingDisplacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics, samus.HorizontalSpeed);
        BlockMoveResult grounding = SamusGroundedMovement.StepKnockbackEnding(
            room.LevelData,
            samus,
            _clock.Counter,
            room.RoomObjects);
        frame.Record(new GroundedMovementResult(default, grounding));
        // When the knockback ending's downward probe hits the floor, Samus lands as on any landing.
        if (grounding.Collided && !groundingDisplacement.IsNegative)
            frame.Record(new AerialMovementResult(Landed: true, HitCeiling: false));
    }

    /// <summary>
    /// Chooses Samus's pose from her input: X-ray's beam, the frame's camera point, the frame-start captures, the live radius
    /// refresh, the pose her input chooses and the fallback pose when it chooses none.
    /// </summary>
    private void ChooseSamusPoseFromInput(SamusControlFrame frame, SamusState samus,
        FixedPosition<RoomPixels> cameraPointAtFrameStart, RoomScope room)
    {
        // X-ray's beam is not part of Samus's frame phases. Advance its explicit
        // setup/beam/deactivation state before the input phase so its deactivation can restore the
        // ordinary pose and her normal input and movement modes before this frame samples pose input. The
        // reveal and the beam's cone are the frame boundary's to show.
        if (samus.Xray.IsActive && !samus.DeathSequence.IsActive)
        {
            frame.Record(samus.Xray.StepBeam(
                samus,
                _playerActions.Current));
        }

        // The camera point is the room camera's previous Samus point or, for a new camera, the point sampled at the
        // frame's start, before enemy-owned motion and her movement.
        frame.SampleCameraPoint(room.Camera.PreviousSamusPoint is SamusScrolledCheckpoint { Point: var point } ? point : cameraPointAtFrameStart);

        // Retain the frame-start pose because the turn-transition animation cue can replace Samus.Pose during
        // animation later in this same frame. Input/movement/transition phases all
        // agree on that order; using the mutable value afterward would apply an input
        // match selected for the old pose to the newly installed one.
        _attractDemo.BeginAttractSamusInput();
        frame.SampleFrameStartPose(samus.Pose);
        // The movement phase moves her by the pose installed when it runs, which the enemy pass can have
        // replaced since the input phase: Draygon's death releases a sparking Samus to facing right normally,
        // and standing movement runs. The pose update then reads that pose's type.
        // The ordinary input phase publishes the current pose's live radius before input
        // and collision. Prospective transitions later in the movement phase can retain the
        // previous radius until this point in the following frame. The locked
        // input mode has no radius update, so e.g. an elevator ride keeps
        // the boarding pose's radius.
        if (_player.GroundedSamusMovementEnabled && !_freeze.TimeIsFrozen && !frame.InputHandlingLocked(samus) &&
            !samus.DeathSequence.IsActive && !_doorPassage.IsUnderWay)
            SamusPoseMetadata.RefreshCollisionRadii(samus);
        // Samus movement phase precedes room objects. A lock/unlock issued by a room object
        // affects the next movement phase, not the animation already owned this frame.
        // The suit pickup lock installs an empty movement mode, not just locked pose input:
        // the suspended movement and all of its timers survive, so restoring
        // Samus's control can resume it after the suit's light-beam transformation (including Blue Suit).
        // The empty movement mode also skips periodic damage and the energy checks.
        frame.SampleMovementType(SamusPoseMetadata.ReadMovementType(samus));

        // The held directions are already the actions the input transitions match. Input
        // matching belongs to the input phase, before the movement phase moves the CURRENT pose;
        // the winning pose is not installed until after animation below.
        // The front-view poses get one special input rule: while an elevator
        // rides, the input phase returns immediately, before their input transitions
        // are even matched. The power suit's front pose's input transitions were never authored and
        // can appear to select nonsense transitions after several frames, so
        // suppressing only their application would be too late and observably wrong.
        frame.SampleDeathOwnership(samus.DeathSequence.IsActive);
        SamusControlMode mode = new(samus.Control.Mode.Input, SelectedMovementMode(frame, samus));
        // Sound queues persist across the whole game, but this control publication covers one update of
        // Samus's control. It opens before her movement and collision, so it is in place for the landing
        // sounds her collision plays.
        frame.BeginControl(mode, samus.Xray.OwnsSamusControl && !frame.DeathOwnsSamus, SamusPoseMetadata.ReadMovementType(samus));
        if (frame.XrayOwnsPoseInput)
        {
            samus.Xray.HandlePoseInput(
                samus,
                _playerActions.Current);
        }

        // The posture-transition movement type's input step is empty. Transition
        // poses can still have input locks, but reading them here prematurely
        // interrupts crouch/morph animations before their animation cue wins.
        frame.SamplePoseInputLocks(samus,
            ((samus.InputSnapshot.ShinesparkPoseInputLocked || samus.InputSnapshot.CrystalFlashPoseInputLocked) && !samus.InputSnapshot.AutoJump.IsInstalled) ||
            (SamusPoseMetadata.IsForwardFacingPose(samus.Pose) && room.Enemies.Elevator.ElevatorStatus != ElevatorActorStatus.Inactive));
        SamusPoseTransitionLookup poseLookup =
            frame.DeathOwnsSamus || frame.XrayOwnsPoseInput || frame.ActorLocksPoseInput || frame.PostureTransitionSkipsPoseInput
            ? default
            : SamusPoseInputTransitions.Lookup(
                samus.Pose,
                _playerActions.Current,
                _attractDemo.IsAttractDemo ? _playerActions.NewlyPressed : samus.InputSnapshot.ApplyAutoJumpInput(_playerActions.NewlyPressed));
        _player.ProspectiveSamusPose = poseLookup.Transition;
        // An input transition to a different pose cancels a requested bomb jump at once, before collision and
        // movement.
        if (_player.ProspectiveSamusPose is { } matchedBombCancel &&
            matchedBombCancel.ProspectivePose != samus.Pose)
            samus.BombJump = null;
        frame.SamplePoseLookup(poseLookup, samus);
        _player.ProspectiveSamusFallbackPose = null;
        _player.ProspectiveSamusWallCollisionPose = null;

        // When no input transition matches, no buttons held or a combination none matches, her pose's no-transition
        // pose is chosen; a match that keeps the pose is not. The pose her input chooses takes effect after movement
        // and animation, so the choice is captured here, and a kept pose still chooses a pose transition whose change
        // runs later.
        if (_player.GroundedSamusMovementEnabled && frame.InputChoseNoPose)
        {
            _player.ProspectiveSamusFallbackPose = SamusNoTransitionPose.Resolve(
                SamusPoseMetadata.ReadMovementType(samus),
                samus.Pose,
                SamusPoseMetadata.ReadNoInputPose(samus),
                frame.DeceleratingFallbackHasMomentum).ProspectivePose;
        }
    }

    /// <summary>
    /// Animates and settles Samus after her movement: the speed echo, landing effects, the checks that run after
    /// her movement (the drained get-up and Draygon's escape), animation or the death sequence, the pose transitions, and
    /// periodic damage with the low-energy check.
    /// </summary>
    private void AnimateAndSettleSamus(SamusControlFrame frame, SamusState samus, bool runsLowHealthCheck, RoomScope room)
    {
        // Samus's normal movement phase updates the speed echo after her movement owner has moved her
        // and before animation/pose transitions. At its capture cadence at the full boost stage, it alternates
        // between two of her places in the room, so the echoes drawn after it stand where she has moved to.
        if (frame.Mode.Movement != SamusMovementMode.SuitPickup && frame.ShinesparkMovement is null)
        {
            samus.HorizontalSpeed.CaptureSpeedEchoPosition(_gameTime.Frames, samus.Position.Whole);
        }

        // Every ordinary landing runs the landing effects inside movement, before
        // the checks after her movement, liquid animation, prospective-pose selection, and velocity
        // cleanup. Knockback clears its vertical speed on landing before the landing effects read it, so they see no
        // speed.
        // Drained movement publishes its impact separately.
        bool ordinaryOrBallLanded =
            frame.AerialMovement is { Landed: true } ||
            frame.MorphBallMovement is { Landed: true };
        bool knockbackLanded = frame.KnockbackMovement is { Landed: true };
        DrainedSamusMovementResult? drainedLanding = frame.DrainedMovement is { Landed: true } landing ? landing : null;
        bool drainedLanded = drainedLanding is not null;
        if (!frame.DeathOwnsSamus && (ordinaryOrBallLanded || knockbackLanded || drainedLanded))
        {
            SamusVerticalVelocity impact = drainedLanding is { } drained
                ? drained.ImpactVelocity
                : samus.Kinematics.VerticalVelocity;
            samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
                samus,
                frame.MovementTypeAtFrameStart,
                frame.PoseAtFrameStart,
                impact.Speed);
        }

        // The drained get-up runs after movement and before animation.
        // Mother Brain's two drain setups choose what a press of Up does in it; the
        // step does nothing in every ordinary gameplay state.
        if (frame.Mode.Movement != SamusMovementMode.SuitPickup)
            samus.Drained.RunGetUpCheck(samus, _playerActions.NewlyPressed);

        if (frame.Mode.Movement != SamusMovementMode.SuitPickup && samus.DraygonGrabbed.IsActive)
        {
            // Draygon's escape check runs after movement and before
            // animation. A locked grapple cancels the prospective pose chosen in the input phase;
            // an escape release additionally clears every pending route while installing
            // the ordinary facing-normal poses and zeroing the documented velocities.
            bool grappleLockedInPlace = samus.Grapple.Phase == GrapplePhase.ConnectedLocked;
            if (samus.DraygonGrabbed.CheckDraygonEscape(samus, _playerActions.NewlyPressed) || grappleLockedInPlace)
            {
                _player.ClearProspectivePoses();
            }
        }

        // Normal gameplay advances animation during the movement phase, before the
        // pose transitions, drawing, and next frame's picture selection.
        // Animation sees the same held input as movement. The dash action both
        // builds up the running delay and holds the running animation at its
        // cue; passing a reconstructed input later would show the wrong frames.
        if (frame.Mode.Movement == SamusMovementMode.Fatal)
        {
            frame.Record(samus.DeathSequence.Step(
                samus,
                _player.Colours,
                samus.Artwork.Body.DeathSteps));
        }
        // The station lock's empty movement mode skips animation like the locked movement mode.
        else if (frame.Mode.Movement is not (SamusMovementMode.ScriptLocked or SamusMovementMode.RefillStation or SamusMovementMode.SuitPickup))
        {
            SamusPresentation.Animate(samus, new SamusAnimationStep { BossPresent = room.Enemies.RoomStatus.Boss is not null, ControllerInput = _playerActions.Current, FrameCounter = _clock.Counter, Random = _system.Random, ProspectiveInputPose = _player.ProspectiveSamusPose?.ProspectivePose ?? _player.ProspectiveSamusFallbackPose, DemoPoseInput = _attractDemo.IsAttractDemo });
            StepSamusLoadAppearance(samus);
        }

        _transitions.Apply(frame, samus, new SamusTerrain { Level = room.LevelData, RoomObjects = room.RoomObjects });

        if (frame.Mode.Movement is not (SamusMovementMode.Fatal or SamusMovementMode.TitleDemo or SamusMovementMode.SuitPickup))
        {
            // The normal movement phase ends with the periodic damage, the pause check and
            // the low-energy check, before the enemy projectiles' and room objects' steps. Damage those
            // steps accumulate this frame therefore lands next frame.
            // The demo's movement mode skips these checks, and so does the suit pickup lock's empty movement mode:
            // damage pending during the transformation stays accumulated
            // until the first ordinary movement phase after it, which divides it by the new suit.
            // X-ray freezes time and therefore clears
            // rather than applies accumulated damage; fatal zero-energy game-state
            // acquisition remains the outer seam.
            samus.PeriodicDamage.Apply(
                samus,
                timeIsFrozen: _freeze.TimeIsFrozen);
            // Locked, elevator and appearance movement modes omit the low-energy check; the gunship setup
            // installs a dedicated checker. Automatic reserves invoke their external
            // check in the frontend.
            if (frame.Mode.Movement is SamusMovementMode.Normal or SamusMovementMode.Gunship && !samus.Xray.OwnsSamusControl &&
                runsLowHealthCheck)
                samus.HealthWarning.Update(samus.Health, samus.AudioRequests);
        }
    }

    /// <summary>
    /// Advances Samus's appearance after a saved game loads. Her ordinary animation and colour steps have already run
    /// earlier in this frame; this step counts down the fanfare's lifetime
    /// (<see cref="SavedGameAppearanceDefinitions.FrameCount"/>) and gives her back her normal control when it ends.
    /// </summary>
    private void StepSamusLoadAppearance(SamusState samus)
    {
        if (!_player.LoadAppearanceActive)
            return;

        if (!_player.AdvanceLoadAppearance())
            return;

        // The saved game's appearance fanfare publishes its same-pose history shift only on the
        // update it ends, before her ordinary input and movement modes regain control.
        samus.CommitPoseHistory();
        samus.Control.Unlock();
        _player.GroundedSamusMovementEnabled = true;
    }
}
