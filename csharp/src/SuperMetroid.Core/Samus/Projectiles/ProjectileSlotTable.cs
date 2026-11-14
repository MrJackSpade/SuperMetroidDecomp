namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Samus's shot pool: the places her ordinary projectiles take and the shot count.</summary>
/// <remarks>
/// Place order: Samus has <see cref="SamusProjectileSystem.SlotCount"/> shot places. A shot takes the lowest place no
/// projectile holds; the projectile update and drawing run in update order, from the highest place down, and enemy
/// collision from the lowest up. Gameplay names some places: a special attack's particles replace the shots in the first
/// four (<see cref="SpecialAttackReplacesFirstShots"/>), each playing its part (<see cref="SpecialAttackParticleDefinitions"/>); the crash echoes take the last two; an unlinked Super Missile names
/// the first (<see cref="UnlinkedSuperMissileLink"/>); and some enemies react to the first place's projectile or to the
/// highest one.
/// </remarks>
internal sealed class ProjectileSlotTable
{
    /// <summary>Each place's projectile, live or the last one that ended there; occupancy does not replace the shot count.</summary>
    private readonly SamusProjectile[] _places =
        Enumerable.Range(0, SamusProjectileSystem.SlotCount).Select(place => new SamusProjectile(place)).ToArray();

    /// <summary>Starts an empty pool whose places hand their sub-pixel positions on through <paramref name="muzzleCarry"/>.</summary>
    internal ProjectileSlotTable(LeftMuzzleSubpixelCarry muzzleCarry) => MuzzleCarry = Ensure.NotNull(muzzleCarry);

    /// <summary>The horizontal sub-pixel position each place hands its next projectile.</summary>
    internal LeftMuzzleSubpixelCarry MuzzleCarry { get; }

    /// <summary>Each place's projectile, in place order.</summary>
    internal IReadOnlyList<SamusProjectile> Slots => _places;

    /// <summary>The projectile now holding the place <paramref name="projectile"/> took: itself, or one that replaced it.</summary>
    internal SamusProjectile HolderOfPlaceOf(SamusProjectile projectile) => _places[Ensure.NotNull(projectile).Place];
    /// <summary>The lowest place no projectile holds, which firing and the Super Missile's link take, or null when all are held.</summary>
    internal int? FindFreePlace() => _places.FirstOrDefault(projectile => !projectile.IsOccupied)?.Place;

    /// <summary>
    /// Creates the new projectile of <paramref name="place"/>, ending the one there. It starts at the room's origin with
    /// the horizontal sub-pixel position the place hands on: the replaced projectile's when it still held the place,
    /// otherwise what the place kept (<see cref="LeftMuzzleSubpixelCarry"/>).
    /// </summary>
    /// <param name="place">The zero-based shot place.</param>
    /// <returns>The new projectile, holding no damage until its setup gives it some.</returns>
    internal SamusProjectile Occupy(int place)
    {
        SamusProjectile replaced = _places[place];
        int kept = MuzzleCarry.Start(place);
        int fraction = replaced.IsOccupied ? replaced.Position.X.Fraction : kept;
        SamusProjectile projectile = OccupyAtRest(place);
        projectile.Position = projectile.Position.WithFractionX(fraction);
        return projectile;
    }

    /// <summary>
    /// Creates the new projectile of <paramref name="place"/> at rest at the room's origin, ending the one there; the
    /// place keeps no sub-pixel position for it. The Super Missile's link starts so.
    /// </summary>
    /// <param name="place">The zero-based shot place.</param>
    /// <returns>The new projectile, holding no damage until its setup gives it some.</returns>
    internal SamusProjectile OccupyAtRest(int place)
    {
        _ = MuzzleCarry.Start(place);
        _places[place].End();
        var projectile = new SamusProjectile(place);
        _places[place] = projectile;
        return projectile;
    }

    /// <summary>
    /// Clears Samus's shots all at once: each place a projectile holds keeps that projectile's horizontal sub-pixel
    /// position for its next one, and every projectile ends.
    /// </summary>
    internal void ClearAll()
    {
        foreach (SamusProjectile projectile in _places)
        {
            if (projectile.IsOccupied)
                MuzzleCarry.Keep(projectile.Place, projectile.Position.X.Fraction);
            projectile.End();
        }
    }

    /// <summary>
    /// The horizontal sub-pixel position <paramref name="place"/> would keep if Samus's shots were cleared now: its
    /// projectile's while one holds it, otherwise what it already keeps.
    /// </summary>
    /// <param name="place">The zero-based shot place.</param>
    internal int SubpixelKeptOnClear(int place)
    {
        SamusProjectile projectile = _places[place];
        return projectile.IsOccupied ? projectile.Position.X.Fraction : MuzzleCarry.Kept(place);
    }

    /// <summary>Each place's projectile in update order, highest place first.</summary>
    internal IEnumerable<SamusProjectile> InUpdateOrder
    {
        get
        {
            for (int place = _places.Length - 1; place >= 0; place--)
                yield return _places[place];
        }
    }

    /// <summary>
    /// How many shots Samus has out by the firing count, zero or more; kept separately from the free-place
    /// search, so the two can disagree (a special attack sets it to four whatever its fifth place holds), and a deletion
    /// at zero leaves it at zero.
    /// </summary>
    public int ProjectileCounter
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
}
