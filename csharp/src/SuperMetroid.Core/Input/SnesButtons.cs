namespace SuperMetroid.Core.Input;

/// <summary>
/// Named combinations and checked conversions for the standard SNES controller word.
/// Keeping these combinations beside <see cref="SnesButton"/> prevents gameplay code from
/// reintroducing opaque masks such as <c>$0300</c> for the horizontal directions.
/// </summary>
public static class SnesButtons
{
    /// <summary>Every bit physically supplied by a standard twelve-button controller.</summary>
    public const SnesButton All =
        SnesButton.R | SnesButton.L | SnesButton.X | SnesButton.A |
        SnesButton.Right | SnesButton.Left | SnesButton.Down | SnesButton.Up |
        SnesButton.Start | SnesButton.Select | SnesButton.Y | SnesButton.B;

    /// <summary>The four directional-pad bits.</summary>
    public const SnesButton DirectionalPad =
        SnesButton.Right | SnesButton.Left | SnesButton.Down | SnesButton.Up;

    /// <summary>The mutually opposed horizontal directional bits.</summary>
    public const SnesButton HorizontalDirections = SnesButton.Right | SnesButton.Left;

    /// <summary>The fixed portion of the retail Crystal Flash chord.</summary>
    public const SnesButton CrystalFlashWithoutShot =
        SnesButton.Down | SnesButton.L | SnesButton.R;

    /// <summary>
    /// Converts an external 16-bit controller sample once at its boundary and rejects bits
    /// that a standard SNES controller cannot produce. This makes corrupt recordings and
    /// accidental non-input words fail at their source instead of influencing gameplay.
    /// </summary>
    public static SnesButton FromRaw(ushort rawInput, string sourceContext)
    {
        const ushort knownMask = (ushort)All;
        if ((rawInput & ~knownMask) != 0)
        {
            throw new InvalidDataException(
                $"{sourceContext} supplied non-controller bits ${rawInput & ~knownMask:X4} " +
                $"in input word ${rawInput:X4}.");
        }

        return (SnesButton)rawInput;
    }

    /// <summary>Tests whether at least one requested button is present.</summary>
    public static bool HasAny(this SnesButton input, SnesButton buttons) =>
        (input & buttons) != SnesButton.None;

    /// <summary>Tests whether every requested button is present.</summary>
    public static bool HasAll(this SnesButton input, SnesButton buttons) =>
        (input & buttons) == buttons;
}
