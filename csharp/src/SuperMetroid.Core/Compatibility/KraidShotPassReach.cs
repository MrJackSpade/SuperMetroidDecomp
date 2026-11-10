namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which of Samus's projectiles Kraid's mouth pass and body pass test, and in what order (#1592). Each pass counts from
/// Samus's shot count itself, not one below it, down to her first shot place, and skips no free place: so it also tests
/// the place just past her counted shots, and with every shot place counted it tests her first bomb, which lies past
/// the last shot place. A bomb can hit Kraid's mouth or body this way. With no shots counted neither pass tests anything.
/// The passes test the projectiles in this order and leave their bounds for the next one (<see cref="KraidPassBounds"/>).
/// </summary>
/// <remarks>
/// Consulted once per update of Kraid's body, before his mouth pass, and the same projectiles serve the body pass after
/// his palette handling. Evidence: the original passes (<c>$A7:AFAA</c>, <c>$A7:B181</c>) load the shot count as their
/// first index into the projectile table and loop down to zero inclusive; the table entry past the five shot places is
/// the first bomb's.
/// </remarks>
[CompatibilityService("Kraid's shot passes count from Samus's shot count, so they test the place past her counted shots and, with every place counted, her first bomb")]
public static class KraidShotPassReach
{
    /// <summary>The projectiles each of Kraid's shot passes tests, in the order it tests them.</summary>
    /// <typeparam name="TProjectile">A projectile a pass tests: a shot place's or the first bomb's.</typeparam>
    /// <param name="shotCount">How many shots Samus has out by her firing count.</param>
    /// <param name="shotPlaces">Samus's shot places' projectiles, in place order.</param>
    /// <param name="firstBomb">Samus's first bomb, which lies past the last shot place.</param>
    /// <exception cref="InvalidDataException">The count exceeds Samus's shot places.</exception>
    public static IReadOnlyList<TProjectile> Tested<TProjectile>(
        int shotCount,
        IReadOnlyList<TProjectile> shotPlaces,
        TProjectile firstBomb)
    {
        Ensure.NotNull(shotPlaces);
        if (Ensure.AtLeastZero(shotCount) > shotPlaces.Count)
            throw new InvalidDataException($"Samus's shot count {shotCount} exceeds her {shotPlaces.Count} shot places.");
        if (shotCount == 0)
            return [];
        var tested = new List<TProjectile>(shotCount + 1);
        for (int place = shotCount; place >= 0; place--)
            tested.Add(place < shotPlaces.Count ? shotPlaces[place] : firstBomb);
        return tested;
    }
}
