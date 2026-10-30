using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The Shinespark echo projectile: its spawn and per-frame step.</summary>
internal sealed class ShinesparkEchoProjectile
{
    /// <summary>Animation owner that clears an echo when its installed handler completes.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Live Samus projectile slots, including the two fixed indices reserved for released crash echoes.</summary>
    private readonly ProjectileSlotTable _slotTable;

    /// <summary>Binds projectile animation and slot ownership for Shinespark's departing echoes.</summary>
    internal ShinesparkEchoProjectile(ProjectileAnimation animation, ProjectileSlotTable slotTable)
    {
        _animation = animation;
        _slotTable = slotTable;
    }
    /// <summary>
    /// $90:D40D allocates fixed slots without clearing their unrelated parallel words.
    /// Echo drawing storage remains in Samus independently of this projectile owner.
    /// </summary>
    internal void InitializeShinesparkEcho(int index, RotationAngle angle)
    {
        if (index is not (3 or 4)) throw new ArgumentOutOfRangeException(nameof(index));
        var slot = _slotTable.Slots[index];
        _slotTable.ProjectileCounter = WrappingCounter.IncrementWord(_slotTable.ProjectileCounter);
        slot.Type = SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho);
        BeamCombos.InitializeComboData(slot, BeamComboData.Echo);
        slot.PreInstruction = SamusProjectilePreInstruction.ShinesparkEcho;
        slot.EchoAngle = angle.ToByteAngle();
        slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(0));
    }

    /// <summary>Runs only the live slot's installed $90:D4D2 handler.</summary>
    internal void StepShinesparkEcho(SamusState samus,
        SamusProjectileSlot slot, Position16<RoomPixels> camera)
    {
        if (slot.PreInstruction != SamusProjectilePreInstruction.ShinesparkEcho)
            throw new InvalidOperationException("Departing echo no longer owns this projectile slot.");
        if (!samus.Shinespark.StepOwnedReleasedCrashEcho(samus, camera, slot))
            _animation.ClearProjectile(slot);
    }
}
