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
/// <param name="nail">Which of his fingernails it is, whose state it holds from its setup (#2298).</param>
/// <param name="encounter">The encounter his body began, which the part joins as it is built.</param>
internal sealed class KraidNailEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, KraidNailPart nail,
    KraidEnemyState encounter)
    : KraidEntity(species, occupant, place, common, () => KraidProjectiles.InitializeKraidNail(place, nail, encounter)), IEnemyTypedAnimation
{

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Projectiles.RunKraidNailMain(Place, Part, frame.Level);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Limbs.AdvanceKraidPartAnimation(Part);

    /// <summary>The fingernail's part of the encounter, which its sibling reads in its place.</summary>
    internal KraidNailState Part => Encounter.Nail(nail);

    /// <summary>Whether the fingernail's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == KraidReactions.NailTouch || base.ReactsToTouch;

    /// <summary>Runs the fingernail's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == KraidReactions.NailTouch)
            Species.Projectiles.ReactToKraidNailTouch(context);
        else
            base.ReactToTouch(context);
    }
}
