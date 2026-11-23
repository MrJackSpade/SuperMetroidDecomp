using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One link of Ridley's tail as his drawing shows it (#2238): the link and the frame drawn at it.</summary>
/// <param name="Link">The link drawn.</param>
/// <param name="Frame">The installed frame drawn at the link.</param>
internal readonly partial record struct RidleyTailLinkPicture(RidleyTailLink Link, EnemyFrameId Frame);
