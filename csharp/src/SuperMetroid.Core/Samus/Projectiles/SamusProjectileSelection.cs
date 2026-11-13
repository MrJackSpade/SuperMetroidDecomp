using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileSelectionDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which projectile kind a beam shot, missile, bomb, special attack or echo is.</summary>
internal static class SamusProjectileSelection
{
    /// <summary>
    /// The kind a shot of <paramref name="beams"/> is, charged when <paramref name="charged"/>. A Spazer and
    /// Plasma combination fires what the game selects past the beam selectors (<see cref="UnauthoredBeamShots{TBeams, TProjectile}"/>).
    /// </summary>
    internal static SamusProjectileKind BeamShot(SamusBeamCombination beams, bool charged) => beams.IsRetail
        ? Lookup(charged ? ChargedBeamShots : UnchargedBeamShots, beams)
        : UnauthoredShots.Shot(beams, charged);

    /// <summary>
    /// The animation a shot of <paramref name="beams"/>, charged when <paramref name="charged"/>, runs fired in
    /// <paramref name="direction"/>, or null for none: its kind's program for the direction, except the Murder Beam's,
    /// which reads past the Power Bomb kind it takes its damage from (<see cref="MurderBeamDirectionAnimations{TDirection, TAnimation}"/>).
    /// </summary>
    internal static SamusProjectileSegment? BeamShotAnimation(SamusBeamCombination beams, bool charged, SamusProjectileDirection direction) =>
        IsMurderBeam(beams, charged)
            ? MurderBeamAnimations.Animation(direction)
            : SamusProjectileKinds.Animation(BeamShot(beams, charged), direction);

    /// <summary>Whether a shot of <paramref name="beams"/>, charged when <paramref name="charged"/>, is the Murder Beam's.</summary>
    internal static bool IsMurderBeam(SamusBeamCombination beams, bool charged) => charged && beams == MurderBeam;

    /// <summary>The kind a fired missile or placed bomb of <paramref name="family"/> is.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No missile or bomb is of the family.</exception>
    internal static SamusProjectileKind NonBeam(SamusProjectileFamily family) => Lookup(NonBeamProjectiles, family);

    /// <summary>
    /// The kind of the particles of the special attack <paramref name="beams"/> fires, or null for a combination whose attack
    /// has no particles of its own.
    /// </summary>
    /// <exception cref="InvalidOperationException">The combination pairs Spazer with Plasma, which fires no special attack.</exception>
    internal static SamusProjectileKind? SpecialAttack(SamusBeamCombination beams) =>
        !beams.IsRetail ? throw new InvalidOperationException($"{beams} fires no special attack.")
            : SpecialAttacks.TryGetValue(beams, out SamusProjectileKind kind) ? kind : null;

    /// <summary>The kind <paramref name="echo"/> is.</summary>
    internal static SamusProjectileKind Echo(SamusBeamEcho echo) => Lookup(Echoes, echo);

    /// <summary>The kind <paramref name="selections"/> names for <paramref name="key"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The selections name no kind for the key.</exception>
    private static SamusProjectileKind Lookup<TKey>(IReadOnlyDictionary<TKey, SamusProjectileKind> selections, TKey key)
        where TKey : notnull =>
        selections.TryGetValue(key, out SamusProjectileKind kind)
            ? kind
            : throw new ArgumentOutOfRangeException(nameof(key), key, "No projectile kind is selected for this key.");
}
