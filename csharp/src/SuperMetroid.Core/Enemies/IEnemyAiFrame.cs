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
    SnesButton NewlyPressedControllerInput { get; }

    /// <summary>Controller buttons held this frame.</summary>
    SnesButton ControllerInput { get; }

    /// <summary>Room collision data, when the frame has a room.</summary>
    RoomLevelData? Level { get; }

    /// <summary>Samus's beam and missile projectiles that main AI may inspect.</summary>
    SamusProjectileSystem? SamusProjectiles { get; }

    /// <summary>Active Mode 7 transform for rotating actors.</summary>
    SamusMode7Transform? Mode7Transform { get; }

    /// <summary>Shared bomb and power-bomb state.</summary>
    SamusBombProjectileSystem? SharedProjectiles { get; }

    /// <summary>Destination for actor graphics transfers.</summary>
    VramWriteQueue? VramWriteQueue { get; }

    /// <summary>The accepted-frame counters this enemy pass reads.</summary>
    FrameCounters Clock { get; }
}
