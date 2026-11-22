using System.Runtime.ExceptionServices;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Rendering.Direct3D11.Frames;

namespace SuperMetroid.Rendering.Direct3D11.Device;

/// <summary>
/// The render owner's device-loss bookkeeping and decisions (#2602): the context a device loss is reported with, how many
/// device generations were recreated after startup, whether a failed generation is recreated
/// (<see cref="D3D11RecoveryPolicy"/>) and the terminal fault the owner keeps for its callers once it is not. The owner
/// thread decides; any thread reads.
/// </summary>
internal sealed class D3D11DeviceRecovery
{
    /// <summary>The recreations since the last successful submission, which only the owner thread touches.</summary>
    private int consecutiveLosses;

    /// <summary>The device generations recreated after startup.</summary>
    private long recoveries;

    /// <summary>The owner's terminal failure, with its original stack.</summary>
    private ExceptionDispatchInfo? fault;

    /// <summary>The device generations recreated after startup, excluding the first device and failed recreations.</summary>
    internal long Recoveries => Interlocked.Read(ref recoveries);

    /// <summary>
    /// The context of a device generation's loss, captured before its device is released, which the owner reports: the
    /// failing call's own result, the removal reason the still-live device reports, its adapter and backend, the swapchain
    /// size and the frame it was drawing.
    /// </summary>
    /// <param name="error">The failure that ended the generation, a device loss (<see cref="D3D11RecoveryPolicy.IsDeviceLoss"/>).</param>
    /// <param name="removalReason">The removal reason the failed device reports; zero when the device was not removed.</param>
    /// <param name="adapter">The failed device's adapter, not its replacement's.</param>
    /// <param name="backend">The backend the failed device was made for.</param>
    /// <param name="target">The swapchain's size when the device failed.</param>
    /// <param name="frame">The frame being drawn, or none before the first.</param>
    /// <exception cref="ArgumentException">The failure is no device loss.</exception>
    internal static D3D11DeviceLossDiagnostic Loss(Exception error, int removalReason, string adapter, D3D11DeviceKind backend,
        Extent16<DisplayPixels> target, RenderFrameIdentity? frame) =>
        D3D11RecoveryPolicy.IsDeviceLoss(error.HResult)
            ? new D3D11DeviceLossDiagnostic(error.HResult, removalReason, adapter, backend, target, frame)
            : throw new ArgumentException($"Failure 0x{error.HResult:X8} is no device loss.", nameof(error));

    /// <summary>
    /// Whether the owner recreates its device after a generation failed with <paramref name="error"/>
    /// (<see cref="D3D11RecoveryPolicy.Recreates"/>), counting the recreation it allows.
    /// </summary>
    /// <param name="error">The failure that ended the device generation.</param>
    /// <param name="started">Whether the first device generation became ready.</param>
    internal bool Recreates(Exception error, bool started) => D3D11RecoveryPolicy.Recreates(error, started, ref consecutiveLosses);

    /// <summary>Records a device generation made ready after the first: one recovery.</summary>
    internal void Recovered() => Interlocked.Increment(ref recoveries);

    /// <summary>Records a successful submission, which ends a run of consecutive losses.</summary>
    internal void Submitted() => consecutiveLosses = 0;

    /// <summary>Keeps the owner's terminal failure for its callers, the first one only.</summary>
    internal void Fault(Exception error) => Interlocked.CompareExchange(ref fault, ExceptionDispatchInfo.Capture(error), null);

    /// <summary>Rethrows the owner's terminal failure, with its original type, result and stack; returns when there is none.</summary>
    internal void ThrowIfFaulted() => Volatile.Read(ref fault)?.Throw();
}
