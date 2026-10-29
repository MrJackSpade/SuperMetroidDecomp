using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The body canvases bosses assemble besides the shown one (#1395, #1466): a body frame is drawn into each that exists
/// now. Crocomire's death assembles his melt in one; Mother Brain stages her body in one until her second phase shows
/// it. The bosses implement it.
/// </summary>
internal interface IBossBodyCopies
{
    /// <summary>Draws <paramref name="frame"/> into each canvas that exists now.</summary>
    void Draw(BossBodyFrame frame);

    /// <summary>
    /// Whether a boss stages its body instead of showing it: Mother Brain stages hers until her phase-two transition
    /// shows it (#1356).
    /// </summary>
    bool StagesBody { get; }
}
