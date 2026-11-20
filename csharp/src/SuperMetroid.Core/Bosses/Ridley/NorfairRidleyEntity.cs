using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Lower Norfair Ridley (#2146): the Lower Norfair encounter his body holds from its initialization (#2231), his fight,
/// his post-hit hurt window, and the shots and bombs his definition leaves to him, which meet his frames' hitboxes. A
/// Ridley found defeated is deleted as he initializes and begins no encounter.
/// </summary>
/// <param name="norfair">Lower Norfair Ridley's encounter behaviour.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place Ridley occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairRidleyEntity(NorfairRidleyBehavior norfair, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : RidleyEntity(norfair.Body, occupant, place, common)
{
    /// <summary>The Lower Norfair encounter his body holds from its initialization.</summary>
    internal override NorfairRidleyState Encounter => Place.RequireSpeciesState<NorfairRidleyState>();

    /// <inheritdoc/>
    public override void Initialize() => norfair.Combat.InitializeNorfairRidley(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => norfair.RunRidleyMainAi(Place, Encounter, frame);

    /// <summary>Lower Norfair Ridley's lift-off hands control to the fight: he flies into the arena.</summary>
    public override void LiftOff() => NorfairRidleyCombat.LiftOff(Encounter);

    /// <summary>Lower Norfair Ridley never cuts his fireballing short; he plays on.</summary>
    /// <param name="samus">Samus.</param>
    public override RidleyBodySegment? CutFireballingShort(SamusState samus) => null;

    /// <summary>
    /// Ridley's hurt behaviour for the post-hit window, which owns hurt frames instead of falling back to ordinary main AI:
    /// movement and phase work run only on even actor frames, while tail projectile armour and hurt colours stay live on
    /// every frame.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        norfair.RunRidleyHurt(Place, Encounter, frame);
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
