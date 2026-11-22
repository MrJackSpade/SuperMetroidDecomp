namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The Tourian corpse definitions the corpses' species installs and the species whose corpse each places.</summary>
internal static partial class DeadTourianCorpseSpeciesDefinitions
{
    /// <summary>Each Tourian corpse definition and the species whose corpse it places.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, DeadTourianCorpseSpecies> Species { get; } = new Dictionary<EnemyDefinitionId, DeadTourianCorpseSpecies>
    {
        [EnemyDefinitionId.CorpseRipper] = DeadTourianCorpseSpecies.Ripper,
        [EnemyDefinitionId.CorpseSkree] = DeadTourianCorpseSpecies.Skree,
        [EnemyDefinitionId.CorpseZoomer] = DeadTourianCorpseSpecies.Zoomer,
    };
}
