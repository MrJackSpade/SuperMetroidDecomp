namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One suit-specific Samus-in-heat palette control program.</summary>
/// <remarks>Each program shows sixteen fifteen-color phases, one record each. Independently reviewed for #1165.</remarks>
public sealed class PaletteFxHeatProgramDefinition
{
    /// <summary>Builds the definition from its owner's program values.</summary>
    internal PaletteFxHeatProgramDefinition(PaletteFxHeatSuit suit, PaletteFxProgram program) =>
        (Suit, Rows) = (suit, PaletteFxColorRows.Of(program, ColorsPerFrame));

    /// <summary>Fifteen BGR555 colors follow every timed duration word.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>The mutually exclusive suit palette represented by this program.</summary>
    public PaletteFxHeatSuit Suit { get; }

    /// <summary>The sixteen phase records and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
