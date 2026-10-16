using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Samus's phases of a gameplay frame: grounded movement, animation transitions, uninitialized-pose movement,
/// the grapple beam, X-ray from the HUD, load appearance and beam tile loading.
/// </summary>
internal sealed class SamusFramePhases
{
    /// <summary>Supplies installed beam tiles and palettes for pause teardown and grapple selection.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Provides bomb occupancy and cooldown state for HUD X-ray admission.</summary>
    private readonly SamusBombProjectileSystem _bombProjectiles;
    /// <summary>Receives Samus and grapple palette writes at their gameplay phase.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Supplies held and newly pressed controller bits to Samus phase handlers.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Provides enemy collision and grapple targets for Samus movement.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Preserves the enemy-main ordering seam relative to Samus movement.</summary>
    private readonly EnemyFramePhase _enemyPhase;
    /// <summary>Gates movement and animation branches during gameplay time freeze.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Supplies elapsed gameplay frames to the Shinespark movement handler.</summary>
    private readonly GameTimeState _gameTime;
    /// <summary>Provides accepted-NMI cadence for movement and animation behavior.</summary>
    private readonly NmiClock _nmi;
    /// <summary>Supplies player options such as configured movement controls.</summary>
    private readonly GameplayOptions _options;
    /// <summary>Owns the live Samus state and phase-specific handler selection.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Publishes terrain and special-block reactions reached during movement.</summary>
    private readonly RoomPlmSystem _plms;
    /// <summary>Supplies projectile and beam-charge state for Samus phase dispatch.</summary>
    private readonly SamusProjectileSystem _projectiles;
    /// <summary>Provides active level data, camera, and room state for movement scans.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Tracks the palette effect that ends Samus's saved-game load appearance.</summary>
    private readonly RoomPaletteFxSystem _roomPaletteFx;
    /// <summary>Supplies persistent bank-$80 state consulted by movement handlers.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>Queues beam and Samus character transfers before the next display frame.</summary>
    private readonly VramWriteQueue _vramWrites;
    /// <summary>Provides native movement tables and routines still read at the runtime boundary.</summary>
    private readonly ISnesAddressSpace _addressSpace;

    /// <summary>Binds the frame owners required to preserve Samus's alpha, beta, and presentation ordering.</summary>
    internal SamusFramePhases(SamusFramePhasesDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _bombProjectiles = dependencies.BombProjectiles;
        _cgram = dependencies.Cgram;
        _controller1 = dependencies.Controller1;
        _enemies = dependencies.Enemies;
        _enemyPhase = dependencies.EnemyPhase;
        _freeze = dependencies.Freeze;
        _gameTime = dependencies.GameTime;
        _nmi = dependencies.Nmi;
        _options = dependencies.Options;
        _player = dependencies.Player;
        _plms = dependencies.Plms;
        _projectiles = dependencies.Projectiles;
        _room = dependencies.Room;
        _roomPaletteFx = dependencies.RoomPaletteFx;
        _system = dependencies.System;
        _vramWrites = dependencies.VramWrites;
        _addressSpace = dependencies.AddressSpace;
    }

    /// <summary>
    /// Loads the palette half of <c>Update_Beam_Tiles_and_Palette</c> at <c>$90:AC8D</c> for the live beam
    /// word supplied by pause teardown. State `$11` performs this restore under forced blank so the first
    /// resumed gameplay frame uses the newly equipped combination; the projectile pages bind its characters.
    /// </summary>
    internal void LoadGameplayBeamPalette(ushort equippedBeams) =>
        SamusProjectileSystem.LoadBeamPalette(
            _addressSpace,
            _cgram,
            equippedBeams, _artwork.BeamArtwork?.Palettes);

    /// <summary>
    /// <c>GrappleBeamHandler</c> ($9B:C490), reached from the HUD-selection dispatch of
    /// <c>HandleHUDSpecificBehaviorAndProjectiles</c> ($90:DCDD) in Samus's alpha pass.
    /// It therefore runs before Samus's projectiles and before <c>Main_Enemy_Routine</c>,
    /// so enemies see the beam endpoint and Samus position it leaves this frame. Only
    /// connected and release functions own Samus's movement; an extending or cancelling
    /// beam coexists with the current pose's ordinary movement.
    /// </summary>
    private bool RunGrappleBeamHandler(bool deathOwnsSamus, bool suitOwnsSamus)
    {
        if (_player.Samus is null || _room.LevelData is null)
            throw new InvalidOperationException("The grapple handler requires Samus and room collision data.");
        _player.LastGrappleMovement = null;
        bool grappleOwnsMovement = false;
        if (deathOwnsSamus || suitOwnsSamus)
        {
            // Game states `$15-$18` do not call the ordinary Samus alpha/beta
            // handlers. The death state advances later at the animation seam.
        }
        else if (_player.Samus.Grapple.Phase == GrapplePhase.Firing)
        {
            _player.LastGrappleMovement = SamusGrappleMovement.StepFiring(
                _addressSpace,
                _room.LevelData,
                _player.Samus,
                _controller1.Current,
                _plms,
                _enemies.GrappleBehavior.ResolveGrappleEndpoint,
                deferConnectionPoseChange: true);
            grappleOwnsMovement = _player.LastGrappleMovement.Value.OwnsMovement;
        }
        else if (_player.Samus.Grapple.Phase == GrapplePhase.CancelPending)
        {
            _player.LastGrappleMovement =
                SamusGrappleMovement.CompleteFiringCancellation(_addressSpace, _room.LevelData, _player.Samus);
            grappleOwnsMovement = _player.LastGrappleMovement.Value.OwnsMovement;
        }
        else if (_player.Samus.Grapple.Phase is
            GrapplePhase.ConnectedSwinging or GrapplePhase.ReleaseFromSwing or
            GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab or
            GrapplePhase.WallGrabRelease or GrapplePhase.WallJumping or
            GrapplePhase.Dropped)
        {
            _player.LastGrappleMovement = SamusGrappleMovement.Step(new SamusTerrain { Bus = _addressSpace, Level = _room.LevelData, Plms = _plms }, _player.Samus, _controller1.Current, _controller1.NewlyPressed, _nmi.NmiFrameCounter, _enemies.GrappleBehavior.ResolveGrappleEndpoint, deferDropPoseChange: true);
            grappleOwnsMovement = _player.LastGrappleMovement.Value.OwnsMovement;
        }
        else if (!_freeze.TimeIsFrozen &&
                 (_player.DebugGrappleItemSelected || SamusGrappleHudInput.IsSelectedAndAdmitted(_addressSpace, _player.Samus)) &&
                 ((_controller1.NewlyPressed | _player.Samus.InputSnapshot.PreviousDrawNewInput) & SnesButton.X) != 0)
        {
            // Normal HUD item four now reaches the same bank-$9B actor as the
            // diagnostic entry point, after bank-$90's movement-type admission.
            SamusGrappleMovement.BeginFiring(_addressSpace, _player.Samus, _controller1.Current);
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
            (_artwork.BeamArtwork?.Palettes ?? throw new InvalidOperationException(
                "Grapple cleanup requires installed beam artwork."))
                .LoadTo(_cgram, new SamusBeamLoadoutWord(_player.Samus.EquippedBeams).Combination ?? throw new InvalidOperationException(
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
        (_artwork.BeamArtwork?.Palettes ?? throw new InvalidOperationException(
            "Grapple palette requires installed beam artwork."))
            .LoadTo(_cgram, SamusGrappleRomData.Palettes.FiringSelection);
        new SamusPalettes(_cgram).GrappleFlare.Write(0, SamusGrappleRomData.Palettes.FlareColor);
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
    public bool TryBeginXrayFromSelectedHudItem(ushort gameState, bool deferActivation)
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

        return _player.Samus.Xray.TryBegin(_addressSpace, _player.Samus, _player.PreviousMovementTypeForXray, new XrayBeginConditions { GameState = gameState, PowerBombExplosionStatus = _bombProjectiles.PowerBombExplosion.Status, ProjectileCooldownTimer = _bombProjectiles.CooldownTimer.RawValue, BombCounter = _bombProjectiles.BombCounter }, deferActivation);
    }

    /// <summary>
    /// Runs <c>$90:A337</c>'s movement-type table for a pose stored without
    /// <c>InitializeSamusPose_1</c>: the cached <c>$0A1F</c> type, not the pose, selects the mover.
    /// </summary>
    private void StepUninitializedPoseMovement()
    {
        RoomLevelData level = _room.LevelData ?? throw new InvalidOperationException(
            "Uninitialized-pose movement requires active room level data.");
        switch (_player.IntroducedSamus.PoseMetadata.ReadMovementType(_addressSpace))
        {
            case SamusMovementType.Special:
                // `$90:A7DA` only clears the momentum-transition selector.
                break;
            case SamusMovementType.SpinJumping:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepSpinJump(
                    _addressSpace, level, _player.IntroducedSamus, _controller1.Current, _nmi.NmiFrameCounter,
                    _controller1.NewlyPressed, _plms);
                break;
            case SamusMovementType.NormalJumping:
                _player.LastAerialSamusMovement = SamusAerialMovement.StepNormalJump(
                    _addressSpace, level, _player.IntroducedSamus, _controller1.Current, _nmi.NmiFrameCounter, _plms);
                break;
            default:
                throw new NotSupportedException(
                    $"Cached movement type ${(byte)_player.IntroducedSamus.PoseMetadata.ReadMovementType(_addressSpace):X2} under " +
                    $"uninitialized pose ${(int)_player.IntroducedSamus.Pose:X2} has no translated $90:A337 mover.");
        }
    }
    /// <summary>Runs Samus's terrain probes and movement dispatcher after alpha input, honoring script locks and enemy-main ordering.</summary>
    internal void StepGroundedSamusMovement(SamusHandlerFrame frame, ushort gameState, bool enemyMainAlreadyRan, Func<bool> alphaInputLocked, SamusState samus, ScrollBoundaryCamera camera)
    {
        if (_player.GroundedSamusMovementEnabled)
        {
            if (_room.LevelData is null)
                throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

            // Frame-handler alpha calls `$90:9C5B` after pose input/radius setup and
            // before beta movement. Refreshing the split gravity words every frame is
            // essential at a liquid surface: a jump can cross from water to air without
            // any pose transition that would otherwise reinitialize acceleration.
            SamusAerialMovement.ConfigureEnvironmentGravity(_addressSpace, samus);

            if (_room.ActiveRoom is null)
                throw new InvalidOperationException("Live Samus terrain reactions require an active cartridge room.");
            // `$94:9B60` samples the body after alpha's SetSamusRadius: a pose whose
            // radius grew in last frame's beta reaches air spikes with its new body now.
            // Samus command zero replaces the ordinary alpha handler with `$90:E713`,
            // which never dispatches block-inside detection. Automatic Reserve recovery
            // uses this exact lock; re-sampling spikes while it owns Samus changes the
            // post-recovery knockback window used by Shinespark Suit.
            if (!samus.ScriptLocks.StationaryScriptControlLocked)
            {
                SamusTerrainHazardCollision.PrepareFrame(
                    _addressSpace,
                    _room.LevelData,
                    samus,
                    _room.ActiveRoom.AreaIndex,
                    _system.HasAnyBossBits(_room.ActiveRoom.AreaIndex, BossBits.AreaBoss));
            }

            if (!_freeze.TimeIsFrozen && !alphaInputLocked() && _room.ActiveRoom is { } insideRoom)
                SamusInsideBlockReactions.PrepareFrame(_room.LevelData, samus, insideRoom.AreaIndex,
                    _system.HasAnyBossBits(insideRoom.AreaIndex, BossBits.AreaBoss), _plms);

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
            bool alphaRuns = !_freeze.GameplayTimeFrozen && !frame.DeathOwnsSamus && !_enemies.ElevatorBehavior.ElevatorDoorTransitionActive;
            if (alphaRuns)
            {
                // `$90:C4E7` runs before the movement-type HUD projectile producer.
                // Consequently a Select edge can choose missiles and an X edge can
                // fire one during this same alpha pass. Input-locked message/elevator
                // handlers do not execute the normal selection owner.
                // The switched-to handler of the item the loop settles on runs on every
                // cancel or select edge, even one that leaves the selection unchanged.
                // Each clears the charge and restores suit colors, except grapple's
                // while a grapple is already active ($90:C59A), which changes nothing.
                if (!alphaInputLocked() && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
                    samus.HudSelection.HandleHudSelection(_controller1.Current, _controller1.NewlyPressed).ItemHandlerAccepted &&
                    (samus.SelectedHudItem != SamusHudRomData.GrappleSelectedItem ||
                     samus.Grapple.Phase == GrapplePhase.Inactive))
                {
                    _projectiles.Firing.CancelChargeForHudSelection();
                    samus.ProjectileFlareCounter = 0;
                    samus.Presentation.LoadSuitPalette(_addressSpace, _cgram);
                }

                // The selected scope uses held Run, not Fire. Setup installs its
                // own pose handlers, superseding alpha's ordinary pending pose.
                if (!alphaInputLocked() && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
                    samus.SelectedHudItem == SamusXrayRomData.SelectedHudItem &&
                    samus.Grapple.Phase == GrapplePhase.Inactive &&
                    (_controller1.Current & SnesButton.B) != 0 &&
                    TryBeginXrayFromSelectedHudItem(gameState, deferActivation: true))
                {
                    frame.XrayActivatedThisFrame = true;
                    _player.ProspectiveSamusPose = null;
                    _player.ProspectiveSamusFallbackPose = null;
                }

                // `$91:E231-$E23D` disables enemy projectiles, PLMs, animated tiles,
                // and palette FX while time is frozen. Normal bombs cannot be placed or
                // advanced through this translated producer during the X-ray interval.
                BombProjectileFrameResult bombFrame = _bombProjectiles.StepFrame(new SamusTerrain { Bus = _addressSpace, Level = _room.LevelData, Plms = _plms }, samus, _controller1.Current, _controller1.NewlyPressed, deferSamusOverlap: true, advancePowerBombHdma: false);

                if (bombFrame.BeamChargeConsumed)
                {
                    _projectiles.Firing.CancelChargeForBombSpread();
                    samus.ProjectileFlareCounter = 0;
                    samus.Presentation.LoadSuitPalette(_addressSpace, _cgram);
                }

            }

            bool grappleOwnsMovement = RunGrappleBeamHandler(frame.DeathOwnsSamus, frame.SuitOwnsSamus);

            if (alphaRuns)
            {
                // The bomb half above has already decremented shared cooldown $0CCC.
                // `$90:DD31` then dispatches the humanoid HUD producer before the same
                // HandleProjectile pass. Passing the live layer-1 position also gives
                // `$90:B16A` its exact off-screen deletion window.
                // Bank-$80's typed progression owners and the cartridge's live SRAM
                // mirror are normally equivalent. Publish them before bank-$90 alpha
                // so malformed callback-table entries can perform their literal WRAM
                // writes, then reclaim ownership only when such a write occurred.
                _system.WritePersistentMirror(RuntimeMemory.RequireBus(_addressSpace));
                SamusProjectileFrameResult projectileFrame = _projectiles.StepFrame(new SamusTerrain { Bus = _addressSpace, Level = _room.LevelData, Plms = _plms }, samus, new ProjectileButtonInput { ControllerInput = _controller1.Current, ControllerNewInput = _controller1.NewlyPressed, ControllerPreviousNewInput = samus.InputSnapshot.PreviousDrawNewInput }, camera.PixelPosition, _bombProjectiles, projectileProducerEnabled: !alphaInputLocked() && !_player.DebugGrappleItemSelected, producerSoundSuppressed: _bombProjectiles.SoundSuppressedBeforeProjectileHandling);
                if (projectileFrame.PersistentMemoryCorrupted)
                    _system.LoadPersistentMirror(RuntimeMemory.RequireBus(_addressSpace));

                if (!_freeze.TimeIsFrozen)
                    _bombProjectiles.ResolveSamusOverlap(samus, _projectiles.Firing.ProjectileInvincibilityTimer.RawValue);

                // `$90:E6C0` dispatches the selected HUD producer and `$90:EB20`
                // immediately clears `$0B5E`. Pose initialization occurs later in the
                // new-state handler, so a bridge published below survives precisely
                // until this point in the following gameplay frame.
                samus.PoseTransitions.ClearPoseTransitionShotDirection();
                SamusProjectileInheritance.ClearMovement(_addressSpace);

                // `$0CD0` is one shared WRAM word, not independent projectile/movement
                // state. Samus's spin/wall-jump contact-damage handlers run in beta and
                // must see the counter that `$90:B80D` just updated during alpha.
                samus.ProjectileFlareCounter = _projectiles.Charge.FlareCounter;
            }

            // Samus_Func10 ($90:EB30) snapshots movement at the end of alpha,
            // before beta applies animation/input transitions. Admission reads the
            // preceding snapshot; a newly queued X-ray pose is not installed yet.
            _player.PreviousMovementTypeForXray = frame.XrayActivatedThisFrame
                ? frame.MovementBeforeXrayAdmission : samus.PoseMetadata.ReadMovementType(_addressSpace);
            if (!enemyMainAlreadyRan)
                _enemyPhase.RunEnemyMainPhase(processingListPrepared: true, frame.PreviousCameraPoint);
            // A carry-set Samus command issued by enemy AI cleared alpha's prospective
            // poses and commands ($90:F093), so nothing pending is committed this frame.
            if (samus.ConsumePendingPoseTransitionCancellation())
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
            // Actor commands replace beta before its dispatch in this same update.
            frame.StationaryScriptControlLocked = samus.ScriptLocks.StationaryScriptControlLocked;
            if (!_freeze.TimeIsFrozen && !frame.DeathOwnsSamus)
                _enemyPhase.ResolveUpdatedBeamHits();

            // EnemyMain consumes the preceding frame's contact-damage index.
            // Only beta clears it, before movement republishes the current value.
            if (!frame.SuitOwnsSamus)
                samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.None;

            // $90:E725 dispatches movement type before animation. Every admitted pose
            // below has its own verified direction/mode path; a newly reachable pose
            // cannot accidentally inherit generic standing or running physics.
            _player.LastGroundedSamusMovement = null;
            _player.LastAerialSamusMovement = null;
            _player.LastMorphBallMovement = null;
            _player.LastBombJumpMovement = null;
            _player.LastKnockbackMovement = null;
            _player.LastCeresRidleyEjection = null;
            _player.LastShinesparkMovement = null;
            _player.LastCrystalFlashMovement = null;
            _player.LastXrayAnimationFrame = null;
            // X-ray publishes its pose-input and beam results above because both run
            // outside the normal movement-type dispatcher. Its movement result is the
            // directly inspectable frame returned by `StepMovement` below.
            _player.LastDrainedSamusMovement = null;
            _player.LastDraygonGrabbedMovement = null;
            _player.LastDraygonEscape = null;

            bool grappleReleaseAcceptsPoseInput = false;
            if (samus.Grapple.ReleasedMovementActive)
            {
                // The replacement beta mover does not replace normal airborne alpha
                // input. Preserve its pending aim/turn transition after release cleanup;
                // the connection/release frames that themselves replace the pose still
                // invalidate a lookup sampled from their previous grapple body.
                grappleReleaseAcceptsPoseInput =
                    frame.MovementTypeAtFrameStart != SamusMovementType.Grappling &&
                    samus.Pose == frame.PoseAtFrameStart;
                // `$9B:C7C1` replaces the normal beta movement-handler pointer on the
                // release-input frame. It remains independent of the beam function, so
                // it must also run after `$9B:CB8B` has made that function inactive.
                SamusAerialMovement.ConfigureEnvironmentGravity(_addressSpace, samus);
                _player.LastAerialSamusMovement = SamusAerialMovement.StepReleasedFromGrapple(
                    _addressSpace,
                    _room.LevelData,
                    samus,
                    _controller1.Current,
                    _nmi.NmiFrameCounter,
                    _plms);
                grappleOwnsMovement = true;
            }

            if (_player.LastGrappleMovement is { CameraPrevious: { } grapplePrevious })
            {
                // `$9B:BAD5-$BB5F` clamps native previous-position words to twelve
                // pixels after a close-collision snap. Feed those corrected whole words
                // to bank-$90 tracking while preserving the pre-frame subpositions.
                frame.PreviousCameraPoint = frame.PreviousCameraPoint.WithWhole(
                    grapplePrevious);
            }

            frame.PoseAtBetaMovement = samus.Pose;

            if (frame.DeathOwnsSamus || frame.SuitOwnsSamus)
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
            // Command zero installs an empty beta handler independently of the
            // movement pointer that was active beforehand. Automatic Reserve can
            // therefore freeze an in-progress hurt launch; special owners such as
            // `$90:DF38` must not outrank this explicitly installed handler.
            else if (frame.StationaryScriptControlLocked)
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
            else if (grappleOwnsMovement)
            {
                // Movement type $16's beta handler is empty. Discard input transitions
                // sampled from the pre-grapple pose once connection installs $B2/$B3.
                if (!grappleReleaseAcceptsPoseInput)
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
                _player.LastXrayAnimationFrame = samus.Xray.StepMovement(_addressSpace, samus);
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
                _player.LastCeresRidleyEjection = samus.CeresRidleyEjection.Step(
                    _addressSpace,
                    _room.LevelData,
                    samus,
                    camera.PixelPosition.X,
                    _nmi.NmiFrameCounter);
            }
            // Knockback's `$90:DF38` handler takes precedence over the normal movement-
            // type dispatcher. Unlike bomb jump, normal pose input remains active so
            // `$53/$54` can still select the retail damage-boost escape chord.
            else if (samus.KnockbackActive)
            {
                SamusAerialMovement.ConfigureEnvironmentGravity(_addressSpace, samus);
                _player.LastKnockbackMovement = SamusKnockbackMovement.Step(
                    _addressSpace,
                    _room.LevelData,
                    samus,
                    _nmi.NmiFrameCounter,
                    _plms);
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
                SamusAerialMovement.ConfigureEnvironmentGravity(_addressSpace, samus);
                _player.LastBombJumpMovement = samus.BombJumpStarting
                    ? SamusBombJumpMovement.Start(_addressSpace, samus)
                    : SamusBombJumpMovement.Step(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _plms);
            }
            // `$90:D5A2` installs one of three Crystal Flash movement pointers and an
            // RTS pose-input handler. Mother Brain's rainbow command keeps those words,
            // but temporarily replaces frame-handler beta with `$E8D9`, which calls no
            // movement at all. Once command one restores `$E725`, Flash resumes under
            // the drained pose. The Super Metroid never installs that beta lock, so its
            // interruption continues Flash immediately. Model the physical beta owner,
            // not merely the fact that a cinematic pose exists.
            else if (samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive &&
                !(samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked && samus.InputLocked))
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                _player.LastCrystalFlashMovement = samus.CrystalFlash.Step(
                    _addressSpace,
                    samus,
                    _nmi.NmiFrameCounter);
                if (_player.LastCrystalFlashMovement.Value.PhaseAtStart == CrystalFlashPhase.Raising &&
                    _player.LastCrystalFlashMovement.Value.BubbleHdmaRequested)
                {
                    // `$90:D6AE` clears `$0CEA`, copies Samus's raised center into the
                    // shared explosion words, then spawns `$88:A2BD/$A32A`. This beta
                    // pass occurs after bomb alpha, so the retained type-`$0300` slot
                    // observes the released flag and deletes on the following frame.
                    _bombProjectiles.PowerBombExplosion.BeginCrystalFlash(samus.Position.Whole);
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
                !(samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked && !samus.InputLocked) &&
                (samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked || !samus.Shinespark.OwnsMovementHandler))
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                if (samus.Drained.Phase == DrainedSamusPhase.Falling)
                {
                    _player.LastDrainedSamusMovement = samus.Drained.StepFalling(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter);
                }
                else if (samus.InitializedPose != samus.Pose)
                {
                    // Controllers $91:E571/$E60C store a drained pose without
                    // InitializeSamusPose_1, and $90:A337 keeps dispatching the cached
                    // $0A1F movement type. An airborne Samus therefore keeps her jump
                    // physics in the drained pose until a real initializer runs.
                    StepUninitializedPoseMovement();
                }
            }
            // `$90:CFFA` replaces the normal movement-handler pointer. Windup, active
            // launch, and crash therefore own beta movement regardless of the pose's
            // table index; ordinary type-$1B physics does not exist to fall back to.
            else if (samus.Shinespark.OwnsMovementHandler)
            {
                // Determine_Samus_YAcceleration supplies the environment-selected pair
                // used as spark acceleration. Projectile_Func7 does not replace it.
                SamusAerialMovement.ConfigureEnvironmentGravity(_addressSpace, samus);
                _player.LastShinesparkMovement = samus.Shinespark.Step(new SamusTerrain { Bus = _addressSpace, Level = _room.LevelData, Plms = _plms }, samus, new ShinesparkFrame { NmiFrameCounter = _nmi.NmiFrameCounter, ProjectileCounter = _projectiles.SlotTable.ProjectileCounter, PlayerInvincibilityEnabled = _options.PlayerInvincibilityEnabled, Projectiles = _projectiles, GameTimeFrames = _gameTime.Frames, DeferTimeoutPoseChange = true, PreviousCheckpoint = frame.PreviousCameraPoint });
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
            // projectiles but a beta handler which performs no movement at all. All
            // translated special movement owners above take priority; an otherwise
            // locked standing body must not run generic ground collision or drift while
            // state $20 waits at the Ceres elevator.
            else if (samus.InputLocked &&
                !(SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
                  _enemies.ElevatorBehavior.ElevatorRunsSamusMovement))
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
            else if (samus.PoseMetadata.ReadMovementType(_addressSpace) == SamusMovementType.Special)
            {
                // No positional movement.
            }
            else switch (samus.Pose)
            {
                case SamusPoseId.ForwardFacingPowerSuitPose:
                case SamusPoseId.ForwardFacingSuitedPose:
                    // `$00/$9B` enter through controller-locked demo/elevator commands.
                    // The actor-owned `$0E18` word remains an explicit producer, while
                    // this consumer now includes both the zero-status no-op and nonzero
                    // `$94:9763` one-pixel downward block scan.
                    SamusGroundedMovement.StepFacingForward(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        elevatorIsMoving:
                            _enemies.ElevatorBehavior.ElevatorStatus != ElevatorActorStatus.Inactive,
                        plms: _plms,
                        publishDoorSideEffects:
                            RoomMainRoutines.ShouldPublishFacingForwardElevatorDoorSideEffects(
                                _enemies.ElevatorBehavior.ElevatorStatus));
                    break;
                case SamusPoseId.FacingRightNormalPose:
                case SamusPoseId.StandingAimUpRightPose:
                case SamusPoseId.StandingAimDiagonalUpRightPose:
                case SamusPoseId.StandingAimDiagonalDownRightPose:
                    _player.LastGroundedSamusMovement = SamusGroundedMovement.StepStandingRight(
                    _addressSpace,
                    _room.LevelData,
                    samus,
                    _nmi.NmiFrameCounter,
                    _controller1.Current,
                    _plms);
                    break;
                case SamusPoseId.MovingRightNormalPose:
                case SamusPoseId.MovingRightGunExtendedPose:
                case SamusPoseId.RunningAimUpRightPose:
                case SamusPoseId.RunningAimDiagonalUpRightPose:
                case SamusPoseId.RunningAimDiagonalDownRightPose:
                    _player.LastGroundedSamusMovement = SamusGroundedMovement.StepRunningRight(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _controller1.Current,
                        _plms);
                    break;
                case SamusPoseId.FacingLeftNormalPose:
                case SamusPoseId.StandingAimUpLeftPose:
                case SamusPoseId.StandingAimDiagonalUpLeftPose:
                case SamusPoseId.StandingAimDiagonalDownLeftPose:
                    _player.LastGroundedSamusMovement = SamusGroundedMovement.StepStandingLeft(
                    _addressSpace,
                    _room.LevelData,
                    samus,
                    _nmi.NmiFrameCounter,
                    _controller1.Current,
                    _plms);
                    break;
                case SamusPoseId.MovingLeftNormalPose:
                case SamusPoseId.MovingLeftGunExtendedPose:
                case SamusPoseId.RunningAimUpLeftPose:
                case SamusPoseId.RunningAimDiagonalUpLeftPose:
                case SamusPoseId.RunningAimDiagonalDownLeftPose:
                    _player.LastGroundedSamusMovement = SamusGroundedMovement.StepRunningLeft(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _controller1.Current,
                        _plms);
                    break;
                case SamusPoseId.MoonwalkFacingLeftPose:
                case SamusPoseId.MoonwalkFacingRightPose:
                case SamusPoseId.MoonwalkAimUpLeftPose:
                case SamusPoseId.MoonwalkAimUpRightPose:
                case SamusPoseId.MoonwalkAimDownLeftPose:
                case SamusPoseId.MoonwalkAimDownRightPose:
                    _player.LastGroundedSamusMovement = SamusGroundedMovement.StepMoonwalking(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _plms);
                    break;
                case SamusPoseId.RanIntoWallRightPose:
                case SamusPoseId.RanIntoWallLeftPose:
                case SamusPoseId.RanIntoWallAimUpRightPose:
                case SamusPoseId.RanIntoWallAimUpLeftPose:
                case SamusPoseId.RanIntoWallAimDownRightPose:
                case SamusPoseId.RanIntoWallAimDownLeftPose:
                    _player.LastGroundedSamusMovement = SamusGroundedMovement.StepRanIntoWall(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _plms);
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
                    // `$90:A337` dispatches strictly through the pose definition's
                    // movement-type byte. Y direction is data consumed by the selected
                    // handler, never a replacement dispatcher. In particular, crouched
                    // turn `$44` can retain downward direction while its type remains
                    // `$0E`; treating that word as type `$17` sent it into A790 and
                    // produced the live replay crash from issue 58.
                    SamusMovementType turnMovementType = samus.PoseMetadata.ReadMovementKind(_addressSpace);
                    if (turnMovementType == SamusMovementType.TurningOnGround ||
                        (turnMovementType == SamusMovementType.TurningWhileJumping &&
                         samus.Kinematics.YDirection == SamusVerticalDirection.None))
                    {
                        _player.LastGroundedSamusMovement = SamusGroundedMovement.StepTurningOnGround(
                            _addressSpace,
                            _room.LevelData,
                            samus,
                            _nmi.NmiFrameCounter,
                            _plms);
                    }
                    else if (turnMovementType is
                        SamusMovementType.TurningWhileJumping or
                        SamusMovementType.TurningWhileFalling)
                    {
                        _player.LastAerialSamusMovement = SamusAerialMovement.StepTurningInAir(
                            _addressSpace,
                            _room.LevelData,
                            samus,
                            _nmi.NmiFrameCounter,
                            _plms);
                    }
                    else
                    {
                        throw new InvalidDataException(
                            $"Turn pose ${(int)samus.Pose:X2} has unsupported movement type " +
                            $"${(byte)turnMovementType:X2}.");
                    }
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
                    _player.LastGroundedSamusMovement = SamusGroundedMovement.StepLanding(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _plms);
                    break;
                case SamusPoseId.MorphBallGroundRightPose:
                case SamusPoseId.MorphBallGroundLeftPose:
                case SamusPoseId.MorphBallMovingRightPose:
                case SamusPoseId.MorphBallMovingLeftPose:
                case SamusPoseId.SpringBallGroundRightPose:
                case SamusPoseId.SpringBallGroundLeftPose:
                case SamusPoseId.SpringBallMovingRightPose:
                case SamusPoseId.SpringBallMovingLeftPose:
                    _player.LastMorphBallMovement = SamusMorphBallMovement.StepGrounded(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _plms);
                    break;
                case SamusPoseId.MorphBallFallingRightPose:
                case SamusPoseId.MorphBallFallingLeftPose:
                case SamusPoseId.SpringBallFallingRightPose:
                case SamusPoseId.SpringBallFallingLeftPose:
                    _player.LastMorphBallMovement = SamusMorphBallMovement.StepFalling(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _controller1.Current,
                        _nmi.NmiFrameCounter,
                        _plms);
                    break;
                case SamusPoseId.SpringBallJumpRightPose:
                case SamusPoseId.SpringBallJumpLeftPose:
                    _player.LastMorphBallMovement = SamusMorphBallMovement.StepSpringBallInAir(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _controller1.Current,
                        _nmi.NmiFrameCounter,
                        _plms);
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
                    _player.LastAerialSamusMovement = SamusAerialMovement.StepNormalJump(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _controller1.Current,
                        _nmi.NmiFrameCounter,
                        _plms);
                    break;
                case SamusPoseId.SpinJumpRightPose:
                case SamusPoseId.SpinJumpLeftPose:
                case SamusPoseId.SpaceJumpRightPose:
                case SamusPoseId.SpaceJumpLeftPose:
                case SamusPoseId.ScrewAttackRightPose:
                case SamusPoseId.ScrewAttackLeftPose:
                    _player.LastAerialSamusMovement = SamusAerialMovement.StepSpinJump(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _controller1.Current,
                        _nmi.NmiFrameCounter,
                        _controller1.NewlyPressed,
                        _plms);
                    break;
                case SamusPoseId.WallJumpRightPose:
                case SamusPoseId.WallJumpLeftPose:
                    _player.LastAerialSamusMovement = SamusAerialMovement.StepWallJump(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _controller1.Current,
                        _nmi.NmiFrameCounter,
                        _plms);
                    break;
                case SamusPoseId.DamageBoostRightPose:
                case SamusPoseId.DamageBoostLeftPose:
                    _player.LastAerialSamusMovement = SamusAerialMovement.StepDamageBoost(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _controller1.Current,
                        _nmi.NmiFrameCounter,
                        _plms);
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
                    _player.LastAerialSamusMovement = SamusAerialMovement.StepTurningInAir(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _plms);
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
                    _player.LastAerialSamusMovement = SamusAerialMovement.StepFalling(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _controller1.Current,
                        _nmi.NmiFrameCounter,
                        _plms);
                    break;
                case SamusPoseId.CrouchingRightPose:
                case SamusPoseId.CrouchingLeftPose:
                case SamusPoseId.CrouchingAimUpRightPose:
                case SamusPoseId.CrouchingAimUpLeftPose:
                case SamusPoseId.CrouchingAimDiagonalUpRightPose:
                case SamusPoseId.CrouchingAimDiagonalUpLeftPose:
                case SamusPoseId.CrouchingAimDiagonalDownRightPose:
                case SamusPoseId.CrouchingAimDiagonalDownLeftPose:
                    _player.LastGroundedSamusMovement = SamusPostureMovement.StepCrouching(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _plms);
                    break;
                case SamusPoseId.KnockbackRightPose:
                case SamusPoseId.KnockbackLeftPose:
                {
                    // `$90:A5FC` is the normal type-$0A movement-table entry. Active
                    // knockback and Ceres ejection were intercepted above; this branch
                    // is their authentic one-frame normal-dispatch tail before bank-$91
                    // installs the input/no-input target chosen during alpha.
                    int groundingDisplacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
                        samus.Kinematics, samus.HorizontalSpeed);
                    BlockMoveResult grounding = SamusGroundedMovement.StepKnockbackOrCrystalFlashEnding(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _plms);
                    _player.LastGroundedSamusMovement = new GroundedMovementResult(default, grounding);
                    // Normal type-$0A still publishes collision result one when its
                    // downward probe hits the floor. Reuse the ordinary landing seam
                    // for presentation, pose selection and command-five speed cleanup.
                    if (grounding.Collided && groundingDisplacement >= 0)
                        _player.LastAerialSamusMovement = new AerialMovementResult(
Landed: true, HitCeiling: false);
                    break;
                }
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
                    _player.LastGroundedSamusMovement = SamusPostureMovement.StepCrouchStandTransition(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _plms);
                    break;
                case SamusPoseId.MorphingTransitionRightPose:
                case SamusPoseId.MorphingTransitionLeftPose:
                case SamusPoseId.UnmorphingTransitionRightPose:
                case SamusPoseId.UnmorphingTransitionLeftPose:
                    _player.LastMorphBallMovement = SamusMorphBallMovement.StepTransition(
                        _addressSpace,
                        _room.LevelData,
                        samus,
                        _nmi.NmiFrameCounter,
                        _plms);
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
                default:
                    // Every pose reachable from the translated retail input, animation,
                    // enemy-grab, death, drained, grapple, Morph Ball, and shinespark
                    // producers has an explicit branch above. Reaching this point means
                    // the pose byte no longer agrees with the cartridge-backed producer
                    // set; it is corrupt state, not a recoverable movement family.
                    throw new InvalidDataException(
                        $"Runtime movement dispatcher received invalid pose ${(int)samus.Pose:X2}.");
            }
        }
    }

    /// <summary>Applies post-movement pose and animation transitions after the frame's selected Samus handler.</summary>
    internal void ApplySamusAnimationTransitions(SamusHandlerFrame frame, SamusState samus)
    {
        if (_player.GroundedSamusMovementEnabled && !frame.DeathOwnsSamus && !frame.SuitOwnsSamus &&
            !frame.StationaryScriptControlLocked)
        {
            // Hit interruption observes the old movement type before UpdateSamusPose.
            // Its carry-clear bomb rejection still occurs when an animation transition
            // wins pose selection; otherwise a stale request survives unmorph/morph.
            if (samus.KnockbackTimer.IsZero && samus.KnockbackDirection == 0)
                samus.PoseMetadata.RejectUnsupportedPublishedBombJump(_addressSpace, _freeze.TimeIsFrozen);

            // Command $F8's command-three “super-special” transition wins at this seam.
            // F8 publishes a turn-completion pose only if alpha did not select one
            // of its four jump exceptions. When published, it still wins here.
            // `$90:DDE9` samples hurt expiry before bank $91 applies a queued
            // animation command. Preserve that decision even if Shinespark setup
            // clears the live knockback direction while installing `$C7/$C8`.
            bool hitInterruptionExpiryPending =
                SamusKnockbackMovement.IsExpiredHitInterruptionPending(samus);
            bool animationTransitionApplied =
                samus.PoseTransitions.ApplyPendingVerifiedAnimationTransition(_addressSpace);

            // C8C5 queues a transitional pose rather than replacing beta movement.
            // Commit after animation, suppressing the lower-priority input/grounding
            // pose. Its speed clears already happened before movement in bank $9B.
            if (!animationTransitionApplied && _player.LastGrappleMovement is { PendingDropPose: SamusPoseId dropPose })
            {
                samus.PoseTransitions.ApplyGrappleDropTransition(_addressSpace,
                    _room.LevelData ?? throw new InvalidOperationException("Grapple drop requires room geometry."),
                    dropPose, _nmi.NmiFrameCounter, _plms, clearMovementSpeed: false);
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                _player.ProspectiveSamusWallCollisionPose = null;
                animationTransitionApplied = true;
            }

            // $9B:CB8B queues its release pose the same way, so the hit interruption
            // earlier this frame still saw the grappling movement type.
            if (!animationTransitionApplied && _player.LastGrappleMovement is { PendingReleasePose: SamusPoseId releasePose })
            {
                SamusGrappleMovement.ApplyReleasePose(_addressSpace, samus, releasePose);
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                _player.ProspectiveSamusWallCollisionPose = null;
                animationTransitionApplied = true;
            }

            // Crash movement queues a transitional pose; it must not replace the
            // body seen by AnimateSamus earlier in this frame. Command-three
            // animation transitions retain their higher cartridge priority.
            if (!animationTransitionApplied && _player.LastShinesparkMovement is { CrashSequenceFinished: true })
            {
                SamusShinesparkState.ApplyCrashFinishPose(_addressSpace, samus);
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                _player.ProspectiveSamusWallCollisionPose = null;
                animationTransitionApplied = true;
            }

            // Expiry still permits this frame's movement and animation. Its
            // transitional slot wins over the ordinary input selected in alpha,
            // even when a damage boost has already restored normal movement.
            // With an animation command three already pending, native changes it
            // to command eight: keep the animation pose and ALSO clear hurt state.
            if (hitInterruptionExpiryPending)
            {
                SamusKnockbackMovement.FinishExpiredHitInterruption(_addressSpace, samus);
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                _player.ProspectiveSamusWallCollisionPose = null;
                animationTransitionApplied = true;
            }

            // `$90:DDE9` runs after AnimateSamus and before UpdateSamusPose. Terrain
            // spike reactions only publish `$18A8/$18AA/$0A54`; this common interruption
            // converts that request into the same hurt pose and special movement handler
            // used by every cartridge damage producer. Animation command three retains
            // its higher UpdateSamusPose priority and therefore suppresses this branch.
            if (!animationTransitionApplied &&
                SamusKnockbackMovement.TryStartPendingHitInterruption(
                    _addressSpace,
                    samus,
                    _controller1.Current,
                    _freeze.TimeIsFrozen,
                    _room.LevelData,
                    _nmi.NmiFrameCounter,
                    _plms))
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                _player.ProspectiveSamusWallCollisionPose = null;
                animationTransitionApplied = true;
            }

            // Bomb overlap is published between alpha and beta, but the native
            // interruption dispatcher considers it only AFTER movement/animation.
            // Active hurt words retain the request without arming or rejecting it;
            // expiry owns this frame even after clearing those words. This permits
            // a later bomb launch to retain the damaged pose, with input locked by
            // special command three rather than an early speculative pose change.
            if (!animationTransitionApplied && samus.KnockbackTimer.IsZero &&
                samus.KnockbackDirection == 0 &&
                samus.PoseMetadata.TrySetupPublishedBombJump(
                    _addressSpace,
                    _room.LevelData ?? throw new InvalidOperationException(
                        "A published bomb-jump direction requires active room level data."),
                    _freeze.TimeIsFrozen,
                    _nmi.NmiFrameCounter,
                    _controller1.NewlyPressed,
                    _plms))
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                _player.ProspectiveSamusWallCollisionPose = null;
                animationTransitionApplied = true;
            }

            // Windup expiry installs vertical movement immediately, but publishes
            // an interrupted pose for this later phase. AnimateSamus must still see
            // the windup body; otherwise the launch animation loses its first tick.
            if (!animationTransitionApplied &&
                _player.LastShinesparkMovement is { PendingLaunchPose: SamusPoseId launchPose })
            {
                SamusShinesparkState.ApplyDirectionalLaunchPose(_addressSpace, samus, launchPose);
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                _player.ProspectiveSamusWallCollisionPose = null;
                animationTransitionApplied = true;
            }

            if (samus.Xray.CommitPendingActivation(_addressSpace, samus, animationTransitionApplied))
            {
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                _player.ProspectiveSamusWallCollisionPose = null;
                animationTransitionApplied = true;
            }

            // Grapple connection publishes a bank-$91 special prospective command.
            // Super-special animation/hurt owners above can suppress it without
            // undoing the already-installed bank-$9B beam function. This is the
            // cartridge ordering used by bounded Grapple momentum-retention setups.
            if (!animationTransitionApplied &&
                _player.LastGrappleMovement is { PendingConnection: { } pendingConnection })
            {
                Position16<RoomPixels> grapplePrevious =
                    SamusGrappleMovement.ApplyPendingConnectionPose(
                        _addressSpace,
                        samus,
                        pendingConnection);
                frame.PreviousCameraPoint = frame.PreviousCameraPoint.WithWhole(
                    grapplePrevious);
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                _player.ProspectiveSamusWallCollisionPose = null;
                animationTransitionApplied = true;
            }

            // $91:E8E5 replaces the ordinary input target with the current pose
            // on ceiling collision. Apply the ball's deferred EFDF vertical
            // stop and consume this same-pose transition so history still shifts
            // and an alpha aim/turn selection cannot override it. Higher-priority
            // animation and hurt interruptions above retain their native priority.
            // Ball movement can resume before the bomb arc reaches a ceiling;
            // its collision must also outrank the stop-rolling input fallback.
            if (!animationTransitionApplied &&
                (_player.LastAerialSamusMovement is { HitCeiling: true } ||
                 _player.LastMorphBallMovement is { HitCeiling: true } ||
                 _player.LastBombJumpMovement is { Vertical.Collided: true }))
            {
                if (_player.LastMorphBallMovement is { HitCeiling: true })
                    samus.PoseTransitions.ApplySolidCeilingCollision();
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                animationTransitionApplied = true;
            }

            // $91:E8F2 overrides the input target when the bomb arc moved down through
            // its apex with no floor below; the airborne pose is installed this frame.
            if (!animationTransitionApplied &&
                _player.LastBombJumpMovement is { FellWithoutFloor: true })
            {
                samus.PoseTransitions.ApplyBombJumpFallingPose(_addressSpace);
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                animationTransitionApplied = true;
            }

            // `$91:EADE` runs inside UpdateSamusPose after beta movement/animation and
            // only when no super-special animation command has already won. Its first
            // branch consumes the X-speed-killed flag produced by CURRENT type-one
            // running movement. This creates a wall-stop pose even with no input match.
            // An unobstructed upward platform carry leaves the prospective input
            // pose intact too. Requiring a vertical collision here wrongly skipped
            // the native one-pixel run check while a Kamer was carrying Samus upward.
            // A special movement pointer can also retain a standing/running pose
            // (suit-interrupted windup). EADE does not require normal grounded beta.
            if (!animationTransitionApplied &&
                _player.LastGroundedSamusMovement is not
                    { Vertical.IsUnobstructedDownwardMovement: true })
            {
                bool currentRunHitWall =
                    (SamusPoseMetadata.IsRightFacingRunningPose(frame.PoseAtFrameStart) ||
                     SamusPoseMetadata.IsLeftFacingRunningPose(frame.PoseAtFrameStart)) &&
                    _player.LastGroundedSamusMovement is { Horizontal.Collided: true };
                SamusPoseId? prospectiveRunningPose =
                    _player.ProspectiveSamusPose is { } prospective
                        ? prospective.ProspectivePose
                        // Native alpha writes the same prospective-pose slot for a
                        // table match and a no-input fallback. A decelerating run
                        // retains its running pose and must still take EADE's real
                        // one-pixel forward move; omitting it shortens release drift.
                        : _player.ProspectiveSamusFallbackPose;
                _player.ProspectiveSamusWallCollisionPose =
                    samus.PoseTransitions.CheckProspectiveRunningPoseForWall(
                        _addressSpace,
                        _room.LevelData ?? throw new InvalidOperationException(
                            "Ran-into-wall probe requires active room level data."),
                        prospectiveRunningPose,
                        currentRunHitWall,
                        out BlockMoveResult? onePixelProbe,
                        _plms);
                _player.LastRanIntoWallProbe = onePixelProbe;
            }

            if (!animationTransitionApplied &&
                _player.ProspectiveSamusWallCollisionPose is { } wallCollisionPose)
            {
                // The selector result replaces the ordinary input target. Applying it
                // here also covers the no-input “running speed was killed” branch.
                samus.PoseTransitions.ApplyRanIntoWallPoseChange(_addressSpace, wallCollisionPose);
                animationTransitionApplied = true;
                // `$91:EADE` replaces only ProspectivePose. When alpha's table lookup
                // failed, UpdateSamusPose ($91:EBF0) still runs the command `$91:8304`
                // selected for the pre-wall movement type.
                if (_player.ProspectiveSamusPose is null && _player.ProspectiveSamusFallbackPose is not null)
                {
                    SamusProspectivePoseChangeCommand command = SamusPoseChangeCommandDefinitions.ForLookupFailure(
                        SamusPoseMetadata.ReadMovementType(_addressSpace, frame.PoseAtFrameStart));
                    if (command is not (SamusProspectivePoseChangeCommand.Decelerate or SamusProspectivePoseChangeCommand.Stop))
                        throw new InvalidDataException(
                            $"Ran-into-wall source ${(int)frame.PoseAtFrameStart:X2} selected unexpected lookup-failure command {command}.");
                    samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                        command == SamusProspectivePoseChangeCommand.Decelerate && frame.DeceleratingFallbackHasMomentum,
                        samus.PoseMetadata.ReadFacingDirection(_addressSpace));
                }
            }

            // Morph landing has its own prospective-pose and collision tables. A hard
            // impact retains `$31/$32` and launches bounce one; the next collision
            // launches bounce two; only a gentle/second-bounce collision installs
            // grounded `$1D/$41`. Treat every branch as a consumed transition so input
            // selected before collision cannot override the cartridge's bounce result.
            // The ROM collision table ignores ground-family type four, even if hurt
            // movement left that pose with nonzero Y speed. Do not infer an airborne
            // landing dispatcher from velocity alone.
            if (!animationTransitionApplied &&
                _player.LastMorphBallMovement is { Landed: true } &&
                SamusPoseMetadata.IsAirborneMorphBallPose(frame.PoseAtFrameStart))
            {
                samus.PoseTransitions.ApplyMorphBallLanding(_addressSpace);
                animationTransitionApplied = true;
            }

            // `$90:9D35` publishes solid-vertical collision result five and returns
            // carry set, preventing vertical motion. The matching bank-$91 command
            // installs `$83/$84`, clears old momentum, and launches from ROM constants.
            if (!animationTransitionApplied &&
                _player.LastAerialSamusMovement is { WallJumpTriggered: true })
            {
                samus.PoseTransitions.ApplyWallJumpTrigger(_addressSpace, _room.LevelData, _nmi.NmiFrameCounter, _plms);
                animationTransitionApplied = true;
            }

            // Spring Ball's three movement families share collision result three, but
            // `$91:F25E` makes held Jump an immediate relaunch and stores `$0601/$0602`
            // during automatic rebounds. Keep it distinct from ordinary-ball state.
            // Ground-family type $11 likewise has the table's no-transition entry.
            if (!animationTransitionApplied &&
                _player.LastMorphBallMovement is { Landed: true } &&
                SamusPoseMetadata.IsAirborneSpringBallPose(frame.PoseAtFrameStart))
            {
                samus.PoseTransitions.ApplySpringBallLanding(_addressSpace, _controller1.Current);
                animationTransitionApplied = true;
            }

            // A downward collision publishes result one. $91:E95D chooses normal or
            // spin landing from the movement type that executed this frame, then pose
            // command five clears both velocity axes. This ordinary prospective pose
            // is lower priority than an animation command-three transition above.
            if (!animationTransitionApplied &&
                _player.LastAerialSamusMovement is { Landed: true } &&
                !SamusPoseMetadata.IsAerialTurnPose(frame.PoseAtFrameStart))
            {
                if (SamusPoseMetadata.IsCompactAerialPose(frame.PoseAtFrameStart))
                {
                    samus.PoseTransitions.TryApplyCompactAerialLanding(
                        _addressSpace,
                        _room.LevelData ?? throw new InvalidOperationException(
                            "Compact landing requires active room level data."),
                        _nmi.NmiFrameCounter,
                        _plms);
                }
                else
                {
                    bool wasSpinning = SamusPoseMetadata.IsSpinJumpPose(frame.PoseAtFrameStart) ||
                        SamusPoseMetadata.IsWallJumpPose(frame.PoseAtFrameStart);
                    samus.PoseTransitions.TryApplyAerialLanding(
                        _addressSpace,
                        _room.LevelData ?? throw new InvalidOperationException(
                            "Aerial landing requires active room level data."),
                        wasSpinning,
                        _controller1.Current,
                        _nmi.NmiFrameCounter,
                        _plms);
                }
                animationTransitionApplied = true;
            }

            // An unobstructed DOWNWARD grounding probe publishes result two. An
            // unobstructed upward platform carry publishes result zero at `$90:E606`
            // and must leave the pose alone; treating every non-collision as result two
            // made rising Kamers repeatedly alternate falling and landing. For admitted
            // standing (including landing art), running, and stable crouching types,
            // data zero at `$90:E65A`
            // selects airborne family zero and `$91:E8F2` chooses falling art.
            // Turn types `$0E/$17` contain `$04` (no pose change) in that literal table;
            // they finish `$F8`, then the destination pose detects the missing floor.
            if (!animationTransitionApplied &&
                _player.LastGroundedSamusMovement is
                    { Vertical.IsUnobstructedDownwardMovement: true } &&
                (SamusPoseMetadata.IsRightFacingStandingPose(frame.PoseAtBetaMovement) ||
                 SamusPoseMetadata.IsLeftFacingStandingPose(frame.PoseAtBetaMovement) ||
                 SamusPoseMetadata.IsRightFacingLandingPose(frame.PoseAtBetaMovement) ||
                 SamusPoseMetadata.IsLeftFacingLandingPose(frame.PoseAtBetaMovement) ||
                 SamusPoseMetadata.IsRightFacingRunningPose(frame.PoseAtBetaMovement) ||
                 SamusPoseMetadata.IsLeftFacingRunningPose(frame.PoseAtBetaMovement) ||
                 SamusPoseMetadata.IsMoonwalkingPose(frame.PoseAtBetaMovement) ||
                 frame.PoseAtBetaMovement is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose ||
                 SamusPoseMetadata.IsRanIntoWallPose(frame.PoseAtBetaMovement) ||
                 SamusPoseMetadata.IsRightFacingCrouchingPose(frame.PoseAtBetaMovement) ||
                 SamusPoseMetadata.IsLeftFacingCrouchingPose(frame.PoseAtBetaMovement)))
            {
                SamusPoseId fallingPose = samus.PoseTransitions.SelectFallingPoseForCurrentAim(_addressSpace);
                samus.PoseTransitions.ApplyWalkedOffFloorTransition(_addressSpace,
                    _room.LevelData ?? throw new InvalidOperationException("Walk-off requires room collision data."),
                    fallingPose, _nmi.NmiFrameCounter, _plms);
                animationTransitionApplied = true;
            }

            // Movement type four publishes ordinary airborne `$31/$32` when its
            // no-speed grounding probe finds no floor. The shared rolling animation is
            // preserved, while Y speed starts at zero/down exactly like `$91:E8F2`.
            if (!animationTransitionApplied &&
                _player.LastMorphBallMovement is
                    { Vertical.IsUnobstructedDownwardMovement: true } &&
                (SamusPoseMetadata.IsGroundedMorphBallPose(frame.PoseAtFrameStart) ||
                 SamusPoseMetadata.IsGroundedSpringBallPose(frame.PoseAtFrameStart)))
            {
                samus.PoseTransitions.ApplyMorphBallWalkOff(_addressSpace);
                animationTransitionApplied = true;
            }

            if (!animationTransitionApplied && _player.ProspectiveSamusPose is { } inputTransition)
            {
                SamusPoseId targetPose = unchecked(inputTransition.ProspectivePose);

                // Self-transitions in the turn tables mean “remain in this animation.”
                // Reinitializing frame zero would prevent $F8 from ever being reached.
                if (targetPose != frame.PoseAtFrameStart)
                {
                    switch ((frame.PoseAtFrameStart, targetPose))
                    {
                        case var (source, target)
                            when SamusPoseMetadata.IsDraygonGrabbedPose(source) &&
                                 SamusPoseMetadata.IsDraygonGrabbedPose(target):
                            // Both ROM tables stay within one facing. The helper makes
                            // that invariant explicit and initializes the target's real
                            // `$91:B2B4/$B53C` animation list.
                            samus.PoseMetadata.ApplyDraygonGrabbedPoseChange(
                                _addressSpace,
                                targetPose);
                            break;
                        case var (source, target)
                            when (((SamusPoseMetadata.IsRightFacingStandingPose(source) ||
                                    SamusPoseMetadata.IsRightFacingRanIntoWallPose(source) ||
                                    SamusPoseMetadata.IsRightFacingLandingPose(source)) &&
                                   SamusPoseMetadata.IsMoonwalkingFacingRightPose(target)) ||
                                  ((SamusPoseMetadata.IsLeftFacingStandingPose(source) ||
                                    SamusPoseMetadata.IsLeftFacingRanIntoWallPose(source) ||
                                    SamusPoseMetadata.IsLeftFacingLandingPose(source)) &&
                                   SamusPoseMetadata.IsMoonwalkingFacingLeftPose(target)) ||
                                  (SamusPoseMetadata.IsMoonwalkingPose(source) &&
                                   (SamusPoseMetadata.IsMoonwalkingPose(target) ||
                                    target is SamusPoseId.MovingRightNormalPose or
                                        SamusPoseId.MovingLeftNormalPose))):
                            samus.PoseTransitions.ApplyMoonwalkPoseChange(
                                _addressSpace,
                                targetPose,
                                _options.MoonwalkEnabled);
                            break;
                        case var (source, target)
                            when SamusPoseMetadata.IsMoonwalkingPose(source) &&
                                 SamusPoseMetadata.IsMoonwalkTurnJumpPose(target):
                            // The stable `$49/$4A/$75-$78` table publishes a generic
                            // reversal target. `$91:F8D3` validates it against the old
                            // shot direction, folds momentum, and leaves `$BF-$C4`
                            // grounded until either its input table or `$F8` starts jump.
                            samus.PoseTransitions.ApplyMoonwalkTurnJump(_addressSpace, targetPose);
                            break;
                        case var (source, target)
                            when SamusPoseMetadata.IsMoonwalkTurnJumpLeftPose(source) &&
                                     target is SamusPoseId.SpinJumpLeftPose or
                                         SamusPoseId.NeutralJumpTransitionLeftPose ||
                                 SamusPoseMetadata.IsMoonwalkTurnJumpRightPose(source) &&
                                     target is SamusPoseId.SpinJumpRightPose or
                                         SamusPoseId.NeutralJumpTransitionRightPose:
                            samus.PoseTransitions.ApplyOrdinaryJumpTransition(
                                _addressSpace,
                                targetPose,
                                _controller1.NewlyPressed);
                            break;
                        case (SamusPoseId.KnockbackRightPose, SamusPoseId.DamageBoostRightPose):
                        case (SamusPoseId.KnockbackLeftPose, SamusPoseId.DamageBoostLeftPose):
                            // The accepted pose restores normal movement while
                            // preserving the hurt arc and remaining timer.
                            SamusKnockbackMovement.ApplyDamageBoostTransition(
                                _addressSpace,
                                samus,
                                frame.PoseAtFrameStart,
                                targetPose);
                            break;
                        case (SamusPoseId.DamageBoostRightPose,
                              SamusPoseId.NeutralJumpRightPose or SamusPoseId.NormalJumpForwardRightPose):
                        case (SamusPoseId.DamageBoostLeftPose,
                              SamusPoseId.NeutralJumpLeftPose or SamusPoseId.NormalJumpForwardLeftPose):
                            SamusKnockbackMovement.ApplyDamageBoostPoseTransition(
                                _addressSpace,
                                samus,
                                targetPose,
                                _controller1.NewlyPressed);
                            break;
                        case var (source, target)
                            when ((SamusPoseMetadata.IsRightFacingNormalJumpPose(source) &&
                                   target == SamusPoseId.TurningRightToLeftJumpPose) ||
                                  (SamusPoseMetadata.IsLeftFacingNormalJumpPose(source) &&
                                   target == SamusPoseId.TurningLeftToRightJumpPose) ||
                                  (SamusPoseMetadata.IsRightFacingFallingPose(source) &&
                                   target == SamusPoseId.TurningRightToLeftFallingPose) ||
                                  (SamusPoseMetadata.IsLeftFacingFallingPose(source) &&
                                   target == SamusPoseId.TurningLeftToRightFallingPose)):
                            // `$2F/$30/$87/$88` are generic table outputs. The helper
                            // reads the source shot-direction record, chooses the exact
                            // `$8F-$A1` art when needed, folds momentum, and runs compact
                            // pose-expansion collision before committing the turn.
                            samus.PoseTransitions.TryApplyAerialTurn(
                                _addressSpace,
                                _room.LevelData ?? throw new InvalidOperationException(
                                    "Aerial turn requires active room level data."),
                                target,
                                _nmi.NmiFrameCounter,
                                _plms);
                            break;
                        case var (source, target)
                            when (SamusPoseMetadata.IsSpinJumpPose(source) ||
                                  SamusPoseMetadata.IsWallJumpPose(source)) &&
                                 SamusPoseMetadata.IsSpinJumpPose(target):
                            samus.PoseTransitions.ApplySpinJumpDirectionTransition(_addressSpace, targetPose);
                            break;
                        case var (source, target)
                            when SamusPoseMetadata.IsStableBallPose(source) &&
                                 SamusPoseMetadata.IsStableBallPose(target) &&
                                 !(SamusPoseMetadata.IsGroundedSpringBallPose(source) &&
                                   target is SamusPoseId.SpringBallJumpRightPose or
                                       SamusPoseId.SpringBallJumpLeftPose):
                            // `$1D/$1E/$1F/$31/$32/$41` all share delay list `$B378`.
                            // The initializer preserves frame/timer and applies mode-one
                            // reversal momentum only when direction actually changes.
                            samus.PoseTransitions.ApplyMorphBallPoseChange(_addressSpace, target);
                            break;
                        case var (source, target)
                            when SamusPoseMetadata.IsGroundedSpringBallPose(source) &&
                                 target is SamusPoseId.SpringBallJumpRightPose or
                                     SamusPoseId.SpringBallJumpLeftPose:
                            samus.PoseTransitions.ApplySpringBallJump(_addressSpace, target);
                            break;
                        case var (source, target)
                            when ((SamusPoseMetadata.ReadPoseXDirection(_addressSpace, source) == 8 &&
                                   target == SamusPoseId.MorphingTransitionRightPose) ||
                                  (SamusPoseMetadata.ReadPoseXDirection(_addressSpace, source) == 4 &&
                                   target == SamusPoseId.MorphingTransitionLeftPose) ||
                                  (SamusPoseMetadata.IsStableBallPose(source) &&
                                   target is SamusPoseId.UnmorphingTransitionRightPose or
                                       SamusPoseId.UnmorphingTransitionLeftPose)):
                            samus.PoseTransitions.TryApplyMorphTransition(
                                _addressSpace,
                                _room.LevelData ?? throw new InvalidOperationException(
                                    "Morph transition requires active room level data."),
                                targetPose,
                                _nmi.NmiFrameCounter,
                                _plms);
                            break;
                        case var (source, target)
                            when (SamusPoseMetadata.IsCompactAerialPose(source) ||
                                  SamusPoseMetadata.IsCompactAerialPose(target)) &&
                                 ((SamusPoseMetadata.IsRightFacingNormalJumpPose(source) &&
                                   SamusPoseMetadata.IsRightFacingNormalJumpPose(target)) ||
                                  (SamusPoseMetadata.IsLeftFacingNormalJumpPose(source) &&
                                   SamusPoseMetadata.IsLeftFacingNormalJumpPose(target)) ||
                                  (SamusPoseMetadata.IsRightFacingFallingPose(source) &&
                                   SamusPoseMetadata.IsRightFacingFallingPose(target)) ||
                                  (SamusPoseMetadata.IsLeftFacingFallingPose(source) &&
                                   SamusPoseMetadata.IsLeftFacingFallingPose(target))):
                            samus.PoseTransitions.TryApplyCompactAerialTransition(
                                _addressSpace,
                                _room.LevelData ?? throw new InvalidOperationException(
                                    "Compact aerial transition requires active room level data."),
                                targetPose,
                                _nmi.NmiFrameCounter,
                                _plms);
                            break;
                        case var (source, target)
                            when (SamusPoseMetadata.IsGroundedAimPose(source) ||
                                  SamusPoseMetadata.IsGroundedAimPose(target) ||
                                  SamusPoseMetadata.IsGunExtendedPose(source) ||
                                  SamusPoseMetadata.IsGunExtendedPose(target)) &&
                                 (((SamusPoseMetadata.IsRightFacingStandingPose(source) ||
                                    SamusPoseMetadata.IsRightFacingRunningPose(source) ||
                                    SamusPoseMetadata.IsRightFacingRanIntoWallPose(source) ||
                                    SamusPoseMetadata.IsRightFacingLandingPose(source)) &&
                                   (SamusPoseMetadata.IsRightFacingStandingPose(target) ||
                                    SamusPoseMetadata.IsRightFacingRunningPose(target) ||
                                    SamusPoseMetadata.IsRightFacingRanIntoWallPose(target))) ||
                                  ((SamusPoseMetadata.IsLeftFacingStandingPose(source) ||
                                    SamusPoseMetadata.IsLeftFacingRunningPose(source) ||
                                    SamusPoseMetadata.IsLeftFacingRanIntoWallPose(source) ||
                                    SamusPoseMetadata.IsLeftFacingLandingPose(source)) &&
                                   (SamusPoseMetadata.IsLeftFacingStandingPose(target) ||
                                    SamusPoseMetadata.IsLeftFacingRunningPose(target) ||
                                    SamusPoseMetadata.IsLeftFacingRanIntoWallPose(target))) ||
                                  (SamusPoseMetadata.IsRightFacingCrouchingPose(source) &&
                                   SamusPoseMetadata.IsRightFacingCrouchingPose(target)) ||
                                  (SamusPoseMetadata.IsLeftFacingCrouchingPose(source) &&
                                   SamusPoseMetadata.IsLeftFacingCrouchingPose(target))):
                            samus.PoseTransitions.ApplyGroundedAimTransition(_addressSpace, targetPose);
                            break;
                        case var (source, target)
                            when SamusPoseMetadata.IsSameFacingAerialAimFireOrForwardTransition(
                                source,
                                target):
                            samus.PoseTransitions.ApplyAerialAimTransition(_addressSpace, targetPose);
                            break;
                        case (SamusPoseId.FacingRightNormalPose, SamusPoseId.MovingRightNormalPose):
                            samus.PoseTransitions.ApplyStandingRightToRunningRight(_addressSpace);
                            break;
                        case (SamusPoseId.FacingLeftNormalPose, SamusPoseId.MovingLeftNormalPose):
                            samus.PoseTransitions.ApplyStandingLeftToRunningLeft(_addressSpace);
                            break;
                        case var (source, target)
                            when SamusPoseMetadata.IsRanIntoWallPose(source) &&
                                 (SamusPoseMetadata.IsRightFacingRunningPose(target) ||
                                  SamusPoseMetadata.IsLeftFacingRunningPose(target)):
                            samus.PoseTransitions.ApplyRanIntoWallToRunning(_addressSpace, target);
                            break;
                        case var (forwardSource, forwardTarget)
                            when SamusPoseMetadata.IsForwardFacingPose(forwardSource) &&
                                 forwardTarget is
                                     SamusPoseId.TurningRightToLeftPose or
                                     SamusPoseId.TurningLeftToRightPose:
                            // Ceres releases controls while `$00` is still active.
                            // Its own ROM transition table maps Left/Right to generic
                            // `$25/$26`; `$91:F8D3` recognizes old pose `$00/$9B` and
                            // preserves that target without inventing a source facing.
                            samus.PoseTransitions.ApplyGroundedTurn(_addressSpace, targetPose, _controller1.Current);
                            break;
                        case var (rightSource, rightTarget)
                            when rightTarget is
                                     SamusPoseId.TurningRightToLeftPose or
                                     SamusPoseId.TurningRightToLeftCrouchingPose &&
                                 (SamusPoseMetadata.IsRightFacingStandingPose(rightSource) ||
                                  SamusPoseMetadata.IsRightFacingRunningPose(rightSource) ||
                                  SamusPoseMetadata.IsMoonwalkingFacingRightPose(rightSource) ||
                                  SamusPoseMetadata.IsRightFacingRanIntoWallPose(rightSource) ||
                                  SamusPoseMetadata.IsRightFacingCrouchingPose(rightSource) ||
                                  SamusPoseMetadata.IsRightFacingLandingPose(rightSource)):
                        case var (leftSource, leftTarget)
                            when leftTarget is
                                     SamusPoseId.TurningLeftToRightPose or
                                     SamusPoseId.TurningLeftToRightCrouchingPose &&
                                 (SamusPoseMetadata.IsLeftFacingStandingPose(leftSource) ||
                                  SamusPoseMetadata.IsLeftFacingRunningPose(leftSource) ||
                                  SamusPoseMetadata.IsMoonwalkingFacingLeftPose(leftSource) ||
                                  SamusPoseMetadata.IsLeftFacingRanIntoWallPose(leftSource) ||
                                  SamusPoseMetadata.IsLeftFacingCrouchingPose(leftSource) ||
                                  SamusPoseMetadata.IsLeftFacingLandingPose(leftSource)):
                            samus.PoseTransitions.ApplyGroundedTurn(_addressSpace, targetPose, _controller1.Current);
                            break;
                        case var (source, target)
                            when ((SamusPoseMetadata.IsRightFacingStandingPose(source) ||
                                   SamusPoseMetadata.IsRightFacingRanIntoWallPose(source)) &&
                                  SamusPoseMetadata.IsRightFacingNormalJumpPose(target) &&
                                  target is
                                      SamusPoseId.NeutralJumpTransitionRightPose or
                                      SamusPoseId.NormalJumpTransitionAimUpRightPose or
                                      SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
                                      SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose) ||
                                 ((SamusPoseMetadata.IsLeftFacingStandingPose(source) ||
                                   SamusPoseMetadata.IsLeftFacingRanIntoWallPose(source)) &&
                                  SamusPoseMetadata.IsLeftFacingNormalJumpPose(target) &&
                                  target is
                                      SamusPoseId.NeutralJumpTransitionLeftPose or
                                      SamusPoseId.NormalJumpTransitionAimUpLeftPose or
                                      SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
                                      SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose) ||
                                 SamusPoseMetadata.IsLandingToNormalJumpTransition(source, target):
                            // All `$A4-$A7/$E0-$E7` landing records expose the same
                            // standing input table while their brief streams are active.
                            // A fresh Jump edge can therefore preserve any authored
                            // normal-jump aim target, even from horizontal-fire `$E6/$E7`
                            // before `$F8` fallback.
                        case (SamusPoseId.MovingRightNormalPose,
                              SamusPoseId.SpinJumpRightPose):
                        case (SamusPoseId.MovingRightGunExtendedPose,
                              SamusPoseId.SpinJumpRightPose):
                        case (SamusPoseId.RunningAimUpRightPose or
                              SamusPoseId.RunningAimDiagonalUpRightPose or
                              SamusPoseId.RunningAimDiagonalDownRightPose,
                              SamusPoseId.SpinJumpRightPose):
                        case (SamusPoseId.MovingLeftNormalPose,
                              SamusPoseId.SpinJumpLeftPose):
                        case (SamusPoseId.MovingLeftGunExtendedPose,
                              SamusPoseId.SpinJumpLeftPose):
                        case (SamusPoseId.RunningAimUpLeftPose or
                              SamusPoseId.RunningAimDiagonalUpLeftPose or
                              SamusPoseId.RunningAimDiagonalDownLeftPose,
                              SamusPoseId.SpinJumpLeftPose):
                        case var (turnSourcePose, turnTargetPose)
                            when SamusPoseMetadata.IsStandingGroundTurnToJumpTransition(
                                turnSourcePose,
                                turnTargetPose):
                            // Every standing `$0E` turn—including the aimed `$8B-$8E`
                            // and `$9C/$9D` records—continues consulting `$91:8142`.
                            // That table can interrupt the art with either same-facing
                            // spin or neutral jump. Preserve newly pressed Shoot as well:
                            // the normal-jump initializer owns the simultaneous
                            // Jump+Fire bridge when the selected body is type two.
                            samus.PoseTransitions.ApplyOrdinaryJumpTransition(
                                _addressSpace,
                                targetPose,
                                _controller1.NewlyPressed);
                            break;
                        case var (source, target)
                            when ((SamusPoseMetadata.IsRightFacingCrouchingPose(source) &&
                                   target == SamusPoseId.NeutralJumpTransitionRightPose) ||
                                  (SamusPoseMetadata.IsLeftFacingCrouchingPose(source) &&
                                   target == SamusPoseId.NeutralJumpTransitionLeftPose)):
                            // `$91:A66C/$91:A6BC` use the ordinary `$4B/$4C` transition
                            // art for every crouched aim direction. The helper preserves
                            // `$91:FC7D`'s literal `$27/$28`-only ten-pixel adjustment.
                            samus.PoseTransitions.TryApplyCrouchJumpTransition(
                                _addressSpace,
                                _room.LevelData ?? throw new InvalidOperationException(
                                    "Crouch jump requires active room level data."),
                                targetPose,
                                _nmi.NmiFrameCounter,
                                _plms);
                            break;
                        case var (source, target)
                            when (SamusPoseMetadata.IsSpinJumpPose(source) ||
                                  SamusPoseMetadata.IsWallJumpPose(source)) &&
                                 (SamusPoseMetadata.IsRightFacingNormalJumpPose(target) ||
                                  SamusPoseMetadata.IsLeftFacingNormalJumpPose(target)):
                            // Aim and Fire cancel both compact spin and `$83/$84` wall-
                            // jump bodies through the same normal-jump initializer without
                            // restarting the jump arc. `$91:A9EC`, for example, maps Shot
                            // directly from `$83` to `$13`. The shared helper performs
                            // changed-pose collision for every radius expansion; a rejected
                            // body leaves the compact source pose installed.
                            samus.PoseTransitions.TryApplySpinOrWallJumpToNormalJumpTransition(
                                _addressSpace,
                                _room.LevelData ?? throw new InvalidOperationException(
                                    "Spin-to-normal-jump transition requires active room level data."),
                                targetPose,
                                _nmi.NmiFrameCounter,
                                _controller1.NewlyPressed,
                                _plms);
                            break;
                        case var (source, target)
                            when (source == SamusPoseId.UnmorphingTransitionRightPose &&
                                  SamusPoseMetadata.IsRightFacingFallingPose(target)) ||
                                 (source == SamusPoseId.UnmorphingTransitionLeftPose &&
                                  SamusPoseMetadata.IsLeftFacingFallingPose(target)):
                            // Route an animation/collision-selected falling body through
                            // ordinary expansion collision while preserving the fall.
                            // Posture transitions do not poll controller pose tables:
                            // the type-$0F input handler returns before lookup.
                            samus.PoseTransitions.TryApplyUnmorphToFallingTransition(
                                _addressSpace,
                                _room.LevelData ?? throw new InvalidOperationException(
                                    "Unmorph-to-falling transition requires active room level data."),
                                targetPose,
                                _nmi.NmiFrameCounter,
                                _plms);
                            break;
                        case var (source, target)
                            when ((SamusPoseMetadata.IsRightFacingCrouchingPose(source) &&
                                   target == SamusPoseId.FacingRightNormalPose) ||
                                  (SamusPoseMetadata.IsLeftFacingCrouchingPose(source) &&
                                   target == SamusPoseId.FacingLeftNormalPose)):
                            // These direct `$01/$02` records bypass the animated
                            // `$F7-$FC` stand-up family, but still run pose-expansion
                            // collision before installing the final standing body.
                            samus.PoseTransitions.TryApplyDirectCrouchToStandingTransition(
                                _addressSpace,
                                _room.LevelData ?? throw new InvalidOperationException(
                                    "Direct crouch exit requires active room level data."),
                                targetPose,
                                _nmi.NmiFrameCounter,
                                _plms);
                            break;
                        case var (landingSource, landingTarget)
                            when (landingTarget == SamusPoseId.MovingRightNormalPose &&
                                  SamusPoseMetadata.IsRightFacingLandingPose(landingSource)) ||
                                 (landingTarget == SamusPoseId.MovingLeftNormalPose &&
                                  SamusPoseMetadata.IsLeftFacingLandingPose(landingSource)):
                            samus.PoseTransitions.ApplyLandingToRunning(_addressSpace, targetPose);
                            break;
                        case var (source, postureTarget)
                            when SamusPoseMetadata.IsCrouchStandTransitionPose(postureTarget) &&
                                 (SamusPoseMetadata.IsRightFacingStandingPose(source) ||
                                  SamusPoseMetadata.IsLeftFacingStandingPose(source) ||
                                  SamusPoseMetadata.IsRightFacingRunningPose(source) ||
                                  SamusPoseMetadata.IsLeftFacingRunningPose(source) ||
                                  SamusPoseMetadata.IsMoonwalkingPose(source) ||
                                  SamusPoseMetadata.IsRanIntoWallPose(source) ||
                                  SamusPoseMetadata.IsRightFacingLandingPose(source) ||
                                  SamusPoseMetadata.IsLeftFacingLandingPose(source) ||
                                  SamusPoseMetadata.IsRightFacingCrouchingPose(source) ||
                                  SamusPoseMetadata.IsLeftFacingCrouchingPose(source)):
                            samus.PoseTransitions.TryApplyPostureTransition(
                                _addressSpace,
                                _room.LevelData ?? throw new InvalidOperationException(
                                    "Posture transition requires active room level data."),
                                targetPose,
                                _nmi.NmiFrameCounter,
                                _plms);
                            break;
                        case var (source, sparkTarget)
                            when source is
                                     SamusPoseId.ShinesparkWindupRightPose or
                                     SamusPoseId.ShinesparkWindupLeftPose &&
                                 sparkTarget is
                                     SamusPoseId.ShinesparkHorizontalRightPose or
                                     SamusPoseId.ShinesparkHorizontalLeftPose or
                                     SamusPoseId.ShinesparkVerticalRightPose or
                                     SamusPoseId.ShinesparkVerticalLeftPose or
                                     SamusPoseId.ShinesparkDiagonalRightPose or
                                     SamusPoseId.ShinesparkDiagonalLeftPose:
                            // `$91:AD6C/$AD80` are ordinary held/new input records, but
                            // `$91:F80F` installs a special movement pointer instead of
                            // invoking a normal movement-type initializer.
                            samus.PoseTransitions.ApplyShinesparkDirectionTransition(
                                _addressSpace,
                                targetPose);
                            break;
                        default:
                            // MOVEMENT_COVERAGE exhaustively audits every active retail
                            // transition pair entering this dispatcher. A ROM match not
                            // owned by one of the explicit families above means the pose
                            // table or state was mixed from an incompatible revision.
                            throw new InvalidDataException(
                                $"Grounded input transition ${(int)frame.PoseAtFrameStart:X2} -> " +
                                $"${(int)targetPose:X2} escaped the exhaustive retail dispatcher.");
                    }
                }
            }
            else if (!animationTransitionApplied &&
                     frame.PoseAtFrameStart is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose &&
                     _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
            {
                // The retained hurt pose still executes prospective command two.
                // Reset deceleration and cancel the momentum flag after movement.
                // Extra speed survives this command, but the next normal mover
                // consumes the cancellation through HandleExtraRunSpeed.
                samus.HorizontalSpeed.AccelerationMode = 0;
                samus.HorizontalSpeed.CancelRunningMomentum(samus.PoseMetadata.ReadFacingDirection(_addressSpace));
            }
            else if (!animationTransitionApplied &&
                     frame.PoseAtFrameStart is SamusPoseId.DamageBoostRightPose or SamusPoseId.DamageBoostLeftPose &&
                     _player.ProspectiveSamusFallbackPose is { } boostFallback &&
                     boostFallback != frame.PoseAtFrameStart)
            {
                // Releasing every boost chord reaches definition byte two, `$4D/$4E`:
                // the same initializer as the table's held-Jump exit.
                SamusKnockbackMovement.ApplyDamageBoostPoseTransition(
                    _addressSpace,
                    samus,
                    unchecked(boostFallback),
                    _controller1.NewlyPressed);
            }
            else if (!animationTransitionApplied &&
                     SamusPoseMetadata.IsDraygonGrabbedPose(frame.PoseAtFrameStart) &&
                     _player.ProspectiveSamusFallbackPose is { } draygonFallback)
            {
                // Zero input uses pose-definition byte two, returning aimed/firing/
                // struggling art to neutral `$BA/$EC` without touching owner placement.
                samus.PoseMetadata.ApplyDraygonGrabbedPoseChange(
                    _addressSpace,
                    unchecked(draygonFallback));
            }
            else if (!animationTransitionApplied &&
                     SamusPoseMetadata.IsWallJumpPose(frame.PoseAtFrameStart) &&
                     _player.ProspectiveSamusFallbackPose is
                         SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose)
            {
                // Definition byte two leaves the launch animation for ordinary spin
                // art. Type $14 selects command six in $91:8304. HandleSamusPoseChange
                // ($91:F426) sets carry only when InitializeSamusPose itself replaces the
                // installed pose; then UpdateSamusPose ($91:EBEE) skips the command. So
                // a spin jump keeps its launch speed only when initialization promotes it
                // to Space Jump or Screw Attack; otherwise EC85 clears base/extra speed.
                SamusPoseId spinFallback = unchecked(_player.ProspectiveSamusFallbackPose.Value);
                samus.PoseTransitions.ApplySpinJumpDirectionTransition(_addressSpace, spinFallback);
                if (samus.Pose == spinFallback)
                    samus.HorizontalSpeed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection(_addressSpace));
            }
            else if (!animationTransitionApplied &&
                     frame.PoseAtFrameStart is SamusPoseId.MorphBallFallingRightPose or SamusPoseId.MorphBallFallingLeftPose &&
                     _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
            {
                samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                    frame.DeceleratingFallbackHasMomentum, samus.PoseMetadata.ReadFacingDirection(_addressSpace));
            }
            else if (!animationTransitionApplied &&
                     frame.PoseAtFrameStart is SamusPoseId.SpringBallJumpRightPose or SamusPoseId.SpringBallJumpLeftPose or
                         SamusPoseId.SpringBallFallingRightPose or SamusPoseId.SpringBallFallingLeftPose &&
                     _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
            {
                // Spring Ball in air/falling (types $12/$13) select command six in
                // $91:8304. After this frame's movement, $91:EC85 clears mode and base
                // speed and falls through to $91:EC8E's boost/extra-speed cancel.
                samus.HorizontalSpeed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection(_addressSpace));
            }
            else if (!animationTransitionApplied &&
                     frame.PoseAtFrameStart is SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallGroundLeftPose or
                         SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose &&
                     _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
            {
                // $91:EC85 clears base/mode, then cancels extra running momentum.
                // Consume the same-pose command after this frame's movement.
                samus.HorizontalSpeed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection(_addressSpace));
            }
            else if (!animationTransitionApplied &&
                     (frame.PoseAtFrameStart is SamusPoseId.MorphBallMovingRightPose or
                         SamusPoseId.MorphBallMovingLeftPose or
                         SamusPoseId.SpringBallMovingRightPose or
                         SamusPoseId.SpringBallMovingLeftPose) &&
                     _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
            {
                // Prospective command one rechecks the post-movement base words. A
                // residue selects mode two deceleration; exact zero falls through to
                // command two's stopped mode and definition fallback on the next frame.
                samus.HorizontalSpeed.AccelerationMode =
                    samus.HorizontalSpeed.BaseFixed != 0 ? (ushort)2 : (ushort)0;
            }
            else if (!animationTransitionApplied &&
                     (frame.PoseAtFrameStart is SamusPoseId.MorphBallMovingRightPose or
                         SamusPoseId.MorphBallMovingLeftPose or
                         SamusPoseId.SpringBallMovingRightPose or
                         SamusPoseId.SpringBallMovingLeftPose) &&
                     _player.ProspectiveSamusFallbackPose is
                         SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallGroundLeftPose or
                         SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose)
            {
                // $91:EC85 command six clears base and extra momentum after the
                // current movement frame for both grounded ball movement types.
                samus.HorizontalSpeed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection(_addressSpace));
                samus.PoseTransitions.ApplyMorphBallPoseChange(
                    _addressSpace,
                    unchecked(_player.ProspectiveSamusFallbackPose.Value));
            }
            else if (!animationTransitionApplied &&
                     (SamusPoseMetadata.IsRightFacingRunningPose(frame.PoseAtFrameStart) ||
                      SamusPoseMetadata.IsLeftFacingRunningPose(frame.PoseAtFrameStart)) &&
                     _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
            {
                // Retaining the running fallback selected command one in alpha.
                // After movement, fold extra dash speed into base and cancel boost
                // before decelerating; changing only the mode leaves stale dash
                // speed alive and changes the subsequent stopping trajectory.
                samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                    selectedMomentumCommand: true, samus.PoseMetadata.ReadFacingDirection(_addressSpace));
            }
            else if (!animationTransitionApplied &&
                     SamusPoseMetadata.IsRightFacingRunningPose(frame.PoseAtFrameStart) &&
                     _player.ProspectiveSamusFallbackPose == SamusPoseId.FacingRightNormalPose)
            {
                samus.HorizontalSpeed.AccelerationMode = 0;
                if (frame.PoseAtFrameStart == SamusPoseId.MovingRightNormalPose)
                    samus.PoseTransitions.ApplyRunningRightToStandingRight(_addressSpace);
                else
                    samus.PoseTransitions.ApplyGroundedAimTransition(_addressSpace, SamusPoseId.FacingRightNormalPose);
            }
            else if (!animationTransitionApplied &&
                     SamusPoseMetadata.IsLeftFacingRunningPose(frame.PoseAtFrameStart) &&
                     _player.ProspectiveSamusFallbackPose == SamusPoseId.FacingLeftNormalPose)
            {
                samus.HorizontalSpeed.AccelerationMode = 0;
                if (frame.PoseAtFrameStart == SamusPoseId.MovingLeftNormalPose)
                    samus.PoseTransitions.ApplyRunningLeftToStandingLeft(_addressSpace);
                else
                    samus.PoseTransitions.ApplyGroundedAimTransition(_addressSpace, SamusPoseId.FacingLeftNormalPose);
            }
            else if (!animationTransitionApplied &&
                     SamusPoseMetadata.IsMoonwalkingPose(frame.PoseAtFrameStart) &&
                     _player.ProspectiveSamusFallbackPose is { } moonwalkFallback)
            {
                // Command two selected the literal standing/aim fallback during alpha.
                // Applying it after movement preserves the native one-last-step timing.
                samus.PoseTransitions.ApplyMoonwalkPoseChange(
                    _addressSpace,
                    unchecked(moonwalkFallback),
                    _options.MoonwalkEnabled);
            }
            else if (!animationTransitionApplied &&
                     SamusPoseMetadata.IsAimedRanIntoWallPose(frame.PoseAtFrameStart) &&
                     _player.ProspectiveSamusFallbackPose is { } wallFallback)
            {
                // `$CF-$D2` store `$89/$8A` in pose-definition byte two. The stable
                // target shares radius, animation list, and type-$15 cleanup physics.
                samus.PoseTransitions.ApplyGroundedAimTransition(
                    _addressSpace,
                    unchecked(wallFallback));
            }
            else if (!animationTransitionApplied &&
                     (frame.PoseAtFrameStart is
                          SamusPoseId.StandingAimUpRightPose or
                          SamusPoseId.StandingAimUpLeftPose or
                          SamusPoseId.StandingAimDiagonalUpRightPose or
                          SamusPoseId.StandingAimDiagonalUpLeftPose or
                          SamusPoseId.StandingAimDiagonalDownRightPose or
                          SamusPoseId.StandingAimDiagonalDownLeftPose ||
                      SamusPoseMetadata.IsAimedCrouchingPose(frame.PoseAtFrameStart)) &&
                     _player.ProspectiveSamusFallbackPose is { } aimFallback)
            {
                // Only `$91:82D9`'s explicit fallback path above populates this value.
                // Validate and apply it through the same radius/animation seam as
                // held-input aim changes; never assign the ROM byte directly to Pose.
                samus.PoseTransitions.ApplyGroundedAimTransition(
                    _addressSpace,
                    unchecked(aimFallback));
            }
            else if (!animationTransitionApplied &&
                     frame.UsePoseDefinitionFallback && _player.ProspectiveSamusPose is null &&
                     samus.Pose == frame.PoseAtFrameStart &&
                     SamusPoseMetadata.IsAimedCrouchingTurnPose(frame.PoseAtFrameStart) &&
                     _player.ProspectiveSamusFallbackPose is { } crouchingTurnFallback)
            {
                samus.PoseTransitions.ApplyAimedCrouchingTurnInputFallback(
                    _addressSpace,
                    unchecked(crouchingTurnFallback));
            }
            else if (!animationTransitionApplied &&
                     frame.UsePoseDefinitionFallback && _player.ProspectiveSamusPose is null &&
                     samus.Pose == frame.PoseAtFrameStart &&
                     (SamusPoseMetadata.IsAerialTurnPose(frame.PoseAtFrameStart) ||
                      frame.MovementTypeAtFrameStart == SamusMovementType.TurningOnGround))
            {
                // A $FF definition fallback retains the animation, not the momentum
                // command. Native command two still cancels reverse acceleration,
                // for ground turns ($0E) exactly as for aerial ones ($17/$18).
                samus.PoseTransitions.ApplyTurnInputFallback(_addressSpace, frame.MovementTypeAtFrameStart);
            }
            else if (!animationTransitionApplied &&
                     frame.MovementTypeAtFrameStart == SamusMovementType.NormalJumping &&
                     _player.ProspectiveSamusFallbackPose is { } jumpFallback)
            {
                // An unchanged prospective pose must not reset animation or invoke
                // compact-radius expansion. Command one still folds momentum after
                // beta movement and rechecks the remaining base speed there.
                if (jumpFallback != samus.Pose)
                    samus.PoseTransitions.ApplyAerialAimTransition(_addressSpace, unchecked(jumpFallback));
                samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                    frame.DeceleratingFallbackHasMomentum, samus.PoseMetadata.ReadFacingDirection(_addressSpace));
            }
            else if (!animationTransitionApplied &&
                     SamusPoseMetadata.IsAimedAerialPose(frame.PoseAtFrameStart) &&
                     _player.ProspectiveSamusFallbackPose is { } aerialFallback)
            {
                // Install the fallback's pose metadata and native initializer first.
                // Falling command eight below then clears only the extra component;
                // it must not be folded into base speed like running deceleration.
                // A retained compact pose selects history/momentum work only;
                // native pose initialization and animation restart require a change.
                if (aerialFallback != samus.Pose)
                    samus.PoseTransitions.ApplyAerialAimTransition(
                        _addressSpace,
                        unchecked(aerialFallback));
            }

            // Crouching selects command two only on lookup failure. A matched
            // angle-held pose must retain boost even though movement zeroed X speed.
            if (!animationTransitionApplied && frame.UsePoseDefinitionFallback &&
                _player.ProspectiveSamusPose is null &&
                frame.MovementTypeAtFrameStart == SamusMovementType.Crouching &&
                samus.PoseMetadata.ReadMovementType(_addressSpace) == SamusMovementType.Crouching)
                samus.HorizontalSpeed.ApplyStoppedInputFallback(samus.PoseMetadata.ReadFacingDirection(_addressSpace));

            // Windup still runs the ordinary type-$1B input lookup. Its unmatched
            // chord selects command two at $91:8304, just like crouching. Skipping
            // this loses the one-frame momentum cancellation before a delayed
            // midair launch. A selected launch or timeout must win instead.
            if (!animationTransitionApplied && frame.UsePoseDefinitionFallback &&
                _player.ProspectiveSamusPose is null &&
                frame.PoseAtFrameStart is SamusPoseId.ShinesparkWindupRightPose or SamusPoseId.ShinesparkWindupLeftPose &&
                samus.Shinespark.Phase == ShinesparkPhase.Windup)
                samus.HorizontalSpeed.ApplyStoppedInputFallback(samus.PoseMetadata.ReadFacingDirection(_addressSpace));

            // Falling's lookup-failure command still runs when its definition
            // retains the current pose. It follows pose initialization, and must
            // not run when a higher-priority collision or interruption won.
            if (!animationTransitionApplied && frame.UsePoseDefinitionFallback &&
                _player.ProspectiveSamusPose is null &&
                frame.MovementTypeAtFrameStart == SamusMovementType.Falling &&
                samus.PoseMetadata.ReadMovementType(_addressSpace) == SamusMovementType.Falling)
                samus.PoseTransitions.ApplyFallingInputFallback(_addressSpace);

            // F433's normal-jump initializer publishes the fresh Shoot direction
            // for every accepted route, including aim changes within a jump. The
            // earlier projectile epilogue cleared the previous update's handoff.
            if (samus.Pose != samus.PoseHistory.PreviousPose)
                samus.PoseTransitions.PublishNormalJumpPoseShotDirection(_addressSpace, _controller1.NewlyPressed);

            // $91:EB88 shifts history after consuming any transition slot,
            // including a self-transition. A frame with no selected transition
            // does not shift it. Keep this after final collision/pose selection.
            // The translated grapple launch applies its $9B:C9CE transitional
            // slot early and clears ordinary alpha input. It still reaches this
            // same native history epilogue, once, after the forced launch pose.
            // X-ray setup applies interrupted-slot command five early and clears
            // ordinary alpha targets. Preserve its final history commit here.
            if (animationTransitionApplied || frame.XrayActivatedThisFrame || _player.LastGrappleMovement is { WallJumpStarted: true } ||
                _player.ProspectiveSamusPose is not null ||
                _player.ProspectiveSamusFallbackPose is not null)
                samus.CommitPoseHistory(_addressSpace);
        }
    }

    /// <summary>
    /// Advances `$90:E86A/$92:ED24`'s saved-game appearance owner. The ordinary Samus
    /// animation and bank-$8D palette interpreter have already run earlier in this frame;
    /// this tail owns only the exact 360-call fanfare lifetime and handler restoration.
    /// </summary>
    internal void StepSamusLoadAppearance()
    {
        if (_player._samusLoadAppearanceFramesRemaining.IsZero)
            return;
        SamusState samus = _player.Samus
            ?? throw new InvalidOperationException("Samus load appearance lost its actor.");

        _player._samusLoadAppearanceFramesRemaining = _player._samusLoadAppearanceFramesRemaining.Decrement().Remaining;
        if (!_player._samusLoadAppearanceFramesRemaining.IsZero)
            return;
        if (_roomPaletteFx.IsDefinitionActive(_player._samusLoadAppearancePaletteFxDefinition))
        {
            throw new InvalidDataException(
                $"Samus load palette FX $8D:{_player._samusLoadAppearancePaletteFxDefinition:X4} " +
                "outlived the cartridge's 360-frame appearance handler.");
        }

        // PlaySamusFanfare publishes its same-pose history shift only on the
        // completion call, before the ordinary handlers regain control.
        samus.CommitPoseHistory(_addressSpace);
        samus.InputLocked = false;
        _player.GroundedSamusMovementEnabled = true;
        _player._samusLoadAppearancePaletteFxDefinition = 0;
    }
}
