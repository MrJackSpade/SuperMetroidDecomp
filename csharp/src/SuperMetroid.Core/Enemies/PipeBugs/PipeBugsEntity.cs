namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>One PipeBugs occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The PipeBugs species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PipeBugsEntity(PipeBugsBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => PipeBugsPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        PipeBugsPart.Zeb or PipeBugsPart.Zebbo => PipeBugsBehavior.InitializeBrinstarPipeBug,
        PipeBugsPart.Gamet => PipeBugsBehavior.InitializeNorfairPipeBug,
        PipeBugsPart.Geega => PipeBugsBehavior.InitializeYellowPipeBug,
        _ => throw new InvalidOperationException($"PipeBugs entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunPipeBugsMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => PipeBugsBehavior.AdvancePipeBugsTypedAnimation;
}
