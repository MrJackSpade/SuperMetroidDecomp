using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Baby Metroid occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Baby Metroid species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">How the enemy comes to be set up, whose surroundings its initialization reads.</param>
internal sealed class BabyMetroidEntity(BabyMetroidBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnBabyMetroidShotReaction => field ??= species.BabyMetroidShotReactionOf(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal BabyMetroidEnemyState State { get; } = BabyMetroidBehavior.SetUpBabyMetroid(place, setup.SurroundingsOf(place));

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBabyMetroidMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceBabyMetroidAnimation(Place, State);

    /// <summary>Whether Baby Metroid's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs Baby Metroid's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            BabyMetroidBehavior.ReactToBabyMetroidTouch(context, State);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Baby Metroid's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnBabyMetroidShotReaction : base.ShotReaction;

    /// <summary>Runs Baby Metroid's its own bomb reaction, which comes before the ignoring check, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (!(context.SelectedShot is null))
            return base.TryReactToBomb(context);
        BabyMetroidBehavior.ReactToBabyMetroidBomb(context, State);
        return true;
    }

    /// <summary>Baby Metroid's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
