namespace SuperMetroid.Desktop.Audio;

/// <summary>Preserves the WinMM operation and error number for endpoint-specific recovery.</summary>
internal sealed class WaveOutDeviceException(uint result, string operation)
    : InvalidOperationException($"{operation} failed with multimedia error {result}.")
{
    /// <summary>Signals WinMM's invalid-handle or missing-driver result, allowing audio endpoint recovery without treating other failures as disconnections.</summary>
    internal bool EndpointUnavailable => result is 5 or 6;
}
