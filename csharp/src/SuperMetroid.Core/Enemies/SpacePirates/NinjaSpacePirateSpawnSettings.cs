namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a ninja space pirate patrols (#1472, #1519).</summary>
/// <param name="StartPost">The post it starts at.</param>
/// <param name="PostDistance">The pixels from its placement to each post.</param>
internal sealed record NinjaSpacePirateSpawnSettings(NinjaSpacePirateStartPost StartPost, int PostDistance) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
