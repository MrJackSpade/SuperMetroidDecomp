namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which enemy an idle shutter takes Samus to be pressing against (#1471): not the enemy she met in any one direction,
/// but the room's place whose position in the enemy places is the positions of the solid enemies she met in all four
/// directions this enemy update, combined bit by bit, with a direction that met no enemy counting as every bit set.
/// Meeting one enemy, in one or several directions, names that enemy; meeting two different enemies at once names the
/// place their positions share, which can be a third enemy.
/// </summary>
/// <remarks>
/// Consulted by the shutters' idle contact test, after Samus's movement has recorded the enemies she met for the update
/// and before the enemy pass clears them. Her records hold the enemies themselves (#1702); only this test reads their
/// positions. Evidence: the shared idle-shutter contact test at <c>$A2:EEE9</c> and <c>$A2:F1F6</c> ANDs the four
/// direction words, which hold the enemy's slot offset or all ones, and compares the result with the shutter's own
/// offset; the offsets are the positions shifted left, so the combination is the same in positions.
/// </remarks>
[CompatibilityService("An idle shutter takes Samus to be pressing against the enemy whose place the places of the solid enemies she met this update, combined bit by bit, name")]
public static class SolidContactSlotCombination
{
    /// <summary>
    /// The place Samus's contacts combine to, or null when she met no solid enemy in any direction.
    /// </summary>
    /// <typeparam name="TPlace">An enemy place.</typeparam>
    /// <param name="contactsByDirection">The enemy she met in each direction, or null where she met none.</param>
    /// <param name="places">
    /// The room's enemy places in order, a power of two of them; a direction with no contact counts as every position bit
    /// set.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">The place count is not a positive power of two.</exception>
    /// <exception cref="ArgumentException">A contact is not one of <paramref name="places"/>.</exception>
    public static TPlace? NamedPlace<TPlace>(IReadOnlyList<TPlace?> contactsByDirection, IReadOnlyList<TPlace> places)
        where TPlace : class
    {
        Ensure.NotNull(contactsByDirection);
        Ensure.NotNull(places);
        int placeCount = places.Count;
        if (placeCount <= 0 || (placeCount & (placeCount - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(places), placeCount, "The place count is not a positive power of two.");

        int combined = placeCount - 1;
        bool metAny = false;
        foreach (TPlace? contact in contactsByDirection)
        {
            if (contact is null)
                continue;
            combined &= PositionOf(contact, places);
            metAny = true;
        }
        return metAny ? places[combined] : null;
    }

    /// <summary>The position of <paramref name="contact"/> among <paramref name="places"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="contact"/> is not one of <paramref name="places"/>.</exception>
    private static int PositionOf<TPlace>(TPlace contact, IReadOnlyList<TPlace> places)
        where TPlace : class
    {
        for (int position = 0; position < places.Count; position++)
        {
            if (ReferenceEquals(places[position], contact))
                return position;
        }
        throw new ArgumentException("Samus's contact names an enemy that is not one of the room's places.", nameof(contact));
    }
}
