namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What holds the species registry a room's enemy places create their entities with (#2553): the room's enemy system,
/// whose places exist before the species composed with them, or a registry that already exists.
/// </summary>
internal interface IEnemySpeciesSource
{
    /// <summary>The species registry that creates the entity for each occupant a place installs.</summary>
    EnemySpeciesRegistry Species { get; }
}