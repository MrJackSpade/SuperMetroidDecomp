namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Full control-flow result from <c>$91:81A9</c>, including its otherwise invisible
/// branch into pose-definition fallback at <c>$91:82D9</c>.
/// </summary>
public readonly record struct SamusPoseTransitionLookup(
    SamusPoseTransition? Transition,
    bool UsesPoseDefinitionFallback);
