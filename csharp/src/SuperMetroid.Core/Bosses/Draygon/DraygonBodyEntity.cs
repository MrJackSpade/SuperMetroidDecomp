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
/// <param name="parts">The places of his eye, tail and arms, which his room lists right after his body.</param>
internal sealed class DraygonBodyEntity(DraygonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork,
    IReadOnlyList<RoomEnemyPlace> parts)
    : DraygonEntity(species, occupant, place, common, bossArtwork), IEnemyPrivateShotReaction
{
    /// <summary>His body's own shot reaction, built over the encounter (#2554).</summary>
    private EnemyShotReaction OwnShotReaction => field ??= Species.Combat.DraygonShotReactionOf(Encounter);

    /// <summary>His body's own bomb reaction, built over the encounter (#2554).</summary>
    private EnemyBombReaction OwnBombReaction => field ??= Species.Combat.DraygonBombReactionOf(Encounter);

    /// <summary>His body's own Power Bomb reaction, built over the encounter (#2554).</summary>
    private EnemyPowerBombReaction OwnPowerBombReaction => field ??= Species.Combat.DraygonPowerBombReactionOf(Encounter);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal DraygonEnemyState State { get; } = species.Parts.InitializeDraygonBody(place, parts);

    /// <summary>The body's part of the encounter it set up.</summary>
    internal override DraygonPart Part => State.BodyPart;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunDraygonBodyMainAi(Place, State, frame);

    /// <summary>
    /// The body's hurt behaviour: it flashes body, eye and tail together, while a connected grapple's electrocution may
    /// drain another <see cref="DraygonArenaDefinitions.GrappleShockDamage"/> at its shock cadence.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        Species.Combat.ApplyDraygonHurt(Place, Encounter, frame.Samus);
        return true;
    }

    /// <summary>Whether the body's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == DraygonReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the body's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == DraygonReactions.Touch)
            Species.Combat.ReactToDraygonTouch(context, Encounter);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The body's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == DraygonReactions.Shot ? OwnShotReaction : base.ShotReaction;

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
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == DraygonReactions.Shot ? OwnBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The body's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction =>
        LeavesPowerBombToSpecies ? OwnPowerBombReaction : base.PowerBombReaction;

    /// <summary>The body's contact and shots meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The body's shot reaction and its dud shot, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == DraygonReactions.Shot || reaction == EnemyReaction.DudShot;
}
