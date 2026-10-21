namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The index and mask of one difference.</summary>
/// <param name="Index">The index.</param>
/// <param name="Mask">The mask.</param>
internal readonly record struct Difference(int Index, int Mask);
