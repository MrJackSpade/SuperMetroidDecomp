namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>Compiled NTSC patrol rules for Owtch.</summary>
internal static class OwtchMovementDefinitions
{
    /// <summary>The random bit that sends a surfacing Owtch right rather than left: bit eight.</summary>
    internal static readonly RandomSampleField MoveRightChoice = new(8, 1);
}
