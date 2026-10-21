namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>A replacement for an authored anchor; a null coordinate keeps the authored one.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
public readonly record struct AnchorOverride(int? X, int? Y);
