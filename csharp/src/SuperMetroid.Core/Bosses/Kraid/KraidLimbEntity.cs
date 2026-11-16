using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's arm (<see cref="KraidArmEntity"/>) or foot (<see cref="KraidFootEntity"/>): both animate as his parts do, meet
/// Samus and her shots by their frames' hitboxes, and run their own shot and bomb reactions behind their ignoring
/// definitions.
/// </summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="place">The place the limb occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="reference">Kraid's reference to the limb, which it joins as it initializes.</param>
internal abstract class KraidLimbEntity(KraidBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, KraidPartReference reference)
    : KraidEntity(species, place, common, reference), IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Limbs.AdvanceKraidPartAnimation(Place);

    /// <summary>The limb's shot reaction behind its ignoring definition, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction =>
        Place.Definition.Shot == EnemyReaction.Ignore ? Species.Combat.ArmAndFootShotReaction : base.ShotReaction;

    /// <summary>Runs the limb's own reaction to every bomb that overlaps it, before the ignoring check.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        Species.Combat.ReactToArmOrFootBomb(context);
        return true;
    }

    /// <summary>The limb's contact and shots meet its frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;
}
