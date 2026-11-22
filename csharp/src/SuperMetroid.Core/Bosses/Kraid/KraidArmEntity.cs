using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's arm (#2146): its swipe, its push of Samus, and its own shot reaction.</summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the arm occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="encounter">The encounter his body began, which the part joins as it is built.</param>
internal sealed partial class KraidArmEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    KraidEnemyState encounter)
    : KraidLimbEntity(species, occupant, place, common, species.Limbs.InitializeKraidArm(place, encounter)), IEnemyPrivateShotReaction
{
    /// <summary>The <c>encounter</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
    private readonly KraidEnemyState encounter = encounter;


    /// <summary>The arm's own part of the encounter.</summary>
    internal override KraidAnimatedPartState Part => Encounter.ArmState;

    /// <summary>The arm's hitboxes, which its frame's components hold.</summary>
    public override EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe) =>
        KraidCombat.FindKraidArmHitboxReaction(Place, probe);

    /// <summary>The arm's shot reaction.</summary>
    protected override EnemyShotReaction LimbShotReaction => Species.Combat.ArmShotReaction;

    /// <summary>
    /// Runs the arm's own reaction to every bomb that overlaps it, before the ignoring check: a bomb its hitboxes select
    /// (#2301).
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        Species.Combat.ReactToArmBomb(context);
        return true;
    }

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => KraidLimbs.RunKraidArmMain(Place, Encounter, frame.Camera.Y);

    /// <summary>Whether the arm's push, where its definition leaves Samus's touch to it, or a shared touch applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == KraidReactions.ArmTouch || base.ReactsToTouch;

    /// <summary>Runs the arm's push where its definition leaves Samus's touch to it, or the shared touch.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == KraidReactions.ArmTouch)
            Species.Combat.ReactToArmTouch(context, Encounter);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The arm's shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == KraidReactions.ArmShot;
}
