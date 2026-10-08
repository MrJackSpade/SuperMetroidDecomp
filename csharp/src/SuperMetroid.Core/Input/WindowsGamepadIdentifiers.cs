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

/// <summary>One USB vendor/product pair reported through WinMM's JOYCAPS structure.</summary>
/// <param name="ManufacturerId">Raw sixteen-bit manufacturer identifier from <c>JOYCAPS.wMid</c>, distinct from the device's WinMM slot number.</param>
/// <param name="ProductId">Raw sixteen-bit product identifier from <c>JOYCAPS.wPid</c>, paired with the manufacturer to select a known physical layout.</param>
public readonly record struct UsbGamepadIdentifier(ushort ManufacturerId, ushort ProductId);

/// <summary>Mutually exclusive WinMM MMRESULT values consumed by the adapter.</summary>
public enum WindowsMultimediaResult : uint
{
    /// <summary>WinMM success result <c>JOYERR_NOERROR</c>/<c>MMSYSERR_NOERROR</c>; the adapter accepts capabilities and position data only when the native call returns zero.</summary>
    NoError = 0,
}

/// <summary>Actual bit mask accepted by WinMM's JOYINFOEX query.</summary>
[Flags]
public enum JoystickPositionQueryFlags : uint
{
    /// <summary><c>JOY_RETURNX</c>: requests valid X-axis data in <c>JOYINFOEX.dwXpos</c>, subsequently normalized by the desktop adapter for horizontal input.</summary>
    ReturnX = 0x00000001,
    /// <summary><c>JOY_RETURNY</c>: requests valid Y-axis data in <c>JOYINFOEX.dwYpos</c>, subsequently normalized by the desktop adapter for vertical input.</summary>
    ReturnY = 0x00000002,
    /// <summary><c>JOY_RETURNZ</c>: requests valid third-axis data in <c>JOYINFOEX.dwZpos</c>; this axis is not mapped to SNES controls.</summary>
    ReturnZ = 0x00000004,
    /// <summary><c>JOY_RETURNR</c>: requests valid rudder or fourth-axis data in <c>JOYINFOEX.dwRpos</c>; this axis is not mapped to SNES controls.</summary>
    ReturnR = 0x00000008,
    /// <summary><c>JOY_RETURNU</c>: requests valid fifth-axis data in <c>JOYINFOEX.dwUpos</c>; this axis is not mapped to SNES controls.</summary>
    ReturnU = 0x00000010,
    /// <summary><c>JOY_RETURNV</c>: requests valid sixth-axis data in <c>JOYINFOEX.dwVpos</c>; this axis is not mapped to SNES controls.</summary>
    ReturnV = 0x00000020,
    /// <summary><c>JOY_RETURNPOV</c>: requests the directional hat in <c>JOYINFOEX.dwPOV</c>; the adapter uses the centered sentinel when capabilities report no hat.</summary>
    ReturnPointOfView = 0x00000040,
    /// <summary><c>JOY_RETURNBUTTONS</c>: requests <c>JOYINFOEX.dwButtons</c>, where bit zero represents button one and multiple pressed buttons combine as a bitmask.</summary>
    ReturnButtons = 0x00000080,
    /// <summary><c>JOY_RETURNALL</c> (<c>$FF</c>): combines all six axis, POV, and button requests used for each desktop poll; does not request raw, centered, or continuous-POV modifiers.</summary>
    ReturnAll = ReturnX | ReturnY | ReturnZ | ReturnR | ReturnU | ReturnV |
        ReturnPointOfView | ReturnButtons,
}

/// <summary>Actual bit mask returned by WinMM's JOYCAPS capability field.</summary>
[Flags]
public enum JoystickCapabilityFlags : uint
{
    /// <summary><c>JOYCAPS_HASPOV</c>: the device reports directional-hat information; without this capability, the adapter supplies the centered POV value regardless of the returned position field.</summary>
    HasPointOfView = 0x00000010,
}
