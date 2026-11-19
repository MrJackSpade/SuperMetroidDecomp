namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Definition data for the four pause fades.</summary>
internal static class PauseFadeTiming
{
    /// <summary>
    /// The updates pausing, the pause fades and unpausing wait before each brightness step, the first included: the
    /// screen holds its brightness for that wait, then steps.
    /// </summary>
    public const int WaitBeforeEachStep = 1;
}
