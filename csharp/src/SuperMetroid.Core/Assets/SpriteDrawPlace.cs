namespace SuperMetroid.Core.Assets;

/// <summary>
/// A place among a frame's sprite draws (#2618), taken by <see cref="SpriteFrameBuffer.NextPlace"/>: just after the draw
/// <paramref name="After"/>, or before every draw when none had been emitted.
/// </summary>
/// <param name="After">The draw the place follows, or null for the place before every draw.</param>
public readonly partial record struct SpriteDrawPlace(SpriteDraw? After);
