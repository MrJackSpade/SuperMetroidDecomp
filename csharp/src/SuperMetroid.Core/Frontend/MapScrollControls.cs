using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Application-owned input order from the mixed visual/control records at $81:AF32.</summary>
internal static class MapScrollControls
{
    /// <summary>$81:AF32..AF59 contain four arrow records, processed left/right/up/down.</summary>
    public const int DirectionCount = 4;

    /// <summary>The direction of arrow record <paramref name="index"/>, in native left/right/up/down order.</summary>
    public static MapScrollDirection ArrowAt(int index) => index switch
    {
        0 => MapScrollDirection.Left,
        1 => MapScrollDirection.Right,
        2 => MapScrollDirection.Up,
        3 => MapScrollDirection.Down,
        _ => throw new InvalidOperationException($"Map arrow record {index} has no {nameof(MapScrollDirection)}."),
    };

    /// <summary>
    /// $81:AF38/AF42/AF4C/AF56 associate each arrow's direction with its matching
    /// controller direction. Drawing position and animation fields are separate.
    /// </summary>
    public static ushort ButtonFor(MapScrollDirection direction) => (ushort)(direction switch
    {
        MapScrollDirection.Left => SnesButton.Left,
        MapScrollDirection.Right => SnesButton.Right,
        MapScrollDirection.Up => SnesButton.Up,
        MapScrollDirection.Down => SnesButton.Down,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    });
}
