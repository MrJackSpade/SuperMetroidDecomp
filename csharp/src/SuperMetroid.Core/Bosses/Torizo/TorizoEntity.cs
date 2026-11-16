using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One Torizo occupant, Bomb or Golden: its own AI, with the shared reactions.</summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class TorizoEntity(TorizoBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction, ILitTorizo
{
    /// <summary>Whether this Torizo has no health left: its belly's glow ends on the first update it sees so (#1700).</summary>
    public bool IsDefeated => Place.Health == 0;

    /// <inheritdoc/>
    public void Initialize(EnemySpawnSurroundings surroundings) => species.Movement.RunTorizoInitializationAi(Place, surroundings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (TorizoPartDefinitions.Parts[Place.DefinitionId])
        {
            case TorizoPart.BombTorizo:
                species.RunBombTorizoMainAi(Place, frame);
                break;
            case TorizoPart.GoldenTorizo:
                species.RunGoldenTorizoMainAi(Place, frame);
                break;
            default:
                throw new InvalidOperationException($"Torizo entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceTorizoTypedAnimation(Place, context);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return TorizoCombat.FindTorizoHitboxReaction(Place, probe, out reaction);
    }

    /// <summary>Both Torizos' contact and shots meet their own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Both Torizos' touch reaction, which their definitions leave to them.</summary>
    public EnemyReaction? SpeciesTouch => TorizoReactions.Touch;

    /// <summary>The shot reaction each Torizo's definition leaves to it: Golden Torizo's own, or Bomb Torizo's.</summary>
    public EnemyReaction? SpeciesShot =>
        Place.DefinitionId == EnemyDefinitionId.GoldenTorizo ? TorizoReactions.GoldenShot : TorizoReactions.Shot;

    /// <summary>
    /// Both Torizos' hurt behaviour; the common animation runner still advances after it, as the enemies' update does.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        species.Combat.ApplyTorizoHurt(Place);
        return true;
    }

    /// <summary>Whether the Torizo's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Torizo's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.Combat.ReactToTorizoTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Torizo's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.Combat.TorizoShotReaction : base.ShotReaction;

    /// <summary>Runs the Torizos' own reaction to every bomb that overlaps them, before the ignoring check.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        species.Combat.ReactToTorizoBomb(context);
        return true;
    }

    /// <summary>The Torizo shot reactions, which a normal bomb triggers as it does the common one; only Golden Torizo owns its own.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == TorizoReactions.Shot || reaction == TorizoReactions.StandUpSitDownShot ||
        (Place.DefinitionId == EnemyDefinitionId.GoldenTorizo && reaction == TorizoReactions.GoldenShot);
}
