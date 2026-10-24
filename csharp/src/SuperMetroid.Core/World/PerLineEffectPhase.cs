using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>
/// The per-line effect phase (#1407): each frame it advances the room liquid's shared wave state and the Tourian
/// statues' descent, the per-line effects whose state gameplay reads back. The cartridge runs this work as its HDMA
/// objects' pre-instructions (<c>$88:84B9</c>).
/// </summary>
/// <remarks>
/// Compatibility behaviour: the phase runs at a fixed point of the outer frame, before the main loop draws the frame's
/// random number, and the statues' descent reads the random number as it stands then. Its place in the frame, the
/// order of its two steps, and the frames it runs on (gameplay frames without a message box, the message box's
/// restoring frames, and the door-transition frames <see cref="Rooms.Doors.PerLineEffectProcessing"/> selects) are
/// kept exactly, so the random sequence and every decision drawn from it match the cartridge's.
/// </remarks>
internal sealed class PerLineEffectPhase
{
    /// <summary>Supplies the authored freeze gate to the shared room liquid update.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Shared system state the liquid's wave step reads.</summary>
    private readonly Bank80SystemState _system;

    /// <summary>Connects the per-line effect phase to its live freeze and system owners.</summary>
    internal PerLineEffectPhase(GameplayTimeFreeze freeze, Bank80SystemState system)
    {
        _freeze = freeze;
        _system = system;
    }

    /// <summary>
    /// Advances the effects in their fixed order, before the frame's random number is drawn: the room liquid's shared
    /// state, then the Tourian statues' delay and descent (<c>$88:DBD7-$DCBA</c>), whose rumble reads the random number.
    /// </summary>
    internal void Advance(RoomPlmSystem plms, RoomScope room)
    {
        room.Layer3Fx.AdvanceHdmaSharedState(_system, _freeze.TimeIsFrozen);
        room.TourianStatues.StepDescent(plms);
    }
}
