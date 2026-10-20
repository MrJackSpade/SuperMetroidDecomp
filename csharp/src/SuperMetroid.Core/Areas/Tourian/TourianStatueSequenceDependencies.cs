using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The collaborators a <see cref="TourianStatueSequence"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record TourianStatueSequenceDependencies
{


    /// <summary>Cgram.</summary>
    public required SnesCgram Cgram { get; init; }


    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }


    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }



    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }

    /// <summary>Tester.</summary>
    public required TesterTools Tester { get; init; }

    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>Vram writes.</summary>
    public required VramWriteQueue VramWrites { get; init; }
}
