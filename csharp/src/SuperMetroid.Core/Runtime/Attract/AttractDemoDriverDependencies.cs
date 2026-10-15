using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>The collaborators a <see cref="AttractDemoDriver"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record AttractDemoDriverDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>Attract session.</summary>
    public required AttractDemoSession AttractSession { get; init; }

    /// <summary>Cgram.</summary>
    public required SnesCgram Cgram { get; init; }

    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }

    /// <summary>Enemies.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>Hud setup.</summary>
    public required GameplayHudSetup HudSetup { get; init; }

    /// <summary>Nmi handler.</summary>
    public required NmiHandler NmiHandler { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Room loader.</summary>
    public required RoomLoader RoomLoader { get; init; }

    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }

    /// <summary>Address space.</summary>
    public required ISnesAddressSpace AddressSpace { get; init; }
}
