namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>Defines one timed pause-selector animation phase for all three categories.</summary>
public sealed record PauseSelectorPhase
{
    /// <summary>Gets the phase duration in accepted update ticks.</summary>
    public required int DurationTicks { get; init; }
    /// <summary>Gets the named frame used by the reserve selector.</summary>
    public required string Reserve { get; init; }
    /// <summary>Gets the named frame used by the beam selector.</summary>
    public required string Beam { get; init; }
    /// <summary>Gets the named frame used by the suit-and-equipment selector.</summary>
    public required string Equipment { get; init; }
}
