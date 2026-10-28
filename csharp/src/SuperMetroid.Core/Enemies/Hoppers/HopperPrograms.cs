namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Chooses a Sidehopper's or Dessgeega's program for its size, surface and movement phase.</summary>
internal static class HopperPrograms
{
    /// <summary>The program of variant <paramref name="variantIndex"/> on <paramref name="surface"/> in <paramref name="phase"/>.</summary>
    /// <exception cref="InvalidDataException">The variant is not one of the four authored records.</exception>
    internal static HopperProgram Program(int variantIndex, HopperSurface surface, HopperPhase phase) =>
        HopperAnimationDefinitions.Programs.TryGetValue(new HopperProgramKey(variantIndex, surface, phase), out HopperProgram program)
            ? program
            : throw new InvalidDataException($"Hopper animation variant {variantIndex} exceeds four authored records.");
}
