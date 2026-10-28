namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One suit-specific Samus-in-heat palette control program.</summary>
/// <remarks>Each program shows sixteen fifteen-color phases, one record each. Independently reviewed for #1165.</remarks>
public sealed class PaletteFxHeatProgramDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal PaletteFxHeatProgramDefinition()
    {
    }

    /// <summary>Fifteen BGR555 colors follow every timed duration word.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>The mutually exclusive suit palette represented by this program.</summary>
    public required PaletteFxHeatSuit Suit { get; init; }

    /// <summary>The sixteen phase records and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }
}
