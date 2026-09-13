using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Application-owned input order from the mixed visual/control records at $81:AF32.</summary>
internal static class MapScrollControls
{
    /// <summary>$81:AF32..AF59 contain four arrow records, processed left/right/up/down.</summary>
    public const int DirectionCount = 4;

    /// <summary>
    /// $81:AF38/AF42/AF4C/AF56: held-button masks for native directions one through four.
    /// Drawing position and animation fields from those records are not input bindings.
    /// </summary>
    public static ReadOnlySpan<ushort> Buttons =>
        [(ushort)SnesButton.Left, (ushort)SnesButton.Right, (ushort)SnesButton.Up, (ushort)SnesButton.Down];
}
