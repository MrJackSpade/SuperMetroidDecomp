using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// One composed boss body part (#1466): its name, its picture's frame in the sheet, where it lies and what it covers. A
/// still part whose cells paint no pixel has no picture: it clears the cells it covers, which then draw nothing (#1513).
/// </summary>
/// <param name="Name">The part's name in the boss's sheet.</param>
/// <param name="Picture">The sheet frame of its picture before any animation frame is selected, or null for a part that clears its cells.</param>
/// <param name="Top">The layer row its picture's top edge lies on.</param>
/// <param name="Footprint">The cells it covers.</param>
/// <param name="Animation">The animation whose frames it shows, or null for a still part.</param>
/// <param name="AnimationPictures">The sheet frame picturing it for each frame of its animation, by frame name.</param>
internal sealed record ComposedBossBodyPart(string Name, string? Picture, int Top, BossBodyFootprint Footprint, ArtworkAnimation? Animation,
    IReadOnlyDictionary<string, string> AnimationPictures);
