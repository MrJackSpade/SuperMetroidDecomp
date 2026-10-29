using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>One composed boss body part (#1466): its picture's frame in the sheet, where it lies and what it covers.</summary>
/// <param name="Picture">The sheet frame of its picture before any animation frame is selected.</param>
/// <param name="Top">The layer row its picture's top edge lies on.</param>
/// <param name="Footprint">The cells it covers.</param>
/// <param name="Animation">The animation whose frames it shows, or null for a still part.</param>
/// <param name="AnimationPictures">The sheet frame picturing it for each frame of its animation, by frame name.</param>
internal sealed record ComposedBossBodyPart(string Picture, int Top, BossBodyFootprint Footprint, ArtworkAnimation? Animation,
    IReadOnlyDictionary<string, string> AnimationPictures);
