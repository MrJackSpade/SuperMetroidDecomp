namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>What one region shows for a picture of a frame (#1392).</summary>
/// <param name="Region">The region.</param>
/// <param name="Upload">The upload showing over it, or none while the graphics set's own characters show there.</param>
/// <param name="RotStep">
/// The rot step of the rotting corpse whose characters a corpse upload showed over the region last, or none while no
/// corpse has.
/// </param>
public sealed record CharacterRegionContent(CharacterRegion Region, RuntimeCharacterUpload? Upload, int? RotStep = null);
