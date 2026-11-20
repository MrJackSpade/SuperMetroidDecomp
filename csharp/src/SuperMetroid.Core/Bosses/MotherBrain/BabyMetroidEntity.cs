using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Baby Metroid occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Baby Metroid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BabyMetroidEntity(BabyMetroidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemySurroundingsInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize(EnemyPopulationSurroundings surroundings) => BabyMetroidBehavior.SetUpBabyMetroid(Place, surroundings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBabyMetroidMainAi(Place, Place.RequireSpeciesState<BabyMetroidEnemyState>(), Place.RequireSpeciesState<BabyMetroidEnemyState>(), Place.RequireSpeciesState<BabyMetroidEnemyState>(), frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceBabyMetroidAnimation(Place, Place.RequireSpeciesState<BabyMetroidEnemyState>());

    /// <summary>Whether Baby Metroid's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == BabyMetroidReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs Baby Metroid's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == BabyMetroidReactions.Touch)
            BabyMetroidBehavior.ReactToBabyMetroidTouch(context, context.Place.RequireSpeciesState<BabyMetroidEnemyState>(), context.Place.RequireSpeciesState<BabyMetroidEnemyState>());
        else
            base.ReactToTouch(context);
    }

    /// <summary>Baby Metroid's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == BabyMetroidReactions.Shot ? species.BabyMetroidShotReaction : base.ShotReaction;

    /// <summary>Runs the Baby Metroid's own bomb reaction, which comes before the ignoring check, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (context.SelectedShot != BabyMetroidReactions.Shot)
            return base.TryReactToBomb(context);
        BabyMetroidBehavior.ReactToBabyMetroidBomb(context, context.Enemy.RequireSpeciesState<BabyMetroidEnemyState>());
        return true;
    }

    /// <summary>Baby Metroid's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == BabyMetroidReactions.Shot;
}
