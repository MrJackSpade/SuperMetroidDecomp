namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>One PipeBugs occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The PipeBugs species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PipeBugsEntity(PipeBugsBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (PipeBugsPartDefinitions.Parts[Slot.DefinitionId])
        {
            case PipeBugsPart.Zeb or PipeBugsPart.Zebbo:
                PipeBugsBehavior.InitializeBrinstarPipeBug(Slot);
                break;
            case PipeBugsPart.Gamet:
                species.InitializeNorfairPipeBug(Slot);
                break;
            case PipeBugsPart.Geega:
                PipeBugsBehavior.InitializeYellowPipeBug(Slot);
                break;
            default:
                throw new InvalidOperationException($"PipeBugs entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPipeBugsMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvancePipeBugsTypedAnimation(Slot, context);
}
