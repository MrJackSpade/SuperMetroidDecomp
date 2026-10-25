using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>The collaborators a <see cref="AttractDemoDriver"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record AttractDemoDriverDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>Attract session.</summary>
    public required AttractDemoSession AttractSession { get; init; }

    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }


    /// <summary>The enemy words and installed enemy artwork that outlive each room.</summary>
    public required EnemySession EnemySession { get; init; }

    /// <summary>Hud setup.</summary>
    public required GameplayHudSetup HudSetup { get; init; }

    /// <summary>Nmi handler.</summary>
    public required AcceptedFrameBoundary FrameBoundary { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Room loader.</summary>
    public required RoomLoader RoomLoader { get; init; }

    /// <summary>System.</summary>
    public required SharedGameState System { get; init; }
}
