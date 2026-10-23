namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Typed view of Samus's CGRAM ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: Samus CGRAM ranges")]
public readonly struct SamusPalettes(IPaletteColors cgram)
{
    /// <summary>Samus's suit palette.</summary>
    public CgramPalette Suit => new(cgram, SamusPaletteLayout.Suit);

    /// <summary>The suitless palette.</summary>
    public CgramPalette Suitless => new(cgram, SamusPaletteLayout.Suitless);

    /// <summary>Colours below the suit palette whited out at death.</summary>
    public CgramPalette DeathWhiteoutBelowSuit => new(cgram, SamusPaletteLayout.DeathWhiteoutBelowSuit);

    /// <summary>Colours between the suit and suitless palettes whited out at death.</summary>
    public CgramPalette DeathWhiteoutBetweenSuits => new(cgram, SamusPaletteLayout.DeathWhiteoutBetweenSuits);
}
