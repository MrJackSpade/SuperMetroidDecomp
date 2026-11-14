using SuperMetroid.Core.Input;
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


    /// <summary>The session's unauthored Speed Booster cadence, which a demo Samus runs on like any other.</summary>
    public required UnauthoredBoostCadence UnauthoredBoostCadence { get; init; }

    /// <summary>Hud setup.</summary>
    public required GameplayHudSetup HudSetup { get; init; }

    /// <summary>The accepted frame boundary.</summary>
    public required AcceptedFrameBoundary FrameBoundary { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Room loader.</summary>
    public required RoomLoader RoomLoader { get; init; }

    /// <summary>System.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>What a demo Samus's animation frame past her pose's animation reads.</summary>
    public required FramesBeyondAnimation<SuperMetroid.Core.Samus.Poses.SamusPoseId, SuperMetroid.Core.Samus.Poses.SamusPoseAnimationStep> FramesBeyondAnimation { get; init; }

    /// <summary>The session's beam charge, which a demo Samus reads and clears (#1577).</summary>
    public required SuperMetroid.Core.Samus.Projectiles.BeamChargeState BeamCharge { get; init; }
}
