namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One Mini-Kraid pose; its spritemap is confined to <see cref="Artwork.FakeKraidFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct FakeKraidFrame(int Pose);
