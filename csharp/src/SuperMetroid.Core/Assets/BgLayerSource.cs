namespace SuperMetroid.Core.Assets;

/// <summary>The display element a background layer shows.</summary>
/// <param name="Kind">The element's kind.</param>
/// <param name="Instance">Distinguishes several elements of one kind; zero for single elements.</param>
public readonly record struct BgLayerSource(BgLayerSourceKind Kind, int Instance = 0);
