using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What an enemy's main AI reads from the enemy frame (#633). <see cref="RoomEnemyFrame"/> is the
/// superset the enemy pass owns; main AI and the common grapple wrapper accept only this contract.
/// </summary>
internal interface IEnemyAiFrame
{
    /// <summary>Camera position used by off-screen and relative-position checks.</summary>
    Position16<RoomPixels> Camera { get; }

    /// <summary>Samus, when the frame has one.</summary>
    SamusState? Samus { get; }

    /// <summary>Controller buttons newly pressed this frame.</summary>
    ControllerButton NewlyPressedControllerInput { get; }

    /// <summary>Controller buttons held this frame.</summary>
    ControllerButton ControllerInput { get; }

    /// <summary>Room collision data, when the frame has a room.</summary>
    RoomLevelData? Level { get; }

    /// <summary>Samus's beam and missile projectiles that main AI may inspect.</summary>
    SamusProjectileSystem? SamusProjectiles { get; }

    /// <summary>The turning floor of Ceres Ridley's getaway shaft, which rotating actors follow, or null when none turns.</summary>
    SamusMode7Transform? TurningFloor { get; }

    /// <summary>Shared bomb and power-bomb state.</summary>
    SamusBombProjectileSystem? SharedProjectiles { get; }

    /// <summary>The accepted-frame counters this enemy pass reads.</summary>
    FrameCounters Clock { get; }
}
