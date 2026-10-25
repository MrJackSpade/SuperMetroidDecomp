namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>One compressed options-page source and its diagnostic name.</summary>
/// <param name="Address">The bank-$97 address of the compressed tilemap.</param>
/// <param name="Description">Human-readable page and language identity for extraction diagnostics.</param>
internal readonly record struct GameOptionsPageResource(int Address, string Description);
