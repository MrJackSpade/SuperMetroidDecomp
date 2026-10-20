using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Host recovery snapshots taken after the destination fade and that frame's audio have completed. The state is captured
/// on the game thread and written in the background, so a door costs no compression or disk flush (#1398).
/// </summary>
internal static class DoorTransitionAutosave
{
    /// <summary>Captures only the completed loading-B to gameplay transition; replay and disabled sessions skip it, and a failed write retains the prior recovery state.</summary>
    internal static DebuggerSaveStateMetadata? TrySave(bool enabled, bool replay,
        SuperMetroidGameState previousState, DebuggerSaveStateStore store,
        SuperMetroidAddressSpace bus, SuperMetroidGame game)
    {
        if (!enabled || replay || previousState != SuperMetroidGameState.LoadingNextRoomB ||
            game.GameState != SuperMetroidGameState.MainGameplay) return null;
        // Recovery I/O must not terminate a healthy emulation session. The store
        // replaces the previous state only after serialization and durable flush.
        return store.SaveInBackground(DebuggerStateFormat.AutomaticSlot, bus, game,
            saved => Console.WriteLine($"Door autosave: {saved.Path} | frame {saved.FrameNumber} | room {saved.Room}"),
            exception =>
            {
                Console.Error.WriteLine("Door autosave failed; previous recovery state retained.");
                Console.Error.WriteLine(exception.ToString());
            });
    }
}
