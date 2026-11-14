using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One MotherBrain occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class MotherBrainEntity(MotherBrainBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IBossBodyFrames, IEnemyPrivateShotReaction
{
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork = bossArtwork;
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (MotherBrainPartDefinitions.Parts[Slot.DefinitionId])
        {
            case MotherBrainPart.Body:
                species.Body.InitializeMotherBrainBody(Slot);
                break;
            case MotherBrainPart.Head:
                species.Head.InitializeMotherBrainHead(Slot);
                break;
            case MotherBrainPart.Tubes:
                MotherBrainFakeDeath.InitializeMotherBrainFallingTube(Slot);
                break;
            default:
                throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (MotherBrainPartDefinitions.Parts[Slot.DefinitionId])
        {
            case MotherBrainPart.Body:
                species.RunMotherBrainBodyMainAi(Slot, frame);
                break;
            case MotherBrainPart.Head:
                species.RunMotherBrainHeadMainAi(Slot, frame);
                break;
            case MotherBrainPart.Tubes:
                species.FakeDeath.RunMotherBrainFallingTubeMain(Slot);
                break;
            default:
                throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => MotherBrainPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        MotherBrainPart.Tubes => (enemy, _) => MotherBrainBody.AdvanceMotherBrainTubeAnimation(enemy),
        MotherBrainPart.Body or MotherBrainPart.Head => species.AdvanceMotherBrainBodyTypedAnimation,
        _ => throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => MotherBrainCombat.FindMotherBrainHitboxReaction(Slot, probe, out reaction);

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        Slot.DefinitionId == EnemyDefinitionId.MotherBrainBody && MotherBrainBodyVisualDefinitions.BodyLayerFrames.Contains(selected)
            ? _bossArtwork.Body(BossBodyOwner.MotherBrain).Frame(selected) : null;

    /// <summary>Shots meet the body's and head's frames' hitboxes; their contact and the tubes' use the definition's radius.</summary>
    public override EnemyCapabilities Capabilities =>
        Slot.DefinitionId is EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead ? EnemyCapabilities.ProjectileFrameHitboxes : EnemyCapabilities.None;

    /// <summary>The body's and head's own shot reactions, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        Slot.DefinitionId == EnemyDefinitionId.MotherBrainBody && reaction == EnemyReaction.MotherBrainBodyShot ||
        Slot.DefinitionId == EnemyDefinitionId.MotherBrainHead && reaction == EnemyReaction.MotherBrainHeadShot;
}
