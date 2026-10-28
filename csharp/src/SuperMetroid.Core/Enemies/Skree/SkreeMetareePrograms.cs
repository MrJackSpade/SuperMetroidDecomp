namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Chooses a Skree's or Metaree's program for its animation phase.</summary>
internal static class SkreeMetareePrograms
{
    /// <summary>The Metaree program of <paramref name="phase"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="phase"/> is outside the four-entry table.</exception>
    internal static SkreeMetareeProgram MetareeProgram(SkreeMetareeAnimationPhase phase) =>
        SkreeMetareeAnimationDefinitions.MetareePrograms.TryGetValue(phase, out SkreeMetareeProgram program)
            ? program
            : throw InvalidPhase(phase, "Metaree");

    /// <summary>The Skree program of <paramref name="phase"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="phase"/> is outside the four-entry table.</exception>
    internal static SkreeMetareeProgram SkreeProgram(SkreeMetareeAnimationPhase phase) =>
        SkreeMetareeAnimationDefinitions.SkreePrograms.TryGetValue(phase, out SkreeMetareeProgram program)
            ? program
            : throw InvalidPhase(phase, "Skree");

    /// <summary>Reports an out-of-table animation selector with its enemy name.</summary>
    private static InvalidDataException InvalidPhase(SkreeMetareeAnimationPhase phase, string enemyName) =>
        new($"{enemyName} animation phase {phase} exceeds its four-entry table.");
}
