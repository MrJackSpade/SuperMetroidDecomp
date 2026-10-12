using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>Host recovery snapshots taken after the destination fade and that frame's audio have completed.</summary>
internal static class DoorTransitionAutosave
{
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
