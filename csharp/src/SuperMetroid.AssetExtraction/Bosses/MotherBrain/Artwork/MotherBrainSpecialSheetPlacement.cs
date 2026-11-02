using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Which sheet one of Mother Brain's special sheets is, and the sprite character its first page shows from.</summary>
/// <param name="Sheet">The sheet this is.</param>
/// <param name="FirstCharacter">The sprite character the first page shows from; each later page follows the one before.</param>
internal readonly record struct MotherBrainSpecialSheetPlacement(MotherBrainSpritePageSheet Sheet, int FirstCharacter);
