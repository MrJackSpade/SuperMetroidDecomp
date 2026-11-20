using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The room's Tourian statue as its grey caps see it (#2486, #2554), composed with the room: the statue room's, whose
/// finished sinking a cap there waits for, or none in every other room, which places no cap waiting for one.
/// </summary>
internal abstract class GreyDoorStatue
{
    /// <summary>Only the two below exist.</summary>
    private GreyDoorStatue()
    {
    }

    /// <summary>The lock of a cap waiting for the room's statue to finish sinking.</summary>
    internal abstract GreyDoorLock FinishedLock();

    /// <summary>The statue room's statue.</summary>
    /// <param name="statue">The statue, whose finished sinking the room's cap waits for.</param>
    internal sealed class InStatueRoom(ITourianStatueProgress statue) : GreyDoorStatue
    {
        /// <summary>The statue.</summary>
        private readonly ITourianStatueProgress _statue = Ensure.NotNull(statue);

        /// <inheritdoc/>
        internal override GreyDoorLock FinishedLock() => new GreyDoorLock.TourianStatueFinished(_statue);
    }

    /// <summary>Every other room, which has no Tourian statue for a cap to wait for.</summary>
    internal sealed class Absent : GreyDoorStatue
    {
        /// <inheritdoc/>
        /// <exception cref="InvalidDataException">
        /// Always: the room's population places a cap waiting for a statue the room does not have, which only the statue
        /// room's population does.
        /// </exception>
        internal override GreyDoorLock FinishedLock() =>
            throw new InvalidDataException("A grey door waits for the Tourian statue in a room other than the statue room.");
    }
}
