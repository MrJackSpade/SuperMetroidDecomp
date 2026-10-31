namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Domain-owned transitions over <see cref="GreyDoorCondition"/>.</summary>
public static class GreyDoorConditions
{
    /// <summary>The number of conditions in the grey-door condition table.</summary>
    internal const int Count = 7;

    /// <summary>The condition at <paramref name="index"/> of the seven-entry grey-door condition table.</summary>
    public static GreyDoorCondition AtTableIndex(int index) => index switch
    {
        0 => GreyDoorCondition.AreaBossDefeated,
        1 => GreyDoorCondition.AreaMiniBossDefeated,
        2 => GreyDoorCondition.AreaTorizoDefeated,
        3 => GreyDoorCondition.EnemyDeathQuota,
        4 => GreyDoorCondition.Never,
        5 => GreyDoorCondition.TourianStatueFinished,
        6 => GreyDoorCondition.CrittersEscaped,
        _ => throw new InvalidOperationException($"Grey-door condition table index {index} has no {nameof(GreyDoorCondition)}."),
    };
}
