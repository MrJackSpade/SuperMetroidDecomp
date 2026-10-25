namespace SuperMetroid.Core.Input;

/// <summary>
/// Named combinations of the player's controller buttons and the checked conversion from a recorded controller word.
/// Keeping these combinations beside <see cref="ControllerButton"/> prevents gameplay code from
/// reintroducing opaque masks such as <c>$0300</c> for the horizontal directions.
/// </summary>
public static class ControllerButtons
{
    /// <summary>Every bit physically supplied by a standard twelve-button controller.</summary>
    public const ControllerButton All =
        ControllerButton.R | ControllerButton.L | ControllerButton.X | ControllerButton.A |
        ControllerButton.Right | ControllerButton.Left | ControllerButton.Down | ControllerButton.Up |
        ControllerButton.Start | ControllerButton.Select | ControllerButton.Y | ControllerButton.B;

    /// <summary>The four directional-pad bits.</summary>
    public const ControllerButton DirectionalPad =
        ControllerButton.Right | ControllerButton.Left | ControllerButton.Down | ControllerButton.Up;

    /// <summary>The mutually opposed horizontal directional bits.</summary>
    public const ControllerButton HorizontalDirections = ControllerButton.Right | ControllerButton.Left;

    /// <summary>The fixed portion of the retail Crystal Flash chord.</summary>
    public const ControllerButton CrystalFlashWithoutShot =
        ControllerButton.Down | ControllerButton.L | ControllerButton.R;

    /// <summary>
    /// Converts an external 16-bit controller sample once at its boundary and rejects bits
    /// that the twelve-button controller cannot produce. This makes corrupt recordings and
    /// accidental non-input words fail at their source instead of influencing gameplay.
    /// </summary>
    [ControllerWordBoundary("the validated conversion every inbound controller word passes through")]
    public static ControllerButton FromRaw(ushort rawInput, string sourceContext)
    {
        var input = (ControllerButton)rawInput;
        ControllerButton unknown = input & ~All;
        if (unknown != ControllerButton.None)
        {
            throw new InvalidDataException(
                $"{sourceContext} supplied non-controller bits ${unknown:X} " +
                $"in input word ${rawInput:X4}.");
        }

        return input;
    }

    /// <summary>Tests whether at least one requested button is present.</summary>
    public static bool HasAny(this ControllerButton input, ControllerButton buttons) =>
        (input & buttons) != ControllerButton.None;
}
