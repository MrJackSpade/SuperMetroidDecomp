using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Advances the HDMA objects' pre-instructions each frame: the shared layer-3 HDMA state and the Tourian statue
/// descent.
/// </summary>
internal sealed class HdmaObjectPhase
{
    /// <summary>Supplies the authored freeze gate to the shared room liquid update.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Owner of the room's liquid and layer-3 HDMA pre-instruction state.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Shared system state passed to native liquid pre-instructions.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>Owner of the Tourian statues' delayed BG2 descent.</summary>
    private readonly TourianStatueSequence _tourianStatues;

    /// <summary>Connects the HDMA pre-instruction phase to its live freeze, liquid, system, and statue owners.</summary>
    internal HdmaObjectPhase(GameplayTimeFreeze freeze, RoomLayer3FxState roomLayer3Fx, Bank80SystemState system, TourianStatueSequence tourianStatues)
    {
        _freeze = freeze;
        _roomLayer3Fx = roomLayer3Fx;
        _system = system;
        _tourianStatues = tourianStatues;
    }

    /// <summary>
    /// The bank-$88 HDMA objects' pre-instructions ($88:84B9), which run before
    /// <c>GenerateRandomNumber</c>: the room liquid's shared state and the Tourian
    /// statues' BG2 delay and descent ($88:DBD7-$DCBA), whose rumble reads that RNG.
    /// </summary>
    internal void AdvanceHdmaObjectPreInstructions()
    {
        _roomLayer3Fx.AdvanceHdmaSharedState(_system, _freeze.TimeIsFrozen);
        _tourianStatues.StepDescent();
    }
}
