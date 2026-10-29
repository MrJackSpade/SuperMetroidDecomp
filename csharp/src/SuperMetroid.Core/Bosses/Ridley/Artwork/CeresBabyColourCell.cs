using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>One colour of one Ceres Baby shade, the key of a supplied colour edit.</summary>
/// <param name="Shade">The shade the colour belongs to.</param>
/// <param name="Color">The colour within the shade's fifteen colours.</param>
internal readonly record struct CeresBabyColourCell(CeresBabyShade Shade, int Color);
