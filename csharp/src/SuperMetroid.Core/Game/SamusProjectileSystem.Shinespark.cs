using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    /// <summary>
    /// $90:D40D allocates fixed slots without clearing their unrelated parallel words.
    /// Echo drawing storage remains in Samus independently of this projectile owner.
    /// </summary>
    internal void InitializeShinesparkEcho(ISnesAddressSpace bus, int index, SnesAngle angle)
    {
        if (index is not (3 or 4)) throw new ArgumentOutOfRangeException(nameof(index));
        var slot = _slots[index];
        ProjectileCounter = unchecked((ushort)(ProjectileCounter + 1));
        slot.Type = SamusShinesparkProjectileRomData.EchoType;
        InitializeComboData(bus, slot, ordinary: false, echo: true);
        slot.PreInstruction = SamusProjectilePreInstruction.ShinesparkEcho;
        slot.Variable = angle.TableIndex;
        slot.XVelocity = 0;
    }

    /// <summary>Runs only the live slot's installed $90:D4D2 handler.</summary>
    internal void StepShinesparkEcho(ISnesAddressSpace bus, SamusState samus,
        SamusProjectileSlot slot, ushort cameraX, ushort cameraY)
    {
        if (slot.PreInstruction != SamusProjectilePreInstruction.ShinesparkEcho)
            throw new InvalidOperationException("Departing echo no longer owns this projectile slot.");
        if (!samus.Shinespark.StepOwnedReleasedCrashEcho(samus, cameraX, cameraY, slot))
            ClearProjectile(slot);
    }
}
