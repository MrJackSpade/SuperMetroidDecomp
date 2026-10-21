namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>A selected artwork file and its bytes.</summary>
/// <param name="Path">The path.</param>
/// <param name="Bytes">The bytes.</param>
public readonly record struct SelectedArtworkFile(string Path, byte[] Bytes);
