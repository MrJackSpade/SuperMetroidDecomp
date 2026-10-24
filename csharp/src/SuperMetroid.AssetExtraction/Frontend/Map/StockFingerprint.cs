namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Whether a presentation resource's stock bytes join the catalog fingerprint as well as its selected bytes.</summary>
internal enum StockFingerprint
{
    /// <summary>Only the selected bytes are fingerprinted.</summary>
    SelectedOnly,

    /// <summary>The stock bytes are fingerprinted before the selected bytes.</summary>
    StockAndSelected,
}
