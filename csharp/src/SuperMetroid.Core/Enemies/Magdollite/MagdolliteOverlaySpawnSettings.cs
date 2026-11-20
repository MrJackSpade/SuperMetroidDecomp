namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's tracking overlay: the hand that follows the pillar's top and throws the lava.</summary>
internal sealed record MagdolliteOverlaySpawnSettings : MagdolliteSpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyOccupant Create(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Of(new MagdolliteOverlayEntity(species, occupant, place, common));
}
