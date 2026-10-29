using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>What one region shows for a picture of a frame (#1392, #1463).</summary>
/// <param name="Region">The region.</param>
/// <param name="Look">The encounter look showing over it, or none while the graphics set's own look shows there.</param>
/// <param name="RotStep">
/// The rot step of the rotting corpse whose rows showed over the region last, or none while no corpse has.
/// </param>
public sealed record LookRegionContent(LookRegion Region, SpriteArtworkSource? Look, int? RotStep = null);
