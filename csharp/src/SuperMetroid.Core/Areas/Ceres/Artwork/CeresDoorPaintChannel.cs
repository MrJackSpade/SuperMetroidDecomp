using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>One colour channel of one one-based normal Ceres door palette slot, the key of an authored paint seed.</summary>
/// <param name="Slot">The one-based door palette slot.</param>
/// <param name="Channel">The colour channel of that slot.</param>
internal readonly record struct CeresDoorPaintChannel(int Slot, ColorChannel Channel);
