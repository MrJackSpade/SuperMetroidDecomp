namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What the input-transition lookup decides: a new pose, the pose's no-input fallback, or keeping the pose.
/// </summary>
public readonly record struct SamusPoseTransitionLookup(
    SamusPoseTransition? Transition,
    bool UsesPoseDefinitionFallback);
