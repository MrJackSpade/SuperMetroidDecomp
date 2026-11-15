using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Places a Ceres door or wall in a fixture slot as a placement of it would (#1953, #1989).</summary>
internal static class CeresDoorOrWallSlots
{
    /// <summary>Binds <paramref name="slot"/> to the Ceres door or wall <paramref name="actor"/> placed with <paramref name="settings"/>, without starting it.</summary>
    internal static void BindCeresDoorOrWall(this RoomEnemySlot slot, EnemyDefinitionId actor, EnemySpawnSettings settings)
    {
        slot.SetSpawnSettings(settings);
        slot.Bind(actor, slot.Definition);
    }

    /// <summary>Binds <paramref name="slot"/> to the Ceres door or wall <paramref name="actor"/> placed with <paramref name="settings"/> and runs its initialization.</summary>
    internal static void StartCeresDoorOrWall(this RoomEnemySlot slot, EnemyDefinitionId actor, EnemySpawnSettings settings)
    {
        slot.BindCeresDoorOrWall(actor, settings);
        EnemyLifecycle.RunInitializationAi(slot, EnemySpawnSurroundings.None);
    }

    /// <summary>The elevator door <paramref name="slot"/> holds.</summary>
    internal static CeresElevatorDoorEntity ElevatorDoor(this RoomEnemySlot slot) =>
        slot.Entity as CeresElevatorDoorEntity
            ?? throw new InvalidOperationException($"Enemy slot {slot.SlotIndex} holds no Ceres elevator door.");
}
