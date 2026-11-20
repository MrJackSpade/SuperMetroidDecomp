using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// The runtime's Samus as resident room objects see her (#633): every read reads the runtime when called, never a
/// load-time snapshot, so a room object sees her after a room transition too.
/// </summary>
/// <param name="player">The runtime's player.</param>
internal sealed class RoomObjectSamusAccess(SamusFrameState player) : IRoomSamusAccess
{
    /// <summary>The runtime's player.</summary>
    private readonly SamusFrameState _player = Ensure.NotNull(player);

    /// <summary>
    /// The current Samus; a room object reading her before the scenario introduces her fails there.
    /// </summary>
    public IRoomSamus Current => _player.Samus.RoomView;
}
