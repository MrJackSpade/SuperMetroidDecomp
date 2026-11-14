namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>One Kzan occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Kzan species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KzanEntity(KzanBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (KzanPartDefinitions.Parts[Slot.DefinitionId])
        {
            case KzanPart.Top:
                KzanBehavior.InitializeKzanTop(Slot);
                break;
            case KzanPart.Bottom:
                species.InitializeKzanBottom(Slot);
                break;
            default:
                throw new InvalidOperationException($"Kzan entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (KzanPartDefinitions.Parts[Slot.DefinitionId])
        {
            case KzanPart.Top:
                species.RunKzanTopMainAi(Slot, frame);
                break;
            case KzanPart.Bottom:
                species.RunKzanBottomMain(Slot);
                break;
            default:
                throw new InvalidOperationException($"Kzan entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => KzanPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        KzanPart.Top => KzanBehavior.AdvanceKzanTypedAnimation,
        KzanPart.Bottom => null,
        _ => throw new InvalidOperationException($"Kzan entity bound to {Slot.DefinitionId}."),
    };
}
