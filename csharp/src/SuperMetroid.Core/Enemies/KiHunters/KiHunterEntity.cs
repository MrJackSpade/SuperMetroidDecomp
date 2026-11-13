namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One Ki-Hunter occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Ki-Hunter species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KiHunterEntity(KiHunterBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => KiHunterPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        KiHunterPart.KihunterGreen or KiHunterPart.KihunterYellow or KiHunterPart.KihunterRed => species.InitializeKiHunter,
        KiHunterPart.KihunterGreenWings or KiHunterPart.KihunterYellowWings or KiHunterPart.KihunterRedWings => species.InitializeKiHunterWings,
        _ => throw new InvalidOperationException($"KiHunter entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunKiHunterMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => species.AdvanceKiHunterTypedAnimation;

    /// <summary>The Ki-Hunter body's own shot reaction, which normal bombs dispatch like the common one; the wings own none.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        KiHunterBehavior.IsKiHunterBodyDefinition(Slot.DefinitionId) && reaction == EnemyReaction.KiHunterShot;
}
