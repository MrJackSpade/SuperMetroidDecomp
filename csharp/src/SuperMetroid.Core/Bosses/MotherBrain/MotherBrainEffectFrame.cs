namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Mother Brain effect pose; its installed artwork key is confined to <see cref="Artwork.MotherBrainEffectFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct MotherBrainEffectFrame(int Pose);
