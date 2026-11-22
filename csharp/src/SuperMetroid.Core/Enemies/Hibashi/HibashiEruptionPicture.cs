using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>One picture of the Hibashi's eruption (#2557) and how long it shows.</summary>
/// <param name="Picture">The picture the flame shows.</param>
/// <param name="Updates">The updates it shows for.</param>
internal readonly partial record struct HibashiEruptionPicture(EnemyFrameId Picture, int Updates);
