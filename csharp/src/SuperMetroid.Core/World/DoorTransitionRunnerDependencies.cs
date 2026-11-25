using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="DoorTransitionRunner"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed partial record DoorTransitionRunnerDependencies
{
    /// <summary>Draws Samus and the room's actors during the door's waits and fades.</summary>
    public required GameplayActorDrawing ActorDrawing { get; init; }

    /// <summary>What the gameplay display shows, carried across the transition.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>The enemies' part of an update, which runs while Samus waits at the door.</summary>
    public required EnemyFramePhase EnemyPhase { get; init; }

    /// <summary>The liquid wave and statue descent the door transition's frames advance.</summary>
    public required LiquidWaveAndStatueDescent LiquidWaveAndStatueDescent { get; init; }

    /// <summary>The game's frame counters, which time enemies, animations and blinking.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>The accepted frame boundary.</summary>
    public required AcceptedFrameBoundary FrameBoundary { get; init; }

    /// <summary>Sprite frame the runner begins, draws into and finalizes.</summary>
    public required SpriteFrameBuffer Sprites { get; init; }

    /// <summary>The session's shared game state: the defeated bosses the door's setup reads and each update's random number.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>Whether time is frozen, which holds the room effect's overlay still.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }
}
