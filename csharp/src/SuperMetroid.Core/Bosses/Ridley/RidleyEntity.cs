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
    private bool IsBody => Slot.EnemyDefinitionPointer is EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres;

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => RidleyPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        RidleyPart.Ridley => ridley.InitializeNorfairRidley,
        RidleyPart.Ceres => ceres.InitializeCeresRidley,
        RidleyPart.Explosion => ridley.RunRidleyExplosionInitializationAi,
        _ => throw new InvalidOperationException($"Ridley entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => RidleyPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        RidleyPart.Ridley => ridley.RunRidleyMainAi,
        RidleyPart.Ceres => ceres.RunCeresRidleyMainAi,
        RidleyPart.Explosion => (enemy, _) => ridley.RunNorfairRidleyExplosionMain(enemy),
        _ => throw new InvalidOperationException($"Ridley entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => RidleyPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        RidleyPart.Ridley or RidleyPart.Ceres => ridley.AdvanceRidleyBodyTypedAnimation,
        RidleyPart.Explosion => (enemy, _) => ridley.AdvanceRidleyBreakupAnimation(enemy),
        _ => throw new InvalidOperationException($"Ridley entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback) =>
        RidleyBehavior.FindRidleyHitboxCallback(Slot, probe, out callback);

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
            _ = ceres.ResolveRidleyBodySamusContact(samus);
    }

    /// <summary>Ceres Ridley never enters the ordinary shot pass; its encounter resolves its own shots.</summary>
    public bool AdmitsShots() => Slot.EnemyDefinitionPointer != EnemyDefinitionId.RidleyCeres;

    /// <summary>Ridley's AI publishes the common writer's palette: its body, wings and tail share one latched phase.</summary>
    public ushort? CommonDrawPaletteIndex() => IsBody ? encounter.Ridley?.CommonDrawPaletteIndex.Raw : null;

    /// <summary>Both Ridley mains call DrawRidleyTail/DrawRidleyWings before the common writer emits the body.</summary>
    public void DrawBeforeBody(SpriteFrameBuffer sprites, SpriteArtwork objects, Position16<RoomPixels> camera)
    {
        if (IsBody)
            ceres.DrawRidleySupplementalSprites(sprites, objects, Slot, camera);
    }

    /// <summary>
    /// Only Lower Norfair Ridley's shots enter the extended-spritemap walker; its contact, Ceres Ridley's collision and the
    /// breakup actor's take neither.
    /// </summary>
    public override EnemyCapabilities Capabilities =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.Ridley ? EnemyCapabilities.ExtendedProjectileHitboxes : EnemyCapabilities.None;

    /// <summary>Both Ridley bodies' shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        IsBody && callback == EnemyInteractionCallback.RidleyShot;
}
