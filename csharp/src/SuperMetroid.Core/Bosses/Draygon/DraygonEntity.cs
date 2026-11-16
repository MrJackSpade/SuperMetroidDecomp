using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// One of Draygon's occupants: his body (<see cref="DraygonBodyEntity"/>), eye (<see cref="DraygonEyeEntity"/>), tail
/// (<see cref="DraygonTailEntity"/>) or arms (<see cref="DraygonArmsEntity"/>). Every part plays its animation through the
/// encounter, answers his engine-owned hitboxes and may show its frames on his boss body layer; the parts other than the
/// body take the shared reactions their definitions name.
/// </summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal abstract class DraygonEntity(DraygonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IBossBodyFrames
{
    /// <summary>The Draygon species owner.</summary>
    protected DraygonBehavior Species => species;

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceDraygonTypedAnimation(Place, context);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return DraygonCombat.FindDraygonHitboxReaction(Place, probe, out reaction);
    }

    /// <summary>None: the parts other than the body name shared touch reactions.</summary>
    public virtual EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the parts other than the body name shared shot reactions.</summary>
    public virtual EnemyReaction? SpeciesShot => null;

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        DraygonBodyLayerFrameDefinitions.Frames.Contains(selected) ? bossArtwork.Body(BossBodyOwner.Draygon).Frame(selected) : null;
}
