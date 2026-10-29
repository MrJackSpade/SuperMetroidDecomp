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
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IBossBodyFrames, IEnemyPrivateShotCallback
{
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork = bossArtwork;
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => MotherBrainPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        MotherBrainPart.Body => species.Body.InitializeMotherBrainBody,
        MotherBrainPart.Head => species.Head.InitializeMotherBrainHead,
        MotherBrainPart.Tubes => species.FakeDeath.InitializeMotherBrainFallingTube,
        _ => throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => MotherBrainPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        MotherBrainPart.Body => species.RunMotherBrainBodyMainAi,
        MotherBrainPart.Head => species.RunMotherBrainHeadMainAi,
        MotherBrainPart.Tubes => (enemy, _) => species.FakeDeath.RunMotherBrainFallingTubeMain(enemy),
        _ => throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => MotherBrainPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        MotherBrainPart.Tubes => (enemy, _) => species.Body.AdvanceMotherBrainTubeAnimation(enemy),
        MotherBrainPart.Body or MotherBrainPart.Head => species.AdvanceMotherBrainBodyTypedAnimation,
        _ => throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback) => MotherBrainCombat.FindMotherBrainHitboxCallback(Slot, probe, out callback);

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        Slot.DefinitionId == EnemyDefinitionId.MotherBrainBody && MotherBrainBodyVisualDefinitions.Bg2FrameIds.Contains(selected)
            ? _bossArtwork.Body(BossBodyOwner.MotherBrain).Frame(selected) : null;

    /// <summary>The body's and head's shots enter the extended-spritemap walker; their contact and the tubes' use the header radius.</summary>
    public override EnemyCapabilities Capabilities =>
        Slot.DefinitionId is EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead ? EnemyCapabilities.ExtendedProjectileHitboxes : EnemyCapabilities.None;

    /// <summary>The body's and head's own shot callbacks, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        Slot.DefinitionId == EnemyDefinitionId.MotherBrainBody && callback == EnemyInteractionCallback.MotherBrainBodyShot ||
        Slot.DefinitionId == EnemyDefinitionId.MotherBrainHead && callback == EnemyInteractionCallback.MotherBrainHeadShot;
}
