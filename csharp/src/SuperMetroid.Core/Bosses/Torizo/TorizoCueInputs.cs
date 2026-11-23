using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// What Bomb and Golden Torizo cues read: the Torizo playing them, Samus, the room, controller input and the frame
/// counter that times the attack choice (<see cref="TorizoChoices.InFirstAttackPhase"/>) (#633).
/// </summary>
internal readonly partial record struct TorizoCueInputs
{
    /// <summary>The Torizo playing the cues, whose belly the glow its fight's start lights watches (#2420).</summary>
    public required ILitTorizo Torizo { get; init; }

    /// <summary>Samus, whom the Torizo's cues act on.</summary>
    public required SamusState Samus { get; init; }

    /// <summary>The room's blocks, which the Torizo's cues meet.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>The player's held actions, which the Torizo's cues read.</summary>
    public required GameplayAction ControllerInput { get; init; }

    /// <summary>The frame counter, whose sum with Samus's halved X sets an attack choice's phase (<see cref="TorizoChoices.InFirstAttackPhase"/>).</summary>
    public required int FrameCounter { get; init; }
}
