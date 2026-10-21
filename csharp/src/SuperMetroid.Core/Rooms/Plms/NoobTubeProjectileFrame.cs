namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One n00b tube projectile pose; its installed artwork key is confined to <see cref="SuperMetroid.Core.Enemies.NoobTube.Artwork.NoobTubeProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
/// <param name="ReflectedPose">The pose a reflecting flicker shows instead on even frames, if the frame has one.</param>
internal readonly record struct NoobTubeProjectileFrame(int Pose, int? ReflectedPose = null);
