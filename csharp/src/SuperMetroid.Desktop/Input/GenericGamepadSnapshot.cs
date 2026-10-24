using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Desktop.Input;

/// <summary>
/// Platform-neutral snapshot of the controls exposed by a conventional USB gamepad.
/// </summary>
/// <remarks>
/// Windows reports joystick axes in device-specific unsigned ranges and POV hats in
/// hundredths of a degree. The desktop adapter normalizes those axes to signed 16-bit
/// values before constructing this record, so the mapping to controller buttons is
/// deterministic and testable without a physical controller or a native call.
/// </remarks>
/// <param name="Stick">Normalized analog-stick deflection on both axes.</param>
/// <param name="PointOfView">POV hat angle in hundredths of a degree clockwise from north, or <see cref="GenericGamepadInput.CenteredPointOfView"/>.</param>
/// <param name="Buttons">Host button bits, button one in bit zero.</param>
internal readonly record struct GenericGamepadSnapshot(
    Offset16<GamepadStick> Stick,
    uint PointOfView,
    uint Buttons);
