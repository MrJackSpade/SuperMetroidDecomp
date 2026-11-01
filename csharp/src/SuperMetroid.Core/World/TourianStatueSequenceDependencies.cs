using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="TourianStatueSequence"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record TourianStatueSequenceDependencies
{


    /// <summary>Gameplay's colours.</summary>
    public required GameplayColours Colours { get; init; }


    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }


    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }



    /// <summary>System.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>The runtime's assists, which can stand the statues open.</summary>
    public required ActiveAssists Assists { get; init; }

    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

}
