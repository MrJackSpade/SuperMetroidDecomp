using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>The gameplay one update of a sequence runs on, matched once from its need and the session (#2554).</summary>
internal abstract class PhaseGameplay
{
    /// <summary>Only the three below exist.</summary>
    private PhaseGameplay() { }

    /// <summary>The step runs without gameplay.</summary>
    internal sealed class Frontend : PhaseGameplay
    {
        /// <summary>The one step without gameplay.</summary>
        internal static Frontend Instance { get; } = new();

        /// <summary>The value has no data, so one instance serves every update.</summary>
        private Frontend() { }
    }

    /// <summary>The step runs on the session's runtime.</summary>
    /// <param name="runtime">The session's gameplay runtime.</param>
    internal sealed class OfRuntime(SuperMetroidRuntime runtime) : PhaseGameplay
    {
        /// <summary>The session's gameplay runtime.</summary>
        internal SuperMetroidRuntime Runtime { get; } = runtime;
    }

    /// <summary>The step runs gameplay in the runtime's loaded room.</summary>
    /// <param name="live">The session's runtime and its loaded room.</param>
    [RoomScopeBoundary("A per-update input, built for one update and never kept.")]
    internal sealed class InRoom(LoadedGameplay live) : PhaseGameplay
    {
        /// <summary>The session's runtime and its loaded room.</summary>
        internal LoadedGameplay Live { get; } = live;
    }
}
