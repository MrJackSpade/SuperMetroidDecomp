using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// One composed boss body frame (#1466, #1515, #1540): its whole picture in the sheet, where it lies on the body's area and
/// the depth it is drawn at (#1634). A frame whose cells paint no pixel has no picture and no depth: the part showing it is
/// not drawn (#1513).
/// </summary>
/// <param name="Name">The frame's name in the boss's sheet.</param>
/// <param name="Picture">The sheet frame of its whole picture, or null when it paints nothing.</param>
/// <param name="Corner">The body pixel its picture's top-left corner lies on.</param>
/// <param name="Depth">The depth its picture is drawn at, or null when it paints nothing.</param>
internal sealed record ComposedBossBodyFrame(string Name, string? Picture, Position16<BackgroundPixels> Corner, BossBodyDepth? Depth);
