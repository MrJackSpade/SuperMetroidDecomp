namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>One Powamp spike pose; its installed artwork key is confined to <see cref="Artwork.PowampSpikeFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct PowampSpikeFrame(int Pose);
