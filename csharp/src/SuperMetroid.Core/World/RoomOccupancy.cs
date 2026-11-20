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

    /// <summary>A room is loaded: the scope its load completed.</summary>
    /// <param name="scope">The loaded room's scope.</param>
    internal sealed class Loaded(RoomScope scope) : RoomOccupancy
    {
        /// <summary>The loaded room's scope.</summary>
        internal RoomScope Scope { get; } = Ensure.NotNull(scope);
    }
}
