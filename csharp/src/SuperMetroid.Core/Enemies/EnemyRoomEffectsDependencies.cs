using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Enemies;

/// <summary>The collaborators a <see cref="EnemyRoomEffects"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record EnemyRoomEffectsDependencies
{
    /// <summary>Cgram.</summary>
    public required SnesCgram Cgram { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Enemies.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>Escape timer.</summary>
    public required EscapeTimer EscapeTimer { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Plms.</summary>
    public required RoomPlmSystem Plms { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Room layer3 fx.</summary>
    public required RoomLayer3FxState RoomLayer3Fx { get; init; }

    /// <summary>Room palette fx.</summary>
    public required RoomPaletteFxSystem RoomPaletteFx { get; init; }

    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }
}
