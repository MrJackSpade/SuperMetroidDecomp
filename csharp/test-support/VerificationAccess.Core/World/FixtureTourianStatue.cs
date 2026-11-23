using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>The Tourian statue progress a room-object fixture installs; an uninstalled read fails at the dependency (#2015).</summary>
internal sealed class FixtureTourianStatue : ITourianStatueProgress
{
    /// <summary>Answers whether the entrance statue sequence has finished.</summary>
    public Func<bool>? Finished { get; init; }

    /// <summary>Delegates the query or fails at the missing fixture dependency.</summary>
    public bool IsFinished => (Finished ?? throw new InvalidOperationException(
        "The room-object fixture did not install the Tourian statue's progress."))();
}
