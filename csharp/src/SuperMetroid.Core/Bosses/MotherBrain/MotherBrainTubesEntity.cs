using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of the tubes that fall in Mother Brain's fake death (#2146): its fall and animation, with the shared reactions its
/// definition names.
/// </summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="place">The place the tube occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">The spawn joining the encounter the tube falls in.</param>
internal sealed class MotherBrainTubesEntity(MotherBrainBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    : MotherBrainEntity(species, place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal MotherBrainFallingTubeState State { get; } = MotherBrainFakeDeath.InitializeMotherBrainFallingTube(place);

    /// <inheritdoc/>
    /// <remarks>The tube holds the encounter that spawned it from its setup (#2218).</remarks>
    internal override MotherBrainEnemyState Encounter { get; } = setup.EncounterOf<MotherBrainEnemyState>(place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.FakeDeath.RunMotherBrainFallingTubeMain(Place, State);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MotherBrainBody.AdvanceMotherBrainTubeAnimation(Place, State);
}
