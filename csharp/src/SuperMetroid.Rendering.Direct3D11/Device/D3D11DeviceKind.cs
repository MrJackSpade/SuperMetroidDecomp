namespace SuperMetroid.Rendering.Direct3D11.Device;

/// <summary>Explicit device selection; hardware requests never silently become WARP.</summary>
public enum D3D11DeviceKind
{
    /// <summary>Creates the requested feature-level-11 device on the first enumerated non-software DXGI adapter; absence or creation failure is not replaced with WARP.</summary>
    Hardware,
    /// <summary>Explicitly selects Microsoft's WARP software rasterizer for the D3D11 backend, rather than a portable CPU-renderer fallback.</summary>
    Warp
}
