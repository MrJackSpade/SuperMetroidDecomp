using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>Host recovery snapshots taken after the destination fade and that frame's audio have completed.</summary>
internal static class DoorTransitionAutosave
{
    /// <summary>Saves the recovery slot after a completed door transition, excluding replay sessions.</summary>
    /// <param name="enabled">Whether door-transition autosaving is enabled.</param>
    /// <param name="replay">Whether the game is executing a replay, for which autosave is suppressed.</param>
    /// <param name="previousState">Game state before the transition frame.</param>
    /// <param name="store">Debugger save-state store receiving the automatic slot.</param>
    /// <param name="bus">Current runtime address space.</param>
    /// <param name="game">Current game instance.</param>
    /// <param name="audio">Managed audio player captured with the game, when available.</param>
    /// <returns>Saved metadata when the transition qualifies and persistence succeeds; otherwise <see langword="null"/>.</returns>
    internal static DebuggerSaveStateMetadata? TrySave(bool enabled, bool replay,
        SuperMetroidGameState previousState, DebuggerSaveStateStore store,
        SuperMetroidAddressSpace bus, SuperMetroidGame game, ManagedSpcPlayer? audio)
    {
        if (!enabled || replay || previousState != SuperMetroidGameState.LoadingNextRoomB ||
            game.GameState != SuperMetroidGameState.MainGameplay) return null;
        try
        {
            var saved = store.Save(DebuggerStateFormat.AutomaticSlot, bus, game, audio);
            Console.WriteLine($"Door autosave: {saved.Path} | frame {saved.FrameNumber} | room {saved.RoomPointer:X4}");
            return saved;
        }
        catch (Exception exception)
        {
            // Recovery I/O must not terminate a healthy emulation session. The store
            // replaces the previous state only after serialization and durable flush.
            Console.Error.WriteLine("Door autosave failed; previous recovery state retained.");
            Console.Error.WriteLine(exception.ToString());
            return null;
        }
    }
}
