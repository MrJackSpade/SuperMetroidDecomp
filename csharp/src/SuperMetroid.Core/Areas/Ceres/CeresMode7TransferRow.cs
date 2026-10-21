
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A Mode 7 transfer list and one of its rows.</summary>
/// <param name="List">The list.</param>
/// <param name="Row">The row.</param>
internal readonly record struct CeresMode7TransferRow(CeresMode7TransferList List, int Row);
