namespace SuperMetroid.Core.Progression;

/// <summary>Each area of the game (<see cref="AreaId"/>).</summary>
internal static class AreaDefinitions
{
    /// <summary>Each area: Ceres station forbids pausing; the Wrecked Ship has no power until Phantoon is defeated.</summary>
    internal static readonly IdentityTable<AreaId, AreaDefinition> Areas = new(new Dictionary<AreaId, AreaDefinition>
    {
        [AreaId.Crateria] = new(AllowsPausing: true, PoweredFromBossDefeat: false),
        [AreaId.Brinstar] = new(AllowsPausing: true, PoweredFromBossDefeat: false),
        [AreaId.Norfair] = new(AllowsPausing: true, PoweredFromBossDefeat: false),
        [AreaId.WreckedShip] = new(AllowsPausing: true, PoweredFromBossDefeat: true),
        [AreaId.Maridia] = new(AllowsPausing: true, PoweredFromBossDefeat: false),
        [AreaId.Tourian] = new(AllowsPausing: true, PoweredFromBossDefeat: false),
        [AreaId.Ceres] = new(AllowsPausing: false, PoweredFromBossDefeat: false),
    });
}
