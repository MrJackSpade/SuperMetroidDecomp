namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The way each of a Mama Turtle's four babies first crawls (#1472, #1519, #2338), in the order her room lists them right
/// after her: her placement, which heads them, names it; theirs name nothing (#2554).
/// </summary>
/// <param name="BabyHeadings">The way each baby crawls from its spawn, in listing order.</param>
internal sealed record MamaTurtleSpawnSettings(IReadOnlyList<HorizontalDirection> BabyHeadings) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
