using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy phase of a gameplay frame: preparing the enemy frame, running EnemyMain, and resolving beam hits
/// updated after it.
/// </summary>
internal sealed class EnemyFramePhase
{
    /// <summary>Receives enemy-authored BG2 scroll changes before the next NMI latches the frame.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Owns bombs and shared projectile cooldown reset requested by an elevator.</summary>
    private readonly SamusBombProjectileSystem _bombProjectiles;
    /// <summary>Receives suit-palette reloads after enemies change Samus's pose or charge state.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Supplies current and newly pressed controller words to EnemyMain.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Provides the active Samus Mode 7 transform to the enemy frame.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Runs enemy AI, collision, drawing handoffs, and encounter state for this frame.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Applies room effects requested by enemies after their AI pass.</summary>
    private readonly EnemyRoomEffects _enemyRoomEffects;
    /// <summary>Starts the Ceres escape countdown on the EnemyMain transition that requests it.</summary>
    private readonly EscapeTimer _escapeTimer;
    /// <summary>Controls whether frozen-time encounters advance presentation effects.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Supplies frame counters consumed by enemy AI and animation.</summary>
    private readonly FrameClock _clock;
    /// <summary>Receives Ceres Ridley's immediate Baby and door sprites before queued enemy layers.</summary>
    private readonly OamBuffer _oam;
    /// <summary>Exposes Samus's pose, motion, and collision handoffs around EnemyMain.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Supplies live Samus beams and missiles to enemy collision resolution.</summary>
    private readonly SamusProjectileSystem _projectiles;
    /// <summary>Provides active room geometry and camera and receives enemy camera requests.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Deletes room liquid HDMA objects when Mother Brain requests their removal.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Collects VRAM writes produced during the enemy pass.</summary>
    private readonly VramWriteQueue _vramWrites;


    /// <summary>Binds gameplay owners that exchange state at the native EnemyMain frame boundary.</summary>
    internal EnemyFramePhase(EnemyFramePhaseDependencies dependencies)
    {
        _backgroundScroll = dependencies.BackgroundScroll;
        _bombProjectiles = dependencies.BombProjectiles;
        _cgram = dependencies.Cgram;
        _controller1 = dependencies.Controller1;
        _display = dependencies.Display;
        _enemies = dependencies.Enemies;
        _enemyRoomEffects = dependencies.EnemyRoomEffects;
        _escapeTimer = dependencies.EscapeTimer;
        _freeze = dependencies.Freeze;
        _clock = dependencies.Clock;
        _oam = dependencies.Oam;
        _player = dependencies.Player;
        _projectiles = dependencies.Projectiles;
        _room = dependencies.Room;
        _roomLayer3Fx = dependencies.RoomLayer3Fx;
        _vramWrites = dependencies.VramWrites;

    }
    /// <summary>Resolves Ceres Ridley's dedicated updated-beam adapter; ordinary projectiles collide before each enemy's AI.</summary>
    internal void ResolveUpdatedBeamHits()
    {
        _enemies.Ceres.ResolveCeresRidleyProjectileHits(
            _projectiles,
            _bombProjectiles);
    }

    /// <summary>Publishes prior-frame elevator contact and clears Samus's external displacement before enemies add platform motion.</summary>
    internal void PrepareEnemyFrame()
    {
        if (_room.Camera is not null && _enemies.IsLoaded)
        {
            // Samus's bank-$94 collision phase follows EnemyMain. A pseudo-door contact
            // published last frame therefore becomes `$0E16=1` immediately before this
            // frame's elevator actor dispatcher, preserving the native producer order.
            if (_room.LevelData?.ConsumeElevatorDoorContact() == true)
                _enemies.Elevator.PublishElevatorDoorContact();
            if (_player.Samus is not null)
            {
                // `$94:9B60-$9B72` clears all four external-displacement words before
                // EnemyMain. Rideable enemies then accumulate accepted platform deltas and
                // bank `$90` consumes those live values during Samus movement below. They
                // are producer-owned words, so movement deliberately does not clear them.
                _player.Samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.Zero;
            }
        }
    }

    /// <summary>Runs EnemyMain after Samus alpha and projectiles, then applies enemy-authored palette, scroll, escape, HDMA, and player handoffs before beta movement.</summary>
    /// <param name="processingListPrepared">
    /// True in game state eight, whose frame start already ran $A0:8EB6. Door-transition
    /// enemy passes build the list immediately before EnemyMain instead.
    /// </param>
    /// <param name="samusPreviousPositionCheckpoint">
    /// The camera's frame-start Samus checkpoint, under the live previous-position words that
    /// <c>CapScrollingSpeed</c> reads.
    /// </param>
    /// <param name="plms">The loaded room's PLM system.</param>
    internal void RunEnemyMainPhase(bool processingListPrepared, FixedPosition<RoomPixels>? samusPreviousPositionCheckpoint, RoomPlmSystem plms)
    {
        if (_room.Camera is not null && _enemies.IsLoaded)
        {
            EnemyFrameResult result = _enemies.StepFrame(new RoomEnemyFrame { Camera = _room.Camera.PixelPosition, TimeIsFrozen = _freeze.TimeIsFrozen, Samus = _player.Samus, NewlyPressedControllerInput = _controller1.NewlyPressed, Level = _room.LevelData, ControllerInput = _controller1.Current, SamusProjectiles = _projectiles, NmiFrameCounter8 = _clock.Counter8, Mode7Transform = _display.ActiveSamusMode7Transform, SharedProjectiles = _bombProjectiles, VramWriteQueue = _vramWrites, ResolveSamusContactBeforeAi = true, CollisionPlms = plms, NmiFrameCounter = _clock.Counter, ProcessingListPrepared = processingListPrepared, SamusPreviousPositionCheckpoint = samusPreviousPositionCheckpoint });
            _enemyRoomEffects.ApplyEnemyRequestedRoomEffects(result.RoomEffects, plms);
            if (result.Elevator == ElevatorFrameEvent.DepartureStarted)
            {
                // MakeSamusFaceForward clears all pending pose requests after alpha
                // has sampled this frame's controls. Do not apply that stale Down
                // crouch request to the new forward-facing, elevator-owned body.
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                // MakeSamusFaceForward ends by reloading the suit palette. The actor
                // clears flare/projectile state, but cannot restore runtime-owned CGRAM.
                // Without this write, cancelling charge leaves its last flashing colors
                // behind after the palette handler becomes inactive.
                _player.IntroducedSamus.Presentation.LoadSuitPalette(_cgram);
            }
            if (result.DraygonDiscardedSamusPoseRequests)
            {
                // The grab (`$90:E289-$E295`) and release (`$90:E329-$E332`) discard the transitions
                // alpha chose for the old pose, including the no-input fallback that shares the
                // prospective-pose slot.
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
            if (!_freeze.TimeIsFrozen && _enemies.MotherBrainEncounter.MotherBrain is { Head: { } rainbowHead } rainbowBrain)
            {
                rainbowBrain.RainbowBeamHdma.PresentationColors = _enemies.Presentation.MotherBrainRainbowColors;
                rainbowBrain.RainbowBeamHdma.Step(rainbowBrain.RainbowBeamHdmaActive,
                    rainbowHead.Position.Whole,
                    rainbowBrain.RainbowBeamAngle, rainbowBrain.RainbowBeamAngularWidth);
            }
            // $A7:C85E writes PreviousLayer1XBlock during EnemyMain: every BG1 column streams as new.
            if (result.KraidLayer1XBlockReset)
                _backgroundScroll.ResetPreviousLayer1XBlock();
            if (_enemies.PhantoonBehavior.Phantoon is { } phantoon)
            {
                // Phantoon's body is BG2 artwork anchored by the bank-$A7 scroll writes,
                // while eye/tentacle collision follows enemy positions. Publish those
                // writes before the next accepted NMI latches the matching OAM frame.
                // The room's fixed layer-two axes preserve them during camera scrolling.
                _backgroundScroll.SetBg2ScrollRegisters(phantoon.Bg2Scroll);
            }
            if (result.CeresEscapeStarted)
            {
                // $A6:C117 starts the escape timer on the same EnemyMain call that changes
                // ceres_status from one to two; the actor sets the boss bit itself.
                _escapeTimer.RequestCeresStart();
            }
            if (result.LayerOneXOverride is int layerOneX)
            {
                ushort shitroidCameraX = checked((ushort)layerOneX);
                // `$A9:EFE6` writes layer1_x_pos during EnemyMain, before the ordinary
                // scrolling routine later in this frame. Do not route it through entry
                // placement, which would clear subposition and ideal-camera state.
                _room.Camera.SetLayerOneXFromEnemyAi(shitroidCameraX);
                _backgroundScroll.Layer1Position = _backgroundScroll.Layer1Position.WithX(shitroidCameraX);
            }
            // `$A9:8C0C` deletes HDMA objects 0-3; the next frame's HDMA pass runs none of them.
            if (result.DeleteLiquidHdmaObjects)
                _roomLayer3Fx.DeleteLiquidHdmaObjects();
            if (result.ElevatorClearedProjectileData)
            {
                // `$90:ADB7` clears all ten projectile slots and their counters. Ordinary
                // beam/missile slots were reset inside the actor call; bombs and the shared
                // cooldown live in this companion owner and complete that same operation.
                _bombProjectiles.Reset();
            }
            // `$A6:A2DF` does not install the post-enemy hook until Ridley's animation word
            // becomes nonzero. Before the reveal it branches directly into `$A6:A2E3`
            // during EnemyMain, so emit the Baby/door OBJ now—before queued enemy layers.
            _enemies.Ceres.DrawCeresRidleyImmediateBabyAndDoor(
                _oam,
                _room.Camera.PixelPosition);
            if (_player.Samus is not null)
            {
                _player.Samus.Kinematics.InteractiveEnemies = _enemies.InteractiveCollisionBodies;
                if (result.Gunship == GunshipFrameEvent.EntryStarted)
                {
                    // MakeSamusFaceForward performs a direct suit-palette reload and the
                    // ship explicitly clears elevator status after installing locked demo
                    // handlers. The enemy system owns pose/motion; these two global words
                    // remain runtime-owned and are applied on its typed event boundary.
                    _player.Samus.Presentation.LoadSuitPalette(_cgram);
                    _enemies.Elevator.ResetStatus();
                }
                else if (result.Gunship == GunshipFrameEvent.LandingCompleted)
                {
                    // `$A2:A987` restores the ordinary Samus handler pair only after the
                    // closing-pad hold. Enemy AI has already unlocked Samus.InputLocked;
                    // publish the runtime's matching movement gate at the same boundary.
                    _player.GroundedSamusMovementEnabled = true;
                }
            }
        }
    }
}
