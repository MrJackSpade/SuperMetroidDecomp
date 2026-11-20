namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>Owtch's patrol rules.</summary>
internal static class OwtchMovementDefinitions
{
    /// <summary>
    /// Whether a surfacing Owtch walks off right rather than left (#2479): an even choice, taken by half the random
    /// numbers.
    /// </summary>
    internal static readonly RandomChance SurfacesMovingRight = RandomChance.Highest(1, new RandomSampleField(8, 1));

    /// <summary>
    /// The random field a walking Owtch's burial roll starts from: whether it sinks into the ground. The roll adds the
    /// enemy's update count, so the updates it sinks on move on as the count advances.
    /// </summary>
    internal static readonly RandomSampleField SinkRoll = new(0, 8);

    /// <summary>
    /// The chance a walking Owtch buries itself on an update (#1619): the lowest six of the 256 values its
    /// <see cref="SinkRoll"/> reaches once advanced by the enemy's frame counter, six updates in 256.
    /// </summary>
    internal static readonly RandomChance Sinks = RandomChance.Lowest(6, SinkRoll);

    /// <summary>
    /// The deepest an Owtch sinks below its walking height, in whole pixels, before it goes underground; an Owtch placed
    /// underground starts this deep.
    /// </summary>
    internal const int MaximumBurialDepth = 16;

    /// <summary>
    /// How far a sinking Owtch sinks, or a surfacing one rises, each update, in whole pixels (#2375): one pixel of its burial
    /// depth.
    /// </summary>
    internal const int BurrowSpeed = 1;
}
