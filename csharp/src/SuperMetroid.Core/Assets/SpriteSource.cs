namespace SuperMetroid.Core.Assets;

/// <summary>
/// Which kind of actor or display element a sprite draw shows. Several draws of one kind in a frame are told apart by
/// what they show, where and in which order they are drawn.
/// </summary>
/// <param name="Kind">The kind of actor or display element.</param>
public readonly partial record struct SpriteSource(SpriteSourceKind Kind);
