using SuperMetroid.Core.Input;

namespace SuperMetroid.DesktopVerification.Input;

/// <summary>A gamepad snapshot, its device and the SNES buttons it maps to.</summary>
/// <param name="Device">The device.</param>
/// <param name="Snapshot">The snapshot.</param>
/// <param name="Mapped">The mapped.</param>
internal readonly record struct GamepadReading(string? Device, GenericGamepadSnapshot Snapshot, SnesButton Mapped);
