namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Samus's shot pool: the places her ordinary projectiles take and the shot count.</summary>
/// <remarks>
/// Place order: Samus has <see cref="SamusProjectileSystem.PlaceCount"/> shot places. A shot takes the lowest place no
/// projectile holds; the projectile update and drawing run in update order, from the highest place down, and enemy
/// collision from the lowest up. With every place held a beam takes the last place over the shot holding it
/// (<see cref="PlaceForBeam"/>). Gameplay names some places: a special attack's particles replace the shots in the first
/// four (<see cref="SpecialAttackReplacesFirstShots"/>), each playing its part (<see cref="SpecialAttackParticleDefinitions"/>); the crash echoes take the last two (<see cref="FirstEchoPlace"/>, <see cref="SecondEchoPlace"/>); an unlinked Super Missile names
/// the first (<see cref="UnlinkedSuperMissileLink"/>); and some enemies react to the first place's projectile or to the
/// highest one.
/// </remarks>
internal sealed class SamusShotPool
{
    /// <summary>The place a shinespark crash's first echo takes: the one before the last.</summary>
    internal const int FirstEchoPlace = SamusProjectileSystem.PlaceCount - 2;

    /// <summary>The place a shinespark crash's second echo takes: the last.</summary>
    internal const int SecondEchoPlace = SamusProjectileSystem.PlaceCount - 1;

    /// <summary>Each place's projectile, live or the last one that ended there; occupancy does not replace <see cref="ShotCount"/>.</summary>
    private readonly SamusProjectile[] _places =
        Enumerable.Range(0, SamusProjectileSystem.PlaceCount).Select(place => new SamusProjectile(place)).ToArray();

    /// <summary>
    /// Starts an empty pool whose places hand their sub-pixel positions on through <paramref name="muzzleCarry"/> and whose
    /// shots are counted by <paramref name="shotCount"/>.
    /// </summary>
    /// <param name="muzzleCarry">What each place keeps for its next projectile's horizontal sub-pixel position.</param>
    /// <param name="shotCount">The shots counted out, which the room's builder creates with the pool.</param>
    internal SamusShotPool(LeftMuzzleSubpixelCarry muzzleCarry, SamusShotCount shotCount)
    {
        MuzzleCarry = Ensure.NotNull(muzzleCarry);
        ShotCount = Ensure.NotNull(shotCount);
    }

    /// <summary>The horizontal sub-pixel position each place hands its next projectile.</summary>
    internal LeftMuzzleSubpixelCarry MuzzleCarry { get; }

    /// <summary>Each place's projectile, in place order.</summary>
    internal IReadOnlyList<SamusProjectile> Shots => _places;

    /// <summary>
    /// The projectile now holding the place <paramref name="projectile"/> took: itself, or one that replaced it. A Spazer
    /// special-attack particle that strikes something while it circles or flies (#1683) ends, besides itself, the shot
    /// holding its partner's place: the partner itself until a shinespark crash echo is released onto that place, when the
    /// echo ends instead. Replacing the partner has already ended it, so the echo is the only other shot ended there, and
    /// the spent particle's hit takes Samus's crash echo with it: a crash echo released while the attack runs takes the
    /// last two places, the first of which the attack's last trailing particle holds.
    /// </summary>
    internal SamusProjectile HolderOfPlaceOf(SamusProjectile projectile) => _places[Ensure.NotNull(projectile).Place];
    /// <summary>The lowest place no projectile holds, which firing and the Super Missile's link take, or null when all are held.</summary>
    internal int? FindFreePlace() => _places.FirstOrDefault(projectile => !projectile.IsOccupied)?.Place;

    /// <summary>
    /// The place a beam takes (#2468): the lowest free one, or with every place held the last, over the shot holding it
    /// (<see cref="BeamFiredOverLastShot"/>).
    /// </summary>
    internal int PlaceForBeam() => BeamFiredOverLastShot.Place(FindFreePlace(), _places.Length);

    /// <summary>
    /// Creates the new beam of <paramref name="place"/> as <see cref="Occupy"/> does, ending the shot there, and gives
    /// it the Power Bomb fuse that shot leaves: none from a free place (<see cref="BeamFiredOverLastShot"/>).
    /// </summary>
    /// <param name="place">The zero-based shot place, which <see cref="PlaceForBeam"/> chose.</param>
    /// <returns>The new beam, holding no damage until its setup gives it some.</returns>
    internal SamusProjectile OccupyWithBeam(int place)
    {
        SamusProjectile replaced = _places[place];
        int fuse = replaced.IsOccupied ? replaced.FuseLeftForBeamFiredOver : 0;
        SamusProjectile beam = Occupy(place);
        beam.BeamFuse = fuse;
        return beam;
    }

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

    /// <summary>How many shots Samus has out as firing counts them, which drifts from the places held.</summary>
    public SamusShotCount ShotCount { get; }
}
