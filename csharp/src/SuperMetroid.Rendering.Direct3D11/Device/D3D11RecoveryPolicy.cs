namespace SuperMetroid.Rendering.Direct3D11.Device;

/// <summary>DXGI device-lifetime failures and bounded recreation policy, not generic error suppression.</summary>
internal static class D3D11RecoveryPolicy
{
    /// <summary>Fail loudly after repeated recreation without any successful submission.</summary>
    internal const int MaximumConsecutiveRecreations = 3;
    /// <summary>Recognizes the two DXGI results for which renderer device recreation is appropriate.</summary>
    internal static bool IsDeviceLoss(int result) => Enum.IsDefined((DxgiDeviceLoss)result);

    /// <summary>
    /// Whether the render owner recreates its device after one device generation failed with <paramref name="error"/>: a
    /// device loss after startup, while fewer than <see cref="MaximumConsecutiveRecreations"/> recreations ran since the
    /// last successful submission. A recreation it allows is counted in <paramref name="consecutiveLosses"/>; any other
    /// failure is the owner's terminal fault.
    /// </summary>
    /// <param name="error">The failure that ended the device generation.</param>
    /// <param name="started">Whether the first device generation became ready; a startup failure is never retried.</param>
    /// <param name="consecutiveLosses">The recreations since the last successful submission.</param>
    internal static bool Recreates(Exception error, bool started, ref int consecutiveLosses) =>
        started && IsDeviceLoss(error.HResult) && consecutiveLosses++ < MaximumConsecutiveRecreations;
}
