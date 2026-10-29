using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>One boss's composed body sheet (#1466): its pictures and the parts each frame is drawn as.</summary>
/// <param name="Owner">The boss.</param>
/// <param name="Sheet">The pictures.</param>
/// <param name="Blank">The part the boss clears its layer with.</param>
/// <param name="Frames">Each frame's parts, by frame name.</param>
internal sealed record ComposedBossBodySheet(BossBodyOwner Owner, ComposedSpriteSheet Sheet, ComposedBossBodyPart Blank,
    IReadOnlyDictionary<string, IReadOnlyList<ComposedBossBodyPart>> Frames);
