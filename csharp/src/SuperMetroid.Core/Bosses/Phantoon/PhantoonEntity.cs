using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One of Phantoon's occupants: his body (<see cref="PhantoonBodyEntity"/>), or his eye
/// (<see cref="PhantoonEyeEntity"/>), tentacles (<see cref="PhantoonTentaclesEntity"/>) or mouth
/// (<see cref="PhantoonMouthEntity"/>), drawing parts that play their own animations. Every part answers his
/// engine-owned hitboxes and may show its frames on his boss body layer; the drawing parts take the shared reactions
/// their definitions name.
/// </summary>
/// <param name="species">The Phantoon species owner.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal abstract class PhantoonEntity(PhantoonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IBossBodyFrames
{
    /// <summary>The Phantoon species owner.</summary>
    protected PhantoonBehavior Species => species;

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvancePhantoonPartAnimation(Place);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => PhantoonCombat.FindPhantoonHitboxReaction(Place, probe, out reaction);

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        PhantoonBodyLayerFrameDefinitions.Frames.Contains(selected) ? bossArtwork.Body(BossBodyOwner.Phantoon).Frame(selected) : null;

    /// <summary>None: the drawing parts name shared touch reactions.</summary>
    public virtual EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the drawing parts name shared shot reactions.</summary>
    public virtual EnemyReaction? SpeciesShot => null;
}
