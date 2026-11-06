using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>The pixels of a composed boss body frame one room animation paints (#1466, #1515), pictured over each of its frames.</summary>
/// <param name="Animation">The animation.</param>
/// <param name="Corner">The body pixel each picture's top-left corner lies on.</param>
/// <param name="Covers">The rectangles of the body the animation paints.</param>
/// <param name="Pictures">The sheet frame picturing them over each frame of the animation, by frame name.</param>
internal sealed record ComposedBossBodyOverlay(ArtworkAnimation Animation, Position16<BackgroundPixels> Corner, ImmutableArray<BossBodyRectangle> Covers,
    IReadOnlyDictionary<string, string> Pictures);
