using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's two fingernails (#2146): it flies and bounces about his room, and runs its own touch where its
/// definition leaves Samus's touch to it.
/// </summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="place">The place the fingernail occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="kraid">The encounter his body began, which the part joins.</param>
/// <param name="nail">Which of his fingernails it is, whose state it holds from its setup (#2298).</param>
internal sealed class KraidNailEntity(KraidBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, KraidEnemyState kraid, KraidNailPart nail)
    : KraidEntity(species, place, common), IEnemyTypedAnimation
{
    /// <inheritdoc/>
    internal override KraidEnemyState Encounter { get; } = KraidProjectiles.InitializeKraidNail(place, kraid, nail);

    /// <inheritdoc/>
    internal override KraidPartState PartState => Encounter.Nail(nail);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Projectiles.RunKraidNailMain(Place, Encounter.Nail(nail), frame.Level);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Limbs.AdvanceKraidPartAnimation(Encounter.Nail(nail));

    /// <summary>Whether the fingernail's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the fingernail's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            Species.Projectiles.ReactToKraidNailTouch(context);
        else
            base.ReactToTouch(context);
    }
}
