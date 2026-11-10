namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which of Samus's shots can make a Space Pirate flinch (#1592): only the one in her highest shot place that holds a
/// shot the pirate reacts to. The search stops there, so a far shot in a higher place hides a close shot in a lower one
/// and the pirate does not flinch. A walking pirate reacts to any shot (an invisible one included); a ninja pirate only
/// to a beam or missile.
/// </summary>
/// <remarks>
/// Consulted once per update by a walking pirate's and a ninja pirate's flinch check, during the enemy update, after the
/// update's projectiles have moved. Evidence: the pirates' original flinch checks walk the projectile table from the
/// last shot place down, take the first entry whose projectile type is set and return without testing a lower entry
/// when that projectile lies outside the flinch box.
/// </remarks>
[CompatibilityService("a Space Pirate flinches only from the shot in Samus's highest occupied shot place, even when a lower shot is closer")]
public static class PirateFlinchFromHighestShot
{
    /// <summary>The one shot a pirate's flinch check tests, or null when no place holds a shot it reacts to.</summary>
    /// <typeparam name="TShot">One of Samus's shot places' projectiles.</typeparam>
    /// <param name="shotPlaces">Samus's shot places' projectiles, in place order.</param>
    /// <param name="reactsTo">Whether the pirate's check counts a place's projectile as a shot.</param>
    public static TShot? Shot<TShot>(IReadOnlyList<TShot> shotPlaces, Func<TShot, bool> reactsTo)
        where TShot : class
    {
        Ensure.NotNull(shotPlaces);
        Ensure.NotNull(reactsTo);
        for (int place = shotPlaces.Count - 1; place >= 0; place--)
        {
            if (reactsTo(shotPlaces[place]))
                return shotPlaces[place];
        }
        return null;
    }
}
