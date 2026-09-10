using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    /// <summary>Runs $90:DA08's Wave combo homing oscillator, preserving 8.8 acceleration and 16.16 position carry.</summary>
    internal ushort StepWaveCombo(ISnesAddressSpace bus, SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared)
    {
        if (slot.PreInstruction != SamusProjectilePreInstruction.WaveCombo)
            throw new InvalidOperationException("Wave update requires its own pre-instruction.");
        // A hit skips even the lifetime decrement. Expiry and collision both queue
        // the terminal sound, unlike Ice's silent outward-phase removal.
        if ((slot.Direction & 0xf0) != 0)
        {
            ClearProjectile(slot);
            return SamusComboRomData.WaveRemovalSound;
        }
        slot.YVelocity = unchecked((short)(slot.YVelocity - 1));
        if (slot.YVelocity <= 0)
        {
            ClearProjectile(slot);
            return SamusComboRomData.WaveRemovalSound;
        }
        short oldXSpeed = slot.XVelocity;
        ushort oldTimer = slot.TrailTimer;
        slot.TrailTimer = unchecked((ushort)(oldTimer - 1));
        if (oldTimer == 1)
        {
            slot.TrailTimer = 4;
            SpawnTrail(bus, slot);
        }
        slot.XVelocity = AccelerateWaveAxis(slot.XVelocity, unchecked((short)(samus.XPosition - slot.XPosition)));
        uint x = unchecked(((uint)slot.XPosition << 16) + slot.XSubposition + (uint)(slot.XVelocity << 8));
        slot.XPosition = (ushort)(x >> 16); slot.XSubposition = unchecked((ushort)x);
        short ySpeed = AccelerateWaveAxis(unchecked((short)slot.Variable), unchecked((short)(samus.YPosition - slot.YPosition)));
        slot.Variable = unchecked((ushort)ySpeed);
        uint y = unchecked(((uint)slot.YPosition << 16) + slot.YSubposition + (uint)(ySpeed << 8));
        slot.YPosition = (ushort)(y >> 16); slot.YSubposition = unchecked((ushort)y);
        shared.SetSharedCooldown(2);
        FlareCounter = 0;
        return slot.SlotIndex == 3 && (oldXSpeed < 0) != (slot.XVelocity < 0) ? SamusComboRomData.WavePulseSound : (ushort)0;
    }

    private static short AccelerateWaveAxis(short speed, short targetDelta)
    {
        if (targetDelta < 0)
            return unchecked((short)(speed + SamusComboRomData.WaveSpeedLimit - 1)) >= 0 ?
                unchecked((short)(speed - SamusComboRomData.WaveAcceleration)) : speed;
        return unchecked((short)(speed - SamusComboRomData.WaveSpeedLimit)) < 0 ?
            unchecked((short)(speed + SamusComboRomData.WaveAcceleration)) : speed;
    }
}
