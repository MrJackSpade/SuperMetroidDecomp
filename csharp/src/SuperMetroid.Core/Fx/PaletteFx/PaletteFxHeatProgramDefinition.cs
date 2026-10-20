namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One suit-specific Samus-in-heat palette control program.</summary>
/// <remarks>Each program shows sixteen fifteen-color phases, one record each. Independently reviewed for #1165.</remarks>
public sealed class PaletteFxHeatProgramDefinition
{
    /// <summary>Fifteen BGR555 colors follow every timed duration word.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>Binds a suit identity to its sixteen phase records.</summary>
    internal PaletteFxHeatProgramDefinition(PaletteFxHeatSuit suit, PaletteFxProgram program)
    {
        Suit = suit;
        Rows = PaletteFxColorRows.Of(program, ColorsPerFrame);
    }

    /// <summary>The mutually exclusive suit palette represented by this program.</summary>
    public PaletteFxHeatSuit Suit { get; }

    /// <summary>The sixteen phase records and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
