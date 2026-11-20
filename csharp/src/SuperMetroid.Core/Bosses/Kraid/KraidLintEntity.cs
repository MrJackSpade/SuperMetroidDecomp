using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One of Kraid's three lints (#2146): it buds from his belly and flies at Samus, with the shared reactions.</summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="place">The place the lint occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="reference">Kraid's reference to the lint, which it joins as it initializes.</param>
/// <param name="lint">Which of his lints it is, which sets where it buds.</param>
internal sealed class KraidLintEntity(KraidBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, KraidPartReference reference, KraidLintPart lint)
    : KraidEntity(species, place, common), IEnemyTypedAnimation
{
    /// <inheritdoc/>
    internal override KraidEnemyState Encounter { get; } = species.Projectiles.InitializeKraidLint(place, reference, lint);

    /// <inheritdoc/>
    internal override KraidPartState PartState => Encounter.Lint(lint);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Projectiles.RunKraidLintMain(Place, Encounter.Lint(lint), frame.Samus, frame.PseudoScrew);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Limbs.AdvanceKraidPartAnimation(Encounter.Lint(lint));
}
