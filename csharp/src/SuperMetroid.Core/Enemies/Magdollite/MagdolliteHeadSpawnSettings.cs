namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite head's placement: how it and its arm rise and how close Samus must come before they do.</summary>
/// <param name="Speed">The speed, in pixels per update, at which the head's arm rises and sinks.</param>
/// <param name="Range">The horizontal distance in pixels Samus must come within.</param>
internal sealed partial record MagdolliteHeadSpawnSettings(SignedSixteenSixteen Speed, int Range) : MagdolliteSpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyOccupant Create(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.FromSurroundings(new MagdolliteHeadEntity(species, occupant, place, common));
}
