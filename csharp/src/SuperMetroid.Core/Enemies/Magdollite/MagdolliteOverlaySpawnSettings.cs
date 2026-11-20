namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's tracking overlay: the hand that follows the pillar's top and throws the lava.</summary>
internal sealed record MagdolliteOverlaySpawnSettings : MagdolliteSpawnSettings
{
    /// <inheritdoc/>
    internal override IEnemyEntity Create(MagdolliteBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup) =>
        new MagdolliteOverlayEntity(species, place, common);
}
