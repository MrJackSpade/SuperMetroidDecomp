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
/// The enemy phase of a gameplay frame: preparing the enemy frame, running the enemies, and resolving beam hits
/// updated after it.
/// </summary>
internal sealed class EnemyFramePhase
{
    /// <summary>Receives Phantoon's body-layer placement before the next frame boundary latches the frame.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Supplies current and newly pressed controller buttons to the enemies.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Provides the active Samus turning-floor transform to the enemy frame.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Applies room effects requested by enemies after their AI pass.</summary>
    private readonly EnemyRoomEffects _enemyRoomEffects;
    /// <summary>Controls whether frozen-time encounters advance presentation effects.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Supplies frame counters consumed by enemy AI and animation.</summary>
    private readonly FrameClock _clock;
    /// <summary>Receives Ceres Ridley's immediate Baby and door sprites before queued enemy layers.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Exposes Samus's pose, motion, and collision handoffs around the enemy pass.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Provides active room geometry and camera and receives enemy camera requests.</summary>
    private readonly ActiveRoomState _room;


    /// <summary>Binds gameplay owners that exchange state at the enemy pass's frame boundary.</summary>
    internal EnemyFramePhase(EnemyFramePhaseDependencies dependencies)
    {
        _backgroundScroll = dependencies.BackgroundScroll;
        _controller1 = dependencies.Controller1;
        _display = dependencies.Display;
        _enemyRoomEffects = dependencies.EnemyRoomEffects;
        _freeze = dependencies.Freeze;
        _clock = dependencies.Clock;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
        _room = dependencies.Room;

    }
    /// <summary>Publishes prior-frame elevator contact and clears Samus's external displacement before enemies add platform motion.</summary>
    internal void PrepareEnemyFrame(RoomScope room)
    {
        if (_room.Camera is not null && room.Enemies.IsLoaded)
        {
            // Samus's collision phase follows the enemy pass. A pseudo-door contact
            // published last frame therefore arms the elevator immediately before this
            // frame's elevator actor dispatcher, preserving the producer order.
            if (_room.LevelData?.ConsumeElevatorDoorContact() == true)
                room.Enemies.Elevator.PublishElevatorDoorContact();
            if (_player.Samus is not null)
            {
                // The frame start clears all four external-displacement values before
                // the enemy pass. Rideable enemies then accumulate accepted platform deltas and
                // Samus's movement consumes those live values below. They
                // are producer-owned values, so movement deliberately does not clear them.
                _player.Samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.Zero;
            }
        }
    }

    /// <summary>Runs the enemy pass after Samus's input phase and projectiles, then applies enemy-authored palette, scroll, escape, per-line effect, and player handoffs before movement.</summary>
    /// <param name="processingList">
    /// True in main gameplay, whose frame start already built the processing list. Door-transition
    /// enemy passes build the list immediately before the enemy pass instead.
    /// </param>
    /// <param name="samusPreviousPositionCheckpoint">
    /// The camera's frame-start Samus checkpoint, under the live previous position that
    /// the camera's scrolling-speed cap reads.
    /// </param>
    /// <param name="room">The loaded room.</param>
    internal void RunEnemyMainPhase(EnemyProcessingList processingList, FixedPosition<RoomPixels>? samusPreviousPositionCheckpoint, RoomScope room)
    {
        if (_room.Camera is not null && room.Enemies.IsLoaded)
        {
            bool hyperBeamBefore = _player.Samus?.HyperBeamEnabled == true;
            if (processingList == EnemyProcessingList.Unprepared)
                room.Enemies.ProcessingWindow.PrepareEnemyProcessingList(_room.Camera.PixelPosition);
            EnemyFrameResult result = room.Enemies.StepFrame(new RoomEnemyFrame { Camera = _room.Camera.PixelPosition, TimeIsFrozen = _freeze.TimeIsFrozen, Samus = _player.Samus, SamusColours = _player.Colours, NewlyPressedControllerInput = _controller1.NewlyPressed, Level = _room.LoadedLevelData, ControllerInput = _controller1.Current, SamusProjectiles = room.Projectiles, ShortCounter = _clock.ShortCounter, TurningFloor = _display.ActiveTurningFloor, SharedProjectiles = room.BombProjectiles, CollisionObjects = room.RoomObjects.BreakableTerrain, FrameCounter = _clock.Counter, SamusPreviousPositionCheckpoint = samusPreviousPositionCheckpoint });
            _enemyRoomEffects.ApplyEnemyRequestedRoomEffects(result.RoomEffects, room.RoomObjects, room.FxOverlay, room.PaletteFx, room.Enemies);
            // The Baby's recovery equips the Hyper Beam and shows its beams' colours in the same enemy pass, as an item
            // pickup does (#1561); its shots show the equipped Plasma-Wave combination's pictures (#1586).
            if (!hyperBeamBefore && _player.Samus is { HyperBeamEnabled: true } hyperBeamSamus)
                SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.LoadBeamPalette(_player.Colours, hyperBeamSamus.EquippedBeams);
            if (result.Elevator == ElevatorFrameEvent.DepartureStarted)
            {
                // Making Samus face forward clears all pending pose requests after the input phase
                // has sampled this frame's controls. Do not apply that stale Down
                // crouch request to the new forward-facing, elevator-owned body.
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
                // Making Samus face forward ends by reloading the suit palette. The actor
                // clears flare/projectile state, but cannot restore runtime-owned colours.
                // Without this write, cancelling charge leaves its last flashing colors
                // behind after the charge colouring ends.
                SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.Load(_player.Colours, _player.IntroducedSamus.EquippedItems);
            }
            if (result.DraygonDiscardedSamusPoseRequests)
            {
                // The grab and release discard the transitions
                // the input phase chose for the old pose, including the no-input fallback that shares the
                // prospective-pose slot.
                _player.ProspectiveSamusPose = null;
                _player.ProspectiveSamusFallbackPose = null;
            }
            if (!_freeze.TimeIsFrozen && room.Enemies.DisplaySources.MotherBrain is { Head: { } rainbowHead } rainbowBrain)
            {
                rainbowBrain.RainbowBeam.PresentationColors = room.Enemies.BossArtworkBinding.MotherBrainRainbowColors;
                rainbowBrain.RainbowBeam.Step(rainbowBrain.RainbowBeamActive,
                    rainbowHead.Position.Whole,
                    rainbowBrain.RainbowBeamAngle, rainbowBrain.RainbowBeamAngularWidth);
            }
            if (room.Enemies.DisplaySources.Phantoon is { } phantoon)
            {
                // Phantoon's body layer is placed by his AI, while eye/tentacle collision follows enemy
                // positions. Publish the placement before the next accepted frame latches the matching sprite
                // frame. The room's fixed background axes preserve it during camera scrolling.
                _backgroundScroll.PlaceBackgroundView(phantoon.BodyLayerPlacement);
            }
            if (result.CeresEscapeStarted)
            {
                // Ceres Ridley starts the escape timer on the same enemy pass that moves
                // Ceres from the fight to the escape; the actor records the boss's defeat itself.
                _enemyRoomEffects.StartCeresEscape();
            }
            if (result.CameraXOverride is int shitroidCameraX)
            {
                // The camera's and the foreground's positions hold the written X to the room's coordinates (#1627).
                // The Shitroid writes the camera X position during the enemy pass, before the ordinary
                // scrolling later in this frame. Do not route it through entry
                // placement, which would clear subposition and ideal-camera state.
                _room.Camera.SetCameraXFromEnemyAi(shitroidCameraX);
                _backgroundScroll.ForegroundPosition = _backgroundScroll.ForegroundPosition.WithX(shitroidCameraX);
            }
            // The landing tube stops the liquid surface's motion; the next frame shows it still.
            if (result.StopLiquidMotion)
                room.FxOverlay.StopLiquidMotion();
            if (result.ElevatorClearedProjectileData)
            {
                // The elevator clears all ten projectile slots and their counters. Ordinary
                // beam/missile slots were reset inside the actor call; bombs and the shared
                // cooldown live in this companion owner and complete that same operation.
                room.BombProjectiles.Reset();
            }
            // Ceres Ridley draws the Baby and door over every enemy only once his animation
            // starts. Before the reveal he draws them directly
            // during the enemy pass, so emit the Baby/door sprites now, before queued enemy layers.
            room.Enemies.Ceres.DrawCeresRidleyImmediateBabyAndDoor(
                _sprites,
                _room.Camera.PixelPosition);
            if (_player.Samus is not null)
            {
                _player.Samus.Kinematics.InteractiveEnemies = room.Enemies.InteractiveCollisionBodies;
                if (result.Gunship == GunshipFrameEvent.EntryStarted)
                {
                    // Making Samus face forward performs a direct suit-palette reload and the
                    // ship explicitly clears elevator status after installing locked demo
                    // handlers. The enemy system owns pose/motion; these two global values
                    // remain runtime-owned and are applied on its typed event boundary.
                    SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.Load(_player.Colours, _player.Samus.EquippedItems);
                    room.Enemies.Elevator.ResetStatus();
                }
                else if (result.Gunship == GunshipFrameEvent.LandingCompleted)
                {
                    // The gunship restores the ordinary Samus handler pair only after the
                    // closing-pad hold. Enemy AI has already restored Samus's normal pair;
                    // publish the runtime's matching movement gate at the same boundary.
                    _player.GroundedSamusMovementEnabled = true;
                }
            }
        }
    }
}
