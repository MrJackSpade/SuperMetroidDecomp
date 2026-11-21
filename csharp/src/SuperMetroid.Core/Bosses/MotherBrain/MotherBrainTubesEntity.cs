using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of the tubes that fall in Mother Brain's fake death (#2146): its fall and animation, with the shared reactions its
/// definition names. It joins the encounter whose fake death drops it as it is built (#2218, #2554).
/// </summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the tube occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">The piece of the tube it is.</param>
/// <param name="encounter">The encounter the tube joins.</param>
internal sealed class MotherBrainTubesEntity(MotherBrainBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    MotherBrainTubeSpawnSettings settings, MotherBrainEnemyState encounter)
    : MotherBrainEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    internal override MotherBrainEnemyState Encounter { get; } = Ensure.NotNull(encounter);

    /// <summary>The tube's fall, which its setup builds.</summary>
    private readonly MotherBrainFallingTubeState tube = MotherBrainFakeDeath.InitializeMotherBrainFallingTube(settings, place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.FakeDeath.RunMotherBrainFallingTubeMain(Place, Encounter, tube);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MotherBrainBody.AdvanceMotherBrainTubeAnimation(Place, tube);
}
