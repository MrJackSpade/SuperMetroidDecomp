namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A run of tilemap rows filled with one tile word.</summary>
/// <param name="Rows">The rows.</param>
/// <param name="Word">The word.</param>
internal readonly record struct TourianAccessDrawFill(int Rows, int Word);
