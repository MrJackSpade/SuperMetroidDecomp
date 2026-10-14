namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>The DXGI HRESULTs that report device loss; every other HRESULT is an ordinary failure.</summary>
internal enum DxgiDeviceLoss
{
    /// <summary>winerror.h DXGI_ERROR_DEVICE_REMOVED: the adapter/device must be recreated.</summary>
    DeviceRemoved = unchecked((int)0x887A0005),
    /// <summary>winerror.h DXGI_ERROR_DEVICE_RESET: the device was reset and its resources are invalid.</summary>
    DeviceReset = unchecked((int)0x887A0007),
}
