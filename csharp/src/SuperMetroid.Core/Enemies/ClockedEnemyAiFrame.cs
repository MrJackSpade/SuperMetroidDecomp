using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy pass's AI frame with the counters it resolved once for the pass.</summary>
/// <param name="frame">The enemy frame.</param>
/// <param name="clock">The pass's counters.</param>
internal sealed class ClockedEnemyAiFrame(RoomEnemyFrame frame, FrameCounters clock) : IEnemyAiFrame
{
    /// <inheritdoc/>
    public Position16<RoomPixels> Camera => frame.Camera;

    /// <inheritdoc/>
    public SamusState? Samus => frame.Samus;

    /// <inheritdoc/>
    public SnesButton NewlyPressedControllerInput => frame.NewlyPressedControllerInput;

    /// <inheritdoc/>
    public SnesButton ControllerInput => frame.ControllerInput;

    /// <inheritdoc/>
    public RoomLevelData? Level => frame.Level;

    /// <inheritdoc/>
    public SamusProjectileSystem? SamusProjectiles => frame.SamusProjectiles;

    /// <inheritdoc/>
    public SamusMode7Transform? Mode7Transform => frame.Mode7Transform;

    /// <inheritdoc/>
    public SamusBombProjectileSystem? SharedProjectiles => frame.SharedProjectiles;

    /// <inheritdoc/>
    public FrameCounters Clock => clock;
}
