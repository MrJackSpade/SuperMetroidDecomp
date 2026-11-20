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
        YardDirection.OnLeftWallMovingUp or YardDirection.OnLeftWallMovingDown or
            YardDirection.OnRightWallMovingUp or YardDirection.OnRightWallMovingDown => true,
        YardDirection.OnCeilingMovingLeft or YardDirection.OnCeilingMovingRight or
            YardDirection.OnFloorMovingLeft or YardDirection.OnFloorMovingRight => false,
        _ => throw new InvalidOperationException($"Undefined {nameof(YardDirection)} {direction}."),
    };

    /// <summary>The visible and hidden airborne animations of <paramref name="facing"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="facing"/> is neither facing.</exception>
    internal static YardAirborneAnimationDefinition ForAirborneFacing(HorizontalDirection facing) =>
        YardDirectionDefinitions.AirborneFacings[facing];
}
