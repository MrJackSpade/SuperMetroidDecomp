namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete cinematic PLANET ZEBES text-fade control program.</summary>
public sealed class PlanetZebesTextPaletteFxColorDefinition
{
    /// <summary>Builds the definition from its owner's program values.</summary>
    /// <param name="owner">The title-text fade direction.</param>
    /// <param name="program">The program whose records the fade shows.</param>
    internal PlanetZebesTextPaletteFxColorDefinition(PlanetZebesTextPaletteFxProgramOwner owner, PaletteFxProgram program)
    {
        Owner = owner;
        Rows = PaletteFxColorRows.Of(program, PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame);
    }

    /// <summary>The mutually exclusive text-fade owner.</summary>
    public PlanetZebesTextPaletteFxProgramOwner Owner { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
