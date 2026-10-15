namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>One Skree/Metaree particle pose; its installed artwork key is confined to <see cref="Artwork.SkreeMetareeParticleFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct SkreeMetareeParticleFrame(int Pose);
