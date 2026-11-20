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
/// <param name="settings">What the tube's spawn tells its species.</param>
internal sealed class MotherBrainTubesEntity(MotherBrainBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, MotherBrainTubeSpawnSettings settings)
    : MotherBrainEntity(species, occupant, place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal MotherBrainFallingTubeState State { get; } = MotherBrainFakeDeath.InitializeMotherBrainFallingTube(place, settings);

    /// <inheritdoc/>
    /// <remarks>The tube holds the encounter that spawned it from its setup (#2218).</remarks>
    internal override MotherBrainEnemyState Encounter { get; } = setup.EncounterOf<MotherBrainEnemyState>(place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.FakeDeath.RunMotherBrainFallingTubeMain(Place, State, Encounter);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MotherBrainBody.AdvanceMotherBrainTubeAnimation(Place, State);
}
