using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room liquid's wave and the Tourian statues' descent (#1407), whose state gameplay reads back, advanced at the start
/// of an update and before the update draws its random number.
/// </summary>
/// <remarks>
/// <para>
/// This is gameplay, not a compatibility service: advancing the liquid and the statues is the game's own work.
/// The compatibility behaviour it carries, where that work falls in the sequence of random draws and which updates skip
/// it, is <see cref="EffectPhaseRandomOrder"/>'s, which the runtime creates with itself and injects here; the random
/// sequence itself is the session's <see cref="SharedRandom"/>.
/// </para>
/// <para>
/// The statues' descent reads the random number as it stands before the update's draw, so the advance's place in the
/// update, the order of its two steps, and the updates it runs on are kept exactly. Advanced at the start of every
/// update that draws a random number (gameplay, a message box's restoring update, the door transition's waits and
/// fades, and the update before the pause screen opens), before the draw. Created with each gameplay runtime, whose
/// lifetime its state shares (the liquid and the statues live in the loaded room scope it is handed). Evidence: the
/// original main loop advances the liquid wave and the statues and then draws its random number on every pass; the
/// Ridley full-movie check compares the random number on every update of a door transition.
/// </para>
/// </remarks>
internal sealed class LiquidWaveAndStatueDescent
{
    /// <summary>Supplies the authored freeze gate to the shared room liquid update.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Shared system state the liquid's wave step reads.</summary>
    private readonly SharedGameState _system;
    /// <summary>Which updates run the phase before their random draw.</summary>
    private readonly EffectPhaseRandomOrder _order;

    /// <summary>Connects the liquid wave and statue descent to their live freeze and system owners and the order they run in.</summary>
    internal LiquidWaveAndStatueDescent(GameplayTimeFreeze freeze, SharedGameState system, EffectPhaseRandomOrder order)
    {
        _freeze = freeze;
        _system = system;
        _order = Ensure.NotNull(order);
    }

    /// <summary>Whether this update advances the liquid wave and the statues: false while a door transition has stopped them.</summary>
    internal bool RunsThisUpdate => _order.RunsEffectWork;

    /// <summary>
    /// Advances the liquid wave and the statues in their fixed order, before the update's random number is drawn, unless a
    /// door transition has stopped them: the room liquid's shared state, then the Tourian statues' delay and descent, whose rumble reads
    /// the random number.
    /// </summary>
    internal void Advance(RoomObjectSystem roomObjects, RoomScope room)
    {
        if (!RunsThisUpdate)
            return;
        room.FxOverlay.AdvanceLiquidMotion(_system, _freeze.TimeIsFrozen);
        room.TourianStatues.StepDescent(roomObjects);
    }
}
