using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One of Ridley's tail links that is drawn with a fixed frame (#1935, #2238): its shape and the frame drawn at it.</summary>
/// <param name="Link">The link's lengths and initial angle.</param>
/// <param name="Frame">The installed frame drawn at the link.</param>
internal readonly partial record struct RidleyDrawnTailLinkDefinition(RidleyTailLinkDefinition Link, EnemyFrameId Frame);
