using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room-object phase of a gameplay frame and the room objects' step for a door transition.
/// </summary>
internal sealed class RoomObjectFramePhase
{
    /// <summary>Newly pressed actions passed to the room objects.</summary>
    private readonly GameplayActionInput _playerActions;
    /// <summary>The physical buttons held at the update's controller read, which an opening message box continues from.</summary>
    private readonly ControllerInputState _playerButtons;
    /// <summary>The options whose controller bindings an opening message box's panel names.</summary>
    private readonly GameplayOptions _options;
    /// <summary>Suppresses ordinary room-object updates while gameplay time is frozen.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Owns the active synchronous item or station message.</summary>
    private readonly GameplayMessageBoxState _messageBox;
    /// <summary>Retains pending save-station and suit-pickup handoffs.</summary>
    private readonly MessageBoxFlow _messageBoxes;
    /// <summary>Shows the colours of the beams a pickup equips.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Prevents a second suit transformation from replacing an active one.</summary>
    private readonly SamusSuitPickupState _suitPickup;


    /// <summary>Opens the message box on <paramref name="message"/>, continuing from the buttons held at the update's controller read.</summary>
    private void BeginMessage(GameplayMessageId message) =>
        _messageBox.Begin(message, _playerButtons.Current, _options.ControllerBindings);

    /// <summary>Binds the room objects' step to live runtime state and its presentation effects.</summary>
    internal RoomObjectFramePhase(RoomObjectFramePhaseDependencies dependencies)
    {
        _playerActions = dependencies.PlayerActions;
        _playerButtons = dependencies.PlayerButtons;
        _options = dependencies.Options;
        _freeze = dependencies.Freeze;
        _messageBox = dependencies.MessageBox;
        _messageBoxes = dependencies.MessageBoxes;
        _player = dependencies.Player;
        _suitPickup = dependencies.SuitPickup;

    }
    /// <summary>
    /// Runs the room objects once for the door transition, on the door scroll's final update and
    /// before the following frame boundary.
    /// </summary>
    /// <remarks>
    /// Room objects do not run while a door or an elevator freezes time; this step is the one exception: it advances
    /// the freshly loaded room objects' sequences once before the destination fades in.
    /// </remarks>
    internal static void RunRoomObjectsForDoor(RoomScope room, SamusState samus)
    {
        // The room objects' door-transition update stands still while the X-ray holds the room objects, and Indirect
        // G-Mode carries that hold across the door.
        if (samus.Xray.AreObjectsSuspended)
            return;

        RoomObjectStepOutcome step = StepRoomObjects(newlyPressed: GameplayAction.None, room, samus);

        // Setup-time room objects do not have Samus contact/input available. If a future
        // change publishes a player-facing event here, fail at the exact ownership
        // boundary instead of silently discarding it.
        if (step.Pickups.Count != 0 || step.StationActivations.Count != 0)
        {
            throw new InvalidDataException(
                "A door-transition room-object step unexpectedly published a player interaction.");
        }
        SpawnThrownProjectiles(step);
    }

    /// <summary>
    /// Steps the room objects, for ordinary gameplay and for the explicit
    /// transition-time call. Player-facing events remain with each phase that runs them because the
    /// transition path cannot legitimately collect an item or activate a station.
    /// </summary>
    /// <returns>The step's outcome, which the phase that ran it acts on.</returns>
    private static RoomObjectStepOutcome StepRoomObjects(GameplayAction newlyPressed, RoomScope room, SamusState samus)
    {
        RoomObjectStepOutcome step = room.RoomObjects.Step(room.LevelData, new RoomObjectFrameInputs { Scrolls = room.Camera.Scrolls, EnemyDeaths = room.Enemies.RoomStatus.EnemiesKilled, EnemyDeathQuota = room.Enemies.PopulationLoader.DeathQuota, ControllerNewInput = newlyPressed, CollectedItems = samus.CollectedItems, PowerBombExplosionActive = room.BombProjectiles.PowerBombExplosion.IsActive });
        EnemyRoomEffects.ApplyPendingDownwardGateProjectileRequests(EnemyRoomEffectSite.Of(room, samus));
        return step;
    }

    /// <summary>
    /// Spawns the enemy projectiles the step's room objects threw, in the order they threw them, each through the spawner
    /// its thrower gave it: Mother Brain's glass's shards and the Bomb Torizo statue hand's fragments. The glass and the
    /// hand never share a room, so one step throws only one of the two kinds.
    /// </summary>
    private static void SpawnThrownProjectiles(RoomObjectStepOutcome step)
    {
        foreach (RoomObjectProjectileRequest thrown in step.ThrownProjectiles)
            thrown.Spawn();
    }

    /// <summary>Runs the ordinary room-object phase when gameplay permits it, then applies pickups, station messages, and spawned projectiles.</summary>
    internal void RunObjectPhase(bool deathOwnsSamus, SamusState samus, RoomScope room)
    {
        // Main gameplay reaches the room-object update after Samus's new-state and
        // enemy-projectile passes but before the main scrolling. A grapple block
        // acquired above therefore consumes its initial timer and draws its first frame in this
        // same frame; when it later becomes air, anchor validation has already run and
        // sees that mutation on the following Samus frame.
        if (!deathOwnsSamus && !_freeze.TimeIsFrozen && !samus.Xray.AreObjectsSuspended)
        {
            RoomObjectStepOutcome step = StepRoomObjects(_playerActions.NewlyPressed, room, samus);

            // An item's pickup is known only after its room object has stepped, and its colours change here:
            // a beam pickup shows the newly equipped beams' colours, and a suit pickup shows the suit's colours
            // only when the suit is revealed, after the message. Tanks and other equipment take effect through
            // the HUD and Samus's movement.
            var opening = new RoomObjectMessages(this, samus);
            foreach (CollectiblePickupEvent pickup in step.Pickups)
            {
                pickup.Consequence.Apply(opening);

                if (_messageBox.IsActive)
                {
                    throw new InvalidDataException(
                        "Multiple permanent items attempted to enter the synchronous " +
                        "message box during one room-object pass.");
                }
                BeginMessage(pickup.Message);
            }
            foreach (StationActivationEvent station in step.StationActivations)
                station.Open(opening);
            SpawnThrownProjectiles(step);
        }
    }

    /// <summary>
    /// What one room-object step's pickups and station actions open (#2383): a beam's colours, a suit's transformation, a
    /// save's confirmation or completion, or a station's completion message, each with the message box it starts.
    /// </summary>
    /// <param name="phase">The room-object phase, which owns the message box and its pending work.</param>
    /// <param name="samus">Samus, whose beams a beam pickup shows.</param>
    private sealed class RoomObjectMessages(RoomObjectFramePhase phase, SamusState samus)
        : ICollectiblePickupConsequences, IStationActivationMessages
    {
        /// <inheritdoc/>
        public void ShowEquippedBeamColours() => SamusProjectileSystem.ShowBeamShade(phase._player.Colours, samus.EquippedBeams);

        /// <inheritdoc/>
        public void AwaitSuitTransformation(SamusSuitPickupKind suit)
        {
            if (phase._suitPickup.IsActive)
                throw new InvalidDataException("A second suit pickup attempted to replace an active transformation.");
            phase._messageBoxes.Await(new MessageBoxOutcome.SuitTransformation(suit));
        }

        /// <inheritdoc/>
        public void RequestSaveConfirmation(SaveStationActivation save)
        {
            if (phase._messageBoxes.HasPendingOutcome || phase._messageBox.IsActive)
                throw new InvalidDataException("A save station attempted to replace an active room-object message owner.");
            phase._messageBoxes.Await(new MessageBoxOutcome.SaveConfirmation(save));
            phase.BeginMessage(GameplayMessageId.SaveConfirmation);
        }

        /// <inheritdoc/>
        public void ShowSaveCompleted(SaveStationActivation save)
        {
            if (phase._messageBoxes.HasPendingOutcome || phase._messageBox.IsActive)
                throw new InvalidDataException("A completed save station attempted to replace an active message owner.");
            phase._messageBoxes.Await(new MessageBoxOutcome.SaveCompletion(save));
            phase.BeginMessage(GameplayMessageId.SaveCompleted);
        }

        /// <inheritdoc/>
        public void ShowStationMessage(GameplayMessageId message)
        {
            if (phase._messageBox.IsActive)
            {
                throw new InvalidDataException(
                    "Multiple room objects attempted to open the synchronous message box during one room-object step.");
            }
            phase.BeginMessage(message);
        }
    }
}
