using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Immutable failure context captured on the owner before disposing the failed device.</summary>
/// <param name="FailureHResult">Original failing operation's HRESULT, retained independently of the device-removal reason.</param>
/// <param name="RemovalReason">HRESULT queried from the still-live failed device; may be zero for a synthetic failure without actual removal.</param>
/// <param name="Adapter">Description of the failed device's adapter, not a subsequently recreated adapter.</param>
/// <param name="Backend">Explicit hardware or WARP selection used for the failed device.</param>
/// <param name="Width">Last active swapchain width in client pixels.</param>
/// <param name="Height">Last active swapchain height in client pixels.</param>
/// <param name="Frame">Retained frame identity at failure, or null when no immutable frame has been selected.</param>
public sealed record D3D11DeviceLossDiagnostic(int FailureHResult, int RemovalReason,
    string Adapter, D3D11DeviceKind Backend, int Width, int Height, RenderFrameIdentity? Frame)
{
    /// <summary>Formats the captured backend, hexadecimal failure/removal HRESULTs, adapter, target dimensions, and optional frame identity without querying GPU state.</summary>
    /// <returns>One diagnostic line suitable for the worker's standard-error failure report.</returns>
    public override string ToString() => $"GPU {Backend} failure 0x{FailureHResult:X8}; removal reason 0x{RemovalReason:X8}; " +
        $"adapter={Adapter}; target={Width}x{Height}; frame={Frame?.ToString() ?? "none"}.";
}
