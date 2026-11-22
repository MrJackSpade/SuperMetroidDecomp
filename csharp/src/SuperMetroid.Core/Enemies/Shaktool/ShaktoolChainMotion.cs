namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// How a Shaktool chain moves (#2452): whether it curls or straightens, what the next wall it strikes does to it, and which
/// way round its segments orbit the ones before them. The chain holds one for all its segments (<see cref="ShaktoolChain"/>),
/// and its placement names the one it starts with.
/// </summary>
/// <param name="Phase">Whether the chain curls, each segment at its own speed, or straightens, until a wall stops it.</param>
/// <param name="NextWall">
/// What the next wall the trailing saw strikes does: turns the chain end for end, or, once it has done that since the
/// trailing saw last reached the chain's target, reverses the orbit instead.
/// </param>
/// <param name="OrbitDirection">
/// The way round each segment orbits the one before it, as seen on screen: an orbit angle that grows carries a segment
/// clockwise, from above the segment before it to its right.
/// </param>
internal readonly partial record struct ShaktoolChainMotion(ShaktoolChainPhase Phase, ShaktoolWallResponse NextWall, RotationDirection OrbitDirection)
{
    /// <summary>The motion both Shaktool room states place their chain with: curling clockwise, the next wall turning it end for end.</summary>
    internal static ShaktoolChainMotion CurlingClockwise { get; } =
        new(ShaktoolChainPhase.Curling, ShaktoolWallResponse.TurnEndForEnd, RotationDirection.Clockwise);
}
