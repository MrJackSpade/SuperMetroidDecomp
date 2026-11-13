namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The kind of hopper each hopper definition is (#1624).</summary>
internal static class HopperKindDefinitions
{
    /// <summary>Each hopper definition's kind: the Tourian Sidehopper is a large Sidehopper.</summary>
    internal static readonly Dictionary<EnemyDefinitionId, HopperKind> Kinds = new()
    {
        [EnemyDefinitionId.Sidehopper] = HopperKind.Sidehopper,
        [EnemyDefinitionId.SidehopperLarge] = HopperKind.LargeSidehopper,
        [EnemyDefinitionId.SidehopperTourian] = HopperKind.LargeSidehopper,
        [EnemyDefinitionId.DessgeegaLarge] = HopperKind.LargeDessgeega,
        [EnemyDefinitionId.Dessgeega] = HopperKind.Dessgeega,
    };
}
