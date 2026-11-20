using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileSelectionDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which projectile kind a beam shot, missile, bomb, special attack or echo is.</summary>
internal static class SamusProjectileSelection
{
    /// <summary>
    /// The kind a shot of <paramref name="beams"/> is, charged when <paramref name="charged"/>. Each Spazer and Plasma
    /// combination fires the kind <see cref="UnauthoredBeamShots{TBeams, TProjectile}"/> gives it.
    /// </summary>
    internal static SamusProjectileKind BeamShot(SamusBeamCombination beams, bool charged) => beams.CanBeEquipped
        ? (charged ? ChargedShots : UnchargedShots)[beams]
        : UnauthoredShots.Shot(beams, charged);

    /// <summary>The kind of each equippable combination's charged shot, checked as it loads to name every one.</summary>
    private static IReadOnlyDictionary<SamusBeamCombination, SamusProjectileKind> ChargedShots { get; } = EveryEquippable(ChargedBeamShots);

    /// <summary>The kind of each equippable combination's uncharged shot, checked as it loads to name every one.</summary>
    private static IReadOnlyDictionary<SamusBeamCombination, SamusProjectileKind> UnchargedShots { get; } = EveryEquippable(UnchargedBeamShots);

    /// <summary>
    /// The animation a shot of <paramref name="beams"/>, charged when <paramref name="charged"/>, runs fired in
    /// <paramref name="direction"/>, or null for none: its kind's animation for the direction, except the Murder Beam's
    /// own per-direction animations (<see cref="MurderBeamDirectionAnimations{TDirection, TAnimation}"/>).
    /// </summary>
    internal static SamusProjectileSegment? BeamShotAnimation(SamusBeamCombination beams, bool charged, SamusProjectileDirection direction) =>
        IsMurderBeam(beams, charged)
            ? MurderBeamAnimations.Animation(direction)
            : SamusProjectileKinds.Animation(BeamShot(beams, charged), direction);

    /// <summary>Whether a shot of <paramref name="beams"/>, charged when <paramref name="charged"/>, is the Murder Beam's.</summary>
    internal static bool IsMurderBeam(SamusBeamCombination beams, bool charged) => charged && beams == MurderBeam;

    /// <summary>The kind a fired missile is: a Super Missile when <paramref name="super"/>, else a missile.</summary>
    internal static SamusProjectileKind Missile(bool super) => super ? SamusProjectileKind.SuperMissile : SamusProjectileKind.Missile;

    /// <summary>The kind a placed bomb of <paramref name="kind"/> is.</summary>
    internal static SamusProjectileKind Bomb(SamusBombKind kind) => Bombs[kind];

    /// <summary>
    /// The kind of the particles of the special attack <paramref name="beams"/> fires, or null for a combination whose attack
    /// has no particles of its own.
    /// </summary>
    /// <exception cref="InvalidOperationException">The combination pairs Spazer with Plasma, which fires no special attack.</exception>
    internal static SamusProjectileKind? SpecialAttack(SamusBeamCombination beams) =>
        !beams.CanBeEquipped ? throw new InvalidOperationException($"{beams} fires no special attack.")
            : SpecialAttacks.TryGetValue(beams, out SamusProjectileKind kind) ? kind : null;

    /// <summary>The kind <paramref name="echo"/> is.</summary>
    internal static SamusProjectileKind Echo(SamusBeamEcho echo) => Echoes[echo];

    /// <summary>Returns <paramref name="shots"/> once it names a kind for every combination Samus can equip.</summary>
    /// <exception cref="InvalidDataException">An equippable combination has no kind.</exception>
    private static IReadOnlyDictionary<SamusBeamCombination, SamusProjectileKind> EveryEquippable(
        IReadOnlyDictionary<SamusBeamCombination, SamusProjectileKind> shots)
    {
        foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>())
        {
            if (beams.CanBeEquipped && !shots.ContainsKey(beams))
                throw new InvalidDataException($"The equippable combination {beams} fires no shot kind.");
        }
        return shots;
    }
}
