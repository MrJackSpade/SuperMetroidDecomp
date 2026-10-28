namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete cinematic PLANET ZEBES text-fade control program.</summary>
public sealed class PlanetZebesTextPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal PlanetZebesTextPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive text-fade owner.</summary>
    public required PlanetZebesTextPaletteFxProgramOwner Owner { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
