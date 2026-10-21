namespace SuperMetroid.RenderVerification.Device;

/// <summary>A screen position that may be absent on either axis.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
internal readonly record struct OptionalScreenPosition(ushort? X, ushort? Y);
