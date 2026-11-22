namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One entry of a room's enemy population (#1472): which enemy spawns, where, how it meets Samus, what the room's enemy
/// passes run for it, whether it is present, whether it collides by its frames' hitboxes, and the settings its species
/// starts from.
/// </summary>
/// <param name="DefinitionId">The enemy that spawns.</param>
/// <param name="Placing">Where the entry places its enemy: at a room position, or by its species' setup (#2558).</param>
/// <param name="Contact">How the enemy starts out meeting Samus and her shots.</param>
/// <param name="Processing">What the room's enemy passes start out running for the enemy.</param>
/// <param name="Presence">Whether the enemy starts hidden or deleted, and whether it returns after a kill.</param>
/// <param name="CollidesByFrameHitboxes">Whether the enemy collides by its frames' hitboxes rather than by its radius.</param>
/// <param name="SpawnSettings">What the entry tells its species about how to start, decoded by the import (#1471, #1472).</param>
public sealed partial record RoomEnemyPlacement(
    EnemyDefinitionId DefinitionId,
    EnemyPlacing Placing,
    EnemyContact Contact,
    EnemyProcessing Processing,
    EnemyPresence Presence,
    bool CollidesByFrameHitboxes,
    EnemySpawnSettings SpawnSettings)
{
    /// <summary>What the entry tells its species about how to start.</summary>
    public EnemySpawnSettings SpawnSettings { get; } = Ensure.NotNull(SpawnSettings);
}
