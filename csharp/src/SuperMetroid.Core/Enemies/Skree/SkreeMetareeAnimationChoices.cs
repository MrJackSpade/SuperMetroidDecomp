namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Chooses a Skree's or Metaree's segment for its animation phase.</summary>
internal static class SkreeMetareeAnimationChoices
{
    /// <summary>The Metaree animation of <paramref name="phase"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="phase"/> is not one of the four animation phases.</exception>
    internal static SkreeMetareeSegment MetareeAnimation(SkreeMetareeAnimationPhase phase) =>
        SkreeMetareeAnimationDefinitions.MetareeAnimations.TryGetValue(phase, out SkreeMetareeSegment segment)
            ? segment
            : throw InvalidPhase(phase, "Metaree");

    /// <summary>The Skree animation of <paramref name="phase"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="phase"/> is not one of the four animation phases.</exception>
    internal static SkreeMetareeSegment SkreeAnimation(SkreeMetareeAnimationPhase phase) =>
        SkreeMetareeAnimationDefinitions.SkreeAnimations.TryGetValue(phase, out SkreeMetareeSegment segment)
            ? segment
            : throw InvalidPhase(phase, "Skree");

    /// <summary>Reports an animation phase with no animation, with its enemy name.</summary>
    private static InvalidDataException InvalidPhase(SkreeMetareeAnimationPhase phase, string enemyName) =>
        new($"{enemyName} has no animation for phase {phase}.");
}
