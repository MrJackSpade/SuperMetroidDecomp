using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>
/// The per-line effect phase (#1407): it advances the room liquid's shared wave state and the Tourian statues' descent,
/// the per-line effects whose state gameplay reads back, at the start of an update and before the update draws its random
/// number.
/// </summary>
/// <remarks>
/// <para>
/// This phase is gameplay, not a compatibility service: advancing the liquid and the statues is the game's own work.
/// The compatibility behaviour it carries, where that work falls in the sequence of random draws and which updates skip
/// it, is <see cref="EffectPhaseRandomOrder"/>'s, which the runtime creates with itself and injects here; the random
/// sequence itself is the session's <see cref="SharedRandom"/>.
/// </para>
/// <para>
/// The statues' descent reads the random number as it stands before the update's draw, so the phase's place in the
/// update, the order of its two steps, and the updates it runs on are kept exactly. Advanced at the start of every
/// update that draws a random number (gameplay, a message box's restoring update, the door transition's waits and
/// fades, and the update before the pause screen opens), before the draw. Created with each gameplay runtime, whose
/// lifetime its state shares (the liquid and the statues live in the loaded room scope it is handed). Evidence: the
/// original main loop runs its per-line effect objects (<c>$88:84B9</c>) and then draws its random number
/// (<c>$82:894F</c>) on every pass; the Ridley full-movie check compares the random number on every update of a door
/// transition.
/// </para>
/// </remarks>
internal sealed class PerLineEffectPhase
{
    /// <summary>Supplies the authored freeze gate to the shared room liquid update.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Shared system state the liquid's wave step reads.</summary>
    private readonly SharedGameState _system;
    /// <summary>Which updates run the phase before their random draw.</summary>
    private readonly EffectPhaseRandomOrder _order;

    /// <summary>Connects the per-line effect phase to its live freeze and system owners and the order it runs in.</summary>
    internal PerLineEffectPhase(GameplayTimeFreeze freeze, SharedGameState system, EffectPhaseRandomOrder order)
    {
        _freeze = freeze;
        _system = system;
        _order = Ensure.NotNull(order);
    }

    /// <summary>Whether this update advances the effects: false while a door transition has stopped them.</summary>
    internal bool RunsThisUpdate => _order.RunsEffectWork;

    /// <summary>
    /// Advances the effects in their fixed order, before the update's random number is drawn, unless a door transition
    /// has stopped them: the room liquid's shared state, then the Tourian statues' delay and descent, whose rumble reads
    /// the random number.
    /// </summary>
    internal void Advance(RoomPlmSystem plms, RoomScope room)
    {
        if (!RunsThisUpdate)
            return;
        room.Layer3Fx.AdvanceLiquidMotion(_system, _freeze.TimeIsFrozen);
        room.TourianStatues.StepDescent(plms);
    }
}
