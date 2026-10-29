using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose visual frames may draw on its boss body layer (#1466).</summary>
internal interface IBossBodyFrames : IEnemyEntity
{
    /// <summary>The body frame <paramref name="selected"/> draws on the body layer, or null when it draws none there.</summary>
    /// <param name="selected">The resolved visual frame.</param>
    /// <exception cref="InvalidDataException">The frame draws on the body layer but the installed body sheet lacks it.</exception>
    BossBodyFrame? FindBodyFrame(EnemyFrameId selected);
}
