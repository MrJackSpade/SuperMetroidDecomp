using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>One family of Samus projectile compositions and the poses its compositions show, in the game's storage order.</summary>
/// <param name="Group">The family.</param>
/// <param name="Poses">Each composition's pose, in the order the cartridge stores the compositions.</param>
internal readonly record struct ProjectileSpriteFamily(ProjectileSpriteGroup Group, string[] Poses);
