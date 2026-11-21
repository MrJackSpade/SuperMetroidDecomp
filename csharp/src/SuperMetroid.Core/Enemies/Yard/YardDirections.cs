namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Finds a Yard's surface direction and its airborne animations.</summary>
internal static partial class YardDirections
{
    /// <summary>The orientation of <paramref name="direction"/>.</summary>
    internal static YardDirectionDefinition ForDirection(YardDirection direction) =>
        YardDirectionDefinitions.Directions[direction];

    /// <summary>The is on wall of each direction.</summary>
    private static IdentityTable<YardDirection, bool> IsOnWallByDirection => field ??= new(new Dictionary<YardDirection, bool>
    {
        [YardDirection.OnLeftWallMovingUp] = true,
        [YardDirection.OnLeftWallMovingDown] = true,
        [YardDirection.OnRightWallMovingUp] = true,
        [YardDirection.OnRightWallMovingDown] = true,
        [YardDirection.OnCeilingMovingLeft] = false,
        [YardDirection.OnCeilingMovingRight] = false,
        [YardDirection.OnFloorMovingLeft] = false,
        [YardDirection.OnFloorMovingRight] = false,
    });

    /// <summary>Whether <paramref name="direction"/> crawls along a wall rather than a floor or ceiling.</summary>
    internal static bool IsOnWall(YardDirection direction) => IsOnWallByDirection[direction];

    /// <summary>The visible and hidden airborne animations of <paramref name="facing"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="facing"/> is neither facing.</exception>
    internal static YardAirborneAnimationDefinition ForAirborneFacing(HorizontalDirection facing) =>
        YardDirectionDefinitions.AirborneFacings[facing];
}
