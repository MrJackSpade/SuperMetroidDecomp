namespace SuperMetroid.Core.Game;

/// <summary>Typed view of Samus's CGRAM ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: Samus CGRAM ranges")]
public readonly struct SamusPalettes(SnesCgram cgram)
{
    /// <summary>Samus's suit palette.</summary>
    public CgramPalette Suit => new(cgram, SamusPaletteLayout.Suit);

    /// <summary>Samus's visor colour.</summary>
    public CgramPalette Visor => new(cgram, SamusPaletteLayout.Visor);

    /// <summary>The suitless palette.</summary>
    public CgramPalette Suitless => new(cgram, SamusPaletteLayout.Suitless);

    /// <summary>The beam palette.</summary>
    public CgramPalette Beam => new(cgram, SamusPaletteLayout.Beam);

    /// <summary>The grapple flare colour.</summary>
    public CgramPalette GrappleFlare => new(cgram, SamusPaletteLayout.GrappleFlare);

    /// <summary>The Crystal Flash body colours.</summary>
    public CgramPalette CrystalFlashBody => new(cgram, SamusPaletteLayout.CrystalFlashBody);

    /// <summary>The Crystal Flash bubble colours.</summary>
    public CgramPalette CrystalFlashBubble => new(cgram, SamusPaletteLayout.CrystalFlashBubble);

    /// <summary>Colours below the suit palette whited out at death.</summary>
    public CgramPalette DeathWhiteoutBelowSuit => new(cgram, SamusPaletteLayout.DeathWhiteoutBelowSuit);

    /// <summary>Colours between the suit and suitless palettes whited out at death.</summary>
    public CgramPalette DeathWhiteoutBetweenSuits => new(cgram, SamusPaletteLayout.DeathWhiteoutBetweenSuits);
}
