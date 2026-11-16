using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of the tubes that fall in Mother Brain's fake death (#2146): its fall and animation, with the shared reactions its
/// definition names.
/// </summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="place">The place the tube occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MotherBrainTubesEntity(MotherBrainBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : MotherBrainEntity(species, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => MotherBrainFakeDeath.InitializeMotherBrainFallingTube(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.FakeDeath.RunMotherBrainFallingTubeMain(Place);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MotherBrainBody.AdvanceMotherBrainTubeAnimation(Place);
}
