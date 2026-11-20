namespace SuperMetroid.Core.World;

/// <summary>
/// Whether the runtime has a loaded room (#2554): none from the runtime's creation until its first room load completes,
/// then the room the last completed load built.
/// </summary>
internal abstract class RoomOccupancy
{
    /// <summary>Only the two states below exist.</summary>
    private RoomOccupancy() { }

    /// <summary>No room has loaded yet: the runtime exists, and its first room load has not completed.</summary>
    internal sealed class NoRoomLoaded : RoomOccupancy
    {
        /// <summary>The one state before the first load.</summary>
        internal static NoRoomLoaded Instance { get; } = new();

        /// <summary>The state has no data, so one instance serves every runtime.</summary>
        private NoRoomLoaded() { }
    }

    /// <summary>
    /// A room is loaded: the runtime's loaded room from its first completed load on, which each later load replaces with
    /// the room it built. The code that runs in a loaded room holds this one state and reads the room it is in now.
    /// </summary>
    internal sealed class Loaded : RoomOccupancy
    {
        /// <summary>The runtime's loaded room, holding the room its first load built.</summary>
        /// <param name="scope">The room the runtime's first load built.</param>
        internal Loaded(RoomScope scope) => Scope = Ensure.NotNull(scope);

        /// <summary>The room the runtime's last completed load built.</summary>
        internal RoomScope Scope { get; private set; }

        /// <summary>Makes <paramref name="scope"/>, which a later load built, the loaded room.</summary>
        internal void Replace(RoomScope scope) => Scope = Ensure.NotNull(scope);
    }
}
