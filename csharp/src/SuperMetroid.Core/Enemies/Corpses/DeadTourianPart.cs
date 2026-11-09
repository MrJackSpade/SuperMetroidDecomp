namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Which Tourian corpse occupant a slot holds, one per definition the species installs.</summary>
internal enum DeadTourianPart
{
    /// <summary>The <see cref="EnemyDefinitionId.CorpseRipper"/> occupant.</summary>
    CorpseRipper,
    /// <summary>The <see cref="EnemyDefinitionId.CorpseSkree"/> occupant.</summary>
    CorpseSkree,
    /// <summary>The <see cref="EnemyDefinitionId.CorpseZoomer"/> occupant.</summary>
    CorpseZoomer,
}
