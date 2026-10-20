using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Samus;

/// <summary>The collaborators a <see cref="SamusFramePhases"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record SamusFramePhasesDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }
    /// <summary>Attract demo.</summary>
    public required IAttractSamusInput AttractDemo { get; init; }


    /// <summary>Cgram.</summary>
    public required SnesCgram Cgram { get; init; }

    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }



    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>Game time.</summary>
    public required GameTimeState GameTime { get; init; }

    /// <summary>Nmi.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>Options.</summary>
    public required GameplayOptions Options { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }



    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }


    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }


    /// <summary>Xray.</summary>
    public required XrayReveal Xray { get; init; }
}
