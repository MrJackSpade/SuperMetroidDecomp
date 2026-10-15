namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One Fune or Namihe pose; its spritemap is confined to <see cref="Artwork.FuneNamiheFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct FuneNamiheFrame(int Pose);
