using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>The seven entries in <c>$84:BE4B</c>'s grey-door condition table.</summary>
public enum GreyDoorCondition : byte
{
    /// <summary>Condition-table index 0: the current area's major-boss bit must be set.</summary>
    AreaBossDefeated,
    /// <summary>Condition-table index 1: the current area's miniboss bit must be set.</summary>
    AreaMiniBossDefeated,
    /// <summary>Condition-table index 2: the current area's Torizo boss bit must be set.</summary>
    AreaTorizoDefeated,
    /// <summary>Condition-table index 3: room enemy deaths must reach the configured quota; satisfying it also sets the Zebes-awake event.</summary>
    EnemyDeathQuota,
    /// <summary>Condition-table index 4: this door remains locked regardless of shots or progression.</summary>
    Never,
    /// <summary>Condition-table index 5: the live Tourian statue owner must report its sequence finished.</summary>
    TourianStatueFinished,
    /// <summary>Condition-table index 6: the persistent critters-escaped event must be set.</summary>
    CrittersEscaped,
}
