using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Immutable failure context captured on the owner before disposing the failed device.</summary>
public sealed record D3D11DeviceLossDiagnostic(int FailureHResult, int RemovalReason,
    string Adapter, D3D11DeviceKind Backend, int Width, int Height, RenderFrameIdentity? Frame)
{
    public override string ToString() => $"GPU {Backend} failure 0x{FailureHResult:X8}; removal reason 0x{RemovalReason:X8}; " +
        $"adapter={Adapter}; target={Width}x{Height}; frame={Frame?.ToString() ?? "none"}.";
}
