namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's rising body: the arm that grows and shrinks the lava pillar under its head.</summary>
internal sealed record MagdolliteBodySpawnSettings : MagdolliteSpawnSettings
{
    /// <inheritdoc/>
    internal override IEnemyEntity Create(MagdolliteBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup) =>
        new MagdolliteBodyEntity(species, place, common);
}
