namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Editable schema for the fifteen one-row gameplay messages and their shared border; collections remain caller-mutable until compilation.</summary>
public sealed record GameplayMessageTitleDocument
{
    /// <summary>Schema revision; loading requires <see cref="GameplayMessageTitleDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Thirty-two native BG3 words copied unchanged to both outer rows; the stock row is Small_MessageBox_TopBottomBorder_Tilemap at $85:8040.</summary>
    public required GameplayMessageTitleCell[] Border { get; init; }
    /// <summary>Exactly the fifteen case-sensitive message-name keys listed by <see cref="GameplayMessageTitleDefinitions.MessageIds"/>, each with a nonnull title definition.</summary>
    public required Dictionary<string, GameplayMessageTitle> Titles { get; init; }
}
