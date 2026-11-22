using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One of Phantoon's occupants: his body (<see cref="PhantoonBodyEntity"/>), or his eye
/// (<see cref="PhantoonEyeEntity"/>), tentacles (<see cref="PhantoonTentaclesEntity"/>) or mouth
/// (<see cref="PhantoonMouthEntity"/>), drawing parts that play their own animations. Every part answers his hitboxes
/// and shows its frames as placed pictures; the drawing parts take the shared reactions their definitions name.
/// </summary>
/// <param name="species">The Phantoon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="part">The part's own part of the encounter, which its setup made as the entity was built (#2554).</param>
internal abstract partial class PhantoonEntity(PhantoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork, PhantoonPart part)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IBossBodyFrames
{
    /// <summary>The Phantoon species owner.</summary>
    protected PhantoonBehavior Species => species;

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvancePhantoonPartAnimation(Part);

    /// <summary>The part's own part of the encounter, its playback included, which it holds from its setup (#2252).</summary>
    internal PhantoonPart Part { get; } = part;

    /// <summary>The encounter the part is part of, which it holds from its setup (#2252).</summary>
    internal PhantoonEnemyState Encounter => Part.Phantoon;

    /// <summary>The encounter <paramref name="part"/> is part of, which the part's own entity holds.</summary>
    /// <param name="part">A place holding one of Phantoon's parts.</param>
    /// <exception cref="InvalidOperationException">The place holds none of his parts.</exception>
    internal static PhantoonEnemyState EncounterOf(RoomEnemyPlace part) =>
        Ensure.NotNull(part).Entity is PhantoonEntity entity
            ? entity.Encounter
            : throw new InvalidOperationException($"Enemy place {part.Ordinal} holds no part of Phantoon.");

    /// <summary>
    /// The part's hitboxes, which its frame's components hold, his body's picture those of the hit area his fight has live:
    /// every one of his parts finds its own (#2301).
    /// </summary>
    public EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe) => PhantoonCombat.FindPhantoonHitboxReaction(Place, Encounter.BodyHitArea, probe);

    /// <inheritdoc/>
    public BossBodyParts BodyParts => Encounter.BodyParts;

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        bossArtwork.Body(BossBodyOwner.Phantoon).Find(selected);
}
