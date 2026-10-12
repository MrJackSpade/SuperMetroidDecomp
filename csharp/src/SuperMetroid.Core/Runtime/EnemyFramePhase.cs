using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The enemy phase of a gameplay frame: preparing the enemy frame, running EnemyMain, and resolving beam hits
/// updated after it.
/// </summary>
internal sealed class EnemyFramePhase
{
    /// <summary>Stores background scroll.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Stores bomb projectiles.</summary>
    private readonly SamusBombProjectileSystem _bombProjectiles;
    /// <summary>Stores cgram.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Stores controller1.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Stores display.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Stores enemies.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Stores enemy room effects.</summary>
    private readonly EnemyRoomEffects _enemyRoomEffects;
    /// <summary>Stores escape timer.</summary>
    private readonly EscapeTimer _escapeTimer;
    /// <summary>Stores freeze.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Stores nmi.</summary>
    private readonly NmiClock _nmi;
    /// <summary>Stores oam.</summary>
    private readonly OamBuffer _oam;
    /// <summary>Stores player.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Stores plms.</summary>
    private readonly RoomPlmSystem _plms;
    /// <summary>Stores projectiles.</summary>
    private readonly SamusProjectileSystem _projectiles;
    /// <summary>Stores room.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Stores room layer3 fx.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Stores vram writes.</summary>
    private readonly VramWriteQueue _vramWrites;
    /// <summary>Stores address space.</summary>
    private readonly ISnesAddressSpace _addressSpace;

    /// <summary>Initializes a new enemy frame phase instance.</summary>
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
        _nmi = dependencies.Nmi;
        _oam = dependencies.Oam;
        _player = dependencies.Player;
        _plms = dependencies.Plms;
        _projectiles = dependencies.Projectiles;
        _room = dependencies.Room;
        _roomLayer3Fx = dependencies.RoomLayer3Fx;
        _vramWrites = dependencies.VramWrites;
        _addressSpace = dependencies.AddressSpace;
    }
    // Only the remaining dedicated Ceres adapter publishes here. Ordinary shots, Phantoon
    // beams and bombs dispatch per enemy before AI, and Kraid's passes run inside its AI.
    /// <summary>Resolves updated beam hits.</summary>
    internal void ResolveUpdatedBeamHits()
    {
        _enemies.CeresRidleyBehavior.ResolveCeresRidleyProjectileHits(
            _addressSpace,
            _projectiles,
            _bombProjectiles);
    }

    // Terrain preparation owns the external movement words before alpha. Enemy
    // actors subsequently add platform displacement; beta consumes it unchanged.
    /// <summary>Prepares enemy frame.</summary>
    internal void PrepareEnemyFrame()
    {
        if (_room.Camera is not null && _enemies.IsLoaded)
        {
            // Samus's bank-$94 collision phase follows EnemyMain. A pseudo-door contact
            // published last frame therefore becomes `$0E16=1` immediately before this
            // frame's elevator actor dispatcher, preserving the native producer order.
            if (_room.LevelData?.ConsumeElevatorDoorContact() == true)
                _enemies.ElevatorBehavior.PublishElevatorDoorContact();
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

    // GameState_8 runs this after Samus alpha/projectile update and before beta
    // movement. Keep actor side effects together at that shared frame boundary.
    /// <param name="processingListPrepared">
    /// True in game state eight, whose frame start already ran $A0:8EB6. Door-transition
    /// enemy passes build the list immediately before EnemyMain instead.
    /// </param>
    /// <param name="samusPreviousPositionCheckpoint">
    /// The camera's frame-start Samus checkpoint, under the live previous-position words that
    /// <c>CapScrollingSpeed</c> reads.
    /// </param>
    internal void RunEnemyMainPhase(bool processingListPrepared, FixedPosition<RoomPixels>? samusPreviousPositionCheckpoint)
    {
        if (_room.Camera is not null && _enemies.IsLoaded)
        {
            _enemies.StepFrame(new RoomEnemyFrame { Camera = _room.Camera.PixelPosition, TimeIsFrozen = _freeze.TimeIsFrozen, Samus = _player.Samus, NewlyPressedControllerInput = _controller1.NewlyPressed, Level = _room.LevelData, ControllerInput = _controller1.Current, SamusProjectiles = _projectiles, NmiFrameCounter8 = _nmi.NmiFrameCounter8, Mode7Transform = _display.ActiveSamusMode7Transform, SharedProjectiles = _bombProjectiles, VramWriteQueue = _vramWrites, ResolveSamusContactBeforeAi = true, CollisionPlms = _plms, NmiFrameCounter = _nmi.NmiFrameCounter, ProcessingListPrepared = processingListPrepared, SamusPreviousPositionCheckpoint = samusPreviousPositionCheckpoint });
            _enemyRoomEffects.ApplyEnemyRequestedRoomEffects();
            if (_enemies.ElevatorBehavior.LastElevatorEvent == ElevatorFrameEvent.DepartureStarted)
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
                _player.Samus!.Presentation.LoadSuitPalette(_addressSpace, _cgram);
            }
            if (_enemies.RoomStatus.SamusReleasedByDraygonThisFrame)
            {
                // `$90:E329-$E332` discards the transitions alpha chose for the old pose,
                // including the no-input fallback that shares the prospective-pose slot.
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
            if (!_freeze.TimeIsFrozen && _enemies.MotherBrainEncounter.MotherBrain is { Head: { } rainbowHead } rainbowBrain)
            {
                rainbowBrain.RainbowBeamHdma.PresentationColors = _enemies.Presentation.MotherBrainRainbowColors;
                rainbowBrain.RainbowBeamHdma.Step(_addressSpace, rainbowBrain.RainbowBeamHdmaActive,
                    rainbowHead.Position.Whole,
                    rainbowBrain.RainbowBeamAngle, rainbowBrain.RainbowBeamAngularWidth);
            }
            if (_enemies.PhantoonBehavior.Phantoon is { } phantoon)
            {
                // Phantoon's body is BG2 artwork anchored by the bank-$A7 scroll writes,
                // while eye/tentacle collision follows enemy positions. Publish those
                // writes before the next accepted NMI latches the matching OAM frame.
                // The room's fixed layer-two axes preserve them during camera scrolling.
                _backgroundScroll.SetBg2ScrollRegisters(phantoon.Bg2Scroll);
            }
            if (_enemies.RoomStatus.CeresEscapeStartedThisFrame)
            {
                // $A6:C117 starts the escape timer on the same EnemyMain call that changes
                // ceres_status from one to two; the actor sets the boss bit itself.
                _escapeTimer.RequestCeresStart();
            }
            if (_enemies.ShitroidBehavior.RequestedShitroidCameraX is ushort shitroidCameraX)
            {
                // `$A9:EFE6` writes layer1_x_pos during EnemyMain, before the ordinary
                // scrolling routine later in this frame. Do not route it through entry
                // placement, which would clear subposition and ideal-camera state.
                _room.Camera.SetLayerOneXFromEnemyAi(shitroidCameraX);
                _backgroundScroll.Layer1Position = _backgroundScroll.Layer1Position.WithX(shitroidCameraX);
            }
            // `$A9:8C0C` deletes HDMA objects 0-3; the next frame's HDMA pass runs none of them.
            if (_enemies.MotherBrainEncounter.MotherBrainDeletedHdmaObjects)
                _roomLayer3Fx.DeleteLiquidHdmaObjects();
            if (_enemies.ElevatorBehavior.ElevatorClearedProjectileData)
            {
                // `$90:ADB7` clears all ten projectile slots and their counters. Ordinary
                // beam/missile slots were reset inside the actor call; bombs and the shared
                // cooldown live in this companion owner and complete that same operation.
                _bombProjectiles.Reset();
            }
            // `$A6:A2DF` does not install the post-enemy hook until Ridley's animation word
            // becomes nonzero. Before the reveal it branches directly into `$A6:A2E3`
            // during EnemyMain, so emit the Baby/door OBJ now—before queued enemy layers.
            _enemies.CeresRidleyBehavior.DrawCeresRidleyImmediateBabyAndDoor(
                _oam,
                _room.Camera.PixelPosition);
            if (_player.Samus is not null)
            {
                _player.Samus.Kinematics.InteractiveEnemies = _enemies.InteractiveCollisionBodies;
                if (_enemies.GunshipBehavior.LastGunshipEvent == GunshipFrameEvent.EntryStarted)
                {
                    // MakeSamusFaceForward performs a direct suit-palette reload and the
                    // ship explicitly clears elevator status after installing locked demo
                    // handlers. The enemy system owns pose/motion; these two global words
                    // remain runtime-owned and are applied on its typed event boundary.
                    _player.Samus.Presentation.LoadSuitPalette(_addressSpace, _cgram);
                    _enemies.ElevatorBehavior.SetElevatorStatusForDebugging(0);
                }
                else if (_enemies.GunshipBehavior.LastGunshipEvent == GunshipFrameEvent.LandingCompleted)
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
