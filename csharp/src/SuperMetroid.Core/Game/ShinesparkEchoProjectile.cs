using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The Shinespark echo projectile: its spawn and per-frame step.</summary>
internal sealed class ShinesparkEchoProjectile
{
    private readonly ProjectileAnimation _animation;
    private readonly ProjectileSlotTable _slotTable;

    internal ShinesparkEchoProjectile(ProjectileAnimation animation, ProjectileSlotTable slotTable)
    {
        _animation = animation;
        _slotTable = slotTable;
    }
    /// <summary>
    /// $90:D40D allocates fixed slots without clearing their unrelated parallel words.
    /// Echo drawing storage remains in Samus independently of this projectile owner.
    /// </summary>
    internal void InitializeShinesparkEcho(int index, SnesAngle angle)
    {
        if (index is not (3 or 4)) throw new ArgumentOutOfRangeException(nameof(index));
        var slot = _slotTable._slots[index];
        _slotTable.ProjectileCounter = unchecked((ushort)(_slotTable.ProjectileCounter + 1));
        slot.Type = SamusShinesparkProjectileRomData.EchoType;
        BeamCombos.InitializeComboData(slot, ordinary: false, echo: true);
        slot.PreInstruction = SamusProjectilePreInstruction.ShinesparkEcho;
        slot.Variable = angle.TableIndex;
        slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(0));
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
