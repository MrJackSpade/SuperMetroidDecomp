namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The definition records whose characters Samus's body shows: each half's, or null for a half never selected.</summary>
/// <param name="Top">The top half's record.</param>
/// <param name="Bottom">The bottom half's record.</param>
public readonly record struct SamusBodyRecords(int? Top, int? Bottom);
