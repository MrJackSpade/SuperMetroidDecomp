using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The first, second and third of one verify single frame enemy visuals case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct VerifySingleFrameEnemyVisualsCase(NativeEnemySpritemapDefinition First, ushort Second, int Third);
