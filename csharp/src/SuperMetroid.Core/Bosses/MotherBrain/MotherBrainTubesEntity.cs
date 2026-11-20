using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of the tubes that fall in Mother Brain's fake death (#2146): its fall and animation, with the shared reactions its
/// definition names.
/// </summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the tube occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MotherBrainTubesEntity(MotherBrainBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : MotherBrainEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => MotherBrainFakeDeath.InitializeMotherBrainFallingTube(Occupant, Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.FakeDeath.RunMotherBrainFallingTubeMain(Place, Place.RequireSpeciesState<MotherBrainFallingTubeState>(), Place.RequireSpeciesState<MotherBrainEnemyState>(), Place.RequireSpeciesState<MotherBrainFallingTubeState>());

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MotherBrainBody.AdvanceMotherBrainTubeAnimation(Place, Place.RequireSpeciesState<MotherBrainFallingTubeState>());
}
