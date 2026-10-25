using SuperMetroid.Core.Assets;
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
    /// <summary>Supplies installed beam tiles and palettes for pause teardown and grapple selection.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Opens the attract demo's Samus input window and selects the demo's new-input word.</summary>
    private readonly IAttractSamusInput _attractDemo;
    /// <summary>Receives Samus and grapple palette writes at their gameplay phase.</summary>
    private readonly GameplayColours _colours;
    /// <summary>Supplies held and newly pressed controller bits to Samus phase handlers.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Gates movement and animation branches during gameplay time freeze.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Supplies elapsed gameplay frames to the Shinespark movement handler.</summary>
    private readonly GameTimeState _gameTime;
    /// <summary>Provides accepted-NMI cadence for movement and animation behavior.</summary>
    private readonly FrameClock _clock;
    /// <summary>Supplies player options such as configured movement controls.</summary>
    private readonly GameplayOptions _options;
    /// <summary>Owns the live Samus state and phase-specific handler selection.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Provides active level data, camera, and room state for movement scans.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Supplies the shared game state consulted by movement handlers.</summary>
    private readonly SharedGameState _system;
    /// <summary>Builds X-ray's reveal tilemap at its setup stage.</summary>
    private readonly XrayReveal _xray;
    /// <summary>UpdateSamusPose after normal beta's animation.</summary>
    private readonly SamusPoseTransitionPhase _transitions;

    /// <summary>Binds the frame owners required to preserve Samus's alpha, beta, and presentation ordering.</summary>
    internal SamusFramePhases(SamusFramePhasesDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _attractDemo = dependencies.AttractDemo;
        _xray = dependencies.Xray;
        _colours = dependencies.Colours;
        _controller1 = dependencies.Controller1;
        _freeze = dependencies.Freeze;
        _gameTime = dependencies.GameTime;
        _clock = dependencies.Clock;
        _options = dependencies.Options;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _system = dependencies.System;
        _transitions = new SamusPoseTransitionPhase(new SamusPoseTransitionPhaseDependencies { Controller1 = dependencies.Controller1, Freeze = dependencies.Freeze, Clock = dependencies.Clock, Options = dependencies.Options, Player = dependencies.Player, Room = dependencies.Room });
    }

    /// <summary>
    /// Runs <c>Update_Beam_Tiles_and_Palette</c> at <c>$90:AC8D</c> for the live beam word supplied by pause teardown.
    /// State `$11` performs this restore under forced blank so the first resumed gameplay frame uses the newly
    /// equipped combination.
    /// </summary>
    internal void UpdateGameplayBeamTilesAndPalette(SuperMetroid.Core.Progression.SamusBeamFlags equippedBeams) =>
        _artwork.UpdateBeamTilesAndPalette(_player.Colours, equippedBeams);

    /// <summary>
    /// <c>GrappleBeamHandler</c> ($9B:C490), reached from the HUD-selection dispatch of
    /// <c>HandleHUDSpecificBehaviorAndProjectiles</c> ($90:DCDD) in Samus's alpha pass.
    /// It therefore runs before Samus's projectiles and before <c>Main_Enemy_Routine</c>,
    /// so enemies see the beam endpoint and Samus position it leaves this frame. Only
    /// connected and release functions own Samus's movement; an extending or cancelling
    /// beam coexists with the current pose's ordinary movement.
    /// </summary>
    private bool RunGrappleBeamHandler(SamusBetaHandler beta, ISamusRoomPlms plms, SamusProjectileSystem projectiles, RoomScope room)
    {
        if (_player.Samus is null || _room.LevelData is null)
            throw new InvalidOperationException("The grapple handler requires Samus and room collision data.");
        _player.LastGrappleMovement = null;
        bool grappleOwnsMovement = false;
        if (beta is SamusBetaHandler.Fatal or SamusBetaHandler.SuitPickup)
        {
            // Game states `$15-$18` do not call the ordinary Samus alpha/beta
            // handlers. The death state advances later at the animation seam.
        }
        else if (_player.Samus.Grapple.Phase == GrapplePhase.Firing)
        {
            _player.LastGrappleMovement = SamusGrappleMovement.StepFiring(

                _room.LevelData,
                _player.Samus,
                _controller1.Current,
                plms,
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
            _player.LastGrappleMovement = SamusGrappleMovement.Step(new SamusTerrain { Level = _room.LevelData, Plms = plms }, _player.Samus, _controller1.Current, _controller1.NewlyPressed, _clock.Counter, room.Enemies.GrappleTargets.ResolveGrappleEndpoint, poseChange: GrapplePoseChange.Deferred);
            grappleOwnsMovement = _player.LastGrappleMovement.Value.OwnsMovement;
        }
        else if (!_freeze.TimeIsFrozen &&
                 (_player.DebugGrappleItemSelected || SamusGrappleHudInput.IsSelectedAndAdmitted(_player.Samus, projectiles.Overreads)) &&
                 ((_controller1.NewlyPressed | _player.Samus.InputSnapshot.PreviousDrawNewInput) & ControllerButton.X) != ControllerButton.None)
        {
            // Normal HUD item four now reaches the same bank-$9B actor as the
            // diagnostic entry point, after bank-$90's movement-type admission.
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
            // Native cancellation, drop, wall-jump and swing-release tails all
            // reload the equipped beam palette before returning to inactive.
            (_artwork.BeamPalettes ?? throw new InvalidOperationException(
                "Grapple cleanup requires installed beam artwork."))
                .LoadTo(_player.Colours.Beam, new SamusBeamLoadoutWord(_player.Samus.EquippedBeams).Combination ?? throw new InvalidOperationException(
                    "Equipped-beam index is outside the beam-combination tables."));
        }

        // `$9B:C4B1-$C4EA` runs after every grapple function, including inactive.
        // Swing calculations above therefore consumed last frame's bit; this write
        // publishes the current bottom-boundary result for the next frame exactly
        // where the native bank-$9B handler does.
        SamusGrappleMovement.RefreshLiquidPhysicsFlag(_player.Samus);
        return grappleOwnsMovement;
    }

    /// <summary>Loads the installed grapple firing palette and flare color into Samus's CGRAM slots.</summary>
    internal void LoadGrapplePalette()
    {
        // Both ordinary HUD firing and debug entry select the installed native palette.
        (_artwork.BeamPalettes ?? throw new InvalidOperationException(
            "Grapple palette requires installed beam artwork."))
            .LoadTo(_player.Colours.Beam, GrappleBeamDefinitions.Palettes.FiringSelection);
        _player.Colours.GrappleFlare.Write(0, GrappleBeamDefinitions.Palettes.FlareColor);
    }

    /// <summary>
    /// Runs the complete Samus-side X-ray admission path after the HUD has selected the
    /// equipped scope and observed held Dash, matching `$90:DDD3 -> $91:CAD6 -> $91:E16D`.
    /// </summary>
    /// <remarks>
    /// HUD cursor movement is not yet a general runtime subsystem, so this narrow entry
    /// point represents only the already-selected item. It still requires equipped bit
    /// `$8000` and passes the live dispatch game state (states `$0C` and `$12` also run
    /// this alpha pass but setup rejects them), bomb cooldown/count, power-bomb phase, current/previous
    /// movement types, vertical velocity, landing-pose gate, and speed divisor to native
    /// setup logic. The DebugRunner uses it without substituting a host pose or art frame.
    /// </remarks>
    public bool TryBeginXrayFromSelectedHudItem(SuperMetroid.Core.Progression.SuperMetroidGameState gameState, SamusBombProjectileSystem bombProjectiles)
    {
        if (_player.Samus is null || !_player.GroundedSamusMovementEnabled || _room.LevelData is null)
        {
            throw new InvalidOperationException(
                "X-ray requires initialized gameplay Samus and room level data.");
        }

        // `$90:C5AE` prevents selecting the HUD item without the scope. The lower-level
        // `$91:E16D` routine assumes that selector has already succeeded.
        if (!_player.Samus.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope))
            return false;

        return _player.Samus.Xray.TryBegin(_player.Samus, _player.PreviousMovementTypeForXray, new XrayBeginConditions { GameState = gameState, PowerBombExplosionActive = bombProjectiles.PowerBombExplosion.Status != 0, ProjectileCooldownTimer = bombProjectiles.CooldownTimer, BombCounter = bombProjectiles.BombCounter }, XrayActivation.Deferred);
    }

    /// <summary>
    /// Runs <c>$90:A337</c>'s movement-type table for a pose stored without
    /// <c>InitializeSamusPose_1</c>: the cached <c>$0A1F</c> type, not the pose, selects the mover.
    /// </summary>
    private void StepUninitializedPoseMovement(ISamusRoomPlms plms)
    {
        RoomLevelData level = _room.LevelData ?? throw new InvalidOperationException(
            "Uninitialized-pose movement requires active room level data.");
        switch (SamusPoseMetadata.ReadMovementType(_player.IntroducedSamus))
        {
            case SamusMovementType.Special:
                // `$90:A7DA` only clears the momentum-transition selector.
                break;
            case SamusMovementType.SpinJumping:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepSpinJump(
level, _player.IntroducedSamus, _controller1.Current, _clock.Counter,
                    _controller1.NewlyPressed, plms);
                break;
            case SamusMovementType.NormalJumping:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepNormalJump(
level, _player.IntroducedSamus, _controller1.Current, _clock.Counter, plms);
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
            case SamusMovementType.Unused0B:
            case SamusMovementType.Unused0C:
            case SamusMovementType.Unused0D:
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
                    $"uninitialized pose {_player.IntroducedSamus.Pose} has no translated $90:A337 mover.");
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
    /// Runs the frame handler's alpha (SamusCurrentStateHandler, $0A42): the head, then, when Samus movement is enabled,
    /// the tail. The enable flag is captured once here for every later gate of the frame.
    /// </summary>
    internal void RunAlpha(SamusHandlerFrame frame, SuperMetroid.Core.Progression.SuperMetroidGameState gameState, SamusState samus,
        ScrollBoundaryCamera camera, FixedPosition<RoomPixels>? cameraPointAtFrameStart, RoomScope room)
    {
        RunAlphaHead(frame, samus, camera, cameraPointAtFrameStart, room.Plms, room);
        frame.SampleMovementEnabled(_player.GroundedSamusMovementEnabled);
        if (frame.MovementEnabled)
            RunAlphaTail(frame, gameState, samus, camera, room);
    }

    /// <summary>
    /// Runs the frame handler's beta (SamusNewStateHandler, $0A44): beta movement when Samus movement is enabled, then
    /// the tail.
    /// </summary>
    internal void RunBeta(SamusHandlerFrame frame, SamusState samus, ScrollBoundaryCamera camera, ISamusFrameAudio audio, RoomScope room)
    {
        if (frame.MovementEnabled)
            RunBetaMovement(frame, samus, camera, room);
        RunBetaTail(frame, samus, audio, room.Plms, room);
    }

    /// <summary>
    /// Runs alpha's tail once movement is enabled: environment gravity, terrain hazards, inside-block reactions, HUD
    /// selection, X-ray admission, bombs, grapple and projectiles, then the end-of-alpha movement snapshot ($90:EB30).
    /// </summary>
    private void RunAlphaTail(SamusHandlerFrame frame, SuperMetroid.Core.Progression.SuperMetroidGameState gameState, SamusState samus,
        ScrollBoundaryCamera camera, RoomScope room)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        // Frame-handler alpha calls `$90:9C5B` after pose input/radius setup and
        // before beta movement. Refreshing the split gravity words every frame is
        // essential at a liquid surface: a jump can cross from water to air without
        // any pose transition that would otherwise reinitialize acceleration.
        SamusAerialMovement.ConfigureEnvironmentGravity(samus);

        if (_room.ActiveRoom is null)
            throw new InvalidOperationException("Live Samus terrain reactions require an active cartridge room.");
        // `$94:9B60` samples the body after alpha's SetSamusRadius: a pose whose
        // radius grew in last frame's beta reaches air spikes with its new body now.
        // Samus command zero replaces the ordinary alpha handler with `$90:E713`,
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
                _system.HasAnyBossBits(insideRoom.AreaIndex, BossBits.AreaBoss), room.Plms);

        // Alpha order is cooldown -> movement-type HUD projectile producer ->
        // HandleProjectile. The outer gameplay loop then runs bank-$A0 overlap
        // before beta movement. A newly placed bomb therefore counts 60 -> 59 and
        // selects its first bank-$93 art record in the placement frame itself.
        // The door fade's EnemyMain/draw pass does not execute normal alpha.
        // In particular its draw-only zero input must not release a held charge.
        // X-ray's freeze leaves the normal state handler installed, so `$90:DCDD`
        // still runs cooldown, HUD switching and the HUD handler; only its
        // Handle_Projectiles tail stops (`$90:DCFB`), which both projectile owners
        // apply themselves. A selection cancel during X-ray therefore takes effect.
        frame.SampleAlphaRuns(!_freeze.GameplayTimeFrozen && !frame.DeathOwnsSamus && !room.Enemies.Elevator.ElevatorDoorTransitionActive);
        if (frame.AlphaRuns)
        {
            // `$90:C4E7` runs before the movement-type HUD projectile producer.
            // Consequently a Select edge can choose missiles and an X edge can
            // fire one during this same alpha pass. Input-locked message/elevator
            // handlers do not execute the normal selection owner.
            // The switched-to handler of the item the loop settles on runs on every
            // cancel or select edge, even one that leaves the selection unchanged.
            // Each clears the charge and restores suit colors, except grapple's
            // while a grapple is already active ($90:C59A), which changes nothing.
            if (!frame.AlphaInputLocked(samus) && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
                samus.HudSelection.HandleHudSelection(samus, _controller1.Current, _controller1.NewlyPressed).ItemHandlerAccepted &&
                (samus.SelectedHudItem != HudItem.Grapple ||
                 samus.Grapple.Phase == GrapplePhase.Inactive))
            {
                room.Projectiles.Firing.CancelChargeForHudSelection();
                samus.ProjectileFlareCounter = 0;
                samus.Presentation.LoadSuitPalette(samus, _player.Colours);
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

            // `$91:E231-$E23D` disables enemy room.Projectiles, PLMs, animated tiles,
            // and palette FX while time is frozen. Normal bombs cannot be placed or
            // advanced through this translated producer during the X-ray interval.
            BombProjectileFrameResult bombFrame = room.BombProjectiles.StepFrame(new SamusTerrain { Level = _room.LevelData, Plms = room.Plms }, samus, _controller1.Current, _controller1.NewlyPressed, overlap: BombJumpOverlapPublication.Deferred, explosion: PowerBombExplosionStep.Skip);

            if (bombFrame.BeamChargeConsumed)
            {
                room.Projectiles.Firing.CancelChargeForBombSpread();
                samus.ProjectileFlareCounter = 0;
                samus.Presentation.LoadSuitPalette(samus, _player.Colours);
            }

        }

        frame.SampleGrappleOwnership(RunGrappleBeamHandler(frame.Mode.Beta, room.Plms, room.Projectiles, room));

        if (frame.AlphaRuns)
        {
            // The bomb half above has already decremented shared cooldown $0CCC.
            // `$90:DD31` then dispatches the humanoid HUD producer before the same
            // HandleProjectile pass. Passing the live layer-1 position also gives
            // `$90:B16A` its exact off-screen deletion window.
            // A malformed callback-table entry (SpaceTime) stores raw bytes straight
            // into bank-$80's progression owners during this pass, so later
            // consumers this frame, and saves, observe them.
            room.Projectiles.StepFrame(new SamusTerrain { Level = _room.LevelData, Plms = room.Plms,
                Progression = _system }, samus, new ProjectileButtonInput { ControllerInput = _controller1.Current, ControllerNewInput = _controller1.NewlyPressed, ControllerPreviousNewInput = samus.InputSnapshot.PreviousDrawNewInput }, camera.PixelPosition, room.BombProjectiles, projectileProducerEnabled: !frame.AlphaInputLocked(samus) && !_player.DebugGrappleItemSelected, producerSoundSuppressed: room.BombProjectiles.SoundSuppressedBeforeProjectileHandling);

            if (!_freeze.TimeIsFrozen)
                room.BombProjectiles.ResolveSamusOverlap(samus, room.Projectiles.Firing.ProjectileInvincibilityTimer.RawValue);

            // `$90:E6C0` dispatches the selected HUD producer and `$90:EB20`
            // immediately clears `$0B5E`. Pose initialization occurs later in the
            // new-state handler, so a bridge published below survives precisely
            // until this point in the following gameplay frame.
            SamusAerialPoseTransitions.ClearPoseTransitionShotDirection(samus);
            samus.ProjectileInheritance.ClearMovement();

            // `$0CD0` is one shared WRAM word, not independent projectile/movement
            // state. Samus's spin/wall-jump contact-damage handlers run in beta and
            // must see the counter that `$90:B80D` just updated during alpha.
            samus.ProjectileFlareCounter = room.Projectiles.Charge.FlareCounter;
        }

        // Samus_Func10 ($90:EB30) snapshots movement at the end of alpha,
        // before beta applies animation/input transitions. Admission reads the
        // preceding snapshot; a newly queued X-ray pose is not installed yet.
        _player.PreviousMovementTypeForXray = frame.XrayActivatedThisFrame
            ? frame.MovementBeforeXrayAdmission : SamusPoseMetadata.ReadMovementType(samus);
    }

    /// <summary>
    /// Re-samples what EnemyMain can change before beta dispatches: a cancelled prospective pose ($90:F093), the
    /// stationary-script lock, updated beam hits and the contact-damage index beta clears.
    /// </summary>
    internal void RunPostEnemyMainResample(SamusHandlerFrame frame, SamusState samus, SamusBombProjectileSystem bombProjectiles, SamusProjectileSystem projectiles, RoomScope room)
    {
        if (!frame.MovementEnabled)
            return;
        // A carry-set Samus command issued by enemy AI cleared alpha's prospective
        // poses and commands ($90:F093), so nothing pending is committed this frame.
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

        // EnemyMain consumes the preceding frame's contact-damage index.
        // Only beta clears it, before movement republishes the current value.
        if (frame.Mode.Beta != SamusBetaHandler.SuitPickup)
            samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.None;
    }

    /// <summary>
    /// Runs beta's movement ($90:E725): clears the frame's movement results, runs the grapple-release mover, publishes
    /// grapple's corrected camera point and the dispatch pose, then runs the installed movement owner.
    /// </summary>
    private void RunBetaMovement(SamusHandlerFrame frame, SamusState samus, ScrollBoundaryCamera camera, RoomScope room)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        // $90:E725 dispatches movement type before animation. Every admitted pose
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
        _player.LastDraygonGrabbedMovement = null;
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
            // `$9B:C7C1` replaces the normal beta movement-handler pointer on the
            // release-input frame. It remains independent of the beam function, so
            // it must also run after `$9B:CB8B` has made that function inactive.
            SamusAerialMovement.ConfigureEnvironmentGravity(samus);
            _player.LastAerialSamusMovement = SamusAerialMovement.StepReleasedFromGrapple(

                _room.LevelData,
                samus,
                _controller1.Current,
                _clock.Counter,
                room.Plms);
            frame.SampleGrappleOwnership(true);
        }

        if (_player.LastGrappleMovement is { CameraPrevious: { } grapplePrevious })
        {
            // `$9B:BAD5-$BB5F` clamps native previous-position words to twelve
            // pixels after a close-collision snap. Feed those corrected whole words
            // to bank-$90 tracking while preserving the pre-frame subpositions.
            frame.SnapCameraPoint(grapplePrevious);
        }

        frame.SampleBetaMovementPose(samus.Pose);

        RunBetaMovementOwner(frame, samus, camera, room);
    }

    /// <summary>
    /// Runs the installed movement owner in priority order: the empty and command betas, grapple, Draygon's grab,
    /// X-ray, Ceres Ridley's ejection, knockback, bomb jump, Crystal Flash, drained, Shinespark, the locked beta, frozen
    /// time and the special type, and otherwise the pose's normal movement-table entry ($90:A337).
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
        // movement pointer that was active beforehand. Automatic Reserve can
        // therefore freeze an in-progress hurt launch; special owners such as
        // `$90:DF38` must not outrank this explicitly installed handler.
        else if (frame.Mode.Beta == SamusBetaHandler.ScriptLocked)
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
        }
        else if (frame.GrappleOwnsMovement)
        {
            // Movement type $16's beta handler is empty. Discard input transitions
            // sampled from the pre-grapple pose once connection installs $B2/$B3.
            if (!frame.GrappleReleaseAcceptsPoseInput)
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
        }
        // The real grabbed route uses the explicit RTS handler installed at
        // `$90:E262`, not the otherwise reachable type-$1A normal dispatcher. Keep
        // the latter in the pose switch below for direct/diagnostic dispatcher use.
        else if (samus.DraygonGrabbed.OwnsMovement)
        {
            // Deliberately empty: Draygon's bank-$A5 actor owns coordinates.
        }
        // Special prospective-pose command five installs `$90:E94F` instead of the
        // normal movement-type dispatcher. Stable X-ray bodies select one of five
        // angle frames; type-`$0E` turning deliberately performs no beta movement.
        else if (samus.Xray.OwnsSamusControl)
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            samus.Xray.StepMovement(samus);
        }
        // `$90:E119` installs a separate movement/gamma-handler pair for Ceres
        // Ridley's retreat. It must precede ordinary damage knockback because pose
        // `$53/$54` is shared while ownership, duration, and termination are not.
        else if (samus.CeresRidleyEjection.IsActive)
        {
            // `$90:E12E` installs the hurt pose and clears every prospective pose and
            // command (`$90:E1A5-$E1B7`). Afterwards `$90:E1C8` suppresses only a
            // damage-boost result `$4F/$50` selected from pose `$53/$54`; it does not
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
        // Knockback's `$90:DF38` handler takes precedence over the normal movement-
        // type dispatcher. Unlike bomb jump, normal pose input remains active so
        // `$53/$54` can still select the retail damage-boost escape chord.
        else if (samus.KnockbackActive)
        {
            SamusAerialMovement.ConfigureEnvironmentGravity(samus);
            _player.LastKnockbackMovement = SamusKnockbackMovement.Step(

                _room.LevelData,
                samus,
                _clock.Counter,
                room.Plms);
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

            // The shared alpha environment pass above already selected these words;
            // repeat the explicit special-handler publication here to preserve the
            // old diagnostic seam while allowing water/lava instead of forcing air.
            SamusAerialMovement.ConfigureEnvironmentGravity(samus);
            _player.LastBombJumpMovement = samus.BombJumpStarting
                ? SamusBombJumpMovement.Start(samus)
                : SamusBombJumpMovement.Step(

                    _room.LevelData,
                    samus,
                    _clock.Counter,
                    room.Plms);
        }
        // `$90:D5A2` installs one of three Crystal Flash movement pointers and an
        // RTS pose-input handler. Mother Brain's rainbow command keeps those words,
        // but temporarily replaces frame-handler beta with `$E8D9`, which calls no
        // movement at all. Once command one restores `$E725`, Flash resumes under
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
                _player.LastCrystalFlashMovement.Value.BubbleHdmaRequested)
            {
                // `$90:D6AE` clears `$0CEA`, copies Samus's raised center into the
                // shared explosion words, then spawns `$88:A2BD/$A32A`. This beta
                // pass occurs after bomb alpha, so the retained type-`$0300` slot
                // observes the released flag and deletes on the following frame.
                room.BombProjectiles.PowerBombExplosion.BeginCrystalFlash(samus.Position.Whole);
            }
        }
        // Drained controller functions install movement-type-$1B poses whose normal
        // beta dispatcher is RTS. Only animation command `$F7` replaces the handler
        // with `$90:94CB`; the other phases stay motionless while enemy AI controls
        // pose/timing. This branch is reached only when no independently retained
        // Crystal Flash pointer still owns the physical movement-handler word.
        // Controller zero ($91:E4F8) changes only the pose, so a crashing spark keeps
        // its handler and finishes below; `$F7` relinquishes the spark itself.
        // The rainbow-beam lock is beta `$E8D9` itself: once command one restores
        // `$E725` during enemy AI, this same frame's beta dispatches the knockback
        // pose's type-$0A mover below, as `$90:A5FC` does natively.
        // The controller-held standing/crouching phases last only while Samus keeps a
        // drained body: a pose the movement installs (a wall jump out of a drained
        // spin) returns her to the ordinary movement and input dispatch.
        else if (samus.Drained.Phase != DrainedSamusPhase.Inactive &&
            !(samus.Drained.Phase is DrainedSamusPhase.Standing or DrainedSamusPhase.Crouching &&
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
                // Controllers $91:E571/$E60C store a drained pose without
                // InitializeSamusPose_1, and $90:A337 keeps dispatching the cached
                // $0A1F movement type. An airborne Samus therefore keeps her jump
                // physics in the drained pose until a real initializer runs.
                StepUninitializedPoseMovement(room.Plms);
            }
        }
        // `$90:CFFA` replaces the normal movement-handler pointer. Windup, active
        // launch, and crash therefore own beta movement regardless of the pose's
        // table index; ordinary type-$1B physics does not exist to fall back to.
        else if (samus.Shinespark.OwnsMovementHandler)
        {
            // Determine_Samus_YAcceleration supplies the environment-selected pair
            // used as spark acceleration. Projectile_Func7 does not replace it.
            SamusAerialMovement.ConfigureEnvironmentGravity(samus);
            _player.LastShinesparkMovement = samus.Shinespark.Step(new SamusTerrain { Level = _room.LevelData, Plms = room.Plms }, samus, new ShinesparkFrame { NmiFrameCounter = _clock.Counter, ProjectileCounter = room.Projectiles.SlotTable.ProjectileCounter, PlayerInvincibilityEnabled = _options.PlayerInvincibilityEnabled, Projectiles = room.Projectiles, GameTimeFrames = _gameTime.Frames, DeferTimeoutPoseChange = true, PreviousCheckpoint = frame.PreviousCameraPoint });
            if (_player.LastShinesparkMovement.Value.WindupTimedOut)
            {
                // The movement handler publishes an interrupted vertical pose. That
                // priority beats an ordinary input record sampled from `$C7/$C8`
                // earlier in alpha during the timeout frame.
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
        }
        // SamusCode_00 installs an alpha handler which advances existing
        // room.Projectiles but a beta handler which performs no movement at all. All
        // translated special movement owners above take priority; an otherwise
        // locked standing body must not run generic ground collision or drift while
        // state $20 waits at the Ceres elevator.
        else if (samus.Control.AlphaLocked &&
            !(SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
              room.Enemies.Elevator.ElevatorRunsSamusMovement))
        {
            // Elevator command seven installs the moving beta handler for departure.
            // HandleTransition reinstalls it only on downward arrival. Upward arrival
            // retains command zero until the actor reaches rest; terrain probes there
            // would incorrectly wake the scroll triggers the platform passes.
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
        }
        // `$90:A337` returns before the movement table and echo update while time is
        // frozen. X-ray setup freezes time in alpha, so a running Samus stops on
        // the activation frame before her pose changes.
        else if (_freeze.TimeIsFrozen)
        {
            // No positional movement.
        }
        // `$90:A7DA` is the normal movement-table entry for type `$1B`.
        // It only clears the momentum-transition selector. This path is reachable
        // when hurt-expiry command one replaces a freshly installed Shinespark
        // handler while retaining its `$C7/$C8` body and palette (Shinespark Suit).
        else if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Special)
        {
            // No positional movement.
        }
        else
            RunPoseMovement(samus, room.Plms, room);
    }

    /// <summary>
    /// Runs the pose's normal movement-table entry ($90:A337). Every pose the retail producers reach has a branch; any
    /// other pose is corrupt state.
    /// </summary>
    private void RunPoseMovement(SamusState samus, ISamusRoomPlms plms, RoomScope room)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        switch (samus.Pose)
        {
            case SamusPoseId.ForwardFacingPowerSuitPose:
            case SamusPoseId.ForwardFacingSuitedPose:
                StepForwardFacingMovement(samus, plms, room);
                break;
            case SamusPoseId.FacingRightNormalPose:
            case SamusPoseId.StandingAimUpRightPose:
            case SamusPoseId.StandingAimDiagonalUpRightPose:
            case SamusPoseId.StandingAimDiagonalDownRightPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepStanding(_room.LevelData, samus, _clock.Counter, _controller1.Current, plms);
                break;
            case SamusPoseId.MovingRightNormalPose:
            case SamusPoseId.MovingRightGunExtendedPose:
            case SamusPoseId.RunningAimUpRightPose:
            case SamusPoseId.RunningAimDiagonalUpRightPose:
            case SamusPoseId.RunningAimDiagonalDownRightPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepRunning(_room.LevelData, samus, _clock.Counter, _controller1.Current, plms);
                break;
            case SamusPoseId.FacingLeftNormalPose:
            case SamusPoseId.StandingAimUpLeftPose:
            case SamusPoseId.StandingAimDiagonalUpLeftPose:
            case SamusPoseId.StandingAimDiagonalDownLeftPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepStanding(_room.LevelData, samus, _clock.Counter, _controller1.Current, plms);
                break;
            case SamusPoseId.MovingLeftNormalPose:
            case SamusPoseId.MovingLeftGunExtendedPose:
            case SamusPoseId.RunningAimUpLeftPose:
            case SamusPoseId.RunningAimDiagonalUpLeftPose:
            case SamusPoseId.RunningAimDiagonalDownLeftPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepRunning(_room.LevelData, samus, _clock.Counter, _controller1.Current, plms);
                break;
            case SamusPoseId.MoonwalkFacingLeftPose:
            case SamusPoseId.MoonwalkFacingRightPose:
            case SamusPoseId.MoonwalkAimUpLeftPose:
            case SamusPoseId.MoonwalkAimUpRightPose:
            case SamusPoseId.MoonwalkAimDownLeftPose:
            case SamusPoseId.MoonwalkAimDownRightPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepMoonwalking(_room.LevelData, samus, _clock.Counter, plms);
                break;
            case SamusPoseId.RanIntoWallRightPose:
            case SamusPoseId.RanIntoWallLeftPose:
            case SamusPoseId.RanIntoWallAimUpRightPose:
            case SamusPoseId.RanIntoWallAimUpLeftPose:
            case SamusPoseId.RanIntoWallAimDownRightPose:
            case SamusPoseId.RanIntoWallAimDownLeftPose:
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepRanIntoWall(_room.LevelData, samus, _clock.Counter, plms);
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
                StepTurningPoseMovement(samus, plms);
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
                _player.LastGroundedSamusMovement = SamusGroundedMovement.StepLanding(_room.LevelData, samus, _clock.Counter, plms);
                break;
            case SamusPoseId.MorphBallGroundRightPose:
            case SamusPoseId.MorphBallGroundLeftPose:
            case SamusPoseId.MorphBallMovingRightPose:
            case SamusPoseId.MorphBallMovingLeftPose:
            case SamusPoseId.SpringBallGroundRightPose:
            case SamusPoseId.SpringBallGroundLeftPose:
            case SamusPoseId.SpringBallMovingRightPose:
            case SamusPoseId.SpringBallMovingLeftPose:
                _player.LastMorphBallMovement = SamusMorphBallMovement.StepGrounded(_room.LevelData, samus, _clock.Counter, plms);
                break;
            case SamusPoseId.MorphBallFallingRightPose:
            case SamusPoseId.MorphBallFallingLeftPose:
            case SamusPoseId.SpringBallFallingRightPose:
            case SamusPoseId.SpringBallFallingLeftPose:
                _player.LastMorphBallMovement = SamusMorphBallMovement.StepFalling(_room.LevelData, samus, _controller1.Current, _clock.Counter, plms);
                break;
            case SamusPoseId.SpringBallJumpRightPose:
            case SamusPoseId.SpringBallJumpLeftPose:
                _player.LastMorphBallMovement = SamusMorphBallMovement.StepSpringBallInAir(_room.LevelData, samus, _controller1.Current, _clock.Counter, plms);
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
                _player.LastAerialSamusMovement = SamusAerialMovement.StepNormalJump(_room.LevelData, samus, _controller1.Current, _clock.Counter, plms);
                break;
            case SamusPoseId.SpinJumpRightPose:
            case SamusPoseId.SpinJumpLeftPose:
            case SamusPoseId.SpaceJumpRightPose:
            case SamusPoseId.SpaceJumpLeftPose:
            case SamusPoseId.ScrewAttackRightPose:
            case SamusPoseId.ScrewAttackLeftPose:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepSpinJump(_room.LevelData, samus, _controller1.Current, _clock.Counter, _controller1.NewlyPressed, plms);
                break;
            case SamusPoseId.WallJumpRightPose:
            case SamusPoseId.WallJumpLeftPose:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepWallJump(_room.LevelData, samus, _controller1.Current, _clock.Counter, plms);
                break;
            case SamusPoseId.DamageBoostRightPose:
            case SamusPoseId.DamageBoostLeftPose:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepDamageBoost(_room.LevelData, samus, _controller1.Current, _clock.Counter, plms);
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
                _player.LastAerialSamusMovement = SamusAerialMovement.StepTurningInAir(_room.LevelData, samus, _clock.Counter, plms);
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
                _player.LastAerialSamusMovement = SamusAerialMovement.StepFalling(_room.LevelData, samus, _controller1.Current, _clock.Counter, plms);
                break;
            case SamusPoseId.CrouchingRightPose:
            case SamusPoseId.CrouchingLeftPose:
            case SamusPoseId.CrouchingAimUpRightPose:
            case SamusPoseId.CrouchingAimUpLeftPose:
            case SamusPoseId.CrouchingAimDiagonalUpRightPose:
            case SamusPoseId.CrouchingAimDiagonalUpLeftPose:
            case SamusPoseId.CrouchingAimDiagonalDownRightPose:
            case SamusPoseId.CrouchingAimDiagonalDownLeftPose:
                _player.LastGroundedSamusMovement = SamusPostureMovement.StepCrouching(_room.LevelData, samus, _clock.Counter, plms);
                break;
            case SamusPoseId.KnockbackRightPose:
            case SamusPoseId.KnockbackLeftPose:
                StepKnockbackEndingMovement(samus, plms);
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
                _player.LastGroundedSamusMovement = SamusPostureMovement.StepCrouchStandTransition(_room.LevelData, samus, _clock.Counter, plms);
                break;
            case SamusPoseId.MorphingTransitionRightPose:
            case SamusPoseId.MorphingTransitionLeftPose:
            case SamusPoseId.UnmorphingTransitionRightPose:
            case SamusPoseId.UnmorphingTransitionLeftPose:
                _player.LastMorphBallMovement = SamusMorphBallMovement.StepTransition(_room.LevelData, samus, _clock.Counter, plms);
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
                // This is the normal dispatcher at `$90:A7D2`, which only clears
                // the vertical solid-collision result. The actual boss grab above
                // runs its separately installed RTS handler instead.
                _player.LastDraygonGrabbedMovement =
                    SamusDraygonGrabbedState.StepMovement(samus);
                break;
            case SamusPoseId.AdjacentCodePoseFD:
            case SamusPoseId.AdjacentCodePoseFE:
            case SamusPoseId.AdjacentCodePoseFF:
            case SamusPoseId.UnusedPose65:
            case SamusPoseId.UnusedPose66:
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
            case SamusPoseId.UnusedPoseDb:
            case SamusPoseId.UnusedPoseDc:
            case SamusPoseId.UnusedPoseDd:
            case SamusPoseId.UnusedPoseDe:
            case SamusPoseId.UnusedPoseDf:
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
            case SamusPoseId.UnusedKnockbackRightPose:
            case SamusPoseId.UnusedKnockbackLeftPose:
            case SamusPoseId.UnusedPose20:
            case SamusPoseId.UnusedPose21:
            case SamusPoseId.UnusedPose22:
            case SamusPoseId.UnusedPose23:
            case SamusPoseId.UnusedPose24:
            case SamusPoseId.UnusedPose39:
            case SamusPoseId.UnusedPose3A:
            case SamusPoseId.UnusedPose3F:
            case SamusPoseId.UnusedPose40:
            case SamusPoseId.UnusedPose42:
            case SamusPoseId.UnusedPose45:
            case SamusPoseId.UnusedPose46:
            case SamusPoseId.UnusedPose47:
            case SamusPoseId.UnusedPose48:
            case SamusPoseId.UnusedPose5B:
            case SamusPoseId.UnusedPose5C:
            case SamusPoseId.UnusedPose5D:
            case SamusPoseId.UnusedPose5E:
            case SamusPoseId.UnusedPose5F:
            case SamusPoseId.UnusedPose60:
            case SamusPoseId.UnusedPose61:
            case SamusPoseId.UnusedPose62:
            case SamusPoseId.UnusedPose63:
            case SamusPoseId.UnusedPose64:
            case SamusPoseId.UnusedPoseAC:
            case SamusPoseId.UnusedPoseAD:
            case SamusPoseId.UnusedPoseAE:
            case SamusPoseId.UnusedPoseAF:
            case SamusPoseId.UnusedPoseB0:
            case SamusPoseId.UnusedPoseB1:
            case SamusPoseId.UnusedPoseC5:
            case SamusPoseId.UnusedPoseC6:
            default:
                // Every pose reachable from the translated retail input, animation,
                // enemy-grab, death, drained, grapple, Morph Ball, and shinespark
                // producers has an explicit branch above. Reaching this point means
                // the pose byte no longer agrees with the cartridge-backed producer
                // set; it is corrupt state, not a recoverable movement family.
                throw new InvalidDataException(
                    $"Runtime movement dispatcher received invalid pose {samus.Pose}.");
        }
    }

    /// <summary>Runs forward-facing <c>$00/$9B</c> movement, including the elevator's one-pixel downward block scan.</summary>
    private void StepForwardFacingMovement(SamusState samus, ISamusRoomPlms plms, RoomScope room)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        // `$00/$9B` enter through controller-locked demo/elevator commands.
        // The actor-owned `$0E18` word remains an explicit producer, while
        // this consumer now includes both the zero-status no-op and nonzero
        // `$94:9763` one-pixel downward block scan.
        SamusGroundedMovement.StepFacingForward(

            _room.LevelData,
            samus,
            _clock.Counter,
            elevatorIsMoving:
                room.Enemies.Elevator.ElevatorStatus != ElevatorActorStatus.Inactive,
            plms: plms,
            publishDoorSideEffects:
                RoomMainRoutines.ShouldPublishFacingForwardElevatorDoorSideEffects(
                    room.Enemies.Elevator.ElevatorStatus));
    }

    /// <summary>Runs a turn pose's movement through its movement-type byte: ground turning, or turning in the air.</summary>
    private void StepTurningPoseMovement(SamusState samus, ISamusRoomPlms plms)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        // `$90:A337` dispatches strictly through the pose definition's
        // movement-type byte. Y direction is data consumed by the selected
        // handler, never a replacement dispatcher. In particular, crouched
        // turn `$44` can retain downward direction while its type remains
        // `$0E`; treating that word as type `$17` sent it into A790 and
        // produced the live replay crash from issue 58.
        SamusMovementType turnMovementType = SamusPoseMetadata.ReadMovementKind(samus);
        if (turnMovementType == SamusMovementType.TurningOnGround ||
            (turnMovementType == SamusMovementType.TurningWhileJumping &&
             samus.Kinematics.YDirection == SamusVerticalDirection.None))
        {
            _player.LastGroundedSamusMovement = SamusGroundedMovement.StepTurningOnGround(

                _room.LevelData,
                samus,
                _clock.Counter,
                plms);
        }
        else if (turnMovementType is
            SamusMovementType.TurningWhileJumping or
            SamusMovementType.TurningWhileFalling)
        {
            _player.LastAerialSamusMovement = SamusAerialMovement.StepTurningInAir(

                _room.LevelData,
                samus,
                _clock.Counter,
                plms);
        }
        else
        {
            throw new InvalidDataException(
                $"Turn pose {samus.Pose} has unsupported movement type " +
                $"{turnMovementType}.");
        }
    }

    /// <summary>Runs knockback's normal type-$0A entry ($90:A5FC): the one-frame grounding tail before bank $91's target.</summary>
    private void StepKnockbackEndingMovement(SamusState samus, ISamusRoomPlms plms)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

        // `$90:A5FC` is the normal type-$0A movement-table entry. Active
        // knockback and Ceres ejection were intercepted above; this branch
        // is their authentic one-frame normal-dispatch tail before bank-$91
        // installs the input/no-input target chosen during alpha.
        int groundingDisplacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics, samus.HorizontalSpeed);
        BlockMoveResult grounding = SamusGroundedMovement.StepKnockbackOrCrystalFlashEnding(

            _room.LevelData,
            samus,
            _clock.Counter,
            plms);
        _player.LastGroundedSamusMovement = new GroundedMovementResult(default, grounding);
        // Normal type-$0A still publishes collision result one when its
        // downward probe hits the floor. Reuse the ordinary landing seam
        // for presentation, pose selection and command-five speed cleanup.
        if (grounding.Collided && groundingDisplacement >= 0)
            _player.LastAerialSamusMovement = new AerialMovementResult(Landed: true, HitCeiling: false);
    }

    /// <summary>
    /// Runs alpha's head: X-ray's beam object, the frame's camera point, the frame-start captures, the live radius
    /// refresh, pose-input lookup and the lookup-failure pose ($91:82D9).
    /// </summary>
    private void RunAlphaHead(SamusHandlerFrame frame, SamusState samus, ScrollBoundaryCamera camera,
        FixedPosition<RoomPixels>? cameraPointAtFrameStart, ISamusRoomPlms plms, RoomScope room)
    {
        // X-ray's HDMA object is not part of the Samus handler. Advance its explicit
        // setup/beam/deactivation state before alpha so state five can restore the
        // ordinary pose and handler pair before this frame samples pose input. The
        // revealed-block BG2 copies/window table remain a separate renderer boundary.
        _player.LastXrayBeamStep = null;
        _player.LastDeathSequenceStep = null;
        if (samus.Xray.IsActive && !samus.DeathSequence.IsActive)
        {
            if (samus.Xray.SetupStage == XraySetupStage.BuildRevealReadBg2FirstScreen)
                _xray.BuildXrayRevealTilemap(plms);
            _player.LastXrayBeamStep = samus.Xray.StepBeam(
                samus,
                _controller1.Current);
        }

        // Preserve movement on either side of the frame boundary. A new camera uses
        // the fallback sampled before enemy-owned motion and the bank-$90 handler.
        frame.SampleCameraPoint(camera.PreviousSamusPoint ?? cameraPointAtFrameStart ??
            throw new InvalidOperationException("A gameplay frame with Samus samples her camera point at its start."));

        // Retain the dispatch pose because command $F8 can replace Samus.Pose during
        // animation later in this same frame. Native alpha/beta/transition phases all
        // agree on that order; using the mutable value afterward would apply an input
        // match selected for the old pose to the newly installed one.
        _attractDemo.BeginAttractSamusInput();
        frame.SampleFrameStartPose(samus.Pose);
        // Beta dispatches on the pose installed when it runs, which EnemyMain can have
        // replaced since alpha: Draygon's death releases a sparking Samus to pose $01,
        // and standing movement runs. `$91:E8B6` then reads that pose's type.
        // Ordinary alpha publishes the current pose's live radius before input
        // and collision. Prospective transitions later in beta can retain the
        // previous radius until this point in the following frame. The locked
        // alpha ($90:E713) has no SetSamusRadius, so e.g. an elevator ride keeps
        // the boarding pose's radius.
        if (_player.GroundedSamusMovementEnabled && !_freeze.TimeIsFrozen && !frame.AlphaInputLocked(samus) &&
            !samus.DeathSequence.IsActive && !room.Enemies.Elevator.ElevatorDoorTransitionActive)
            SamusPoseMetadata.RefreshCollisionRadii(samus);
        // Native Samus beta precedes PLMs. A lock/unlock issued by a PLM
        // affects the next beta, not the animation already owned this frame.
        // Suit command $15 installs an empty beta, not just locked pose input:
        // the suspended movement pointer and all of its timers survive, so command
        // $0B can resume it after the HDMA transformation (including Blue Suit).
        // The empty beta also skips periodic damage and the energy checks.
        frame.SampleMovementType(SamusPoseMetadata.ReadMovementType(samus));

        // Direction bits already use the transition table's canonical layout. Input
        // matching belongs to frame-handler alpha, before beta moves the CURRENT pose;
        // the winning pose is not installed until after animation below.
        // `$91:804D-$8065` gives front-view `$00/$9B` one special input rule: a
        // nonzero elevator status returns immediately, before the transition-table
        // pointer is even read. Pose zero's pointer bytes otherwise overlap legitimate
        // data and can appear to select nonsense transitions after several frames, so
        // suppressing only their application would be too late and observably wrong.
        frame.SampleDeathOwnership(samus.DeathSequence.IsActive);
        SamusControlMode mode = new(samus.Control.Mode.Alpha, DispatchedBetaHandler(frame, samus));
        // Sound queues are global persistent engines on hardware, but this typed host
        // publication is scoped to one frame's Samus handler. It opened before the HDMA
        // pass, ahead of `$91:F046`'s landing sounds during collision.
        frame.BeginControl(mode, samus.Xray.OwnsSamusControl && !frame.DeathOwnsSamus, SamusPoseMetadata.ReadMovementType(samus));
        if (frame.XrayOwnsPoseInput)
        {
            samus.Xray.HandlePoseInput(
                samus,
                _controller1.Current);
        }

        // The native type-$0F input dispatcher is an RTS ($91:8146). Transition
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
            samus.BombJumpDirection = 0;
        frame.SamplePoseLookup(poseLookup, samus);
        _player.ProspectiveSamusFallbackPose = null;
        _player.ProspectiveSamusWallCollisionPose = null;

        // Zero input and an unmatched nonzero table chord both reach `$91:82D9`; a matched
        // same-pose record does not. Native writes the prospective pose slot in alpha and
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
    /// Runs normal beta's tail after movement: the speed echo ($90:EEE7), landing effects, the timer/hack handler
    /// ($90:E738) with Draygon's escape, animation or the death sequence, the pose transitions, and periodic damage with
    /// the low-energy check.
    /// </summary>
    private void RunBetaTail(SamusHandlerFrame frame, SamusState samus, ISamusFrameAudio audio, ISamusRoomPlms plms, RoomScope room)
    {
        // Samus_MovementHandler_Normal calls `$90:EEE7` after the selected movement
        // function and before animation/pose transitions. Every fourth gameplay frame
        // at boost stage four, it alternates between two exact world-position snapshots.
        // Keeping this producer here means the draw handler below consumes post-motion
        // coordinates with the same frame ordering as the cartridge.
        if (frame.Mode.Beta != SamusBetaHandler.SuitPickup && _player.LastShinesparkMovement is null)
        {
            samus.HorizontalSpeed.CaptureSpeedEchoPosition(_gameTime.Frames, samus.Position.Whole);
        }

        // Every ordinary downward collision calls `$91:F046` inside movement, before
        // the timer hack, liquid animation, prospective-pose selection, and velocity
        // cleanup. Knockback clears its live vertical speed in DF6E before this
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

        // `$90:E738` runs the timer/hack handler after movement and before animation.
        // Commands five/$18 install the Mother Brain-specific Up-edge branches; the
        // method is a cheap no-op for every ordinary gameplay state.
        if (frame.Mode.Beta != SamusBetaHandler.SuitPickup)
            samus.Drained.StepGetUpHandler(samus, _controller1.NewlyPressed);

        if (frame.Mode.Beta != SamusBetaHandler.SuitPickup && samus.DraygonGrabbed.IsActive)
        {
            // `$90:E738` calls this installed hack handler after movement and before
            // animation. A locked grapple cancels the prospective pose chosen in alpha;
            // an escape release additionally clears every pending route while installing
            // ordinary `$01/$02` and zeroing the documented velocity words.
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
        // pose-transition handler, draw handler, and next-NMI tile selection.
        // Animation sees the same held-input word as movement. Ordinary Dash uses B
        // both to accumulate `$0B42.$0B44` and to intercept the running delay-list
        // command at `$90:852C`; passing a reconstructed input later would skew art.
        if (frame.Mode.Beta == SamusBetaHandler.Fatal)
        {
            _player.LastDeathSequenceStep = samus.DeathSequence.Step(
                samus,
                _colours,
                _player.Colours,
                _artwork.SamusBodyArt?.DeathPalettes);
        }
        // Command six's bare RTL beta ($90:E8D6) skips animation like the locked beta.
        else if (frame.Mode.Beta is not (SamusBetaHandler.ScriptLocked or SamusBetaHandler.RefillStation or SamusBetaHandler.SuitPickup))
        {
            SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { BossPresent = room.Enemies.RoomStatus.BossId != 0, ControllerInput = _controller1.Current, NmiFrameCounter = _clock.Counter, Random = _system.Random, BeginLiquidSoundRequestFrame = false, ProspectiveInputPose = _player.ProspectiveSamusPose?.ProspectivePose ?? _player.ProspectiveSamusFallbackPose, DemoPoseInput = _attractDemo.IsAttractDemo, QueueEchoSound = audio.QueueEchoSound });
            StepSamusLoadAppearance(room);
        }

        _transitions.Apply(frame, samus, plms);

        if (frame.Mode.Beta is not (SamusBetaHandler.Fatal or SamusBetaHandler.TitleDemo or SamusBetaHandler.SuitPickup))
        {
            // Normal beta ends with HandlePeriodicDamageToSamus ($90:E9CE), PauseCheck and
            // LowEnergyCheck, before the enemy-projectile and PLM handlers. Damage those
            // handlers accumulate this frame (e.g. $84:AC9D) therefore lands next frame.
            // Demo beta omits these calls, and so does suit command $15's empty beta
            // (RTL_90E8CD): damage pending during the transformation stays accumulated
            // until the first ordinary beta after it, which divides it by the new suit.
            // X-ray freezes time and therefore clears
            // rather than applies accumulated damage; fatal zero-energy game-state
            // acquisition remains the outer seam.
            samus.LiquidPhysics.ApplyPeriodicDamage(
                samus,
                timeIsFrozen: _freeze.TimeIsFrozen);
            // Locked/elevator/appearance handlers omit LowEnergyCheck; gunship command
            // $1A installs a dedicated checker. Automatic reserves invoke their external
            // check in the frontend.
            if (frame.Mode.Beta is SamusBetaHandler.Normal or SamusBetaHandler.Gunship && !samus.Xray.OwnsSamusControl)
                audio.CheckLowHealth?.Invoke();
        }
    }

    /// <summary>
    /// Advances `$90:E86A/$92:ED24`'s saved-game appearance owner. The ordinary Samus
    /// animation and bank-$8D palette interpreter have already run earlier in this frame;
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
        if (_player.LoadAppearancePaletteFx is { } loadAppearance && room.PaletteFx.IsDefinitionActive(loadAppearance))
        {
            throw new InvalidDataException(
                $"Samus load palette FX {loadAppearance} " +
                "outlived the cartridge's 360-frame appearance handler.");
        }

        // PlaySamusFanfare publishes its same-pose history shift only on the
        // completion call, before the ordinary handlers regain control.
        samus.CommitPoseHistory();
        samus.Control.Unlock();
        _player.GroundedSamusMovementEnabled = true;
        _player.EndLoadAppearance();
    }
}
