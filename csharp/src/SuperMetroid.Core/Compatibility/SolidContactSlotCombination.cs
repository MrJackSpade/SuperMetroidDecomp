namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which enemy an idle shutter takes Samus to be pressing against (#1471): not the enemy she met in any one direction,
/// but the slot ordinals of the solid enemies she met in all four directions this enemy update, combined bit by bit,
/// with a direction that met no enemy counting as every bit set. Meeting one enemy, in one or several directions, names
/// that enemy; meeting two different enemies at once names the slot their ordinals share, which can be a third enemy.
/// </summary>
/// <remarks>
/// Consulted by the shutters' idle contact test, after Samus's movement has recorded her solid-enemy contacts for the
/// update and before the enemy pass clears them. Evidence: the shared idle-shutter contact test at <c>$A2:EEE9</c> and
/// <c>$A2:F1F6</c> ANDs the four direction words, which hold the enemy's slot offset or all ones, and compares the
/// result with the shutter's own offset; the offsets are the ordinals shifted left, so the combination is the same in
/// ordinals.
/// </remarks>
[CompatibilityService("An idle shutter takes Samus to be pressing against the enemy whose slot the slots of the solid enemies she met this update, combined bit by bit, name")]
public static class SolidContactSlotCombination
{
    /// <summary>
    /// The slot ordinal Samus's contacts combine to, or null when she met no solid enemy in any direction.
    /// </summary>
    /// <param name="contactsByDirection">The ordinal of the solid enemy she met in each direction, or null where she met none.</param>
    /// <param name="slotCount">How many enemy slots there are, a power of two; a direction with no contact counts as every ordinal bit set.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="slotCount"/> is not a positive power of two, or a contact is not a slot ordinal.</exception>
    public static int? Combine(IReadOnlyList<int?> contactsByDirection, int slotCount)
    {
        Ensure.NotNull(contactsByDirection);
        if (slotCount <= 0 || (slotCount & (slotCount - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(slotCount), slotCount, "The slot count is not a positive power of two.");

        int noContact = slotCount - 1;
        int combined = noContact;
        bool metAny = false;
        foreach (int? contact in contactsByDirection)
        {
            if (contact is not { } ordinal)
                continue;
            combined &= Ensure.BetweenInclusive(0, noContact, ordinal);
            metAny = true;
        }
        return metAny ? combined : null;
    }
}
