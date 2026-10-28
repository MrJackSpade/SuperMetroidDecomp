namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete cinematic PLANET ZEBES text-fade control program.</summary>
/// <param name="owner">The title-text fade direction.</param>
/// <param name="program">The program whose records the fade shows.</param>
public sealed class PlanetZebesTextPaletteFxColorDefinition(PlanetZebesTextPaletteFxProgramOwner owner, PaletteFxProgram program)
{
    /// <summary>The mutually exclusive text-fade owner.</summary>
    public PlanetZebesTextPaletteFxProgramOwner Owner { get; } = owner;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(program, PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
