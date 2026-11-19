using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What holds one grey door cap locked (#2486): its placed condition, resolved when the cap is set up into the test the
/// cap makes on each update, with what that test reads. The Tourian statue's lock holds the statue room's statue, which
/// only the statue room has, so a cap waiting for the statue can be placed only there.
/// </summary>
[RoomLifetime]
internal abstract class GreyDoorLock
{
    /// <summary>Whether the condition holds on this update.</summary>
    /// <param name="frame">The update's room-object inputs, which carry the room's enemy deaths and quota.</param>
    internal abstract bool IsUnlocked(RoomObjectFrameInputs frame);

    /// <summary>What the condition holding does besides unlocking the cap; only the room's enemy quota does anything.</summary>
    internal virtual void RecordUnlocked()
    {
    }

    /// <summary>A lock that holds until one of the area's <paramref name="bosses"/> is defeated.</summary>
    /// <param name="system">The game's progress, whose defeated bosses the lock reads.</param>
    /// <param name="area">The room's area.</param>
    /// <param name="bosses">The bosses of the area whose defeat unlocks the cap.</param>
    internal sealed class BossDefeated(SharedGameState system, AreaId area, DefeatedBosses bosses) : GreyDoorLock
    {
        /// <summary>The bosses of the area whose defeat unlocks the cap.</summary>
        private readonly DefeatedBosses _bosses = bosses;

        /// <inheritdoc/>
        internal override bool IsUnlocked(RoomObjectFrameInputs frame) => system.HasDefeatedAnyOf(area, _bosses);
    }

    /// <summary>A lock that holds until the room's enemy deaths reach its quota, which also wakes Zebes.</summary>
    /// <param name="system">The game's progress, whose Zebes-awake event the unlocking sets.</param>
    internal sealed class EnemyDeathQuota(SharedGameState system) : GreyDoorLock
    {
        /// <inheritdoc/>
        internal override bool IsUnlocked(RoomObjectFrameInputs frame) => frame.EnemyDeaths >= frame.EnemyDeathQuota;

        /// <inheritdoc/>
        internal override void RecordUnlocked() => system.SetEvent(StoryEvent.ZebesAwake);
    }

    /// <summary>A lock that never opens.</summary>
    internal sealed class Never() : GreyDoorLock
    {
        /// <inheritdoc/>
        internal override bool IsUnlocked(RoomObjectFrameInputs frame) => false;
    }

    /// <summary>A lock that holds until the Tourian entrance statue has finished sinking.</summary>
    /// <param name="statue">The statue room's statue.</param>
    internal sealed class TourianStatueFinished(ITourianStatueProgress statue) : GreyDoorLock
    {
        /// <summary>The statue room's statue.</summary>
        private readonly ITourianStatueProgress _statue = Ensure.NotNull(statue);

        /// <inheritdoc/>
        internal override bool IsUnlocked(RoomObjectFrameInputs frame) => _statue.IsFinished;
    }

    /// <summary>A lock that holds until <paramref name="storyEvent"/> has happened.</summary>
    /// <param name="system">The game's progress, whose events the lock reads.</param>
    /// <param name="storyEvent">The event that unlocks the cap.</param>
    internal sealed class EventSet(SharedGameState system, StoryEvent storyEvent) : GreyDoorLock
    {
        /// <summary>The event that unlocks the cap.</summary>
        private readonly StoryEvent _storyEvent = storyEvent;

        /// <inheritdoc/>
        internal override bool IsUnlocked(RoomObjectFrameInputs frame) => system.HasEvent(_storyEvent);
    }
}
