namespace SuperMetroid.RenderVerification.Samus;

/// <summary>A stock catalog and its edited counterpart.</summary>
/// <param name="Stock">The stock.</param>
/// <param name="Edited">The edited.</param>
internal readonly record struct StockEditedPair<T>(T Stock, T Edited);
