namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One eye door projectile pose; its installed artwork key is confined to <see cref="SuperMetroid.Core.Enemies.EyeDoor.Artwork.EyeDoorProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct EyeDoorProjectileFrame(int Pose);
