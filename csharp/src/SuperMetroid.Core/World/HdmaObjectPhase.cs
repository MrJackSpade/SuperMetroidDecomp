using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.World;

/// <summary>
/// Advances the HDMA objects' pre-instructions each frame: the shared layer-3 HDMA state and the Tourian statue
/// descent.
/// </summary>
internal sealed class HdmaObjectPhase
{
    /// <summary>Supplies the authored freeze gate to the shared room liquid update.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Shared system state passed to native liquid pre-instructions.</summary>
    private readonly Bank80SystemState _system;

    /// <summary>Connects the HDMA pre-instruction phase to its live freeze and system owners.</summary>
    internal HdmaObjectPhase(GameplayTimeFreeze freeze, Bank80SystemState system)
    {
        _freeze = freeze;
        _system = system;
    }

    /// <summary>
    /// The bank-$88 HDMA objects' pre-instructions ($88:84B9), which run before
    /// <c>GenerateRandomNumber</c>: the room liquid's shared state and the Tourian
    /// statues' BG2 delay and descent ($88:DBD7-$DCBA), whose rumble reads that RNG.
    /// </summary>
    internal void AdvanceHdmaObjectPreInstructions(RoomPlmSystem plms, RoomScope room)
    {
        room.Layer3Fx.AdvanceHdmaSharedState(_system, _freeze.TimeIsFrozen);
        room.TourianStatues.StepDescent(plms);
    }
}
