using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

/// <summary>The Bomb Torizo room fixture a grey-door lifecycle check drives (#633).</summary>
internal readonly record struct TorizoGreyDoorRoom
{
    /// <summary>Level.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>Streamer.</summary>
    public required BackgroundTilemapStreamer Streamer { get; init; }

    /// <summary>Plms.</summary>
    public required RoomPlmSystem Plms { get; init; }

    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }
}
