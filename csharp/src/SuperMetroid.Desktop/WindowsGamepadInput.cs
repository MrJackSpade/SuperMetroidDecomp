using SuperMetroid.Core.Input;
using SuperMetroid.Core.Spatial;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    /// <summary>Stores rediscovery interval seconds.</summary>
    private const double RediscoveryIntervalSeconds = 1.0;

    /// <summary>Stores active device id.</summary>
    private uint activeDeviceId = WindowsGamepadIdentifiers.NoDevice;
    /// <summary>Stores active capabilities.</summary>
    private JoystickCapabilities activeCapabilities;
    /// <summary>Stores next discovery timestamp.</summary>
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

    /// <summary>Discover first connected device.</summary>
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

    /// <summary>Forget active device.</summary>
    private void ForgetActiveDevice()
    {
        if (DeviceName is not null)
            Console.WriteLine($"Gamepad disconnected: {DeviceName}.");
        activeDeviceId = WindowsGamepadIdentifiers.NoDevice;
        DeviceName = null;
        activeCapabilities = default;
        LastSnapshot = default;
    }

    /// <summary>Attempts to read.</summary>
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

    /// <summary>Map and remember.</summary>
    private SnesButton MapAndRemember(JoystickPosition position)
    {
        LastSnapshot = CreateSnapshot(position, activeCapabilities);
        return GenericGamepadInput.Map(
            LastSnapshot,
            faceButtonLayout: ResolveFaceButtonLayout(activeCapabilities));
    }

    /// <summary>Resolves face button layout.</summary>
    private static GenericGamepadFaceButtonLayout ResolveFaceButtonLayout(
        JoystickCapabilities capabilities) =>
        WindowsGamepadIdentifiers.ResolveFaceButtonLayout(
            capabilities.ManufacturerId,
            capabilities.ProductId);

    /// <summary>Creates snapshot.</summary>
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
            Stick: new(
                capabilities.AxisCount >= 1
                    ? NormalizeAxis(position.X, capabilities.XMinimum, capabilities.XMaximum) : (short)0,
                capabilities.AxisCount >= 2
                    ? NormalizeAxis(position.Y, capabilities.YMinimum, capabilities.YMaximum) : (short)0),
            PointOfView: pointOfView,
            Buttons: position.Buttons);
    }

    /// <summary>Normalize axis.</summary>
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

    /// <summary>Represents joystick capabilities.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [SpatialComponents("Win32 interop: winmm's JOYCAPSW declares each axis range as separate fields.")]
    private struct JoystickCapabilities
    {
        /// <summary>Stores manufacturer id.</summary>
        public ushort ManufacturerId;
        /// <summary>Stores product id.</summary>
        public ushort ProductId;

        /// <summary>Stores product name.</summary>
        public FixedUtf16Name ProductName;

        /// <summary>Stores xminimum.</summary>
        public uint XMinimum;
        /// <summary>Stores xmaximum.</summary>
        public uint XMaximum;
        /// <summary>Stores yminimum.</summary>
        public uint YMinimum;
        /// <summary>Stores ymaximum.</summary>
        public uint YMaximum;
        /// <summary>Stores zminimum.</summary>
        public uint ZMinimum;
        /// <summary>Stores zmaximum.</summary>
        public uint ZMaximum;
        /// <summary>Stores button count.</summary>
        public uint ButtonCount;
        /// <summary>Stores minimum period.</summary>
        public uint MinimumPeriod;
        /// <summary>Stores maximum period.</summary>
        public uint MaximumPeriod;
        /// <summary>Stores rminimum.</summary>
        public uint RMinimum;
        /// <summary>Stores rmaximum.</summary>
        public uint RMaximum;
        /// <summary>Stores uminimum.</summary>
        public uint UMinimum;
        /// <summary>Stores umaximum.</summary>
        public uint UMaximum;
        /// <summary>Stores vminimum.</summary>
        public uint VMinimum;
        /// <summary>Stores vmaximum.</summary>
        public uint VMaximum;
        /// <summary>Stores capabilities.</summary>
        public uint Capabilities;
        /// <summary>Stores maximum axis count.</summary>
        public uint MaximumAxisCount;
        /// <summary>Stores axis count.</summary>
        public uint AxisCount;
        /// <summary>Stores maximum button count.</summary>
        public uint MaximumButtonCount;

        /// <summary>Stores registry key.</summary>
        public FixedUtf16Name RegistryKey;

        /// <summary>Stores oem vx d.</summary>
        public FixedUtf16Path OemVxD;
    }

    /// <summary>JOYCAPSW's inline <c>WCHAR[MAXPNAMELEN]</c> field, kept blittable for LibraryImport.</summary>
    [InlineArray(32)]
    private struct FixedUtf16Name
    {
        /// <summary>Stores first.</summary>
        private ushort first;

        /// <summary>The text before the field's first NUL terminator.</summary>
        public override readonly string ToString() => FixedUtf16.Read(this);
    }

    /// <summary>JOYCAPSW's inline <c>WCHAR[MAX_PATH]</c> field, kept blittable for LibraryImport.</summary>
    [InlineArray(260)]
    private struct FixedUtf16Path
    {
        /// <summary>Stores first.</summary>
        private ushort first;

        /// <summary>The text before the field's first NUL terminator.</summary>
        public override readonly string ToString() => FixedUtf16.Read(this);
    }

    /// <summary>Represents fixed utf16.</summary>
    private static class FixedUtf16
    {
        // WCHAR units are stored as ushort: char is not blittable under runtime marshalling.
        /// <summary>Reads.</summary>
        public static string Read(ReadOnlySpan<ushort> field)
        {
            ReadOnlySpan<char> text = MemoryMarshal.Cast<ushort, char>(field);
            int length = text.IndexOf('\0');
            return new string(length < 0 ? text : text[..length]);
        }
    }

    /// <summary>Represents joystick position.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [SpatialComponents("Win32 interop: winmm's JOYINFOEX declares each axis reading as separate fields.")]
    private struct JoystickPosition
    {
        /// <summary>Stores size.</summary>
        public uint Size;
        /// <summary>Stores flags.</summary>
        public uint Flags;
        /// <summary>Stores x.</summary>
        public uint X;
        /// <summary>Stores y.</summary>
        public uint Y;
        /// <summary>Stores z.</summary>
        public uint Z;
        /// <summary>Stores r.</summary>
        public uint R;
        /// <summary>Stores u.</summary>
        public uint U;
        /// <summary>Stores v.</summary>
        public uint V;
        /// <summary>Stores buttons.</summary>
        public uint Buttons;
        /// <summary>Stores button number.</summary>
        public uint ButtonNumber;
        /// <summary>Stores point of view.</summary>
        public uint PointOfView;
        /// <summary>Stores reserved1.</summary>
        public uint Reserved1;
        /// <summary>Stores reserved2.</summary>
        public uint Reserved2;
    }

    /// <summary>Represents native methods.</summary>
    private static partial class NativeMethods
    {
        /// <summary>Joy get number of devices.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "joyGetNumDevs")]
        public static partial uint JoyGetNumberOfDevices();

        /// <summary>Joy get device capabilities.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "joyGetDevCapsW")]
        public static partial WindowsMultimediaResult JoyGetDeviceCapabilities(
            uint deviceId,
            ref JoystickCapabilities capabilities,
            uint capabilitiesByteCount);

        /// <summary>Joy get position.</summary>
        [LibraryImport("winmm.dll", EntryPoint = "joyGetPosEx")]
        public static partial WindowsMultimediaResult JoyGetPosition(
            uint deviceId,
            ref JoystickPosition position);
    }
}
