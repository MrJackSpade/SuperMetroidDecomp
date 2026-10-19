namespace SuperMetroid.Core.Enemies;

/// <summary>Authors a fixture slot's spawn parameters, which the enemy's initialization and authored reads take.</summary>
internal static class RoomEnemySlotSpawnParameters
{
    /// <summary>Sets the slot's authored first spawn parameter, 0 through 65535.</summary>
    internal static void SetSpawnParameter1(this RoomEnemySlot slot, int value) =>
        slot.Spawn = slot.Spawn with { Population = slot.Spawn.Population with { Parameter1 = value } };

    /// <summary>Sets the slot's authored second spawn parameter, 0 through 65535.</summary>
    internal static void SetSpawnParameter2(this RoomEnemySlot slot, int value) =>
        slot.Spawn = slot.Spawn with { Population = slot.Spawn.Population with { Parameter2 = value } };
}
