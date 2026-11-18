using SuperMetroid.Core.Animation;
using S = SuperMetroid.Core.Rooms.Doors.DoorCapSegment;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The grey caps' authored animations (#1554, <see cref="DoorCapSequenceDefinitions"/>). A grey cap shows its closed image,
/// flashes while its condition holds and opens at the first shot after; Bomb Torizo's door stays open until Samus has
/// Bombs.
/// </summary>
internal static class GreyDoorDefinitions
{
    /// <summary>A locked cap's closed image, for one update.</summary>
    internal static Animation<DoorCapSegment> Locked => DoorCapSequenceDefinitions.Closed;

    /// <summary>An unlocked cap's flash, the shared flash three times over and over until a shot opens it.</summary>
    internal static Animation<DoorCapSegment> Flash { get; } =
        new([DoorCapSequenceDefinitions.Segments.Repeated(S.Flash, DoorCapSequenceDefinitions.FlashesPerHit)], new AnimationEnd.Loop(0));

    /// <summary>A shot cap opening, after which it leaves the room.</summary>
    internal static Animation<DoorCapSegment> Opening { get; } = DoorCapSequenceDefinitions.Once(S.GreyOpening);

    /// <summary>Bomb Torizo's door held open behind Samus, which plays again until she has Bombs.</summary>
    internal static Animation<DoorCapSegment> BombTorizoHeldOpen { get; } = DoorCapSequenceDefinitions.Once(S.BombTorizoHeldOpen);

    /// <summary>Bomb Torizo's door closing once Samus has Bombs, then its closed image for one update.</summary>
    internal static Animation<DoorCapSegment> BombTorizoClosing { get; } = new(
        [DoorCapSequenceDefinitions.Segments.Whole(S.BombTorizoClosing), DoorCapSequenceDefinitions.Segments.Whole(S.Closed)],
        AnimationEnd.Stops);
}
