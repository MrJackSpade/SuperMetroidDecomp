using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="SamusFramePhases"/> is constructed with, built once by the owner that creates it (#621).</summary>
[ArchitectureReview("wide-dependencies", "Samus's per-update phases: each member is an owner one phase reads or advances (input and " +
    "the attract demo's input, freeze, the clocks, the room, progress, artwork and colours, X-ray, the in-game options pose " +
    "transitions read, and the assists the shinespark reads); none is reached through another, so a narrower record would " +
    "only route the same owners through a pass-through (#1483).")]
internal sealed record SamusFramePhasesDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }
    /// <summary>Attract demo.</summary>
    public required IAttractSamusInput AttractDemo { get; init; }


    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }



    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>Game time.</summary>
    public required GameTimeState GameTime { get; init; }

    /// <summary>The frame clock.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>Options.</summary>
    public required GameplayOptions Options { get; init; }

    /// <summary>The runtime's assists; a shinespark reads whether Samus cannot die.</summary>
    public required ActiveAssists Assists { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }



    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }


    /// <summary>System.</summary>
    public required SharedGameState System { get; init; }


    /// <summary>Xray.</summary>
    public required XrayReveal Xray { get; init; }
}
