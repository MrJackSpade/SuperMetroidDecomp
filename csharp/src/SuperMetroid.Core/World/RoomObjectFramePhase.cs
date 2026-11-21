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
    /// <summary>
    /// The message boxes one room-object step opened that have not shown yet, in the order the step made them (#2553): the
    /// game shows each pickup's or station's box inside its room object's update, so a second one shows only once the box
    /// before it has closed, its pickup's consequence applied as it opens.
    /// </summary>
    private readonly Queue<Action> _waitingMessages = new();

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

        // The freshly loaded objects have met no contact of Samus's, so the step touches no item and no station acts: the
        // untouched view's step yields only the projectiles its objects throw.
        IReadOnlyList<RoomObjectProjectileRequest> thrown =
            room.RoomObjects.Untouched.Step(room.LevelData, FrameInputs(GameplayAction.None, room, samus));
        EnemyRoomEffects.ApplyPendingDownwardGateProjectileRequests(EnemyRoomEffectSite.Of(room, samus));
        SpawnThrownProjectiles(thrown);
    }

    /// <summary>The room objects' inputs for a step with <paramref name="newlyPressed"/> newly pressed.</summary>
    private static RoomObjectFrameInputs FrameInputs(GameplayAction newlyPressed, RoomScope room, SamusState samus) =>
        new() { Scrolls = room.Camera.Scrolls, EnemyDeaths = room.Enemies.RoomStatus.EnemiesKilled, EnemyDeathQuota = room.Enemies.PopulationLoader.DeathQuota, ControllerNewInput = newlyPressed, CollectedItems = samus.CollectedItems, PowerBombExplosionActive = room.BombProjectiles.PowerBombExplosion.IsActive };

    /// <summary>Steps the room objects for ordinary gameplay.</summary>
    /// <returns>The step's outcome, which the phase acts on.</returns>
    private static RoomObjectStepOutcome StepRoomObjects(GameplayAction newlyPressed, RoomScope room, SamusState samus)
    {
        RoomObjectStepOutcome step = room.RoomObjects.Step(room.LevelData, FrameInputs(newlyPressed, room, samus));
        EnemyRoomEffects.ApplyPendingDownwardGateProjectileRequests(EnemyRoomEffectSite.Of(room, samus));
        return step;
    }

    /// <summary>
    /// Spawns the enemy projectiles the step's room objects threw, in the order they threw them, each through the spawner
    /// its thrower gave it: Mother Brain's glass's shards and the Bomb Torizo statue hand's fragments. The glass and the
    /// hand never share a room, so one step throws only one of the two kinds.
    /// </summary>
    private static void SpawnThrownProjectiles(IReadOnlyList<RoomObjectProjectileRequest> thrown)
    {
        foreach (RoomObjectProjectileRequest projectile in thrown)
            projectile.Spawn();
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
                _waitingMessages.Enqueue(() =>
                {
                    pickup.Consequence.Apply(opening);
                    BeginMessage(pickup.Message);
                });
            }
            foreach (StationActivationEvent station in step.StationActivations)
                _waitingMessages.Enqueue(() => station.Open(opening));
            OpenNextMessage();
            SpawnThrownProjectiles(step.ThrownProjectiles);
        }
    }

    /// <summary>
    /// Opens the next message box the last room-object step made, if one waits (#2553): on the step's own update for the
    /// first, and as the box before it closes for each later one, as the game's next room object opens its box once the
    /// box before it has returned.
    /// </summary>
    internal void OpenNextMessage()
    {
        if (_waitingMessages.TryDequeue(out Action? open))
            open();
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
        /// <remarks>A second suit's box closes into its own transformation, which restarts the first's, as the game's does.</remarks>
        public void AwaitSuitTransformation(SamusSuitPickupKind suit) =>
            phase._messageBoxes.Await(new MessageBoxOutcome.SuitTransformation(suit));

        /// <inheritdoc/>
        public void RequestSaveConfirmation(SaveStationActivation save)
        {
            phase._messageBoxes.Await(new MessageBoxOutcome.SaveConfirmation(save));
            phase.BeginMessage(GameplayMessageId.SaveConfirmation);
        }

        /// <inheritdoc/>
        public void ShowSaveCompleted(SaveStationActivation save)
        {
            phase._messageBoxes.Await(new MessageBoxOutcome.SaveCompletion(save));
            phase.BeginMessage(GameplayMessageId.SaveCompleted);
        }

        /// <inheritdoc/>
        public void ShowStationMessage(GameplayMessageId message) => phase.BeginMessage(message);
    }
}
