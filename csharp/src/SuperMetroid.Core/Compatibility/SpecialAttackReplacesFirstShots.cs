namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which of Samus's shots a starting special attack replaces: its particles take Samus's first shot places, the
/// particle playing the first part the first place, whatever shots hold them. A shot still flying in one of them ends
/// as the attack starts, live, just spent or exploding alike, with no sound or explosion of its own, while a shot in a
/// later place flies on; a particle carries nothing from the shot it replaces but its horizontal sub-pixel position
/// (<see cref="LeftBeamStartFromLastShot"/>).
/// </summary>
/// <remarks>
/// Consulted once per special-attack start, after its Power Bomb cost, as the particles are set off. Evidence: the Wave, Ice, Spazer and Plasma starts (<c>$90:CD1A</c>, <c>$90:CD9B</c>, <c>$90:CE14</c>,
/// <c>$90:CE98</c>) write their particles into the first four projectile places, the last first, neither testing nor
/// deleting what those places hold, and set the shot count to four; every value a particle's update and animation read
/// is one its start writes, and the position, collision size and picture the replaced shot leaves are rewritten by the
/// particle's first update, which runs straight after the start in the same update (<c>$90:DCFB</c>). A start meets a
/// live shot there: the attack starts on the 120th update of a held charge, whose first update fired an uncharged shot
/// into the lowest free place (<c>$90:B80D</c>), and a shot lives while it strikes nothing and stays within 64 pixels of
/// the view (<c>$90:B16A</c>). A beam fired from a standstill launches at four pixels an update and gains a sixteenth of
/// a pixel each update; Samus dashing after it with the Speed Booster runs at up to two and three quarter pixels plus an
/// extra speed that grows by the same sixteenth (<c>$90:973E</c>), so the beam is still in view, about 170 pixels ahead
/// of her, when the attack starts.
/// </remarks>
[CompatibilityService("a special attack's particles replace whatever shots hold Samus's first shot places")]
internal static partial class SpecialAttackReplacesFirstShots
{
    /// <summary>
    /// Takes the shot place each of a starting attack's <paramref name="parts"/> names, the last place first, each ending
    /// whatever shot holds it.
    /// </summary>
    /// <typeparam name="TPart">A particle's part.</typeparam>
    /// <typeparam name="TShot">The shot type.</typeparam>
    /// <param name="parts">The attack's particles' parts, one or more, each naming a different shot place.</param>
    /// <param name="placeOf">The shot place a part takes.</param>
    /// <param name="replaceShotInPlace">Creates the new shot of a part's shot place, ending the one there.</param>
    /// <returns>Each part with the shot that plays it, the part in the first place first.</returns>
    internal static IReadOnlyList<ParticleShot<TPart, TShot>> ParticleShots<TPart, TShot>(IReadOnlyList<TPart> parts, Func<TPart, int> placeOf,
        Func<TPart, TShot> replaceShotInPlace)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(placeOf);
        Ensure.NotNull(replaceShotInPlace);
        Ensure.BetweenInclusive(1, int.MaxValue, parts.Count);
        if (parts.Select(placeOf).Distinct().Count() != parts.Count)
            throw new InvalidDataException("Each of a special attack's particles takes a shot place of its own.");
        var shots = new List<ParticleShot<TPart, TShot>>(parts.Count);
        foreach (TPart part in parts.OrderByDescending(placeOf))
            shots.Add(new(part, replaceShotInPlace(part)));
        shots.Reverse();
        return shots;
    }

    /// <summary>One particle of a starting attack: its part and the shot that plays it.</summary>
    /// <typeparam name="TPart">A particle's part.</typeparam>
    /// <typeparam name="TShot">The shot type.</typeparam>
    /// <param name="Part">The particle's part.</param>
    /// <param name="Shot">The shot that plays it.</param>
    internal readonly partial record struct ParticleShot<TPart, TShot>(TPart Part, TShot Shot);
}
