namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>One PipeBugs occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The PipeBugs species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PipeBugsEntity(PipeBugsBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (PipeBugsPartDefinitions.Parts[Place.DefinitionId])
        {
            case PipeBugsPart.Zeb or PipeBugsPart.Zebbo:
                PipeBugsBehavior.InitializeBrinstarPipeBug(Place);
                break;
            case PipeBugsPart.Gamet:
                species.InitializeNorfairPipeBug(Place);
                break;
            case PipeBugsPart.Geega:
                PipeBugsBehavior.InitializeYellowPipeBug(Place);
                break;
            default:
                throw new InvalidOperationException($"PipeBugs entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPipeBugsMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvancePipeBugsTypedAnimation(Place, context);
}
