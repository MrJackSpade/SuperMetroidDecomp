using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// The three installed full-body normal suit palettes, every suit's colours kept as installed; suit selection
/// remains gameplay code.
/// </summary>
public sealed class InstalledSamusSuitColors
{
    /// <summary>The installed Power Suit colours, including its transparent-slot payload.</summary>
    private readonly RgbColor[] power;
    /// <summary>The installed Varia Suit colours.</summary>
    private readonly RgbColor[] varia;
    /// <summary>The installed Gravity Suit colours.</summary>
    private readonly RgbColor[] gravity;

    /// <summary>Keeps copies of the three complete installed suit palettes.</summary>
    /// <param name="power">The Power Suit's <see cref="SamusSuitColorFormat.ColorsPerSuit"/> colours.</param>
    /// <param name="varia">The Varia Suit's <see cref="SamusSuitColorFormat.ColorsPerSuit"/> colours.</param>
    /// <param name="gravity">The Gravity Suit's <see cref="SamusSuitColorFormat.ColorsPerSuit"/> colours.</param>
    /// <exception cref="InvalidDataException">A suit is missing a colour.</exception>
    internal InstalledSamusSuitColors(RgbColor[] power, RgbColor[] varia, RgbColor[] gravity)
    {
        this.power = Complete(power, "Power");
        this.varia = Complete(varia, "Varia");
        this.gravity = Complete(gravity, "Gravity");
    }

    /// <summary>Returns one of a suit's installed normal colours.</summary>
    /// <param name="suit">The suit whose colours are read.</param>
    /// <param name="colorIndex">The suit colour, 0 through 15, including the transparent one.</param>
    /// <returns>The suit's colour.</returns>
    public RgbColor Resolve(SamusSuit suit, int colorIndex)
    {
        RgbColor[] colours = suit switch
        {
            SamusSuit.Power => power,
            SamusSuit.Varia => varia,
            SamusSuit.Gravity => gravity,
            _ => throw new ArgumentOutOfRangeException(nameof(suit)),
        };
        if ((uint)colorIndex >= colours.Length)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));
        return colours[colorIndex];
    }

    /// <summary>Copies <paramref name="colours"/>, rejecting a suit without every colour.</summary>
    private static RgbColor[] Complete(RgbColor[] colours, string name)
    {
        if (colours.Length != SamusSuitColorFormat.ColorsPerSuit)
            throw new InvalidDataException($"Samus {name} suit requires {SamusSuitColorFormat.ColorsPerSuit} colors.");
        return (RgbColor[])colours.Clone();
    }
}
