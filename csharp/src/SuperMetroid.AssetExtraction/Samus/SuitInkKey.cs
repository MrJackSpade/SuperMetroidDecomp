namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>An ink row of a suit's palette.</summary>
/// <param name="Suit">The suit.</param>
/// <param name="Row">The row.</param>
internal readonly record struct SuitInkKey(int Suit, int Row);
