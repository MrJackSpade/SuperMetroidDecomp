using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>What a room's setup reaches while the room's objects are placed (#2287).</summary>
internal sealed partial record RoomSetupPlacing
{
    /// <summary>The room and the state loading.</summary>
    public required SelectedRoom Room { get; init; }

    /// <summary>The room's level data.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>The room's room objects, which the setups that spawn one join.</summary>
    public required RoomObjectSystem RoomObjects { get; init; }

    /// <summary>The room's scroll grid, which Shaktool's controller reads.</summary>
    public required RoomScrollGrid Scrolls { get; init; }

    /// <summary>The session's progress.</summary>
    public required SharedGameState System { get; init; }
}
