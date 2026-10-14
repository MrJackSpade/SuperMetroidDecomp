namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>One complete 32x32 backdrop per area; live buttons are a separate foreground overlay.</summary>
public sealed record PauseBackdropDocument
{
    /// <summary>Schema revision; loading requires <see cref="PauseBackdropDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly Crateria, Brinstar, Norfair, WreckedShip, Maridia, Tourian and Ceres, each containing 1024 nonnull cells in 32-column row-major order; collections remain caller-mutable.</summary>
    public required Dictionary<string, PauseBackdropCell[]> Areas { get; init; }
    /// <summary>Separate foreground/button page with 512 nonnull cells in 32-column, 16-row order, initially corresponding to the lower half of native $B6:E000's backdrop.</summary>
    public required PauseBackdropCell[] Buttons { get; init; }
}
