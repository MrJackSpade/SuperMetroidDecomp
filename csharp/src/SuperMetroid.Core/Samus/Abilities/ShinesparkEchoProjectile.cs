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
    private readonly ProjectileSlotTable _slotTable;

    /// <summary>Binds projectile animation and the shot pool for Shinespark's departing echoes.</summary>
    internal ShinesparkEchoProjectile(ProjectileAnimation animation, ProjectileSlotTable slotTable)
    {
        _animation = animation;
        _slotTable = slotTable;
    }

    /// <summary>The place the first echo takes: the one before the last.</summary>
    private const int FirstEchoPlace = SamusProjectileSystem.SlotCount - 2;

    /// <summary>The place the second echo takes: the last.</summary>
    private const int SecondEchoPlace = SamusProjectileSystem.SlotCount - 1;

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
        int shots = _slotTable.ProjectileCounter;
        if (shots >= SamusProjectileSystem.SlotCount)
            return false;

        bool firstDeparts = shots < SamusProjectileSystem.SlotCount - 1;
        if (firstDeparts)
            first.Depart(Launch(FirstEchoPlace, angles.First, picturedBeams), crashPosition);
        second.Depart(Launch(SecondEchoPlace, angles.Second, picturedBeams), crashPosition);
        return firstDeparts;
    }

    /// <summary>
    /// Creates a departing echo's shot at radius zero in <paramref name="place"/>, replacing whatever shot holds it, and
    /// counts it.
    /// </summary>
    private SamusProjectile Launch(int place, RotationAngle angle, SamusBeamCombination picturedBeams)
    {
        _slotTable.ProjectileCounter++;
        SamusProjectile replaced = _slotTable.Slots[place];
        Position16<RoomPixels> start = ReleasedEchoStartsAtReplacedShot.Start(
            replaced.IsOccupied ? replaced.Position.Whole : null);
        SamusProjectile echo = _slotTable.Occupy(place);
        echo.Position = echo.Position.WithWhole(start);
        echo.Type = SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho);
        echo.PicturedBeams = picturedBeams;
        BeamCombos.InitializeComboData(echo, BeamComboData.Echo);
        echo.FrameBehavior = SamusProjectileFrameBehavior.ShinesparkEcho;
        echo.EchoAngle = angle.ToTrigonometryAngle();
        return echo;
    }

    /// <summary>Runs one update of the echo <paramref name="slot"/> carries, clearing the shot once the echo leaves the screen.</summary>
    internal void StepShinesparkEcho(SamusState samus,
        SamusProjectile slot, Position16<RoomPixels> camera)
    {
        if (slot.FrameBehavior != SamusProjectileFrameBehavior.ShinesparkEcho)
            throw new InvalidOperationException("Departing echo no longer owns this projectile slot.");
        if (!samus.Shinespark.StepReleasedEcho(samus, camera, slot))
            _animation.ClearProjectile(slot);
    }
}
