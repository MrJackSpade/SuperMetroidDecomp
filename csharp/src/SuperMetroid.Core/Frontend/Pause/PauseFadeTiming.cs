namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Definition data for the four pause fades.</summary>
internal static class PauseFadeTiming
{
    /// <summary>
    /// Pausing, the pause fades and unpausing initialize both fade counters
    /// to one. The fades consume a counter-only frame before each change.
    /// </summary>
    public const int CounterReload = 1;
}
