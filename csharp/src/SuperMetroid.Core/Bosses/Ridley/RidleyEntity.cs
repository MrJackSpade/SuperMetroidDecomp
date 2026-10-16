using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;

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
        IEnemyExtendedHitboxes, IEnemyContactHooks, IEnemyShotAdmission, IEnemyDrawHooks
{
    /// <summary>Whether the occupant is one of the two Ridley bodies rather than the breakup actor.</summary>
    private bool IsBody => Slot.EnemyDefinitionPointer is EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres;

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Ridley => ridley.RunRidleyInitializationAi,
        EnemyDefinitionId.RidleyCeres => ceres.RunCeresRidleyInitializationAi,
        EnemyDefinitionId.RidleyExplosion => ridley.RunRidleyExplosionInitializationAi,
        _ => throw new InvalidOperationException($"Ridley entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Ridley => ridley.RunRidleyMainAi,
        EnemyDefinitionId.RidleyCeres => ceres.RunCeresRidleyMainAi,
        EnemyDefinitionId.RidleyExplosion => ridley.RunRidleyExplosionMainAi,
        _ => throw new InvalidOperationException($"Ridley entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres => ridley.AdvanceRidleyBodyTypedAnimation,
        EnemyDefinitionId.RidleyExplosion => ridley.AdvanceRidleyBreakupTypedAnimation,
        _ => throw new InvalidOperationException($"Ridley entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback) =>
        RidleyBehavior.FindRidleyHitboxCallback(Slot, probe, out callback);

    /// <summary>
    /// Both Ridley bodies use a hand-authored extended body plus a separately solved tail tip, resolved by their own
    /// contact after the ordinary pass; the ordinary touch pass must not flatten either to the header's 8x8 radius.
    /// </summary>
    public bool OwnsSamusTouch => IsBody;

    /// <inheritdoc/>
    public void ResolveBeforeOrdinaryHits(ISnesAddressSpace bus, RoomEnemyFrame frame)
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
    public ushort? CommonDrawPaletteIndex() => IsBody ? encounter.Ridley?.CommonDrawPaletteIndex : null;

    /// <summary>Both Ridley mains call DrawRidleyTail/DrawRidleyWings before the common writer emits the body.</summary>
    public void DrawBeforeBody(OamBuffer oam, Position16<RoomPixels> camera)
    {
        if (IsBody)
            ceres.DrawRidleySupplementalSprites(oam, Slot, camera);
    }
}
