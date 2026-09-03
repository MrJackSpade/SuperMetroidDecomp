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
    public static GenericGamepadFaceButtonLayout ResolveFaceButtonLayout(
        ushort manufacturerId,
        ushort productId) =>
        new UsbGamepadIdentifier(manufacturerId, productId) == SnesUsbAdapter0079_0011
            ? GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011
            : GenericGamepadFaceButtonLayout.Positional;
}

/// <summary>One USB vendor/product pair reported through WinMM's JOYCAPS structure.</summary>
public readonly record struct UsbGamepadIdentifier(ushort ManufacturerId, ushort ProductId);

/// <summary>Mutually exclusive WinMM MMRESULT values consumed by the adapter.</summary>
public enum WindowsMultimediaResult : uint
{
    NoError = 0,
}

/// <summary>Actual bit mask accepted by WinMM's JOYINFOEX query.</summary>
[Flags]
public enum JoystickPositionQueryFlags : uint
{
    ReturnX = 0x00000001,
    ReturnY = 0x00000002,
    ReturnZ = 0x00000004,
    ReturnR = 0x00000008,
    ReturnU = 0x00000010,
    ReturnV = 0x00000020,
    ReturnPointOfView = 0x00000040,
    ReturnButtons = 0x00000080,
    ReturnAll = ReturnX | ReturnY | ReturnZ | ReturnR | ReturnU | ReturnV |
        ReturnPointOfView | ReturnButtons,
}

/// <summary>Actual bit mask returned by WinMM's JOYCAPS capability field.</summary>
[Flags]
public enum JoystickCapabilityFlags : uint
{
    None = 0,
    HasZ = 0x00000001,
    HasR = 0x00000002,
    HasU = 0x00000004,
    HasV = 0x00000008,
    HasPointOfView = 0x00000010,
    PointOfViewSupportsFourDirections = 0x00000020,
    PointOfViewSupportsContinuousAngles = 0x00000040,
}
