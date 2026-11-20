using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// One of Draygon's occupants: his body (<see cref="DraygonBodyEntity"/>), eye (<see cref="DraygonEyeEntity"/>), tail
/// (<see cref="DraygonTailEntity"/>) or arms (<see cref="DraygonArmsEntity"/>). Every part plays its animation through the
/// encounter, answers his hitboxes and shows its frames as placed pictures; the parts other than the body take the
/// shared reactions their definitions name.
/// </summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal abstract class DraygonEntity(DraygonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IBossBodyFrames
{
    /// <summary>The Draygon species owner.</summary>
    protected DraygonBehavior Species => species;

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceDraygonTypedAnimation(Part, context);

    /// <summary>The part's own part of the encounter, its playback included, which it holds from its setup (#2298).</summary>
    internal abstract DraygonPart Part { get; }

    /// <summary>The encounter the part is part of, which it holds from its setup (#2177).</summary>
    internal DraygonEnemyState Encounter => Part.Draygon;

    /// <summary>The encounter <paramref name="part"/> is part of, which the part's own entity holds.</summary>
    /// <param name="part">A place holding one of Draygon's parts.</param>
    /// <exception cref="InvalidOperationException">The place holds none of his parts.</exception>
    internal static DraygonEnemyState EncounterOf(RoomEnemyPlace part) =>
        Ensure.NotNull(part).Entity is DraygonEntity entity
            ? entity.Encounter
            : throw new InvalidOperationException($"Enemy place {part.Ordinal} holds no part of Draygon.");

    /// <inheritdoc/>
    public EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe) =>
        DraygonCombat.FindDraygonHitboxReaction(Place, probe);

    /// <inheritdoc/>
    public SuperMetroid.Core.Enemies.BossBodies.BossBodyParts BodyParts => Encounter.BodyParts;

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        DraygonBodyLayerFrameDefinitions.Frames.Contains(selected) ? bossArtwork.Body(BossBodyOwner.Draygon).Frame(selected) : null;
}
