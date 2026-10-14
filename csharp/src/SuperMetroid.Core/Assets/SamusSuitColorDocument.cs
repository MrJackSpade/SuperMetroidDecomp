namespace SuperMetroid.Core.Assets;

/// <summary>JSON schema for the Power, Varia, and Gravity normal body palettes.</summary>
public sealed record SamusSuitColorDocument
{
    /// <summary>Gets the schema version required by <see cref="SamusSuitColorFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the sixteen editable Power-suit RGB5 colors.</summary>
    public required PaletteRgb5[] Power { get; init; }

    /// <summary>Gets the sixteen editable Varia-suit RGB5 colors.</summary>
    public required PaletteRgb5[] Varia { get; init; }

    /// <summary>Gets the sixteen editable Gravity-suit RGB5 colors.</summary>
    public required PaletteRgb5[] Gravity { get; init; }
}
