using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's head (#2146): its own AI, and the touch, shots and bombs its definition leaves to it.
/// </summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the head occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="encounter">The encounter her body began, which the head joins.</param>
internal sealed class MotherBrainHeadEntity(MotherBrainBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, MotherBrainEnemyState encounter)
    : MotherBrainEntity(species, occupant, place, common), IEnemyPrivateShotReaction
{
    /// <summary>Her head's own shot reaction, built over the encounter (#2554).</summary>
    private EnemyShotReaction OwnShotReaction => field ??= Species.Combat.HeadShotReactionOf(Encounter);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal MotherBrainEnemyState State { get; } = MotherBrainHead.InitializeMotherBrainHead(place, encounter);

    /// <inheritdoc/>
    internal override MotherBrainEnemyState Encounter => State;

    /// <summary>The head's hitbox: her brain's rectangle, which it owns (#2220).</summary>
    public override EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe) =>
        MotherBrainCombat.FindMotherBrainHeadHitboxReaction(Place, probe);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => MotherBrainHead.RunMotherBrainHeadMain(Place, Encounter, frame.Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MotherBrainHead.AdvanceMotherBrainHeadPlace(Place);

    /// <summary>Whether the head's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == MotherBrainReactions.HeadTouch || base.ReactsToTouch;

    /// <summary>Runs the head's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == MotherBrainReactions.HeadTouch)
            MotherBrainCombat.ReactToMotherBrainHeadTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The head's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == MotherBrainReactions.HeadShot ? OwnShotReaction : base.ShotReaction;

    /// <summary>Runs the head's own reaction to a bomb that selects its shot reaction, before the ignoring check.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (context.SelectedShot != MotherBrainReactions.HeadShot)
            return base.TryReactToBomb(context);
        Species.Combat.ReactToMotherBrainHeadBomb(context, Encounter);
        return true;
    }

    /// <summary>
    /// The shared Power Bomb reaction; the head leaves the blast to the species, which has no reaction to it, and fails
    /// loudly if a blast reaches it.
    /// </summary>
    /// <exception cref="InvalidDataException">The head's definition leaves the blast to the species.</exception>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies
        ? throw new InvalidDataException($"Enemy {Place.Occupant?.DefinitionId} has no response to its own Power Bomb reaction.")
        : base.PowerBombReaction;

    /// <summary>Shots meet the head's frames' hitboxes; its contact uses the definition's radius.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The head's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == MotherBrainReactions.HeadShot;
}
