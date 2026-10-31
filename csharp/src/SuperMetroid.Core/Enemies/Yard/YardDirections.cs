namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Finds a Yard's surface direction and its airborne animations.</summary>
internal static class YardDirections
{
    /// <summary>The orientation of <paramref name="direction"/>.</summary>
    internal static YardDirectionDefinition ForDirection(YardDirection direction) =>
        Ensure.Listed(YardDirectionDefinitions.Directions, direction);

    /// <summary>Whether <paramref name="direction"/> crawls along a wall rather than a floor or ceiling.</summary>
    internal static bool IsOnWall(YardDirection direction) => direction switch
    {
        YardDirection.UpsideRightMovingUp or YardDirection.UpsideRightMovingDown or
            YardDirection.UpsideLeftMovingUp or YardDirection.UpsideLeftMovingDown => true,
        YardDirection.UpsideDownMovingLeft or YardDirection.UpsideDownMovingRight or
            YardDirection.UpsideUpMovingLeft or YardDirection.UpsideUpMovingRight => false,
        _ => throw new InvalidOperationException($"Undefined {nameof(YardDirection)} {direction}."),
    };

    /// <summary>The visible and hidden airborne animations of <paramref name="facing"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="facing"/> is neither facing.</exception>
    internal static YardAirborneInstructionDefinition ForAirborneFacing(YardAirborneFacing facing) =>
        YardDirectionDefinitions.AirborneFacings.TryGetValue(facing, out YardAirborneInstructionDefinition airborne)
            ? airborne
            : throw new InvalidDataException(
                $"Yard airborne facing {facing} exceeds its two cartridge definitions.");
}
