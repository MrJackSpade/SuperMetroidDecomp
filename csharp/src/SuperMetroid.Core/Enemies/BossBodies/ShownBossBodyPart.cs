using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>One part of a shown boss body (#1466): the picture it shows, the layer row its top edge lies on, and its cells.</summary>
/// <param name="Picture">The picture, repeated across and down from the layer's left edge and row <paramref name="Top"/>.</param>
/// <param name="Top">The layer row the picture's top edge lies on.</param>
/// <param name="Shows">The cells it shows.</param>
public sealed record ShownBossBodyPart(SpriteSheetFrame Picture, int Top, BossBodyFootprint Shows);
