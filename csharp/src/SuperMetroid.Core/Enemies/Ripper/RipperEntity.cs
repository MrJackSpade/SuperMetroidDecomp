namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>One Ripper occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RipperEntity(RipperBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => RipperPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        RipperPart.GRipper => species.InitializeGRipper,
        RipperPart.Ripper2 => species.InitializeRipper2,
        RipperPart.Ripper => species.InitializeRipper,
        _ => throw new InvalidOperationException($"Ripper entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => RipperPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        RipperPart.GRipper => species.RunGRipperMainAi,
        RipperPart.Ripper2 => species.RunRipper2MainAi,
        RipperPart.Ripper => species.RunRipperMainAi,
        _ => throw new InvalidOperationException($"Ripper entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceRipperTypedAnimation;

    /// <summary>The grapple Ripper's and second Ripper's shared shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        Slot.DefinitionId is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 && callback == EnemyInteractionCallback.GRipperRipper2Shot;
}
