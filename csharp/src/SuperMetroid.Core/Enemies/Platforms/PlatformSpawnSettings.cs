namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>How a moving platform travels (#1472, #1519).</summary>
/// <param name="Movement">The way it moves.</param>
/// <param name="Speed">The speed, in pixels per update, at which it moves sideways.</param>
/// <param name="VerticalAccelerationUpdates">
/// The accelerating updates after which its rising or sinking speed stops growing: each accelerating update takes the
/// next step of the shared quadratic speed curve until this many steps are taken.
/// </param>
internal sealed record PlatformSpawnSettings(HorizontalDirection Movement, SignedSixteenSixteen Speed, int VerticalAccelerationUpdates) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
