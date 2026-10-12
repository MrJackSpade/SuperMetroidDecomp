using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>What Bomb and Golden Torizo instructions read: Samus, the room, controller input and the low byte of NMI_FrameCounter ($05B6), which both attack choosers add (#633).</summary>
internal readonly record struct TorizoInstructionInputs
{
    /// <summary>Samus.</summary>
    public required SamusState? Samus { get; init; }

    /// <summary>Level.</summary>
    public required RoomLevelData? Level { get; init; }

    /// <summary>Controller input.</summary>
    public required ushort ControllerInput { get; init; }

    /// <summary>Frame counter low.</summary>
    public required byte FrameCounterLow { get; init; }
}
