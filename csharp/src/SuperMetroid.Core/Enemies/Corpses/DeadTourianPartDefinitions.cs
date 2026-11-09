namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The definitions the Tourian corpses' species installs and the part each one is.</summary>
internal static class DeadTourianPartDefinitions
{
    /// <summary>Each Tourian corpse definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, DeadTourianPart> Parts { get; } = new Dictionary<EnemyDefinitionId, DeadTourianPart>
    {
        [EnemyDefinitionId.CorpseRipper] = DeadTourianPart.CorpseRipper,
        [EnemyDefinitionId.CorpseSkree] = DeadTourianPart.CorpseSkree,
        [EnemyDefinitionId.CorpseZoomer] = DeadTourianPart.CorpseZoomer,
    };
}
