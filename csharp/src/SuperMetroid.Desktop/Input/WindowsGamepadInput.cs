using SuperMetroid.Core.Input;
using SuperMetroid.Core.Spatial;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SuperMetroid.Desktop.Input;

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
    /// <summary>Minimum interval between scans while no joystick is connected.</summary>
    private const double RediscoveryIntervalSeconds = 1.0;

    /// <summary>WinMM device index currently selected, or the disconnected sentinel.</summary>
    private uint activeDeviceId = WindowsGamepadCodes.NoDevice;
    /// <summary>Advertised axis ranges, layout, and product identity of the active device.</summary>
    private JoystickCapabilities activeCapabilities;
    /// <summary>Stopwatch tick at which disconnected-device discovery may run again.</summary>
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
    public ControllerButton Poll()
    {
        if (activeDeviceId != WindowsGamepadCodes.NoDevice &&
            TryRead(activeDeviceId, out JoystickPosition position))
            return MapAndRemember(position);

        if (activeDeviceId != WindowsGamepadCodes.NoDevice)
            ForgetActiveDevice();

        long now = Stopwatch.GetTimestamp();
        if (now < nextDiscoveryTimestamp)
            return ControllerButton.None;

        nextDiscoveryTimestamp = now +
            (long)(RediscoveryIntervalSeconds * Stopwatch.Frequency);
        DiscoverFirstConnectedDevice();
        if (activeDeviceId == WindowsGamepadCodes.NoDevice ||
            !TryRead(activeDeviceId, out position))
            return ControllerButton.None;

        return MapAndRemember(position);
    }

    /// <summary>Selects the first WinMM joystick with readable capabilities and position.</summary>
    private void DiscoverFirstConnectedDevice()
    {
        uint possibleDeviceCount = NativeMethods.JoyGetNumberOfDevices();
        for (uint deviceId = 0; deviceId < possibleDeviceCount; deviceId++)
        {
            var capabilities = new JoystickCapabilities();
            WindowsMultimediaResult result = NativeMethods.JoyGetDeviceCapabilities(
                deviceId,
                ref capabilities,
                (uint)Unsafe.SizeOf<JoystickCapabilities>());
            if (result != WindowsMultimediaResult.NoError ||
                !TryRead(deviceId, out JoystickPosition initialPosition))
                continue;

            activeDeviceId = deviceId;
            activeCapabilities = capabilities;
            string productName = capabilities.ProductName.ToString();
            DeviceName = string.IsNullOrWhiteSpace(productName)
                ? $"Gamepad {deviceId + 1}"
                : productName;
            LastSnapshot = CreateSnapshot(initialPosition, capabilities);
            Console.WriteLine(
                $"Gamepad connected: {DeviceName} " +
                $"({capabilities.ButtonCount} buttons, {capabilities.AxisCount} axes, joystick {deviceId}); " +
                $"raw X/Y={initialPosition.X}/{initialPosition.Y} in " +
                $"{capabilities.XMinimum}..{capabilities.XMaximum}/" +
                $"{capabilities.YMinimum}..{capabilities.YMaximum}, " +
                $"normalized X/Y={LastSnapshot.Stick.X}/{LastSnapshot.Stick.Y}, " +
                $"face layout={ResolveFaceButtonLayout(capabilities)}.");
            return;
        }
    }

    /// <summary>Clears selected device metadata and normalized state after a failed poll.</summary>
    private void ForgetActiveDevice()
    {
        if (DeviceName is not null)
            Console.WriteLine($"Gamepad disconnected: {DeviceName}.");
        activeDeviceId = WindowsGamepadCodes.NoDevice;
        DeviceName = null;
        activeCapabilities = default;
        LastSnapshot = default;
    }

    /// <summary>Requests all joystick position fields in one nonblocking WinMM call.</summary>
    private static bool TryRead(uint deviceId, out JoystickPosition position)
    {
        position = new JoystickPosition
        {
            Size = (uint)Marshal.SizeOf<JoystickPosition>(),
            Flags = (uint)JoystickPositionQueryFlags.ReturnAll,
            PointOfView = WindowsGamepadCodes.CenteredPointOfView,
        };
        return NativeMethods.JoyGetPosition(deviceId, ref position) ==
            WindowsMultimediaResult.NoError;
    }

    /// <summary>Stores the normalized snapshot and maps it to SNES buttons for this poll.</summary>
    private ControllerButton MapAndRemember(JoystickPosition position)
    {
        LastSnapshot = CreateSnapshot(position, activeCapabilities);
        return GenericGamepadInput.Map(
            LastSnapshot,
            faceButtonLayout: ResolveFaceButtonLayout(activeCapabilities));
    }

    /// <summary>Selects face-button ordering from the driver's manufacturer and product IDs.</summary>
    private static GenericGamepadFaceButtonLayout ResolveFaceButtonLayout(
        JoystickCapabilities capabilities) =>
        WindowsGamepadCodes.ResolveFaceButtonLayout(
            capabilities.ManufacturerId,
            capabilities.ProductId);

    /// <summary>Normalizes present axes and ignores a zero-filled POV field when no hat is advertised.</summary>
    internal static GenericGamepadSnapshot CreateSnapshot(
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
                : WindowsGamepadCodes.CenteredPointOfView;
        return new GenericGamepadSnapshot(
            // Drivers can expose buttons without axes while leaving X/Y zero-filled.
            // Zero is a full negative deflection only for an axis that actually exists.
            Stick: new(
                capabilities.AxisCount >= 1
                    ? NormalizeAxis(position.X, capabilities.XMinimum, capabilities.XMaximum) : (short)0,
                capabilities.AxisCount >= 2
                    ? NormalizeAxis(position.Y, capabilities.YMinimum, capabilities.YMaximum) : (short)0),
            PointOfView: pointOfView,
            Buttons: position.Buttons);
    }

    /// <summary>Maps a driver's bounded unsigned axis reading into the signed 16-bit stick range.</summary>
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

    /// <summary>Blittable WinMM JOYCAPSW fields used for joystick identity and axis calibration.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [SpatialComponents("Win32 interop: winmm's JOYCAPSW declares each axis range as separate fields.")]
    internal struct JoystickCapabilities
    {
        /// <summary>Driver-reported manufacturer identifier used to choose button layout.</summary>
        public ushort ManufacturerId;
        /// <summary>Driver-reported product identifier used to choose button layout.</summary>
        public ushort ProductId;

        /// <summary>Fixed-width driver product label shown on connection.</summary>
        public FixedUtf16Name ProductName;

        /// <summary>Minimum X-axis reading advertised by the driver.</summary>
        public uint XMinimum;
        /// <summary>Maximum X-axis reading advertised by the driver.</summary>
        public uint XMaximum;
        /// <summary>Minimum Y-axis reading advertised by the driver.</summary>
        public uint YMinimum;
        /// <summary>Maximum Y-axis reading advertised by the driver.</summary>
        public uint YMaximum;
        /// <summary>Minimum Z-axis reading in the native capabilities layout.</summary>
        public uint ZMinimum;
        /// <summary>Maximum Z-axis reading in the native capabilities layout.</summary>
        public uint ZMaximum;
        /// <summary>Number of buttons exposed by the driver.</summary>
        public uint ButtonCount;
        /// <summary>Driver's minimum polling period in milliseconds.</summary>
        public uint MinimumPeriod;
        /// <summary>Driver's maximum polling period in milliseconds.</summary>
        public uint MaximumPeriod;
        /// <summary>Minimum R-axis reading in the native capabilities layout.</summary>
        public uint RMinimum;
        /// <summary>Maximum R-axis reading in the native capabilities layout.</summary>
        public uint RMaximum;
        /// <summary>Minimum U-axis reading in the native capabilities layout.</summary>
        public uint UMinimum;
        /// <summary>Maximum U-axis reading in the native capabilities layout.</summary>
        public uint UMaximum;
        /// <summary>Minimum V-axis reading in the native capabilities layout.</summary>
        public uint VMinimum;
        /// <summary>Maximum V-axis reading in the native capabilities layout.</summary>
        public uint VMaximum;
        /// <summary>WinMM capability flags, including whether a POV hat exists.</summary>
        public uint Capabilities;
        /// <summary>Maximum axis count the driver can expose.</summary>
        public uint MaximumAxisCount;
        /// <summary>Currently exposed axis count; gates X and Y normalization.</summary>
        public uint AxisCount;
        /// <summary>Maximum button count the driver can expose.</summary>
        public uint MaximumButtonCount;

        /// <summary>Fixed-width registry key reported by JOYCAPSW.</summary>
        public FixedUtf16Name RegistryKey;

        /// <summary>Fixed-width OEM VxD path retained for the native structure layout.</summary>
        public FixedUtf16Path OemVxD;
    }

    /// <summary>JOYCAPSW's inline <c>WCHAR[MAXPNAMELEN]</c> field, kept blittable for LibraryImport.</summary>
    [InlineArray(32)]
    internal struct FixedUtf16Name
    {
        /// <summary>First UTF-16 code unit of the inline WinMM product or registry name.</summary>
        private ushort first;

        /// <summary>The text before the field's first NUL terminator.</summary>
        public override readonly string ToString() => FixedUtf16.Read(this);
    }

    /// <summary>JOYCAPSW's inline <c>WCHAR[MAX_PATH]</c> field, kept blittable for LibraryImport.</summary>
    [InlineArray(260)]
    internal struct FixedUtf16Path
    {
        /// <summary>First UTF-16 code unit of the inline WinMM OEM path.</summary>
        private ushort first;

        /// <summary>The text before the field's first NUL terminator.</summary>
        public override readonly string ToString() => FixedUtf16.Read(this);
    }

    /// <summary>Converts WinMM's fixed-width UTF-16 buffers without runtime marshaling.</summary>
    private static class FixedUtf16
    {
        /// <summary>Reads UTF-16 code units up to the first NUL in a WinMM inline field.</summary>
        public static string Read(ReadOnlySpan<ushort> field)
        {
            ReadOnlySpan<char> text = MemoryMarshal.Cast<ushort, char>(field);
            int length = text.IndexOf('\0');
            return new string(length < 0 ? text : text[..length]);
        }
    }

    /// <summary>Blittable WinMM JOYINFOEX result for axes, buttons, and POV hat.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [SpatialComponents("Win32 interop: winmm's JOYINFOEX declares each axis reading as separate fields.")]
    internal struct JoystickPosition
    {
        /// <summary>Byte size passed to WinMM to select this JOYINFOEX layout.</summary>
        public uint Size;
        /// <summary>Requested result fields, set to ReturnAll by the poller.</summary>
        public uint Flags;
        /// <summary>Current X-axis reading.</summary>
        public uint X;
        /// <summary>Current Y-axis reading.</summary>
        public uint Y;
        /// <summary>Current Z-axis reading.</summary>
        public uint Z;
        /// <summary>Current R-axis reading.</summary>
        public uint R;
        /// <summary>Current U-axis reading.</summary>
        public uint U;
        /// <summary>Current V-axis reading.</summary>
        public uint V;
        /// <summary>Bit field of currently held joystick buttons.</summary>
        public uint Buttons;
        /// <summary>One-based button number reported by JOYINFOEX.</summary>
        public uint ButtonNumber;
        /// <summary>POV hat angle or centered sentinel when a hat is present.</summary>
        public uint PointOfView;
        /// <summary>Native reserved word retained for JOYINFOEX size and alignment.</summary>
        public uint Reserved1;
        /// <summary>Second native reserved word retained for JOYINFOEX size and alignment.</summary>
        public uint Reserved2;
    }

    /// <summary>WinMM entry points used to enumerate and poll HID-backed joysticks.</summary>
    private static partial class NativeMethods
    {
        /// <summary>Gets the number of joystick device slots advertised by WinMM.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "joyGetNumDevs")]
        public static partial uint JoyGetNumberOfDevices();

        /// <summary>Reads the selected device's JOYCAPSW identity and range metadata.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "joyGetDevCapsW")]
        public static partial WindowsMultimediaResult JoyGetDeviceCapabilities(
            uint deviceId,
            ref JoystickCapabilities capabilities,
            uint capabilitiesByteCount);

        /// <summary>Reads current axes, buttons, and POV into a JOYINFOEX result.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "joyGetPosEx")]
        public static partial WindowsMultimediaResult JoyGetPosition(
            uint deviceId,
            ref JoystickPosition position);
    }
}
