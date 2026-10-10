namespace SuperMetroid.Desktop;

/// <summary>Preserves the WinMM operation and error number for endpoint-specific recovery.</summary>
/// <param name="result">WinMM multimedia result code reported by the failed operation.</param>
/// <param name="operation">Name of the WinMM operation that failed.</param>
internal sealed class WaveOutDeviceException(uint result, string operation)
    : InvalidOperationException($"{operation} failed with multimedia error {result}.")
{
    // Windows SDK mmresult: MMSYSERR_INVALHANDLE=5, MMSYSERR_NODRIVER=6.
    /// <summary>Whether WinMM reported an invalid handle or missing driver, allowing endpoint recovery.</summary>
    internal bool EndpointUnavailable => result is 5 or 6;
}
