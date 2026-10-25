namespace SuperMetroid.Core.Input;

/// <summary>
/// The codec between <see cref="ControllerButton"/> and the sixteen-bit joypad word that recordings and replays supply:
/// the one place in Core an inbound controller word becomes a set of buttons.
/// </summary>
[EncodingOwner("recorded controller word")]
public static class ControllerWordCodec
{
    /// <summary>
    /// Converts an external 16-bit controller sample once at its boundary and rejects bits
    /// that the twelve-button controller cannot produce. This makes corrupt recordings and
    /// accidental non-input words fail at their source instead of influencing gameplay.
    /// </summary>
    [ControllerWordBoundary("the validated conversion every inbound controller word passes through")]
    public static ControllerButton FromRaw(ushort rawInput, string sourceContext)
    {
        var input = (ControllerButton)rawInput;
        ControllerButton unknown = input & ~ControllerButtons.All;
        if (unknown != ControllerButton.None)
        {
            throw new InvalidDataException(
                $"{sourceContext} supplied non-controller bits ${unknown:X} " +
                $"in input word ${rawInput:X4}.");
        }

        return input;
    }
}
