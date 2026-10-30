using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What the PLM handler reads from the rest of the frame: scroll zones, enemy deaths and quota, newly pressed input, collected items and whether a Power Bomb explosion is running (#633).</summary>
public readonly record struct PlmFrameInputs
{
    /// <summary>Scrolls.</summary>
    public required RoomScrollGrid? Scrolls { get; init; }

    /// <summary>The room's kill count (<see cref="Enemies.EnemyRoomStatus.EnemiesKilled"/>), 0 through 65535.</summary>
    public required int EnemyDeaths { get; init; }

    /// <summary>The room's kill-count requirement for enemy-clear triggers, 0 through 255.</summary>
    public required int EnemyDeathQuota { get; init; }

    /// <summary>Controller new input.</summary>
    public required ControllerButton ControllerNewInput { get; init; }

    /// <summary>Collected items.</summary>
    public SuperMetroid.Core.Progression.SamusEquipmentFlags CollectedItems { get; init; }

    /// <summary>Whether a Power Bomb explosion is running.</summary>
    public bool PowerBombExplosionActive { get; init; }
}
