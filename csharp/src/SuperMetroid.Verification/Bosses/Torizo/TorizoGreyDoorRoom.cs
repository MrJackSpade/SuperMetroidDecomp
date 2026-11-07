using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>The Bomb Torizo room fixture a grey-door lifecycle check drives (#633).</summary>
internal readonly record struct TorizoGreyDoorRoom
{
    /// <summary>Level.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>RoomObjects.</summary>
    public required RoomObjectSystem RoomObjects { get; init; }

    /// <summary>System.</summary>
    public required SharedGameState System { get; init; }
}
