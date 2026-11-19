using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.Core.World;

/// <summary>
/// The enemy phase of a gameplay frame: preparing the enemy frame, running the enemies, and resolving beam hits
/// updated after it.
/// </summary>
internal sealed class EnemyFramePhase
{
    /// <summary>Receives Phantoon's body-layer placement before the next frame boundary latches the frame.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Supplies the held and newly pressed actions to the enemies.</summary>
    private readonly GameplayActionInput _playerActions;
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


    /// <summary>Binds gameplay owners that exchange state at the enemy pass's frame boundary.</summary>
    internal EnemyFramePhase(EnemyFramePhaseDependencies dependencies)
    {
        _backgroundScroll = dependencies.BackgroundScroll;
        _playerActions = dependencies.PlayerActions;
        _enemyRoomEffects = dependencies.EnemyRoomEffects;
        _freeze = dependencies.Freeze;
        _clock = dependencies.Clock;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;

    }
    /// <summary>Publishes prior-frame elevator contact and clears Samus's external displacement before enemies add platform motion.</summary>
    internal static void PrepareEnemyFrame(RoomScope room, SamusState samus)
    {
        // Samus's collision follows the enemies' update, so her stand at the elevator's door on the update before reaches
        // the elevator here, immediately before this update's elevator step (#2545).
        if (room.RoomObjects.DoorContacts.ElevatorStand.Take())
            room.Enemies.Elevator.PublishElevatorDoorContact();
        // The frame start clears all four external-displacement values before
        // the enemy pass. Rideable enemies then accumulate accepted platform deltas and
        // Samus's movement consumes those live values below; movement
        // leaves them to the enemies that carry her.
        samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.Zero;
    }

    /// <summary>Runs the enemy pass after Samus's input phase and projectiles, then applies enemy-authored colour, scroll, escape, room effect, and player handoffs before movement.</summary>
    /// <param name="processingList">
    /// <see cref="EnemyProcessingList.Prepared"/> in main gameplay, whose frame start already built the processing list;
    /// <see cref="EnemyProcessingList.Unprepared"/> in a door transition's enemy pass, which builds it first.
    /// </param>
    /// <param name="samusPreviousPositionCheckpoint">
    /// The camera's frame-start Samus checkpoint, under the live previous position that
    /// the camera's scrolling-speed cap reads.
    /// </param>
    /// <param name="room">The loaded room.</param>
    /// <param name="samus">Samus, whom the enemies touch, carry and hand over to.</param>
    /// <returns>What the pass's enemies did: the events they published, which this phase has carried out.</returns>
    internal EnemyFrameResult RunEnemyUpdatePhase(EnemyProcessingList processingList, FixedPosition<RoomPixels>? samusPreviousPositionCheckpoint, RoomScope room, SamusState samus)
    {
        bool hyperBeamBefore = samus.HyperBeamEnabled;
        if (processingList == EnemyProcessingList.Unprepared)
            room.Enemies.ProcessingWindow.PrepareEnemyProcessingList(room.Camera.PixelPosition);
        EnemyFrameResult result = room.Enemies.StepFrame(new RoomEnemyFrame { Camera = room.Camera.PixelPosition, TimeIsFrozen = _freeze.TimeIsFrozen, Samus = samus, SamusColours = _player.Colours, NewlyPressedControllerInput = _playerActions.NewlyPressed, Level = room.LevelData, ControllerInput = _playerActions.Current, SamusProjectiles = room.Projectiles, ShortCounter = _clock.ShortCounter, SamusBombs = room.BombProjectiles, FrameCounter = _clock.Counter, SamusPreviousPositionCheckpoint = samusPreviousPositionCheckpoint });
        var effectSite = EnemyRoomEffectSite.Of(room, samus);
        _enemyRoomEffects.ApplyEnemyRequestedRoomEffects(result, effectSite);
        EnemyRoomEffects.StartEnemyGlows(result, effectSite);
        // The Baby's recovery equips the Hyper Beam and shows its beams' colours in the same enemy pass, as an item
        // pickup does (#1561); its shots show the equipped Plasma-Wave combination's pictures (#1586).
        if (!hyperBeamBefore && samus.HyperBeamEnabled)
            SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.ShowBeamShade(_player.Colours, samus.EquippedBeams);
        bool elevatorDeparted = result.Reports(ElevatorFrameEvent.DepartureStarted);
        if (elevatorDeparted)
        {
            // Making Samus face forward clears all pending pose requests after the input phase
            // has sampled this frame's controls, so a held Down does not crouch her forward-facing body on the
            // elevator.
            _player.ClearProspectivePoses();
            // Making Samus face forward ends by showing her suit's colours again, so a cancelled charge leaves no
            // flashing colours behind.
            SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(_player.Colours, samus.EquippedItems);
        }
        if (result.Has<DraygonHoldChanged>())
        {
            // The grab and release discard the transitions the input phase chose for the old pose, including the
            // no-input fallback kept beside the prospective pose.
            _player.ClearProspectivePoses();
        }
        if (!_freeze.TimeIsFrozen && room.Enemies.DisplaySources.MotherBrain is { Head: { } rainbowHead } rainbowBrain)
        {
            rainbowBrain.StepRainbowBeam(rainbowHead.Position.Whole, room.Enemies.BossArtworkBinding.MotherBrainRainbowColors);
        }
        if (result.Has<CeresEscapeStarted>())
        {
            // Ceres Ridley starts the escape timer on the same enemy pass that moves
            // Ceres from the fight to the escape; the actor records the boss's defeat itself.
            _enemyRoomEffects.StartCeresEscape();
        }
        foreach (BabyMetroidCameraMove cameraMove in result.All<BabyMetroidCameraMove>())
        {
            int babyMetroidCameraX = cameraMove.CameraX;
            // The camera's and the foreground's positions hold the written X to the room's coordinates (#1627).
            // The Baby Metroid writes the camera X position during the enemy pass, before the ordinary
            // scrolling later in this frame. Do not route it through entry
            // placement, which would clear the camera's fraction of a pixel and its ideal position.
            room.Camera.SetCameraXFromEnemyAi(babyMetroidCameraX);
            _backgroundScroll.ForegroundPosition = _backgroundScroll.ForegroundPosition.WithX(babyMetroidCameraX);
        }
        // The landing tube stops the liquid surface's motion; the next frame shows it still.
        if (result.Has<MotherBrainTubeLanded>())
            room.FxOverlay.StopLiquidMotion();
        if (elevatorDeparted)
        {
            // The elevator clears every one of Samus's shots and their counts. Her beam and missile shots were
            // cleared as the elevator updated; clearing her bombs and the shared cooldown here completes it.
            room.BombProjectiles.Reset();
        }
        // Until Ceres Ridley's animation starts, his Baby's animation runs as the enemy pass ends.
        room.Enemies.Ceres.StepCeresBabyWithEnemyPass();
        samus.Kinematics.InteractiveEnemies = room.Enemies.InteractiveCollisionBodies;
        if (result.Reports(GunshipFrameEvent.EntryStarted))
        {
            // Making Samus face forward performs a direct suit-colour reload and the
            // ship explicitly clears elevator status after locking Samus's
            // control. The enemy system owns pose/motion; these two global values
            // remain runtime-owned and are applied on its typed event boundary.
            SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(_player.Colours, samus.EquippedItems);
            room.Enemies.Elevator.ResetStatus();
        }
        else if (result.Reports(GunshipFrameEvent.LandingCompleted))
        {
            // The gunship restores Samus's normal input and movement modes only after the
            // closing-pad hold. Enemy AI has already restored Samus's normal modes;
            // publish the runtime's matching movement gate at the same boundary.
            _player.GroundedSamusMovementEnabled = true;
        }
        return result;
    }
}
