using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// The enemy phase of a gameplay frame: preparing the enemy frame, running EnemyMain, and resolving beam hits
/// updated after it.
/// </summary>
internal sealed class EnemyFramePhase
{
    /// <summary>Receives enemy-authored BG2 scroll changes before the next NMI latches the frame.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Receives suit-palette reloads after enemies change Samus's pose or charge state.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Supplies current and newly pressed controller words to EnemyMain.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Provides the active Samus Mode 7 transform to the enemy frame.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Applies room effects requested by enemies after their AI pass.</summary>
    private readonly EnemyRoomEffects _enemyRoomEffects;
    /// <summary>Controls whether frozen-time encounters advance presentation effects.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Supplies frame counters consumed by enemy AI and animation.</summary>
    private readonly FrameClock _clock;
    /// <summary>Receives Ceres Ridley's immediate Baby and door sprites before queued enemy layers.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>The installed gameplay artwork the OBJ character page composes from.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Exposes Samus's pose, motion, and collision handoffs around EnemyMain.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Provides active room geometry and camera and receives enemy camera requests.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Collects VRAM writes produced during the enemy pass.</summary>
    private readonly VramWriteQueue _vramWrites;


    /// <summary>Binds gameplay owners that exchange state at the native EnemyMain frame boundary.</summary>
    internal EnemyFramePhase(EnemyFramePhaseDependencies dependencies)
    {
        _backgroundScroll = dependencies.BackgroundScroll;
        _cgram = dependencies.Cgram;
        _controller1 = dependencies.Controller1;
        _display = dependencies.Display;
        _enemyRoomEffects = dependencies.EnemyRoomEffects;
        _freeze = dependencies.Freeze;
        _clock = dependencies.Clock;
        _sprites = dependencies.Sprites;
        _artwork = dependencies.Artwork;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _vramWrites = dependencies.VramWrites;

    }
    /// <summary>Publishes prior-frame elevator contact and clears Samus's external displacement before enemies add platform motion.</summary>
    internal void PrepareEnemyFrame(RoomScope room)
    {
        if (_room.Camera is not null && room.Enemies.IsLoaded)
        {
            // Samus's bank-$94 collision phase follows EnemyMain. A pseudo-door contact
            // published last frame therefore becomes `$0E16=1` immediately before this
            // frame's elevator actor dispatcher, preserving the native producer order.
            if (_room.LevelData?.ConsumeElevatorDoorContact() == true)
                room.Enemies.Elevator.PublishElevatorDoorContact();
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
    /// <param name="processingList">
    /// True in game state eight, whose frame start already ran $A0:8EB6. Door-transition
    /// enemy passes build the list immediately before EnemyMain instead.
    /// </param>
    /// <param name="samusPreviousPositionCheckpoint">
    /// The camera's frame-start Samus checkpoint, under the live previous-position words that
    /// <c>CapScrollingSpeed</c> reads.
    /// </param>
    /// <param name="room">The loaded room.</param>
    internal void RunEnemyMainPhase(EnemyProcessingList processingList, FixedPosition<RoomPixels>? samusPreviousPositionCheckpoint, RoomScope room)
    {
        if (_room.Camera is not null && room.Enemies.IsLoaded)
        {
            EnemyFrameResult result = room.Enemies.StepFrame(new RoomEnemyFrame { Camera = _room.Camera.PixelPosition, TimeIsFrozen = _freeze.TimeIsFrozen, Samus = _player.Samus, NewlyPressedControllerInput = _controller1.NewlyPressed, Level = _room.LevelData, ControllerInput = _controller1.Current, SamusProjectiles = room.Projectiles, NmiFrameCounter8 = _clock.Counter8, Mode7Transform = _display.ActiveSamusMode7Transform, SharedProjectiles = room.BombProjectiles, VramWriteQueue = _vramWrites, ResolveSamusContactBeforeAi = true, CollisionPlms = room.Plms, NmiFrameCounter = _clock.Counter, ProcessingListPrepared = processingList == EnemyProcessingList.Prepared, SamusPreviousPositionCheckpoint = samusPreviousPositionCheckpoint });
            _enemyRoomEffects.ApplyEnemyRequestedRoomEffects(result.RoomEffects, room.Plms, room.Layer3Fx, room.PaletteFx, room.Enemies);
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
                _player.IntroducedSamus.Presentation.LoadSuitPalette(_player.IntroducedSamus, _cgram);
            }
            if (result.DraygonDiscardedSamusPoseRequests)
            {
                // The grab (`$90:E289-$E295`) and release (`$90:E329-$E332`) discard the transitions
                // alpha chose for the old pose, including the no-input fallback that shares the
                // prospective-pose slot.
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
            if (!_freeze.TimeIsFrozen && room.Enemies.DisplaySources.MotherBrain is { Head: { } rainbowHead } rainbowBrain)
            {
                rainbowBrain.RainbowBeamHdma.PresentationColors = room.Enemies.Presentation.MotherBrainRainbowColors;
                rainbowBrain.RainbowBeamHdma.Step(rainbowBrain.RainbowBeamHdmaActive,
                    rainbowHead.Position.Whole,
                    rainbowBrain.RainbowBeamAngle, rainbowBrain.RainbowBeamAngularWidth);
            }
            if (room.Enemies.DisplaySources.Phantoon is { } phantoon)
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
                _enemyRoomEffects.StartCeresEscape();
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
                room.Layer3Fx.DeleteLiquidHdmaObjects();
            if (result.ElevatorClearedProjectileData)
            {
                // `$90:ADB7` clears all ten projectile slots and their counters. Ordinary
                // beam/missile slots were reset inside the actor call; bombs and the shared
                // cooldown live in this companion owner and complete that same operation.
                room.BombProjectiles.Reset();
            }
            // `$A6:A2DF` does not install the post-enemy hook until Ridley's animation word
            // becomes nonzero. Before the reveal it branches directly into `$A6:A2E3`
            // during EnemyMain, so emit the Baby/door OBJ now—before queued enemy layers.
            room.Enemies.Ceres.DrawCeresRidleyImmediateBabyAndDoor(
                _sprites,
                _artwork.GameplayObjectPage(room.Enemies.Room.Session.ObjCharacters),
                _room.Camera.PixelPosition);
            if (_player.Samus is not null)
            {
                _player.Samus.Kinematics.InteractiveEnemies = room.Enemies.InteractiveCollisionBodies;
                if (result.Gunship == GunshipFrameEvent.EntryStarted)
                {
                    // MakeSamusFaceForward performs a direct suit-palette reload and the
                    // ship explicitly clears elevator status after installing locked demo
                    // handlers. The enemy system owns pose/motion; these two global words
                    // remain runtime-owned and are applied on its typed event boundary.
                    _player.Samus.Presentation.LoadSuitPalette(_player.Samus, _cgram);
                    room.Enemies.Elevator.ResetStatus();
                }
                else if (result.Gunship == GunshipFrameEvent.LandingCompleted)
                {
                    // `$A2:A987` restores the ordinary Samus handler pair only after the
                    // closing-pad hold. Enemy AI has already restored Samus's normal pair;
                    // publish the runtime's matching movement gate at the same boundary.
                    _player.GroundedSamusMovementEnabled = true;
                }
            }
        }
    }
}
