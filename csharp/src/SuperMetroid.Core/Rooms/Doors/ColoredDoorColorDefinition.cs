using System.Collections.Immutable;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One coloured cap colour's rules (#2546): the shots whose hits it counts, the hits it takes before it opens and the paint
/// its images show.
/// </summary>
/// <param name="CountedShots">The families whose shots count a hit on the cap; any other shot makes the dud sound.</param>
/// <param name="HitsToOpen">The hits the cap takes before it opens.</param>
/// <param name="Paint">The paint the cap's closed and opening images show.</param>
internal sealed record ColoredDoorColorDefinition(ImmutableHashSet<SamusProjectileFamily> CountedShots, int HitsToOpen, DoorCapPaint Paint)
{
    /// <summary>Whether a shot of <paramref name="family"/> counts as a hit on a cap of this colour.</summary>
    internal bool Counts(SamusProjectileFamily family) => CountedShots.Contains(family);
}
