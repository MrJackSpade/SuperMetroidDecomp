using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Desktop.Input;

/// <summary>
/// Thumbstick deflection exactly as XInput reports it: signed sixteen-bit axes, X positive to
/// the right and Y positive upwards, centred on zero.
/// </summary>
internal sealed class XInputStick : ISpatialSpace;
