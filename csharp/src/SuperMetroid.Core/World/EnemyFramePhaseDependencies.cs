using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="EnemyFramePhase"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed partial record EnemyFramePhaseDependencies
{
    /// <summary>Where the layers stand: the foreground position follows the camera X an enemy moves the view to.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }

    /// <summary>The player's gameplay actions as the frame boundary latched them.</summary>
    public required GameplayActionInput PlayerActions { get; init; }

    /// <summary>The room effects the enemies request during their pass, applied after it.</summary>
    public required EnemyRoomEffects EnemyRoomEffects { get; init; }

    /// <summary>Whether gameplay time is frozen, which holds the enemies.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>The game's frame counters, which time enemies, animations and blinking.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>The frame's sprite description, which Ceres Ridley's in-main Baby draw publishes into.</summary>
    public required SpriteFrameBuffer Sprites { get; init; }

    /// <summary>Samus, whose pose, motion and collisions the enemies read and change.</summary>
    public required SamusFrameState Player { get; init; }
}
