using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What Bomb and Golden Torizo instructions read: Samus, the room, controller input and the low byte of NMI_FrameCounter ($05B6), which both attack choosers add (#633).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct TorizoInstructionInputs
{
    /// <summary>Samus.</summary>
    public required SamusState? Samus { get; init; }

    /// <summary>Level.</summary>
    public required RoomLevelData? Level { get; init; }

    /// <summary>Controller input.</summary>
    public required ControllerButton ControllerInput { get; init; }

    /// <summary>The low byte of the frame counter, from zero to 255. The attack choosers keep only bit 3 of their sum, so only its low four bits affect a choice.</summary>
    public required int FrameCounterLow { get; init; }
}
