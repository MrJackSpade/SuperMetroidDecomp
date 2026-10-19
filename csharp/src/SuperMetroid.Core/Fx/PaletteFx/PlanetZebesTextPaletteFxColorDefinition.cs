namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete cinematic PLANET ZEBES text-fade control program.</summary>
public sealed class PlanetZebesTextPaletteFxColorDefinition
{
    /// <summary>Selects one of the mutually exclusive title-text fade directions.</summary>
    internal PlanetZebesTextPaletteFxColorDefinition(PlanetZebesTextPaletteFxProgramOwner owner) => Owner = owner;

    /// <summary>The mutually exclusive text-fade owner.</summary>
    public PlanetZebesTextPaletteFxProgramOwner Owner { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows => new(Owner == PlanetZebesTextPaletteFxProgramOwner.FadeIn ? 42 : 50, PlanetZebesTextPaletteFxColorDefinitions.FrameCount, PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
