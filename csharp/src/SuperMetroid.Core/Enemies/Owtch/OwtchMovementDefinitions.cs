namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>Compiled NTSC patrol rules for Owtch.</summary>
internal static class OwtchMovementDefinitions
{
    /// <summary>The random bit that sends a surfacing Owtch right rather than left: bit eight.</summary>
    internal static readonly RandomSampleField MoveRightChoice = new(8, 1);

    /// <summary>
    /// The random field a walking Owtch's burial roll starts from: bits zero to seven, 0 through 255. The roll adds the
    /// enemy's frame counter and continues past the field's last value from zero.
    /// </summary>
    internal static readonly RandomSampleField SinkRoll = new(0, 8);

    /// <summary>
    /// The chance a walking Owtch buries itself on an update (#1619): the lowest six of the 256 values its
    /// <see cref="SinkRoll"/> reaches once advanced by the enemy's frame counter, six updates in 256.
    /// </summary>
    internal static readonly RandomChance Sinks = RandomChance.Lowest(6, SinkRoll);
}
