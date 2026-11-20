using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's body (#2146), the one part with reactions of its own: his fight, his hurt, his own projectile resolver run
/// before the ordinary passes, and the touch and bombs his definition leaves to him.
/// </summary>
/// <param name="species">The Phantoon species owner.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="parts">The places of his eye, tentacles and mouth, which his room lists right after his body.</param>
internal sealed class PhantoonBodyEntity(PhantoonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork,
    IReadOnlyList<RoomEnemyPlace> parts)
    : PhantoonEntity(species, place, common, bossArtwork), IEnemyContactHooks, IEnemyPrivateShotReaction
{
    /// <summary>His body's own bomb reaction, built over the encounter (#2554).</summary>
    private EnemyBombReaction OwnBombReaction => field ??= Species.Combat.PhantoonBombReactionOf(Encounter);

    /// <inheritdoc/>
    /// <remarks>The part sets up as it is built, joining the encounter (#2554).</remarks>
    internal override PhantoonPart Part { get; } = species.InitializePhantoonBody(place, parts);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunPhantoonBodyMainAi(Encounter, frame);

    /// <summary>
    /// Resolves the body's projectile hits before the ordinary passes: a retained Plasma hit must publish its hurt clock
    /// before this frame ticks it.
    /// </summary>
    public void ResolveBeforeOrdinaryHits(RoomEnemyFrame frame) =>
        _ = Species.Combat.ResolvePhantoonProjectileHits(Encounter, Ensure.NotNull(frame).SamusProjectiles, frame.SamusBombs);

    /// <inheritdoc/>
    public bool OwnsSamusTouch => false;

    /// <summary>Only Samus's contact meets the body's frames' hitboxes; its weapon collision is the encounter's own resolver.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.SamusFrameHitboxes;

    /// <summary>The body's touch reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesTouch => PhantoonReactions.Touch;

    /// <summary>The body's shot reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesShot => PhantoonReactions.Shot;

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
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the body's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            Species.Combat.ReactToPhantoonBodyTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The body's own stages around common bomb damage, for a bomb that selects its shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == PhantoonReactions.Shot ? OwnBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The body's shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction == PhantoonReactions.Shot;
}
