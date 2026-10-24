using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Desktop.Input;

/// <summary>
/// Normalized analog-stick deflection of a host gamepad: signed sixteen-bit axes, X positive to the
/// right and Y positive downwards, centred on zero.
/// </summary>
internal sealed class GamepadStick : ISpatialSpace;
