using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Baby Metroid occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Baby Metroid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class BabyMetroidEntity(BabyMetroidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, BabyMetroidEnemyState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Baby Metroid species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
    internal BabyMetroidEntity(BabyMetroidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings)
        : this(species, occupant, place, common, BabyMetroidBehavior.InitializeBabyMetroid(place, surroundings.Camera.X))
    {
    }

    /// <summary>The species' BabyMetroidShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnBabyMetroidShotReaction => field ??= BabyMetroidBehavior.BabyMetroidShotReaction(RecoilFromShot);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal BabyMetroidEnemyState State { get; } = state;

    /// <summary>Marks a shot's collision with the Baby and recoils it from the shot, without hurting it.</summary>
    /// <param name="c">The shot meeting the Baby.</param>
    /// <returns>Always true: the recoil replaces the rest of the shot reaction.</returns>
    internal bool RecoilFromShot(in EnemyShotContext c)
    {
        c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
        BabyMetroidBehavior.ResolveBabyMetroidShot(c.Enemy, State, c.Shot.Damage, MetroidRecoilOrigin.Of(c.Projectiles.Shots));
        return true;
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBabyMetroidMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceBabyMetroidAnimation(Place, State);

    /// <summary>Whether Baby Metroid's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == BabyMetroidReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs Baby Metroid's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == BabyMetroidReactions.Touch)
            BabyMetroidBehavior.ReactToBabyMetroidTouch(context, State);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Baby Metroid's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == BabyMetroidReactions.Shot ? OwnBabyMetroidShotReaction : base.ShotReaction;

    /// <summary>Runs the Baby Metroid's own bomb reaction, which comes before the ignoring check, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (context.SelectedShot != BabyMetroidReactions.Shot)
            return base.TryReactToBomb(context);
        BabyMetroidBehavior.ReactToBabyMetroidBomb(context, State);
        return true;
    }

    /// <summary>Baby Metroid's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == BabyMetroidReactions.Shot;
}
