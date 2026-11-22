using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="TourianStatueSequence"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed partial record TourianStatueSequenceDependencies
{
    /// <summary>Samus, whose equipped items the colour effects a release starts read.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The session's shared game state: the bosses' and statues' events, the defeated bosses, the random number and Tourian's unlock.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>The runtime's assists, which can stand the statues open.</summary>
    public required ActiveAssists Assists { get; init; }

    /// <summary>Whether gameplay time is frozen, which holds the statues' descent.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }
}
