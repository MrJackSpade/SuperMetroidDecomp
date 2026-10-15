namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One Yapping Maw body link pose; its installed artwork key is confined to <see cref="Artwork.YappingMawBodyProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct YappingMawBodyProjectileFrame(int Pose);
