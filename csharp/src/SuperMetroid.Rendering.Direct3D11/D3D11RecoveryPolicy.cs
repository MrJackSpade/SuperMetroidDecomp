namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>DXGI device-lifetime failures and bounded recreation policy, not generic error suppression.</summary>
internal static class D3D11RecoveryPolicy
{
    /// <summary>winerror.h DXGI_ERROR_DEVICE_REMOVED: the adapter/device must be recreated.</summary>
    internal const int DeviceRemoved = unchecked((int)0x887A0005);
    /// <summary>winerror.h DXGI_ERROR_DEVICE_RESET: the device was reset and its resources are invalid.</summary>
    internal const int DeviceReset = unchecked((int)0x887A0007);
    /// <summary>Fail loudly after repeated recreation without any successful submission.</summary>
    internal const int MaximumConsecutiveRecreations = 3;
    internal static bool IsDeviceLoss(int result) => result is DeviceRemoved or DeviceReset;
}
