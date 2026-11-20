namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's head: it faces Samus, sinks into the lava to start an attack and emerges when its arm lands.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="place">The place the head occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">How the enemy comes to be set up, whose surroundings its initialization reads.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class MagdolliteHeadEntity(MagdolliteBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, MagdolliteHeadSpawnSettings settings)
    : MagdollitePartEntity(species, place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal MagdolliteHeadState State { get; } = species.InitializeMagdolliteHead(place, setup.SurroundingsOf(place).Samus, settings);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        Species.RunMagdolliteHead(Place, State, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        Species.AdvanceMagdolliteTypedAnimation(Place, State, context);
}
