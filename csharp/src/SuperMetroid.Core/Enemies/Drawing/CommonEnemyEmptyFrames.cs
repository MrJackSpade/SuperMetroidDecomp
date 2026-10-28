using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Enemies.Drawing.CommonEnemyEmptyExtendedFrameDefinitions;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>Tells which frames draw nothing and which is the shared point-hitbox extended frame.</summary>
internal static class CommonEnemyEmptyFrames
{
    /// <summary>Whether an actor showing <paramref name="frame"/> draws nothing: no frame yet, or the shared empty spritemap.</summary>
    internal static bool DrawsNothing(EnemyFrameId? frame) => frame is not { } shown || EmptySpritemapFrames.Contains(shown);
    /// <summary>Whether <paramref name="frame"/> is the shared point-hitbox extended frame.</summary>
    internal static bool IsFrame(EnemyFrameId? frame) => frame == EnemyFrameId.EmptyExtended;
}
