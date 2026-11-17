using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What Bomb and Golden Torizo cues read: Samus, the room, controller input and the frame counter's count in its 256-update cycle, which both attack choosers add (#633).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct TorizoCueInputs
{
    /// <summary>Samus, whom the Torizo's cues act on.</summary>
    public required SamusState Samus { get; init; }

    /// <summary>The room's blocks, which the Torizo's cues meet.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>The player's held actions, which the Torizo's cues read.</summary>
    public required GameplayAction ControllerInput { get; init; }

    /// <summary>The frame counter, whose sum with Samus's halved X sets an attack choice's phase (<see cref="TorizoChoices.InFirstAttackPhase"/>).</summary>
    public required int FrameCounter { get; init; }
}
