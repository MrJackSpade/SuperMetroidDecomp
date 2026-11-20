namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species owner that builds the entities of the definitions it owns (#2554): it registers each creation it supports,
/// and only those, with the settings type that creation takes, so the registry builds an enemy only through a creation its
/// species has and hands it the settings its population entry carries, checked once where the entry is bound.
/// </summary>
internal interface IEnemyEntityFactory
{
    /// <summary>Registers the creations this species supports for the definitions it owns; each definition has one owner.</summary>
    /// <param name="registrar">The room's registry being composed.</param>
    void Register(EnemySpeciesRegistrar registrar);
}
