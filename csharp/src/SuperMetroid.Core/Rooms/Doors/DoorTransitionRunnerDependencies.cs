using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The collaborators a <see cref="DoorTransitionRunner"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record DoorTransitionRunnerDependencies
{
    /// <summary>Actor drawing.</summary>
    public required GameplayActorDrawing ActorDrawing { get; init; }

    /// <summary>Background scroll.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }



    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Enemies.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>Enemy phase.</summary>
    public required EnemyFramePhase EnemyPhase { get; init; }

    /// <summary>Hdma phase.</summary>
    public required HdmaObjectPhase HdmaPhase { get; init; }


    /// <summary>Nmi.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>Nmi handler.</summary>
    public required AcceptedFrameBoundary FrameBoundary { get; init; }

    /// <summary>Sprite frame the runner begins, draws into and finalizes.</summary>
    public required SpriteFrameBuffer Sprites { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }


    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }


    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }

}
