using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The names enemy frames take in a boss's installed body sheet (#1466): the frame's own identity's. The import names the
/// frames it composes with these, and bosses draw them by them.
/// </summary>
internal static class BossBodyFrameNames
{
    /// <summary>The body sheet frame of enemy frame <paramref name="frame"/>.</summary>
    internal static string Of(EnemyFrameId frame) => frame.ToString();
}
