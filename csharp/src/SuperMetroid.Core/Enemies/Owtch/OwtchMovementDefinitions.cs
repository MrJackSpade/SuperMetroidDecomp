namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>Compiled NTSC patrol rules for Owtch.</summary>
internal static class OwtchMovementDefinitions
{
    /// <summary>The random bit that sends a surfacing Owtch right rather than left: bit eight.</summary>
    internal static readonly RandomSampleField MoveRightChoice = new(8, 1);

    /// <summary>
    /// The random field a walking Owtch's burial roll starts from: the low byte, bits zero to seven. The roll adds the
    /// enemy's frame counter and continues past the field's last value from zero.
    /// </summary>
    internal static readonly RandomSampleField SinkRoll = new(0, 8);
}
