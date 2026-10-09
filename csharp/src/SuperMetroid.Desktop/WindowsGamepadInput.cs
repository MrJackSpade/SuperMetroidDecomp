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
    /// <summary>Minimum seconds between full WinMM device-enumeration attempts.</summary>
    private const double RediscoveryIntervalSeconds = 1.0;

    /// <summary>WinMM device index currently supplying input, or the no-device sentinel.</summary>
    private uint activeDeviceId = WindowsGamepadIdentifiers.NoDevice;
    /// <summary>Capabilities cached for the active device and used to normalize its reports.</summary>
    private JoystickCapabilities activeCapabilities;
    /// <summary>Stopwatch timestamp at which another throttled discovery pass is permitted.</summary>
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
        if (activeDeviceId != WindowsGamepadIdentifiers.NoDevice &&
            TryRead(activeDeviceId, out JoystickPosition position))
            return MapAndRemember(position);

        if (activeDeviceId != WindowsGamepadIdentifiers.NoDevice)
            ForgetActiveDevice();

        long now = Stopwatch.GetTimestamp();
        if (now < nextDiscoveryTimestamp)
            return SnesButton.None;

        nextDiscoveryTimestamp = now +
            (long)(RediscoveryIntervalSeconds * Stopwatch.Frequency);
        DiscoverFirstConnectedDevice();
        if (activeDeviceId == WindowsGamepadIdentifiers.NoDevice ||
            !TryRead(activeDeviceId, out position))
            return SnesButton.None;

        return MapAndRemember(position);
    }

    /// <summary>Finds the first readable WinMM joystick and caches its capabilities and initial state.</summary>
    private void DiscoverFirstConnectedDevice()
    {
        uint possibleDeviceCount = NativeMethods.JoyGetNumberOfDevices();
        for (uint deviceId = 0; deviceId < possibleDeviceCount; deviceId++)
        {
            var capabilities = new JoystickCapabilities();
            WindowsMultimediaResult result = NativeMethods.JoyGetDeviceCapabilities(
                deviceId,
                ref capabilities,
                (uint)Marshal.SizeOf<JoystickCapabilities>());
            if (result != WindowsMultimediaResult.NoError ||
                !TryRead(deviceId, out JoystickPosition initialPosition))
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

    /// <summary>Clears cached device identity and input after its WinMM read fails.</summary>
    private void ForgetActiveDevice()
    {
        if (DeviceName is not null)
            Console.WriteLine($"Gamepad disconnected: {DeviceName}.");
        activeDeviceId = WindowsGamepadIdentifiers.NoDevice;
        DeviceName = null;
        activeCapabilities = default;
        LastSnapshot = default;
    }

    /// <summary>Requests all available joystick axes, buttons, and POV data from WinMM.</summary>
    /// <param name="deviceId">WinMM index of the joystick to query.</param>
    /// <param name="position">Receives the native report when the query succeeds.</param>
    /// <returns><see langword="true"/> when WinMM returned a valid position report.</returns>
    private static bool TryRead(uint deviceId, out JoystickPosition position)
    {
        position = new JoystickPosition
        {
            Size = (uint)Marshal.SizeOf<JoystickPosition>(),
            Flags = (uint)JoystickPositionQueryFlags.ReturnAll,
            PointOfView = WindowsGamepadIdentifiers.CenteredPointOfView,
        };
        return NativeMethods.JoyGetPosition(deviceId, ref position) ==
            WindowsMultimediaResult.NoError;
    }

    /// <summary>Stores a normalized diagnostic snapshot and maps it to the game's SNES buttons.</summary>
    /// <param name="position">Raw report returned by the active WinMM joystick.</param>
    /// <returns>The SNES buttons held by the reported controller state.</returns>
    private SnesButton MapAndRemember(JoystickPosition position)
    {
        LastSnapshot = CreateSnapshot(position, activeCapabilities);
        return GenericGamepadInput.Map(
            LastSnapshot,
            faceButtonLayout: ResolveFaceButtonLayout(activeCapabilities));
    }

    /// <summary>Selects face-button ordering from the controller's reported vendor and product IDs.</summary>
    /// <param name="capabilities">Device identity fields supplied by WinMM.</param>
    /// <returns>The button layout used by the generic gamepad mapper.</returns>
    private static GenericGamepadFaceButtonLayout ResolveFaceButtonLayout(
        JoystickCapabilities capabilities) =>
        WindowsGamepadIdentifiers.ResolveFaceButtonLayout(
            capabilities.ManufacturerId,
            capabilities.ProductId);

    /// <summary>Converts a native report into normalized axes, a valid POV value, and button bits.</summary>
    /// <param name="position">Raw axis, POV, and button values returned by WinMM.</param>
    /// <param name="capabilities">Advertised axis ranges and optional POV support.</param>
    /// <returns>A platform-independent snapshot consumed by the core input mapper.</returns>
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
            (((JoystickCapabilityFlags)capabilities.Capabilities) &
                JoystickCapabilityFlags.HasPointOfView) != 0
                ? position.PointOfView
                : WindowsGamepadIdentifiers.CenteredPointOfView;
        return new GenericGamepadSnapshot(
            // Drivers can expose buttons without axes while leaving X/Y zero-filled.
            // Zero is a full negative deflection only for an axis that actually exists.
            HorizontalAxis: capabilities.AxisCount >= 1
                ? NormalizeAxis(position.X, capabilities.XMinimum, capabilities.XMaximum) : (short)0,
            VerticalAxis: capabilities.AxisCount >= 2
                ? NormalizeAxis(position.Y, capabilities.YMinimum, capabilities.YMaximum) : (short)0,
            PointOfView: pointOfView,
            Buttons: position.Buttons);
    }

    /// <summary>Scales an advertised unsigned joystick range to the signed core-axis range.</summary>
    /// <param name="value">Current raw axis value.</param>
    /// <param name="minimum">Minimum raw value advertised by the driver.</param>
    /// <param name="maximum">Maximum raw value advertised by the driver.</param>
    /// <returns>The value mapped from the advertised range to <see cref="short"/> limits.</returns>
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
    /// <summary>Managed layout matching WinMM's JOYCAPSW capabilities structure.</summary>
    private struct JoystickCapabilities
    {
        /// <summary>USB or driver manufacturer identifier used for controller layout selection.</summary>
        public ushort ManufacturerId;
        /// <summary>Product identifier paired with <see cref="ManufacturerId"/>.</summary>
        public ushort ProductId;

        /// <summary>Fixed-width Unicode product name reported by the joystick driver.</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string ProductName;

        /// <summary>Minimum raw value reported for the X axis.</summary>
        public uint XMinimum;
        /// <summary>Maximum raw value reported for the X axis.</summary>
        public uint XMaximum;
        /// <summary>Minimum raw value reported for the Y axis.</summary>
        public uint YMinimum;
        /// <summary>Maximum raw value reported for the Y axis.</summary>
        public uint YMaximum;
        /// <summary>Minimum raw value reported for the Z axis.</summary>
        public uint ZMinimum;
        /// <summary>Maximum raw value reported for the Z axis.</summary>
        public uint ZMaximum;
        /// <summary>Number of buttons exposed by the device.</summary>
        public uint ButtonCount;
        /// <summary>Shortest polling interval supported by the driver, in milliseconds.</summary>
        public uint MinimumPeriod;
        /// <summary>Longest polling interval supported by the driver, in milliseconds.</summary>
        public uint MaximumPeriod;
        /// <summary>Minimum raw value reported for the R axis.</summary>
        public uint RMinimum;
        /// <summary>Maximum raw value reported for the R axis.</summary>
        public uint RMaximum;
        /// <summary>Minimum raw value reported for the U axis.</summary>
        public uint UMinimum;
        /// <summary>Maximum raw value reported for the U axis.</summary>
        public uint UMaximum;
        /// <summary>Minimum raw value reported for the V axis.</summary>
        public uint VMinimum;
        /// <summary>Maximum raw value reported for the V axis.</summary>
        public uint VMaximum;
        /// <summary>Bitmask describing device features such as POV-hat availability.</summary>
        public uint Capabilities;
        /// <summary>Maximum axis count supported by the device.</summary>
        public uint MaximumAxisCount;
        /// <summary>Number of axes actually exposed by the device.</summary>
        public uint AxisCount;
        /// <summary>Maximum button count supported by the device.</summary>
        public uint MaximumButtonCount;

        /// <summary>Fixed-width Unicode registry key associated with the joystick.</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string RegistryKey;

        /// <summary>Fixed-width Unicode name of the installed joystick driver.</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string OemVxD;
    }

    /// <summary>Managed layout matching the extended JOYINFOEX position report from WinMM.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct JoystickPosition
    {
        /// <summary>Structure byte size supplied to the native query.</summary>
        public uint Size;
        /// <summary>Flags requesting which position fields WinMM should populate.</summary>
        public uint Flags;
        /// <summary>Current raw X-axis value.</summary>
        public uint X;
        /// <summary>Current raw Y-axis value.</summary>
        public uint Y;
        /// <summary>Current raw Z-axis value.</summary>
        public uint Z;
        /// <summary>Current raw R-axis value.</summary>
        public uint R;
        /// <summary>Current raw U-axis value.</summary>
        public uint U;
        /// <summary>Current raw V-axis value.</summary>
        public uint V;
        /// <summary>Bitmask of currently pressed buttons.</summary>
        public uint Buttons;
        /// <summary>One-based number of the most recently pressed button, when requested.</summary>
        public uint ButtonNumber;
        /// <summary>Current POV-hat angle in hundredths of a degree, or the centered sentinel.</summary>
        public uint PointOfView;
        /// <summary>Reserved native field retained to preserve the WinMM structure layout.</summary>
        public uint Reserved1;
        /// <summary>Reserved native field retained to preserve the WinMM structure layout.</summary>
        public uint Reserved2;
    }

    /// <summary>WinMM entry points used for joystick enumeration, capabilities, and state queries.</summary>
    private static partial class NativeMethods
    {
        /// <summary>Returns the number of joystick device indices exposed by WinMM.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "joyGetNumDevs")]
        public static partial uint JoyGetNumberOfDevices();

        // LibraryImport cannot source-generate JOYCAPSW because WinMM embeds fixed UTF-16
        // strings inside the structure. Keep this one runtime-marshalled declaration until
        // the generator supports ByValTStr fields; the other imports remain generated.
#pragma warning disable SYSLIB1054
        /// <summary>Reads the Unicode capabilities structure for a WinMM joystick index.</summary>
        /// <param name="deviceId">Index of the joystick to query.</param>
        /// <param name="capabilities">Receives the driver's advertised ranges and device features.</param>
        /// <param name="capabilitiesByteCount">Native structure size in bytes.</param>
        /// <returns>The WinMM status code for the capabilities query.</returns>
        [DllImport("winmm.dll", EntryPoint = "joyGetDevCapsW", ExactSpelling = true,
            CharSet = CharSet.Unicode)]
        public static extern WindowsMultimediaResult JoyGetDeviceCapabilities(
            uint deviceId,
            ref JoystickCapabilities capabilities,
            uint capabilitiesByteCount);
#pragma warning restore SYSLIB1054

        /// <summary>Reads the current extended position report from a WinMM joystick.</summary>
        /// <param name="deviceId">Index of the joystick to query.</param>
        /// <param name="position">In/out native report structure containing request flags and returned state.</param>
        /// <returns>The WinMM status code for the position query.</returns>
        [LibraryImport("winmm.dll", EntryPoint = "joyGetPosEx")]
        public static partial WindowsMultimediaResult JoyGetPosition(
            uint deviceId,
            ref JoystickPosition position);
    }
}
