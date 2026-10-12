using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>Domain-owned transitions over <see cref="GreyDoorCondition"/>.</summary>
public static class GreyDoorConditions
{
    /// <summary>The condition at <paramref name="index"/> of <c>$84:BE4B</c>'s seven-entry table.</summary>
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
