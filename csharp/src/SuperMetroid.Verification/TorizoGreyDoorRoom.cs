using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Spatial;

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
