using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's body (#2146), the one part with reactions of its own: his fight, his hurt, his own projectile resolver run
/// before the ordinary passes, and the touch and bombs his definition leaves to him.
/// </summary>
/// <param name="species">The Phantoon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="part">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class PhantoonBodyEntity(PhantoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, BossArtworkBinding bossArtwork, PhantoonPart part)
    : PhantoonEntity(species, occupant, place, common, bossArtwork, part), IEnemyContactHooks, IEnemyPrivateShotReaction
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Phantoon species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the body occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="bossArtwork">The session's installed boss artwork.</param>
    internal PhantoonBodyEntity(PhantoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        BossArtworkBinding bossArtwork)
        : this(species, occupant, place, common, bossArtwork, species.InitializePhantoonBody(occupant, place))
    {
    }

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunPhantoonBodyMainAi(Encounter, frame);

    /// <summary>
    /// Resolves the body's projectile hits before the ordinary passes: a retained Plasma hit must publish its hurt clock
    /// before this frame ticks it.
    /// </summary>
    public void ResolveBeforeOrdinaryHits(RoomEnemyFrame frame) =>
        Species.Combat.ResolvePhantoonProjectileHits(Encounter, Ensure.NotNull(frame).SamusProjectiles, frame.SamusBombs);

    /// <inheritdoc/>
    public bool OwnsSamusTouch => false;

    /// <summary>Only Samus's contact meets the body's frames' hitboxes; its weapon collision is the encounter's own resolver.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.SamusFrameHitboxes;

    /// <summary>
    /// The body's hurt behaviour after a vulnerable-eye hit, which owns every Phantoon hurt frame: it alternates the
    /// body's colours between white and the new health band; body movement and main AI resume only after the common
    /// flash timing clears the hurt state.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        PhantoonBehavior.ApplyPhantoonHurt(Place, Encounter);
        return true;
    }

    /// <summary>Whether the body's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == PhantoonReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the body's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == PhantoonReactions.Touch)
            Species.Combat.ReactToPhantoonBodyTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The body's own stages around common bomb damage, for a bomb that selects its shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == PhantoonReactions.Shot ? Species.Combat.PhantoonBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The body's shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == PhantoonReactions.Shot;
}
