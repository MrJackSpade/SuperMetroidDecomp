namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Full control-flow result from the transition-table lookup, including its otherwise invisible
/// branch into pose-definition fallback.
/// </summary>
public readonly record struct SamusPoseTransitionLookup(
    SamusPoseTransition? Transition,
    bool UsesPoseDefinitionFallback);
