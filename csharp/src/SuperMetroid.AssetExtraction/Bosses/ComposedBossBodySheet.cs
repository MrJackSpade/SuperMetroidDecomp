using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>One boss's composed body sheet (#1466, #1515): its pictures and its frames.</summary>
/// <param name="Owner">The boss.</param>
/// <param name="Sheet">The pictures.</param>
/// <param name="Frames">Each frame, by name.</param>
internal sealed record ComposedBossBodySheet(BossBodyOwner Owner, ComposedSpriteSheet Sheet, IReadOnlyDictionary<string, ComposedBossBodyFrame> Frames);
