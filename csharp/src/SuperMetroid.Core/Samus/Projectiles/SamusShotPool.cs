using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Samus's shot pool: the places her ordinary projectiles take and the shot count.</summary>
/// <remarks>
/// Place order: Samus has <see cref="SamusProjectileSystem.PlaceCount"/> shot places. A shot takes the lowest place no
/// projectile holds; the projectile update and drawing run in update order, from the highest place down, and enemy
/// collision from the lowest up. With every place held a beam takes the last place over the shot holding it
/// (<see cref="PlaceForBeam"/>). Gameplay names some places: a special attack's particles replace the shots in the first
/// four (<see cref="SpecialAttackReplacesFirstShots"/>), each playing its part (<see cref="SpecialAttackParticleDefinitions"/>); the crash echoes take the last two (<see cref="ShinesparkCrashEchoPlaces"/>); an unlinked Super Missile names
/// the first (<see cref="UnlinkedSuperMissileLink"/>); and some enemies react to the first place's projectile or to the
/// highest one.
/// </remarks>
internal sealed class SamusShotPool
{
    /// <summary>Each place's projectile, live or the last one that ended there; occupancy does not replace <see cref="ShotCount"/>.</summary>
    private readonly SamusProjectile[] _places =
        Enumerable.Range(0, SamusProjectileSystem.PlaceCount).Select(place => new SamusProjectile(place)).ToArray();

    /// <summary>
    /// Starts an empty pool whose places hand their fractions of a pixel on through <paramref name="leftBeamStart"/> and whose
    /// shots are counted by <paramref name="shotCount"/>.
    /// </summary>
    /// <param name="leftBeamStart">What each place keeps for its next projectile's horizontal fraction of a pixel.</param>
    /// <param name="shotCount">The shots counted out, which the room's builder creates with the pool.</param>
    internal SamusShotPool(LeftBeamStartFromLastShot leftBeamStart, SamusShotCount shotCount)
    {
        LeftBeamStart = Ensure.NotNull(leftBeamStart);
        ShotCount = Ensure.NotNull(shotCount);
    }

    /// <summary>The fraction of a pixel each place keeps from its last shot, which decides where a beam fired to the left starts.</summary>
    internal LeftBeamStartFromLastShot LeftBeamStart { get; }

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
    /// Creates the new beam of <paramref name="type"/> dealing <paramref name="damage"/> in <paramref name="place"/> as
    /// <see cref="Occupy"/> does, ending the shot there, and gives it the Power Bomb fuse that shot leaves: none from a
    /// free place (<see cref="BeamFiredOverLastShot"/>).
    /// </summary>
    /// <param name="place">The zero-based shot place, which <see cref="PlaceForBeam"/> chose.</param>
    /// <param name="type">What the beam is.</param>
    /// <param name="damage">The damage the beam deals, one or more.</param>
    /// <returns>The new beam.</returns>
    internal SamusProjectileOccupant OccupyWithBeam(int place, SamusProjectileType type, int damage)
    {
        int fuse = BeamFiredOverLastShot.FuseLeftBy(_places[place].AsReplacedByBeam);
        SamusProjectileOccupant beam = Occupy(place, type, damage);
        beam.BeamFuse = UpdateCountdown.For(fuse);
        return beam;
    }

    /// <summary>
    /// Creates the new projectile of <paramref name="type"/> dealing <paramref name="damage"/> in <paramref name="place"/>,
    /// ending the one there. It starts at the room's origin with the horizontal fraction of a pixel the place hands on: the
    /// replaced projectile's when it still held the place, otherwise what the place kept (<see cref="LeftBeamStartFromLastShot"/>).
    /// </summary>
    /// <param name="place">The zero-based shot place.</param>
    /// <param name="type">What the projectile is.</param>
    /// <param name="damage">The damage the projectile deals, one or more.</param>
    /// <returns>The new projectile.</returns>
    internal SamusProjectileOccupant Occupy(int place, SamusProjectileType type, int damage)
    {
        PixelFraction kept = LeftBeamStart.Start(place);
        PixelFraction fraction = _places[place].Occupant is { } replaced ? replaced.Position.X.FractionOfPixel : kept;
        return OccupyAtRest(place, type, damage, FixedPosition<RoomPixels>.Zero.WithFractionX(fraction));
    }

    /// <summary>
    /// Creates the new projectile of <paramref name="type"/> dealing <paramref name="damage"/> in <paramref name="place"/>
    /// at rest at the room's origin, ending the one there; the place keeps no fraction of a pixel for it. The Super
    /// Missile's link starts so.
    /// </summary>
    /// <param name="place">The zero-based shot place.</param>
    /// <param name="type">What the projectile is.</param>
    /// <param name="damage">The damage the projectile deals, one or more.</param>
    /// <returns>The new projectile.</returns>
    internal SamusProjectileOccupant OccupyAtRest(int place, SamusProjectileType type, int damage) =>
        OccupyAtRest(place, type, damage, FixedPosition<RoomPixels>.Zero);

    /// <summary>
    /// Creates the new projectile of <paramref name="type"/> dealing <paramref name="damage"/> in <paramref name="place"/>
    /// at <paramref name="position"/>, ending the one there; the place keeps no fraction of a pixel after it.
    /// </summary>
    private SamusProjectileOccupant OccupyAtRest(int place, SamusProjectileType type, int damage, FixedPosition<RoomPixels> position)
    {
        _ = LeftBeamStart.Start(place);
        _places[place].End();
        var projectile = new SamusProjectile(place);
        _places[place] = projectile;
        return projectile.Occupy(type, damage, position);
    }

    /// <summary>
    /// Clears Samus's shots all at once: each place a projectile holds keeps that projectile's horizontal fraction of a
    /// pixel for its next one, and every projectile ends.
    /// </summary>
    internal void ClearAll()
    {
        foreach (SamusProjectile projectile in _places)
        {
            if (projectile.Occupant is { } held)
                LeftBeamStart.Keep(projectile.Place, held.Position.X.FractionOfPixel);
            projectile.End();
        }
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
