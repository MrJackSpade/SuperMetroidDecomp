using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Lower Norfair Ridley (#2146): his fight, his post-hit hurt window, and the shots and bombs his definition leaves to
/// him, which meet his frames' hitboxes.
/// </summary>
/// <param name="ridley">The Lower Norfair Ridley owner.</param>
/// <param name="ceres">The Ceres Ridley owner.</param>
/// <param name="encounter">The shared Ridley encounter.</param>
/// <param name="place">The place Ridley occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairRidleyEntity(
    RidleyBehavior ridley, CeresRidleyBehavior ceres, RidleyEncounter encounter, RoomEnemyPlace place, EnemyCommonParticipation common)
    : RidleyEntity(ridley, ceres, encounter, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => NorfairRidley.Combat.InitializeNorfairRidley(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => NorfairRidley.RunRidleyMainAi(Place, frame);

    /// <summary>
    /// Ridley's hurt behaviour for the post-hit window, which owns hurt frames instead of falling back to ordinary main AI:
    /// movement and phase work run only on even actor frames, while tail projectile armour and hurt colours stay live on
    /// every frame.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        NorfairRidley.RunRidleyHurt(Place, frame);
        return true;
    }

    /// <summary>Ridley's own shot reaction where his definition leaves shots to him, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? RidleyInteractions.RidleyShotReaction : base.ShotReaction;

    /// <summary>Ridley's own stages around common bomb damage, for a bomb that selects his shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == RidleyReactions.Shot ? RidleyInteractions.RidleyBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Only shots meet Ridley's frames' hitboxes; his contact is his own.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.ProjectileFrameHitboxes;
}
