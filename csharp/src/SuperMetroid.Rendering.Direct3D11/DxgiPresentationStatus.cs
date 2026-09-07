namespace SuperMetroid.Rendering.Direct3D11;

internal static class DxgiPresentationStatus
{
    /// <summary>Windows SDK winerror.h DXGI_STATUS_OCCLUDED: target is not visible; not a displayed frame.</summary>
    internal const int Occluded = 0x087a0001;
}
