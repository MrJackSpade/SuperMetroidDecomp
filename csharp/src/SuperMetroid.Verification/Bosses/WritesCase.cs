using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Bosses;

/// <summary>The document and definitions of one writes case.</summary>
/// <param name="Document">The document.</param>
/// <param name="Definitions">The definitions.</param>
internal readonly record struct WritesCase(EnemyBg2FrameDocument Document, SpritemapFrameDefinition[] Definitions);
