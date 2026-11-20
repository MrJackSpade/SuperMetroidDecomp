using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's two fingernails (#2146): it flies and bounces about his room, and runs its own touch where its
/// definition leaves Samus's touch to it.
/// </summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the fingernail occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="reference">Kraid's reference to the fingernail, which it joins as it initializes.</param>
/// <param name="nail">Which of his fingernails it is, whose state it holds from its setup (#2298).</param>
internal sealed class KraidNailEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, KraidPartReference reference, KraidNailPart nail)
    : KraidEntity(species, occupant, place, common, reference), IEnemyTypedAnimation
{
    /// <inheritdoc/>
    protected override void SetUp() => Species.Projectiles.InitializeKraidNail(Place, nail);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Projectiles.RunKraidNailMain(Place, Encounter.Nail(nail), frame.Level);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Limbs.AdvanceKraidPartAnimation(Encounter.Nail(nail));

    /// <summary>Whether the fingernail's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Place.Definition.Touch == KraidReactions.NailTouch || base.ReactsToTouch;

    /// <summary>Runs the fingernail's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Place.Definition.Touch == KraidReactions.NailTouch)
            Species.Projectiles.ReactToKraidNailTouch(context);
        else
            base.ReactToTouch(context);
    }
}
