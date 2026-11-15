using SuperMetroid.Core;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Diagnostics.Recording;

/// <summary>
/// The numbers a controller recording's option field keeps for the map-reveal assist (#2008): none, public and secret
/// counted from zero in its two-bit field (<see cref="ControllerInputRecordingFormat.MapRevealMask"/>). The game holds
/// only <see cref="MapRevealMode"/>, whose values are arbitrary.
/// </summary>
[EncodingOwner("controller recording map-reveal numbers")]
public static class MapRevealRecordingNumbering
{
    /// <summary>The map-reveal assist a recorded number keeps; any other number is corrupt.</summary>
    /// <exception cref="InvalidDataException"><paramref name="number"/> names no map-reveal mode.</exception>
    public static MapRevealMode RecordedMapReveal(int number) => number switch
    {
        0 => MapRevealMode.None,
        1 => MapRevealMode.Public,
        2 => MapRevealMode.Secret,
        _ => throw new InvalidDataException($"Controller recording contains undefined map reveal mode {number}."),
    };

    /// <summary>The number a recording keeps for <paramref name="mode"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="mode"/> is not a defined mode.</exception>
    public static int RecordedNumber(this MapRevealMode mode) => mode switch
    {
        MapRevealMode.None => 0,
        MapRevealMode.Public => 1,
        MapRevealMode.Secret => 2,
        _ => throw new InvalidDataException($"Cannot record undefined map reveal mode {mode}."),
    };
}
