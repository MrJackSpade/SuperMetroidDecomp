namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Finds a Yard's surface direction and its airborne animations.</summary>
internal static class YardDirections
{
    /// <summary>The orientation of direction number <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is outside the eight definitions.</exception>
    internal static YardDirectionDefinition ForDirection(int direction) =>
        (uint)direction < (uint)YardDirectionDefinitions.Directions.Length
            ? YardDirectionDefinitions.Directions[direction]
            : throw new InvalidDataException($"Yard direction ${direction:X4} exceeds its eight cartridge definitions.");

    /// <summary>The visible and hidden airborne animations of <paramref name="facing"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="facing"/> is neither facing.</exception>
    internal static YardAirborneInstructionDefinition ForAirborneFacing(YardAirborneFacing facing) =>
        YardDirectionDefinitions.AirborneFacings.TryGetValue(facing, out YardAirborneInstructionDefinition airborne)
            ? airborne
            : throw new InvalidDataException(
                $"Yard airborne facing {facing} exceeds its two cartridge definitions.");
}
