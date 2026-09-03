using SuperMetroid.Core.Input;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SuperMetroid.Desktop;

/// <summary>
/// Hot-pluggable WinMM adapter for legacy, DirectInput, and generic USB game controllers.
/// </summary>
/// <remarks>
/// This deliberately uses the operating system's joystick API instead of XInput. XInput is
/// excellent for Xbox-compatible devices but does not enumerate the connected VID_0079 /
/// PID_0011 generic HID pad. WinMM sits above the installed HID joystick driver, requires no
/// native package beside the executable, and returns all state in one non-blocking call.
/// </remarks>
internal sealed partial class WindowsGamepadInput
{
    // This is the inexpensive SNES-style USB adapter which motivated the WinMM backend.
    // Its HID/WinMM driver enumerates the face buttons by printed label X/A/B/Y rather than
    // DirectInput's usual positional B/A/Y/X order. Apply that quirk only to this exact
    // device so Xbox-style and other conventional controllers retain positional mapping.
    private const ushort SnesUsbAdapterManufacturerId = 0x0079;
    private const ushort SnesUsbAdapterProductId = 0x0011;
    private const uint NoDevice = uint.MaxValue;
    private const uint JoyReturnAll = 0x000000ff;
    private const uint JoyCapabilitiesHasPointOfView = 0x00000010;
    private const uint JoyPovCentered = GenericGamepadInput.CenteredPointOfView;
    private const uint MmSystemNoError = 0;
    private const double RediscoveryIntervalSeconds = 1.0;

    private uint activeDeviceId = NoDevice;
    private JoystickCapabilities activeCapabilities;
    private long nextDiscoveryTimestamp;

    /// <summary>Name reported by the active joystick driver, or null while disconnected.</summary>
    public string? DeviceName { get; private set; }

    /// <summary>Most recent normalized state, exposed for debugger/CLI device diagnostics.</summary>
    public GenericGamepadSnapshot LastSnapshot { get; private set; }

    /// <summary>
    /// Polls the active controller and returns its SNES buttons. A disconnected controller
    /// contributes zero input and triggers a throttled rescan, so unplug/replug works while
    /// the game remains open without putting device discovery in the 60-Hz hot path.
    /// </summary>
    public SnesButton Poll()
    {
        if (activeDeviceId != NoDevice && TryRead(activeDeviceId, out JoystickPosition position))
            return MapAndRemember(position);

        if (activeDeviceId != NoDevice)
            ForgetActiveDevice();

        long now = Stopwatch.GetTimestamp();
        if (now < nextDiscoveryTimestamp)
            return SnesButton.None;

        nextDiscoveryTimestamp = now +
            (long)(RediscoveryIntervalSeconds * Stopwatch.Frequency);
        DiscoverFirstConnectedDevice();
        if (activeDeviceId == NoDevice || !TryRead(activeDeviceId, out position))
            return SnesButton.None;

        return MapAndRemember(position);
    }

    private void DiscoverFirstConnectedDevice()
    {
        uint possibleDeviceCount = NativeMethods.JoyGetNumberOfDevices();
        for (uint deviceId = 0; deviceId < possibleDeviceCount; deviceId++)
        {
            var capabilities = new JoystickCapabilities();
            uint result = NativeMethods.JoyGetDeviceCapabilities(
                deviceId,
                ref capabilities,
                (uint)Marshal.SizeOf<JoystickCapabilities>());
            if (result != MmSystemNoError || !TryRead(deviceId, out JoystickPosition initialPosition))
                continue;

            activeDeviceId = deviceId;
            activeCapabilities = capabilities;
            DeviceName = string.IsNullOrWhiteSpace(capabilities.ProductName)
                ? $"Gamepad {deviceId + 1}"
                : capabilities.ProductName.TrimEnd('\0');
            LastSnapshot = CreateSnapshot(initialPosition, capabilities);
            Console.WriteLine(
                $"Gamepad connected: {DeviceName} " +
                $"({capabilities.ButtonCount} buttons, {capabilities.AxisCount} axes, joystick {deviceId}); " +
                $"raw X/Y={initialPosition.X}/{initialPosition.Y} in " +
                $"{capabilities.XMinimum}..{capabilities.XMaximum}/" +
                $"{capabilities.YMinimum}..{capabilities.YMaximum}, " +
                $"normalized X/Y={LastSnapshot.HorizontalAxis}/{LastSnapshot.VerticalAxis}, " +
                $"face layout={ResolveFaceButtonLayout(capabilities)}.");
            return;
        }
    }

    private void ForgetActiveDevice()
    {
        if (DeviceName is not null)
            Console.WriteLine($"Gamepad disconnected: {DeviceName}.");
        activeDeviceId = NoDevice;
        DeviceName = null;
        activeCapabilities = default;
        LastSnapshot = default;
    }

    private static bool TryRead(uint deviceId, out JoystickPosition position)
    {
        position = new JoystickPosition
        {
            Size = (uint)Marshal.SizeOf<JoystickPosition>(),
            Flags = JoyReturnAll,
            PointOfView = JoyPovCentered,
        };
        return NativeMethods.JoyGetPosition(deviceId, ref position) == MmSystemNoError;
    }

    private SnesButton MapAndRemember(JoystickPosition position)
    {
        LastSnapshot = CreateSnapshot(position, activeCapabilities);
        return GenericGamepadInput.Map(
            LastSnapshot,
            faceButtonLayout: ResolveFaceButtonLayout(activeCapabilities));
    }

    private static GenericGamepadFaceButtonLayout ResolveFaceButtonLayout(
        JoystickCapabilities capabilities) =>
        capabilities.ManufacturerId == SnesUsbAdapterManufacturerId &&
        capabilities.ProductId == SnesUsbAdapterProductId
            ? GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011
            : GenericGamepadFaceButtonLayout.Positional;

    private static GenericGamepadSnapshot CreateSnapshot(
        JoystickPosition position,
        JoystickCapabilities capabilities)
    {
        // Joystick drivers choose their own unsigned axis limits. Normalizing the live
        // values through the advertised range makes the Core mapper's dead zone identical
        // for an 8-bit adapter, a 10-bit analogue stick, or the usual 16-bit HID range.
        // Some two-axis generic drivers leave dwPOV at zero even though JOYCAPS reports
        // that no POV hat exists. Zero would mean “Up” for a real hat, so consult the
        // capability bit instead of mistaking an unused zero-filled field for held input.
        uint pointOfView =
            (capabilities.Capabilities & JoyCapabilitiesHasPointOfView) != 0
                ? position.PointOfView
                : JoyPovCentered;
        return new GenericGamepadSnapshot(
            HorizontalAxis: NormalizeAxis(position.X, capabilities.XMinimum, capabilities.XMaximum),
            VerticalAxis: NormalizeAxis(position.Y, capabilities.YMinimum, capabilities.YMaximum),
            PointOfView: pointOfView,
            Buttons: position.Buttons);
    }

    private static short NormalizeAxis(uint value, uint minimum, uint maximum)
    {
        if (maximum <= minimum)
        {
            throw new InvalidDataException(
                $"Gamepad driver reported invalid axis range {minimum}..{maximum}.");
        }
        if (value < minimum || value > maximum)
        {
            throw new InvalidDataException(
                $"Gamepad driver reported axis value {value} outside its advertised " +
                $"range {minimum}..{maximum}.");
        }

        long offset = (long)value - minimum;
        long normalized = (offset * ushort.MaxValue) / (maximum - minimum) + short.MinValue;
        return checked((short)normalized);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct JoystickCapabilities
    {
        public ushort ManufacturerId;
        public ushort ProductId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string ProductName;

        public uint XMinimum;
        public uint XMaximum;
        public uint YMinimum;
        public uint YMaximum;
        public uint ZMinimum;
        public uint ZMaximum;
        public uint ButtonCount;
        public uint MinimumPeriod;
        public uint MaximumPeriod;
        public uint RMinimum;
        public uint RMaximum;
        public uint UMinimum;
        public uint UMaximum;
        public uint VMinimum;
        public uint VMaximum;
        public uint Capabilities;
        public uint MaximumAxisCount;
        public uint AxisCount;
        public uint MaximumButtonCount;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string RegistryKey;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string OemVxD;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JoystickPosition
    {
        public uint Size;
        public uint Flags;
        public uint X;
        public uint Y;
        public uint Z;
        public uint R;
        public uint U;
        public uint V;
        public uint Buttons;
        public uint ButtonNumber;
        public uint PointOfView;
        public uint Reserved1;
        public uint Reserved2;
    }

    private static partial class NativeMethods
    {
        [LibraryImport("winmm.dll", EntryPoint = "joyGetNumDevs")]
        public static partial uint JoyGetNumberOfDevices();

        // LibraryImport cannot source-generate JOYCAPSW because WinMM embeds fixed UTF-16
        // strings inside the structure. Keep this one runtime-marshalled declaration until
        // the generator supports ByValTStr fields; the other imports remain generated.
#pragma warning disable SYSLIB1054
        [DllImport("winmm.dll", EntryPoint = "joyGetDevCapsW", ExactSpelling = true,
            CharSet = CharSet.Unicode)]
        public static extern uint JoyGetDeviceCapabilities(
            uint deviceId,
            ref JoystickCapabilities capabilities,
            uint capabilitiesByteCount);
#pragma warning restore SYSLIB1054

        [LibraryImport("winmm.dll", EntryPoint = "joyGetPosEx")]
        public static partial uint JoyGetPosition(
            uint deviceId,
            ref JoystickPosition position);
    }
}
