using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="EnemyFramePhase"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record EnemyFramePhaseDependencies
{
    /// <summary>Background scroll.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }


    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }


    /// <summary>Enemy room effects.</summary>
    public required EnemyRoomEffects EnemyRoomEffects { get; init; }


    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>The frame clock.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>The frame's sprite description, which Ceres Ridley's in-main Baby draw publishes into.</summary>
    public required SpriteFrameBuffer Sprites { get; init; }


    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }



    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }



}
