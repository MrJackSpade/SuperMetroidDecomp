using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>What one enemy pass reads from the rest of the frame: the camera, Samus and her projectiles, controller input, clocks and the room systems enemies touch (#633).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal sealed record RoomEnemyFrame
{
    /// <summary>Camera position (viewport top-left) in whole room pixels.</summary>
    public required Position16<RoomPixels> Camera { get; init; }

    /// <summary>Whether time is frozen, which stops the enemies' AI and animations; it does not stop every timer, the room-wide glows, or frame publication.</summary>
    public required bool TimeIsFrozen { get; init; }

    /// <summary>Samus.</summary>
    public required SamusState Samus { get; init; }

    /// <summary>The actions newly pressed this update, distinct from the held actions.</summary>
    public required GameplayAction NewlyPressedControllerInput { get; init; }

    /// <summary>The room's terrain and collision data.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>The actions held this update.</summary>
    public required GameplayAction ControllerInput { get; init; }

    /// <summary>Samus's beams and missiles, which enemy collision and custom touch paths read.</summary>
    public required SamusProjectileSystem SamusProjectiles { get; init; }

    /// <summary>Samus's colours, which Pseudo Screw's enemy contact changes (#1405).</summary>
    public required SamusColours SamusColours { get; init; }

    /// <summary>Samus's held charge as a Pseudo Screw contact this pass spends it: her projectiles' and her colours' (#2159).</summary>
    public PseudoScrewCharge PseudoScrew => new(SamusProjectiles, SamusColours);

    /// <summary>The game's short frame counter, 0 through 255, which a door's scroll restarts.</summary>
    public required int ShortCounter { get; init; }

    /// <summary>The turning floor of Ceres Ridley's getaway shaft, which rotating Ceres steam samples to derive draw offsets without changing its world collision coordinates; none outside that shaft.</summary>
    public required TurningFloorTransform? TurningFloor { get; init; }

    /// <summary>Shared bomb and power-bomb state used by collision order and actor effects.</summary>
    public required SamusBombProjectileSystem SharedProjectiles { get; init; }

    /// <summary>The game's frame counter.</summary>
    public required int FrameCounter { get; init; }

    /// <summary>The camera's frame-start Samus checkpoint under the live previous positions that Draygon and Yapping Maw cap; null when no camera owns them.</summary>
    public required FixedPosition<RoomPixels>? SamusPreviousPositionCheckpoint { get; init; }
}
