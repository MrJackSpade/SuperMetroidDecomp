using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.World;

/// <summary>
/// The loaded room a check reads once its runtime has loaded one (#2554). Production code receives the runtime's loaded
/// room where it runs; a check reads it from the runtime it set up, and fails when it reads before that runtime's load.
/// </summary>
internal static class LoadedRoomAccess
{
    /// <summary>The runtime's loaded room, which a check reads only after the runtime's first load.</summary>
    /// <exception cref="InvalidOperationException">The check reads before the runtime loaded a room.</exception>
    private static RoomOccupancy.Loaded LoadedOf(ActiveRoomState room) =>
        Ensure.NotNull(room).Occupancy as RoomOccupancy.Loaded
            ?? throw new InvalidOperationException("The check reads the loaded room before its runtime loaded one.");

    extension(ActiveRoomState room)
    {
        /// <summary>The runtime's loaded room.</summary>
        internal RoomOccupancy.Loaded Loaded => LoadedOf(room);

        /// <summary>The loaded room's scope.</summary>
        internal RoomScope LoadedScope => LoadedOf(room).Scope;

        /// <summary>The loaded room's definition.</summary>
        internal SelectedRoom LoadedRoom => LoadedOf(room).Scope.Room;

        /// <summary>The loaded room's scroll camera.</summary>
        internal ScrollBoundaryCamera LoadedCamera => LoadedOf(room).Scope.Camera;

        /// <summary>The loaded room's level data.</summary>
        internal RoomLevelData LoadedLevelData => LoadedOf(room).Scope.LevelData;
    }

    extension(SuperMetroidRuntime runtime)
    {
        /// <summary>Runs one gameplay update in the runtime's loaded room.</summary>
        internal RuntimeFrameOutcome StepFrame(ControllerButton playerInput, RuntimeFrameRequest request) =>
            runtime.StepFrame(playerInput, request, LoadedOf(runtime.Room));

        /// <summary>Publishes the display of the runtime's loaded room.</summary>
        internal GameplayDisplayFrame PublishDisplayFrame() => runtime.PublishDisplayFrame(LoadedOf(runtime.Room));

        /// <summary>The loaded room's last enemy pass's results.</summary>
        internal IEnemyFrameResults EnemyFrameResults => LoadedOf(runtime.Room).Scope.Enemies;
    }

    extension(DoorTransitionState transition)
    {
        /// <summary>Begins a door transition from the runtime's loaded room through the door Samus entered there.</summary>
        internal void Begin(SuperMetroidRuntime runtime)
        {
            RoomOccupancy.Loaded loaded = LoadedOf(runtime.Room);
            transition.Enter(loaded.Scope.RoomObjects.DoorContacts.EnteredDoor
                ?? throw new InvalidOperationException("The fixture's door transition begins after Samus entered a door."));
            transition.Begin(runtime, loaded);
        }

        /// <summary>Runs one update of the door transition in the runtime's loaded room.</summary>
        internal void Step(SuperMetroidRuntime runtime, AudioQueues audio, ControllerButton controllerInput, IRoomLoadProgress loaderProgress) =>
            transition.Step(runtime, LoadedOf(runtime.Room), audio, controllerInput, loaderProgress);
    }

    extension(FrontendSession session)
    {
        /// <summary>The session's gameplay runtime, which a check reads only once it has created one.</summary>
        /// <exception cref="InvalidOperationException">The session holds no runtime.</exception>
        internal SuperMetroidRuntime GameplayRuntime =>
            session.Runtime ?? throw new InvalidOperationException("The check reads the gameplay runtime before the session created one.");

        /// <summary>Shows Ceres's destruction with the session's runtime.</summary>
        internal void BeginCeresDestruction(CeresDestructionOutcome outcome) => session.BeginCeresDestruction(outcome, session.GameplayRuntime);
    }

    extension(SuperMetroidGame game)
    {
        /// <summary>Advances the random generator as the current phase's update does before its step.</summary>
        internal void AdvanceMenuRandom()
        {
            IGamePhase phase = game.CurrentPhase;
            game.AdvanceMenuRandom(phase, game.MatchGameplay(phase));
        }
    }
}
