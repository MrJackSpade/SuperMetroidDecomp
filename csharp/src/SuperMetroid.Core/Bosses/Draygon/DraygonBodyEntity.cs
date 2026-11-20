using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's body (#2146), the one part with reactions of its own: his fight, his hurt, and the touch, shots, bombs and
/// Power Bombs his definition leaves to him, met by his own frames' hitboxes.
/// </summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class DraygonBodyEntity(DraygonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : DraygonEntity(species, occupant, place, common, bossArtwork), IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public override void Initialize() => Species.Parts.InitializeDraygonBody(Place);

    /// <summary>The body's part of the encounter it set up.</summary>
    internal override DraygonPart Part => Place.RequireSpeciesState<DraygonEnemyState>().BodyPart;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunDraygonBodyMainAi(Place, frame);

    /// <summary>The body's touch reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesTouch => DraygonReactions.Touch;

    /// <summary>The body's shot reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesShot => DraygonReactions.Shot;

    /// <summary>
    /// The body's hurt behaviour: it flashes body, eye and tail together, while a connected grapple's electrocution may
    /// drain another 256 health every eighth frame.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        Species.Combat.ApplyDraygonHurt(Place, Encounter, frame.Samus);
        return true;
    }

    /// <summary>Whether the body's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the body's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            Species.Combat.ReactToDraygonTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The body's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? Species.Combat.DraygonShotReaction : base.ShotReaction;

    /// <summary>
    /// The body's dud shot for a bomb whose hitbox selects it, in place of common bomb damage once the ignoring check has
    /// passed.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (context.SelectedShot != EnemyReaction.DudShot)
            return base.TryReplaceCommonBombDamage(context);
        Species.Combat.CreateBombDudShot(context);
        return true;
    }

    /// <summary>The body's own stages around common bomb damage, for a bomb that selects its shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == DraygonReactions.Shot ? Species.Combat.DraygonBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The body's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        LeavesPowerBombToSpecies ? Species.Combat.DraygonPowerBombReaction : base.PowerBombReaction;

    /// <summary>The body's contact and shots meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The body's shot reaction and its dud shot, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction == DraygonReactions.Shot || reaction == EnemyReaction.DudShot;
}
