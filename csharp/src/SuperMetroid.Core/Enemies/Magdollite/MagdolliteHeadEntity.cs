namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's head: it faces Samus, sinks into the lava to start an attack and emerges when its arm lands.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the head occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="surroundings">The room, Samus, controller and camera the population load sets the head up in.</param>
internal sealed class MagdolliteHeadEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    EnemyPopulationSurroundings surroundings)
    : MagdollitePartEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    internal override MagdollitePartState State => Head;

    /// <summary>The head's state, which its setup builds as the enemy is built (#2554).</summary>
    private MagdolliteHeadState Head { get; } = species.InitializeMagdolliteHead(occupant, place, Ensure.NotNull(surroundings).Samus);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        Species.RunMagdolliteHead(Occupant, Occupant.SpawnSettings<MagdolliteHeadSpawnSettings>(), Place, Head, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        Species.AdvanceMagdolliteTypedAnimation<MagdolliteHeadState>(Occupant, Place, Head, context);
}
