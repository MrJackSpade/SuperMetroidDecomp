namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Editable JSON schema for pause-menu wireframe artwork; equipped-item rules still select which variant is shown, independently of authored cell content.</summary>
public sealed record PauseWireframeDocument
{
    /// <summary>Schema revision, which must equal <see cref="PauseWireframeDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>All four <see cref="PauseWireframeKind"/> names, each mapping to 136 cells in eight-column, seventeen-row order with separate atlas, palette, priority, and flip selections.</summary>
    public required Dictionary<string, PauseBackdropCell[]> Frames { get; init; }
}
