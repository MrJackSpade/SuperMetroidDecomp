using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's two fingernails (#2146): it flies and bounces about his room, and runs its own touch where its
/// definition leaves Samus's touch to it.
/// </summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="place">The place the fingernail occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="reference">Kraid's reference to the fingernail, which it joins as it initializes.</param>
internal sealed class KraidNailEntity(KraidBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, KraidPartReference reference)
    : KraidEntity(species, place, common, reference), IEnemyTypedAnimation
{
    /// <inheritdoc/>
    protected override void SetUp() => Species.Projectiles.InitializeKraidNail(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Projectiles.RunKraidNailMain(Place, Species.Parts.NailStateOf(Encounter, Place), frame.Level);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Limbs.AdvanceKraidPartAnimation(Place);

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
