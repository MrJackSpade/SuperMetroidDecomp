using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// One composed boss body frame (#1466, #1515): its whole picture in the sheet, where it lies on the body's area, the
/// rectangles it covers, and the pixels each room animation paints. A frame whose cells paint no pixel has no picture: it
/// covers what lies beneath it and shows nothing there (#1513).
/// </summary>
/// <param name="Name">The frame's name in the boss's sheet.</param>
/// <param name="Picture">The sheet frame of its whole picture, its animated pixels as the room loads them, or null when it paints nothing.</param>
/// <param name="Corner">The body pixel its picture's top-left corner lies on.</param>
/// <param name="Covers">The rectangles of the body it covers.</param>
/// <param name="Overlays">The pixels each room animation paints.</param>
internal sealed record ComposedBossBodyFrame(string Name, string? Picture, Position16<BackgroundPixels> Corner, ImmutableArray<BossBodyRectangle> Covers,
    IReadOnlyList<ComposedBossBodyOverlay> Overlays);
