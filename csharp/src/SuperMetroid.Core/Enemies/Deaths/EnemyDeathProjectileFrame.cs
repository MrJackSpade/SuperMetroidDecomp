namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>One enemy death and drop pose; its installed artwork key is confined to <see cref="Artwork.EnemyDeathProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct EnemyDeathProjectileFrame(int Pose);
