using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Application-owned input order from the mixed visual/control records at $81:AF32.</summary>
internal static class MapScrollControls
{
    /// <summary>$81:AF32..AF59 contain four arrow records, processed left/right/up/down.</summary>
    public const int DirectionCount = 4;

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

    public static ButtonSequence Buttons => default;

    internal readonly struct ButtonSequence
    {

        public ushort this[int index] => (uint)index < DirectionCount
            ? ButtonFor((MapScrollDirection)(index + 1))
            : throw new ArgumentOutOfRangeException(nameof(index));
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < DirectionCount; index++) yield return this[index];
        }
    }
}
