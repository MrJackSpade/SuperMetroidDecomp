using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's foot (#2146): it follows his walk, with the limbs' own shot and bomb reactions.</summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="place">The place the foot occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KraidFootEntity(KraidBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : KraidLimbEntity(species, place, common, species.Parts.FootReference)
{
    /// <inheritdoc/>
    protected override void SetUp() => Species.Limbs.InitializeKraidFoot(Place);

    /// <summary>The foot's own part of the encounter.</summary>
    protected override KraidAnimatedPartState Part => Encounter.FootState;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Limbs.RunKraidFootMain(Place, Encounter.FootState, frame.Camera.Y);
}
