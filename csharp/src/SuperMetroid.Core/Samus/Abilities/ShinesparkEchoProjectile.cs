using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The shots a finishing shinespark crash releases its echoes on: their release and their update.</summary>
/// <remarks>
/// The echoes take the shot pool's last two places, the first echo the place before the last and the second the last,
/// whatever shots those places hold. The place matters to the game: shots update, meet enemies and draw in place order,
/// an echo replaces the shot it lands on, and only the first echo's height carries into the next crash's orbit
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
    private SamusProjectileSlot FirstEchoPlace => _slotTable.Slots[SamusProjectileSystem.SlotCount - 2];

    /// <summary>The place the second echo takes: the last.</summary>
    private SamusProjectileSlot SecondEchoPlace => _slotTable.Slots[SamusProjectileSystem.SlotCount - 1];

    /// <summary>
    /// Releases a finishing crash's echoes as far as Samus's shot count leaves room: the second echo while the count
    /// is below the pool's size, and the first only while it is below that by two, so both are released, only the
    /// second, or neither. The count is compared as its signed difference from each bound, and each released echo adds
    /// one to it. An echo that is not released leaves its place and its last shot as they are.
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
        if (CoordinateAxis.Difference(shots, SamusProjectileSystem.SlotCount) >= 0)
            return false;

        bool firstDeparts = CoordinateAxis.Difference(shots, SamusProjectileSystem.SlotCount - 1) < 0;
        if (firstDeparts)
            first.Depart(Launch(FirstEchoPlace, angles.First, picturedBeams), crashPosition);
        second.Depart(Launch(SecondEchoPlace, angles.Second, picturedBeams), crashPosition);
        return firstDeparts;
    }

    /// <summary>Turns <paramref name="place"/> into a departing echo's shot at radius zero and counts it.</summary>
    private SamusProjectileSlot Launch(SamusProjectileSlot place, RotationAngle angle, SamusBeamCombination picturedBeams)
    {
        _slotTable.ProjectileCounter = WrappingCounter.IncrementCount(_slotTable.ProjectileCounter);
        place.Type = SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho);
        place.PicturedBeams = picturedBeams;
        BeamCombos.InitializeComboData(place, BeamComboData.Echo);
        place.FrameBehavior = SamusProjectileFrameBehavior.ShinesparkEcho;
        place.EchoAngle = angle.ToTrigonometryAngle();
        place.EchoRadius = 0;
        return place;
    }

    /// <summary>Runs one update of the echo <paramref name="slot"/> carries, clearing the shot once the echo leaves the screen.</summary>
    internal void StepShinesparkEcho(SamusState samus,
        SamusProjectileSlot slot, Position16<RoomPixels> camera)
    {
        if (slot.FrameBehavior != SamusProjectileFrameBehavior.ShinesparkEcho)
            throw new InvalidOperationException("Departing echo no longer owns this projectile slot.");
        if (!samus.Shinespark.StepReleasedEcho(samus, camera, slot))
            _animation.ClearProjectile(slot);
    }
}
