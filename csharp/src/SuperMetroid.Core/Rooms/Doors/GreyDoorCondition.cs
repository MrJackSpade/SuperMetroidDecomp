namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The seven entries of the grey-door condition table.</summary>
public enum GreyDoorCondition
{
    /// <summary>Condition-table index 0: the current area's major boss must be defeated.</summary>
    AreaBossDefeated,
    /// <summary>Condition-table index 1: the current area's mini-boss must be defeated.</summary>
    AreaMiniBossDefeated,
    /// <summary>Condition-table index 2: the current area's Torizo must be defeated.</summary>
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
