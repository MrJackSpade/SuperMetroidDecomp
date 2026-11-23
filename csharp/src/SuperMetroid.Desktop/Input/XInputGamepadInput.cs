using SuperMetroid.Core.Input;
using SuperMetroid.Core.Spatial;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SuperMetroid.Desktop.Input;

/// <summary>
/// Hot-pluggable XInput adapter for Xbox controllers and pads in X-input mode.
/// </summary>
/// <remarks>
/// Those controllers are not reliably usable through WinMM: depending on the driver and
/// connection they appear there with no buttons or axes, or not at all. XInput reads them
/// directly. <c>xinput1_4.dll</c> ships with every supported Windows version.
/// </remarks>
internal sealed partial class XInputGamepadInput
{
    /// <summary>Number of controller slots XInput serves (<c>XUSER_MAX_COUNT</c>).</summary>
    private const uint SlotCount = 4;

    /// <summary>Minimum interval between reads of slots that had no controller.</summary>
    /// <remarks>Reading an empty slot is slow inside XInput, so empty slots stay out of the per-update path.</remarks>
    private const double RediscoveryIntervalSeconds = 1.0;

    /// <summary>Which slots held a controller at their last read.</summary>
    private readonly bool[] connected = new bool[SlotCount];
    /// <summary>Stopwatch tick at which empty slots may be read again.</summary>
    private long nextDiscoveryTimestamp;

    /// <summary>Polls every connected controller and returns the union of their buttons.</summary>
    public ControllerButton Poll()
    {
        long now = Stopwatch.GetTimestamp();
        bool discover = now >= nextDiscoveryTimestamp;
        if (discover)
            nextDiscoveryTimestamp = now + (long)(RediscoveryIntervalSeconds * Stopwatch.Frequency);

        ControllerButton result = ControllerButton.None;
        for (uint slot = 0; slot < SlotCount; slot++)
        {
            if (!connected[slot] && !discover)
                continue;
            if (!TryRead(slot, out XInputGamepadState state))
            {
                if (connected[slot])
                    Console.WriteLine($"XInput controller {slot + 1} disconnected.");
                connected[slot] = false;
                continue;
            }
            if (!connected[slot])
            {
                Console.WriteLine($"XInput controller {slot + 1} connected.");
                connected[slot] = true;
            }
            result |= GenericGamepadInput.Map(state.ToSnapshot());
        }
        return result;
    }

    /// <summary>Reads one slot; false when no controller is connected there.</summary>
    private static bool TryRead(uint slot, out XInputGamepadState state)
    {
        uint status = NativeMethods.XInputGetState(slot, out XInputNativeState native);
        if (status == DeviceNotConnected)
        {
            state = default;
            return false;
        }
        if (status != Success)
            throw new Win32Exception((int)status, $"XInputGetState failed for controller slot {slot}.");

        XInputNativeGamepad pad = native.Gamepad;
        state = new XInputGamepadState(
            (XInputButtons)pad.Buttons,
            pad.LeftTrigger,
            pad.RightTrigger,
            new Offset16<XInputStick>(pad.ThumbLeftX, pad.ThumbLeftY));
        return true;
    }

    /// <summary>Win32 <c>ERROR_SUCCESS</c>.</summary>
    private const uint Success = 0;
    /// <summary>Win32 <c>ERROR_DEVICE_NOT_CONNECTED</c>, returned for an empty slot.</summary>
    private const uint DeviceNotConnected = 1167;

    /// <summary>Blittable <c>XINPUT_STATE</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct XInputNativeState
    {
        /// <summary>Changes whenever the controller's state changes.</summary>
        public uint PacketNumber;
        /// <summary>The controls read.</summary>
        public XInputNativeGamepad Gamepad;
    }

    /// <summary>Blittable <c>XINPUT_GAMEPAD</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct XInputNativeGamepad
    {
        /// <summary>Digital controls held.</summary>
        public ushort Buttons;
        /// <summary>Left trigger travel.</summary>
        public byte LeftTrigger;
        /// <summary>Right trigger travel.</summary>
        public byte RightTrigger;
        /// <summary>Left thumbstick X, positive right.</summary>
        public short ThumbLeftX;
        /// <summary>Left thumbstick Y, positive up.</summary>
        public short ThumbLeftY;
        /// <summary>Right thumbstick X, kept for the native layout.</summary>
        public short ThumbRightX;
        /// <summary>Right thumbstick Y, kept for the native layout.</summary>
        public short ThumbRightY;
    }

    /// <summary>XInput entry point used to read controllers.</summary>
    private static partial class NativeMethods
    {
        /// <summary>Reads one controller slot's current state.</summary>
        [LibraryImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
        public static partial uint XInputGetState(uint userIndex, out XInputNativeState state);
    }
}
