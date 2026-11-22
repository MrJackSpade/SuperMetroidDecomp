using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One of Kraid's three lints (#2146): it buds from his belly and flies at Samus, with the shared reactions.</summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the lint occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="lint">Which of his lints it is, which sets where it buds.</param>
/// <param name="encounter">The encounter his body began, which the part joins as it is built.</param>
internal sealed partial class KraidLintEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, KraidLintPart lint,
    KraidEnemyState encounter)
    : KraidEntity(species, occupant, place, common, () => species.Projectiles.InitializeKraidLint(place, lint, encounter)), IEnemyTypedAnimation
{

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Projectiles.RunKraidLintMain(Occupant, Encounter.Lint(lint), frame.Samus, frame.PseudoScrew);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Limbs.AdvanceKraidPartAnimation(Encounter.Lint(lint));
}
