using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What an enemy's main AI reads from the enemy frame (#633). <see cref="RoomEnemyFrame"/> is the
/// superset the enemy pass owns; main AI and the shared grapple step accept only this contract.
/// </summary>
internal interface IEnemyAiFrame
{
    /// <summary>Camera position used by off-screen and relative-position checks.</summary>
    Position16<RoomPixels> Camera { get; }

    /// <summary>Samus.</summary>
    SamusState Samus { get; }

    /// <summary>The actions newly pressed this frame.</summary>
    GameplayAction NewlyPressedControllerInput { get; }

    /// <summary>The actions held this frame.</summary>
    GameplayAction ControllerInput { get; }

    /// <summary>The room's collision data.</summary>
    RoomLevelData Level { get; }

    /// <summary>Samus's beam and missile projectiles that main AI may inspect.</summary>
    SamusProjectileSystem SamusProjectiles { get; }

    /// <summary>Samus's held charge, which a Pseudo Screw contact main AI resolves spends (#2159).</summary>
    PseudoScrewCharge PseudoScrew { get; }

    /// <summary>The turning floor of Ceres Ridley's getaway shaft, which rotating actors follow, or null when none turns.</summary>
    TurningFloorTransform? TurningFloor { get; }

    /// <summary>Shared bomb and power-bomb state.</summary>
    SamusBombProjectileSystem SharedProjectiles { get; }

    /// <summary>The accepted-frame counters this enemy pass reads.</summary>
    FrameCounters Clock { get; }
}
