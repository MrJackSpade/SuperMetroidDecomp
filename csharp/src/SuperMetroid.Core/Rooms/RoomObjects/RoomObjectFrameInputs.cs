using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What the room objects' step reads from the rest of the frame: scroll zones, enemy deaths and quota, newly pressed input, collected items and whether a Power Bomb explosion is running (#633).</summary>
public readonly record struct RoomObjectFrameInputs
{
    /// <summary>Scrolls.</summary>
    public required RoomScrollGrid? Scrolls { get; init; }

    /// <summary>The enemies killed in the room so far (<see cref="Enemies.EnemyRoomStatus.EnemiesKilled"/>).</summary>
    public required int EnemyDeaths { get; init; }

    /// <summary>The room's kill-count requirement for enemy-clear triggers, 0 through 255.</summary>
    public required int EnemyDeathQuota { get; init; }

    /// <summary>Controller new input.</summary>
    public required ControllerButton ControllerNewInput { get; init; }

    /// <summary>Collected items.</summary>
    public SuperMetroid.Core.Progression.SamusEquipmentFlags CollectedItems { get; init; }

    /// <summary>Whether a power-bomb explosion is under way.</summary>
    public bool PowerBombExplosionActive { get; init; }
}
