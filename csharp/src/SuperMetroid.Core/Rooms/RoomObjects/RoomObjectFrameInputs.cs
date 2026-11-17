using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What the room objects' step reads from the rest of the frame: scroll zones, enemy deaths and quota, newly pressed input, collected items and whether a Power Bomb explosion is running (#633).</summary>
public readonly record struct RoomObjectFrameInputs
{
    /// <summary>The room's scroll grid, whose screens scroll triggers and the objects that open or close screens set.</summary>
    public required RoomScrollGrid Scrolls { get; init; }

    /// <summary>The enemies killed in the room so far (<see cref="Enemies.EnemyRoomStatus.EnemiesKilled"/>).</summary>
    public required int EnemyDeaths { get; init; }

    /// <summary>The kills the room's enemy-clear triggers wait for.</summary>
    public required int EnemyDeathQuota { get; init; }

    /// <summary>The gameplay actions newly pressed this update, which break the n00b tube once a Power Bomb has struck it.</summary>
    public required GameplayAction ControllerNewInput { get; init; }

    /// <summary>The items Samus has collected, which a grey door that closes for good once she has the Bombs reads.</summary>
    public SuperMetroid.Core.Progression.SamusEquipmentFlags CollectedItems { get; init; }

    /// <summary>Whether a power-bomb explosion is under way.</summary>
    public bool PowerBombExplosionActive { get; init; }
}
