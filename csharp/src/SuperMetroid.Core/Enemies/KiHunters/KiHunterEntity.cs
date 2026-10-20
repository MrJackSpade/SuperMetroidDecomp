namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One KiHunter occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The KiHunter species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KiHunterEntity(KiHunterBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => KiHunterPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        KiHunterPart.KihunterGreen or KiHunterPart.KihunterYellow or KiHunterPart.KihunterRed => KiHunterBehavior.RunKihunterGreenInitializationAi,
        KiHunterPart.KihunterGreenWings or KiHunterPart.KihunterYellowWings or KiHunterPart.KihunterRedWings => species.RunKihunterGreenWingsInitializationAi,
        _ => throw new InvalidOperationException($"KiHunter entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunKiHunterMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceKiHunterTypedAnimation;

    /// <summary>The Ki-Hunter body's own shot callback, which normal bombs dispatch like the common one; the wings own none.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        KiHunterBehavior.IsKiHunterBodyDefinition(Slot.EnemyDefinitionPointer) && callback == EnemyInteractionCallback.KiHunterShot;
}
