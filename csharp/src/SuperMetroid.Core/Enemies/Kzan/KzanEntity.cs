namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>One Kzan occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Kzan species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KzanEntity(KzanBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => KzanPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        KzanPart.Top => species.RunKzanTopInitializationAi,
        KzanPart.Bottom => species.RunKzanBottomInitializationAi,
        _ => throw new InvalidOperationException($"Kzan entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => KzanPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        KzanPart.Top => species.RunKzanTopMainAi,
        KzanPart.Bottom => species.RunKzanBottomMainAi,
        _ => throw new InvalidOperationException($"Kzan entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => KzanPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        KzanPart.Top => KzanBehavior.AdvanceKzanTypedAnimation,
        KzanPart.Bottom => null,
        _ => throw new InvalidOperationException($"Kzan entity bound to {Slot.EnemyDefinitionPointer}."),
    };
}
