using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Advances the HDMA objects' pre-instructions each frame: the shared layer-3 HDMA state and the Tourian statue
/// descent.
/// </summary>
internal sealed class HdmaObjectPhase
{
    private readonly GameplayTimeFreeze _freeze;
    private readonly RoomLayer3FxState _roomLayer3Fx;
    private readonly Bank80SystemState _system;
    private readonly TourianStatueSequence _tourianStatues;

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
