using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One of Kraid's three lints (#2146): it buds from his belly and flies at Samus, with the shared reactions.</summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the lint occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="lint">Which of his lints it is, which sets where it buds.</param>
/// <param name="encounter">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class KraidLintEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, KraidLintPart lint, KraidEnemyState encounter)
    : KraidEntity(species, occupant, place, common, encounter), IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Kraid species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the lint occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="lint">Which of his lints it is, which sets where it buds.</param>
    /// <param name="encounter">The encounter his body began, which the part joins as it is built.</param>
    /// <returns>The enemy, set up.</returns>
    internal static KraidLintEntity SetUp(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        KraidLintPart lint, KraidEnemyState encounter) =>
        new(species, occupant, place, common, lint, species.Projectiles.InitializeKraidLint(place, lint, encounter));

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Projectiles.RunKraidLintMain(Occupant, Encounter.Lint(lint), frame.Samus, frame.PseudoScrew);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Limbs.AdvanceKraidPartAnimation(Encounter.Lint(lint));
}
