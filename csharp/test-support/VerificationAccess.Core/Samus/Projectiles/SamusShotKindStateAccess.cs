namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A check's reach to the state a shot's kind keeps (#2554), which production hands only to the shot's own behaviour.</summary>
internal static class SamusShotKindStateAccess
{
    /// <summary>The kind state <paramref name="shot"/>'s occupant keeps, or none for a free place, a beam or an explosion.</summary>
    internal static SamusShotKindState? KindStateOrNone(this SamusProjectile shot) => Ensure.NotNull(shot).Occupant?.KindState;

    /// <summary>
    /// Whether <paramref name="shot"/> is a Super Missile's link (#2554): a Super Missile naming the place every link names
    /// (<see cref="Compatibility.UnlinkedSuperMissileLink"/>), whose own place another Super Missile, its owner, names.
    /// </summary>
    /// <param name="shot">The shot.</param>
    /// <param name="pool">Every shot place.</param>
    internal static bool IsSuperMissileLink(this SamusProjectile shot, IEnumerable<SamusProjectile> pool) =>
        shot.KindStateOrNone() is SuperMissileShotState { LinkChainPosition: Compatibility.UnlinkedSuperMissileLink.Place }
        && pool.Any(other => !ReferenceEquals(other, shot)
            && other.KindStateOrNone() is SuperMissileShotState { LinkChainPosition: { } named } && named == shot.Place);
}
