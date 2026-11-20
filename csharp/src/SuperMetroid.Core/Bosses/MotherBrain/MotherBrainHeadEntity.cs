using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's head (#2146): its own AI, and the touch, shots and bombs its definition leaves to it.
/// </summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="place">The place the head occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MotherBrainHeadEntity(MotherBrainBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : MotherBrainEntity(species, place, common), IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal MotherBrainEnemyState State { get; } = species.Head.InitializeMotherBrainHead(place);

    /// <inheritdoc/>
    internal override MotherBrainEnemyState Encounter => State;

    /// <summary>The head's hitbox: her brain's rectangle, which it owns (#2220).</summary>
    public override bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) =>
        MotherBrainCombat.FindMotherBrainHeadHitboxReaction(Place, probe, out reaction);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => MotherBrainHead.RunMotherBrainHeadMain(Place, Encounter, frame.Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MotherBrainHead.AdvanceMotherBrainHeadPlace(Place);

    /// <summary>The head's touch reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesTouch => MotherBrainReactions.HeadTouch;

    /// <summary>The head's shot reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesShot => MotherBrainReactions.HeadShot;

    /// <summary>Whether the head's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the head's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            MotherBrainCombat.ReactToMotherBrainHeadTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The head's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? Species.Combat.HeadShotReaction : base.ShotReaction;

    /// <summary>Runs the head's own reaction to a bomb that selects its shot reaction, before the ignoring check.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (context.SelectedShot != MotherBrainReactions.HeadShot)
            return base.TryReactToBomb(context);
        Species.Combat.ReactToMotherBrainHeadBomb(context);
        return true;
    }

    /// <summary>
    /// The shared Power Bomb reaction; the head leaves the blast to the species, which has no reaction to it, and fails
    /// loudly if a blast reaches it.
    /// </summary>
    /// <exception cref="InvalidDataException">The head's definition leaves the blast to the species.</exception>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies
        ? throw new InvalidDataException($"Enemy {Place.DefinitionId} has no response to its own Power Bomb reaction.")
        : base.PowerBombReaction;

    /// <summary>Shots meet the head's frames' hitboxes; its contact uses the definition's radius.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The head's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction == MotherBrainReactions.HeadShot;
}
