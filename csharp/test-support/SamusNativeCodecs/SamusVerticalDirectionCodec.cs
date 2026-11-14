using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.TestSupport.SamusNativeCodecs;

/// <summary>
/// Converts Samus's vertical direction to and from the native word the cartridge keeps it in, for native traces,
/// snapshot imports and comparisons (#1857). The cartridge writes only these three words.
/// </summary>
internal static class SamusVerticalDirectionCodec
{
    /// <summary>The native word a direction is stored as.</summary>
    internal static ushort ToNative(SamusVerticalDirection direction) => direction switch
    {
        SamusVerticalDirection.None => 0,
        SamusVerticalDirection.Up => 1,
        SamusVerticalDirection.Down => 2,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusVerticalDirection)} {direction}."),
    };

    /// <summary>The direction a native word holds.</summary>
    /// <exception cref="InvalidDataException">The word is none of the three directions the cartridge writes.</exception>
    internal static SamusVerticalDirection FromNative(int word) => word switch
    {
        0 => SamusVerticalDirection.None,
        1 => SamusVerticalDirection.Up,
        2 => SamusVerticalDirection.Down,
        _ => throw new InvalidDataException($"Vertical direction word {word:X4} is none of the three directions the cartridge writes."),
    };
}
