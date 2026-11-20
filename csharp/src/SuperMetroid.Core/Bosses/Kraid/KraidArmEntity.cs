using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's arm (#2146): its swipe, its push of Samus, and its own shot reaction.</summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="place">The place the arm occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="kraid">The encounter his body began, which the part joins.</param>
internal sealed class KraidArmEntity(KraidBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, KraidEnemyState kraid)
    : KraidLimbEntity(species, place, common), IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    internal override KraidEnemyState Encounter { get; } = species.Limbs.InitializeKraidArm(place, kraid);

    /// <inheritdoc/>
    internal override KraidPartState PartState => Encounter.ArmState;

    /// <summary>The arm's own part of the encounter.</summary>
    protected override KraidAnimatedPartState Part => Encounter.ArmState;

    /// <summary>The arm's hitboxes, which its frame's components hold.</summary>
    public override bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) =>
        KraidCombat.FindKraidArmHitboxReaction(Place, probe, out reaction);

    /// <summary>The arm's shot reaction.</summary>
    protected override EnemyShotReaction LimbShotReaction => Species.Combat.ArmShotReaction;

    /// <summary>The arm's bomb reaction.</summary>
    protected override void ReactToLimbBomb(EnemyBombContext context) => Species.Combat.ReactToArmBomb(context);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Limbs.RunKraidArmMain(Place, Encounter, frame.Camera.Y);

    /// <summary>The arm's touch reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesTouch => KraidReactions.ArmTouch;

    /// <summary>Whether the arm's push, where its definition leaves Samus's touch to it, or a shared touch applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the arm's push where its definition leaves Samus's touch to it, or the shared touch.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            Species.Combat.ReactToArmTouch(context, Encounter);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The arm's shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction == KraidReactions.ArmShot;
}
