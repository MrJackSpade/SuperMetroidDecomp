namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's rising body: the arm that grows and shrinks the lava pillar under its head.</summary>
internal sealed record MagdolliteBodySpawnSettings : MagdolliteSpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyOccupant Create(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Of(new MagdolliteBodyEntity(species, occupant, place, common));
}
