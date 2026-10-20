namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The definitions the Gunship species installs and the part each one is.</summary>
internal static class GunshipPartDefinitions
{
    /// <summary>Each Gunship definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, GunshipPart> Parts { get; } = new Dictionary<EnemyDefinitionId, GunshipPart>
    {
        [EnemyDefinitionId.ShipBottomEntrance] = GunshipPart.ShipBottomEntrance,
        [EnemyDefinitionId.ShipTop] = GunshipPart.ShipTop,
    };
}
