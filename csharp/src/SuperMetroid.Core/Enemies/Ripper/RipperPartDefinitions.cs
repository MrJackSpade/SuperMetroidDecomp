namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The definitions the Ripper species installs and the part each one is.</summary>
internal static class RipperPartDefinitions
{
    /// <summary>Each Ripper definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, RipperPart> Parts { get; } = new Dictionary<EnemyDefinitionId, RipperPart>
    {
        [EnemyDefinitionId.GrappleRipper] = RipperPart.GrappleRipper,
        [EnemyDefinitionId.Ripper] = RipperPart.Ripper,
        [EnemyDefinitionId.JetRipper] = RipperPart.JetRipper,
    };
}
