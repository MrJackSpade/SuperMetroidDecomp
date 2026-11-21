using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>What a composed boss body frame shows (#2560): its picture in the sheet and the depth it is drawn at (#1634).</summary>
/// <param name="Picture">The sheet frame of its whole picture.</param>
/// <param name="Depth">The depth its picture is drawn at.</param>
internal sealed record ComposedBossBodyPicture(string Picture, BossBodyDepth Depth);
