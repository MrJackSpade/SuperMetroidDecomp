using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="GameplayActorDrawing"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record GameplayActorDrawingDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>Attract session.</summary>
    public required IAttractDemoState AttractSession { get; init; }



    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }



    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>The frame clock.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>Sprite frame receiving actor OAM sprites and whole-frame draws.</summary>
    public required SpriteFrameBuffer Sprites { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }


    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

}
