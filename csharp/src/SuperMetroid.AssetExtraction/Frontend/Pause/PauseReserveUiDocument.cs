using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Complete editable reserve UI schema; nested arrays and dictionaries are mutable authoring data copied into compiled presentation values.</summary>
public sealed record PauseReserveUiDocument
{
    /// <summary>Schema revision; loading requires <see cref="PauseReserveUiDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly four case-sensitive entries named Mode, ReserveTank, Manual, and Auto, defining placement and ordered character words.</summary>
    public required Dictionary<string, PauseReserveLabelVisual> Labels { get; init; }
    /// <summary>Decimal digit atlas references and starting location of the three-place reserve-energy display.</summary>
    public required PauseReserveDigitVisual Digits { get; init; }
    /// <summary>Arrow cells, enabled/disabled palettes, solid bevel colors, and the 32-phase pulse.</summary>
    public required PauseReserveArrowVisual Arrow { get; init; }
}
