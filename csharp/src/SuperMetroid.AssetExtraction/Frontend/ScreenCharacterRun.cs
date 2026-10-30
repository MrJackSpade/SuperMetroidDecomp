namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>A run of an artwork's characters placed at consecutive tile numbers of a screen (#1467).</summary>
/// <param name="FirstTile">The tile number of the run's first character.</param>
/// <param name="Characters">The run's characters, whole characters of the screen's depth.</param>
internal readonly record struct ScreenCharacterRun(int FirstTile, ReadOnlyMemory<byte> Characters);
