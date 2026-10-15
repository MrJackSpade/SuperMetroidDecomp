namespace SuperMetroid.Core.Assets;

/// <summary>Which actor or display element a sprite draw shows.</summary>
/// <param name="Kind">The source family.</param>
/// <param name="Instance">Distinguishes several sources of one kind (an enemy slot, a projectile slot); zero for single sources.</param>
public readonly record struct SpriteSource(SpriteSourceKind Kind, int Instance = 0);
