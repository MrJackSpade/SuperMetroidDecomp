using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Rooms;

/// <summary>The collaborators a <see cref="RoomMainRoutines"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record RoomMainRoutinesDependencies
{
    /// <summary>Background scroll.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }

    /// <summary>Enemies.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Room spikes.</summary>
    public required RoomSpikeAnimatedTilesState RoomSpikes { get; init; }

    /// <summary>Room treadmills.</summary>
    public required RoomTreadmillAnimatedTilesState RoomTreadmills { get; init; }

    /// <summary>Sand animated tiles.</summary>
    public required RoomSandAnimatedTilesState SandAnimatedTiles { get; init; }

    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }

    /// <summary>Tourian statues.</summary>
    public required TourianStatueSequence TourianStatues { get; init; }

    /// <summary>Vram.</summary>
    public required SnesVram Vram { get; init; }

    /// <summary>Vram writes.</summary>
    public required VramWriteQueue VramWrites { get; init; }

    /// <summary>Wrecked ship treadmill.</summary>
    public required WreckedShipTreadmillAnimatedTilesState WreckedShipTreadmill { get; init; }
}
