namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What unlocks a grey door cap.</summary>
public enum GreyDoorCondition
{
    /// <summary>The current area's major boss must be defeated.</summary>
    AreaBossDefeated,
    /// <summary>The current area's mini-boss must be defeated.</summary>
    AreaMiniBossDefeated,
    /// <summary>The current area's Torizo must be defeated.</summary>
    AreaTorizoDefeated,
    /// <summary>Room enemy deaths must reach the configured quota; satisfying it also sets the Zebes-awake event.</summary>
    EnemyDeathQuota,
    /// <summary>This door remains locked regardless of shots or progression.</summary>
    Never,
    /// <summary>The live Tourian statue owner must report its sequence finished.</summary>
    TourianStatueFinished,
    /// <summary>The persistent critters-escaped event must be set.</summary>
    CrittersEscaped,
}
