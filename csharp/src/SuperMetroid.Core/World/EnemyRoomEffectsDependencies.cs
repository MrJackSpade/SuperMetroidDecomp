using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="EnemyRoomEffects"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record EnemyRoomEffectsDependencies
{
    /// <summary>Cgram.</summary>
    public required GameplayColours Cgram { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }


    /// <summary>Escape timer.</summary>
    public required EscapeTimer EscapeTimer { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }


    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }



    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }
}
