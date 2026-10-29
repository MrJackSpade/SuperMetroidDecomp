using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="RuntimeArtwork"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record RuntimeArtworkDependencies
{


    /// <summary>The session's installed boss artwork.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }

    /// <summary>The loaded room, whose active escape typewriters follow a rebound escape-warning text.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Hud.</summary>
    public required HudState Hud { get; init; }

    /// <summary>Message box.</summary>
    public required GameplayMessageBoxState MessageBox { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }


    /// <summary>The session that holds every room's installed FX catalogs.</summary>
    public required RoomFxSession RoomFx { get; init; }

}
