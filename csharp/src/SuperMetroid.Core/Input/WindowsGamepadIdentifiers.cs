namespace SuperMetroid.Core.Input;

/// <summary>Typed Windows joystick/HID identifiers used by the desktop input adapter.</summary>
public static class WindowsGamepadIdentifiers
{
    /// <summary>The inexpensive SNES-style adapter whose buttons follow printed labels.</summary>
    public static readonly UsbGamepadIdentifier SnesUsbAdapter0079_0011 =
        new(ManufacturerId: 0x0079, ProductId: 0x0011);

    /// <summary>WinMM's sentinel device ID for “no joystick selected.”</summary>
    public const uint NoDevice = uint.MaxValue;

    /// <summary>POV-hat value returned by Windows when no direction is held.</summary>
    public const uint CenteredPointOfView = 0xffff;

    /// <summary>Maps a Windows VID/PID pair to its proven physical face-button ordering.</summary>
    /// <param name="manufacturerId">Manufacturer identifier reported in WinMM's <c>JOYCAPS.wMid</c>.</param>
    /// <param name="productId">Product identifier reported in WinMM's <c>JOYCAPS.wPid</c>.</param>
    /// <returns>The printed-label layout for the recognized <c>0079:0011</c> adapter, otherwise the positional south/east/west/north layout; no device query or gameplay binding change occurs.</returns>
    public static GenericGamepadFaceButtonLayout ResolveFaceButtonLayout(
        ushort manufacturerId,
        ushort productId) =>
        new UsbGamepadIdentifier(manufacturerId, productId) == SnesUsbAdapter0079_0011
            ? GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011
            : GenericGamepadFaceButtonLayout.Positional;
}
