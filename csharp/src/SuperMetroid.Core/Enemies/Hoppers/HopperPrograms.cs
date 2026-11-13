namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Chooses a Sidehopper's or Dessgeega's kind from its definition, and its program for its kind, surface and movement phase.</summary>
internal static class HopperPrograms
{
    /// <summary>The kind of hopper <paramref name="definition"/> is.</summary>
    /// <exception cref="InvalidDataException">The definition is no hopper.</exception>
    internal static HopperKind KindOf(EnemyDefinitionId definition) =>
        HopperKindDefinitions.Kinds.TryGetValue(definition, out HopperKind kind)
            ? kind
            : throw new InvalidDataException($"Enemy definition {definition} is no hopper.");

    /// <summary>The program of a <paramref name="kind"/> hopper on <paramref name="surface"/> in <paramref name="phase"/>.</summary>
    /// <exception cref="InvalidDataException">The kind is undefined.</exception>
    internal static HopperProgram Program(HopperKind kind, HopperSurface surface, HopperPhase phase) =>
        HopperAnimationDefinitions.Programs.TryGetValue(new HopperProgramKey(kind, surface, phase), out HopperProgram program)
            ? program
            : throw new InvalidDataException($"Undefined {nameof(HopperKind)} {kind}.");
}
