using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The shots a finishing shinespark crash releases its echoes on: their release and their update.</summary>
/// <remarks>
/// The echoes take the shot pool's last two places, the first echo the place before the last and the second the last,
/// whatever shots those places hold. The place matters to the game: shots update, meet enemies and draw in place order,
/// an echo replaces the shot it lands on and stands where that shot was until its first update
/// (<see cref="ReleasedEchoStartsAtReplacedShot"/>), and only the first echo's height carries into the next crash's orbit
/// (<see cref="CrashOrbitTravelCarryover"/>).
/// </remarks>
internal sealed class ShinesparkEchoProjectile
{
    /// <summary>Animation owner that clears an echo's shot when the echo leaves the screen.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Samus's shots, whose count admits the echoes and whose last two places they take.</summary>
    private readonly SamusShotPool _pool;

    /// <summary>Binds projectile animation and the shot pool for Shinespark's departing echoes.</summary>
    internal ShinesparkEchoProjectile(ProjectileAnimation animation, SamusShotPool pool)
    {
        _animation = animation;
        _pool = pool;
    }

/// <summary>
    /// Releases a finishing crash's echoes as far as Samus's shot count leaves room: the second echo while the count
    /// is below the pool's size, and the first only while it is below that by two, so both are released, only the
    /// second, or neither. Each released echo adds one to the count. An echo that is not released leaves its place and
    /// its last shot as they are.
    /// </summary>
    /// <param name="first">The crash's first echo.</param>
    /// <param name="second">The crash's second echo.</param>
    /// <param name="angles">The angles the two echoes depart at.</param>
    /// <param name="picturedBeams">The beams Samus has equipped as the crash ends, whose pictures the echoes show.</param>
    /// <param name="crashPosition">Samus's position as the crash finishes.</param>
    /// <returns>Whether the first echo departed.</returns>
    internal bool ReleaseCrashEchoes(ShinesparkCrashEcho first, ShinesparkCrashEcho second,
        ShinesparkDepartureAngles angles, SamusBeamCombination picturedBeams, Position16<RoomPixels> crashPosition)
    {
        int shots = _pool.ShotCount.Count;
        if (shots >= SamusProjectileSystem.PlaceCount)
            return false;

        bool firstDeparts = shots < SamusProjectileSystem.PlaceCount - 1;
        if (firstDeparts)
            Launch(first, ShinesparkCrashEchoPlaces.First(SamusProjectileSystem.PlaceCount), angles.First, picturedBeams, crashPosition);
        Launch(second, ShinesparkCrashEchoPlaces.Second(SamusProjectileSystem.PlaceCount), angles.Second, picturedBeams, crashPosition);
        return firstDeparts;
    }

    /// <summary>
    /// Creates a departing echo's shot at radius zero in <paramref name="place"/>, replacing whatever shot holds it, counts
    /// it, and departs <paramref name="released"/> on it from <paramref name="crashPosition"/>.
    /// </summary>
    private void Launch(ShinesparkCrashEcho released, int place, RotationAngle angle, SamusBeamCombination picturedBeams,
        Position16<RoomPixels> crashPosition)
    {
        _pool.ShotCount.CountShot();
        SamusProjectile replaced = _pool.Shots[place];
        Position16<RoomPixels> start = ReleasedEchoStartsAtReplacedShot.Start(
            replaced.IsOccupied ? replaced.Position.Whole : null);
        var shotKind = new BeamComboShotKind.EchoShot(SamusBeamEcho.ShinesparkEcho);
        SamusProjectileOccupant held = BeamCombos.OccupyComboShot(_pool, place, SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho), shotKind);
        SamusProjectile echo = held.Projectile;
        held.Position = echo.Position.WithWhole(start);
        held.PicturedBeams = picturedBeams;
        BeamCombos.StartComboShot(held, shotKind);
        var flight = new ShinesparkEchoShotState(angle.ToTrigonometryAngle(), released);
        held.Install(SamusShotBehaviors.ShinesparkEcho, flight);
        Ensure.NotNull(released).Depart(echo, flight, crashPosition);
    }

    /// <summary>
    /// Runs one update of the echo <paramref name="shot"/> carries in its <paramref name="flight"/>, clearing the shot once
    /// the echo leaves the screen; the echo's flight runs it (<see cref="ShinesparkEchoFlight"/>).
    /// </summary>
    internal void StepShinesparkEcho(SamusState samus,
        SamusProjectileOccupant shot, ShinesparkEchoShotState flight, Position16<RoomPixels> camera)
    {
        if (!samus.Shinespark.StepReleasedEcho(samus, camera, shot, flight))
            _animation.ClearProjectile(shot.Projectile);
    }
}
