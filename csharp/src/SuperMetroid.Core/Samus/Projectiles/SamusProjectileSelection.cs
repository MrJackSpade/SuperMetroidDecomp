using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileSelectionDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which projectile definition a beam shot, missile, bomb, special attack or echo starts from.</summary>
internal static class SamusProjectileSelection
{
    /// <summary>
    /// The definition a shot of <paramref name="beams"/> starts from, charged when <paramref name="charged"/>. A Spazer and
    /// Plasma combination fires what the game selects past the beam selectors (<see cref="UnauthoredBeamShots{TBeams, TProjectile}"/>).
    /// </summary>
    internal static SamusProjectileHeader BeamShot(SamusBeamCombination beams, bool charged) => beams.IsRetail
        ? Lookup(charged ? ChargedBeamShots : UnchargedBeamShots, beams)
        : UnauthoredShots.Shot(beams, charged);

    /// <summary>
    /// The animation a shot of <paramref name="beams"/>, charged when <paramref name="charged"/>, runs fired in
    /// <paramref name="direction"/>, or null for none: its definition's list for the direction, except the Murder Beam's,
    /// which reads past the Power Bomb definition it takes its damage from (<see cref="MurderBeamDirectionPrograms{TDirection, TProgram}"/>).
    /// </summary>
    internal static SamusProjectileProgram? BeamShotProgram(SamusBeamCombination beams, bool charged, SamusProjectileDirection direction) =>
        IsMurderBeam(beams, charged)
            ? MurderBeamPrograms.Program(direction)
            : SamusProjectileHeaders.Program(BeamShot(beams, charged), direction);

    /// <summary>Whether a shot of <paramref name="beams"/>, charged when <paramref name="charged"/>, is the Murder Beam's.</summary>
    internal static bool IsMurderBeam(SamusBeamCombination beams, bool charged) => charged && beams == MurderBeam;

    /// <summary>The definition a fired missile or placed bomb of <paramref name="family"/> starts from.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No missile or bomb is of the family.</exception>
    internal static SamusProjectileHeader NonBeam(SamusProjectileFamily family) => Lookup(NonBeamProjectiles, family);

    /// <summary>
    /// The definition the particles of the special attack <paramref name="beams"/> fires start from, or null for a combination
    /// whose attack has no particle definition.
    /// </summary>
    /// <exception cref="InvalidOperationException">The combination pairs Spazer with Plasma, which fires no special attack.</exception>
    internal static SamusProjectileHeader? SpecialAttack(SamusBeamCombination beams) =>
        !beams.IsRetail ? throw new InvalidOperationException($"{beams} fires no special attack.")
            : SpecialAttacks.TryGetValue(beams, out SamusProjectileHeader header) ? header : null;

    /// <summary>The definition <paramref name="echo"/> starts from.</summary>
    internal static SamusProjectileHeader Echo(SamusBeamEcho echo) => Lookup(Echoes, echo);

    /// <summary>The definition <paramref name="selections"/> names for <paramref name="key"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The selections name no definition for the key.</exception>
    private static SamusProjectileHeader Lookup<TKey>(IReadOnlyDictionary<TKey, SamusProjectileHeader> selections, TKey key)
        where TKey : notnull =>
        selections.TryGetValue(key, out SamusProjectileHeader header)
            ? header
            : throw new ArgumentOutOfRangeException(nameof(key), key, "No projectile header is selected for this key.");
}
