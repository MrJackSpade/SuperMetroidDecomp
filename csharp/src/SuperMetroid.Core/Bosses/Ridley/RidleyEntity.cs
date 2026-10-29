using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One Ridley occupant: Lower Norfair Ridley, Ceres Ridley or the breakup explosion actor.</summary>
/// <param name="ridley">The Lower Norfair Ridley owner.</param>
/// <param name="ceres">The Ceres Ridley owner.</param>
/// <param name="encounter">The shared Ridley encounter.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RidleyEntity(
    RidleyBehavior ridley, CeresRidleyBehavior ceres, RidleyEncounter encounter, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation,
        IEnemyExtendedHitboxes, IEnemyContactHooks, IEnemyShotAdmission, IEnemyDrawHooks, IEnemyPrivateShotCallback
{
    /// <summary>Whether the occupant is one of the two Ridley bodies rather than the breakup actor.</summary>
    private bool IsBody => Slot.DefinitionId is EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres;

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => RidleyPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        RidleyPart.Ridley => ridley.Combat.InitializeNorfairRidley,
        RidleyPart.Ceres => ceres.Flight.InitializeCeresRidley,
        RidleyPart.Explosion => ridley.Death.RunRidleyExplosionInitializationAi,
        _ => throw new InvalidOperationException($"Ridley entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => RidleyPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        RidleyPart.Ridley => ridley.RunRidleyMainAi,
        RidleyPart.Ceres => ceres.RunCeresRidleyMainAi,
        RidleyPart.Explosion => (enemy, _) => ridley.Death.RunNorfairRidleyExplosionMain(enemy),
        _ => throw new InvalidOperationException($"Ridley entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => RidleyPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        RidleyPart.Ridley or RidleyPart.Ceres => ridley.Animation.AdvanceRidleyBodyTypedAnimation,
        RidleyPart.Explosion => (enemy, _) => ridley.Animation.AdvanceRidleyBreakupAnimation(enemy),
        _ => throw new InvalidOperationException($"Ridley entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback) =>
        RidleyInteractions.FindRidleyHitboxCallback(Slot, probe, out callback);

    /// <summary>
    /// Both Ridley bodies use a hand-authored extended body plus a separately solved tail tip, resolved by their own
    /// contact after the ordinary pass; the ordinary touch pass must not flatten either to the header's 8x8 radius.
    /// </summary>
    public bool OwnsSamusTouch => IsBody;

    /// <inheritdoc/>
    public void ResolveBeforeOrdinaryHits(RoomEnemyFrame frame)
    {
    }

    /// <inheritdoc/>
    public void ResolveAfterSamusContact(SamusState samus)
    {
        if (IsBody)
            _ = ceres.Hits.ResolveRidleyBodySamusContact(samus);
    }

    /// <summary>Ceres Ridley never enters the ordinary shot pass; its encounter resolves its own shots.</summary>
    public bool AdmitsShots() => Slot.DefinitionId != EnemyDefinitionId.RidleyCeres;

    /// <summary>Ridley's AI publishes the common writer's colours: its body, wings and tail share one latched phase.</summary>
    public SpriteColouring? CommonDrawColouring() => IsBody ? encounter.Ridley?.CommonDrawColouring : null;

    /// <summary>Both Ridley mains call DrawRidleyTail/DrawRidleyWings before the common writer emits the body.</summary>
    public void DrawBeforeBody(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        if (IsBody)
            ceres.BabyScene.DrawRidleySupplementalSprites(sprites, Slot, camera);
    }

    /// <summary>
    /// Only Lower Norfair Ridley's shots enter the extended-spritemap walker; its contact, Ceres Ridley's collision and the
    /// breakup actor's take neither.
    /// </summary>
    public override EnemyCapabilities Capabilities =>
        Slot.DefinitionId == EnemyDefinitionId.Ridley ? EnemyCapabilities.ExtendedProjectileHitboxes : EnemyCapabilities.None;

    /// <summary>Both Ridley bodies' shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        IsBody && callback == EnemyInteractionCallback.RidleyShot;
}
