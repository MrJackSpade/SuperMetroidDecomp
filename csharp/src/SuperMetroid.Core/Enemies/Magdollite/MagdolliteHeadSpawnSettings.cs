namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite head's placement: how it and its arm rise and how close Samus must come before they do.</summary>
/// <param name="Speed">The speed, in pixels per update, at which the head's arm rises and sinks.</param>
/// <param name="Range">The horizontal distance in pixels Samus must come within.</param>
internal sealed record MagdolliteHeadSpawnSettings(SignedSixteenSixteen Speed, int Range) : MagdolliteSpawnSettings
{
    /// <inheritdoc/>
    internal override IEnemyEntity Create(MagdolliteBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup) =>
        new MagdolliteHeadEntity(species, occupant, place, common, setup, this);
}
