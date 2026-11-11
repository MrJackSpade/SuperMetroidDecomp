namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The room's thirty-two sprite-object places (#1591), which gameplay reads in one spot: a shot that does no damage to
/// Kraid's mouth leaves twice the place its dud flash took as the bound the rest of that collision pass is tested
/// against (<see cref="KraidPassBounds.LeaveAfterDud"/>), or the shot's X when every place was taken. A spawn takes the
/// highest free place, or none when all thirty-two are taken; a place is free again once the object that took it has
/// ended. An object that takes no place still appears and runs like any other, and its spawner never learns the
/// difference. Nothing else reads a place: sprite objects step and draw in the order they spawned.
/// </summary>
/// <remarks>
/// Taken for every sprite object the room spawns, as it spawns, after the places of the objects that have ended since the
/// last spawn are freed; consulted by Kraid's mouth pass right after a dud flash spawns. Lifetime: the room's; the room
/// loader creates it with each room's enemy system, so a new room starts with every place free. Evidence: the original
/// sprite-object spawn searches its thirty-two places from the last down for one whose object has ended and returns twice
/// that place, or reports none free; Kraid's mouth pass keeps the
/// returned value in the working value it tests the next projectile's top edge against.
/// </remarks>
[CompatibilityService("a dud shot on Kraid's mouth bounds the rest of its collision pass by the place its flash takes among the room's thirty-two sprite-object places")]
public sealed class SpriteObjectPlaces
{
    /// <summary>How many places the room's sprite objects take; a spawn while every place is taken takes none.</summary>
    internal const int PlaceCount = 32;

    /// <summary>Whether each place is held by an object that has not been seen to end.</summary>
    private readonly bool[] _taken = new bool[PlaceCount];

    /// <summary>Takes the highest free place for a spawn.</summary>
    /// <returns>The place taken, or null when every place is taken.</returns>
    internal int? Take()
    {
        for (int place = PlaceCount - 1; place >= 0; place--)
        {
            if (_taken[place])
                continue;
            _taken[place] = true;
            return place;
        }
        return null;
    }

    /// <summary>Frees <paramref name="place"/>, whose object has ended.</summary>
    /// <param name="place">A place a spawn took.</param>
    internal void Free(int place)
    {
        if (!_taken[Ensure.BetweenInclusive(0, PlaceCount - 1, place)])
            throw new InvalidOperationException($"Sprite-object place {place} is not taken.");
        _taken[place] = false;
    }
}
