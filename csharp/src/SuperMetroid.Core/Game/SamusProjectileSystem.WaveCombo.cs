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
        slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.Y.SignedRawValue - 1))));
        if (slot.Velocity.Y.SignedRawValue <= 0)
        {
            ClearProjectile(slot);
            return SamusComboRomData.WaveRemovalSound;
        }
        short oldXSpeed = slot.Velocity.X.SignedRawValue;
        ushort oldTimer = slot.TrailTimer;
        slot.TrailTimer = unchecked((ushort)(oldTimer - 1));
        if (oldTimer == 1)
        {
            slot.TrailTimer = 4;
            SpawnTrail(bus, slot);
        }
        slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(AccelerateWaveAxis(slot.Velocity.X.SignedRawValue, unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)))));
        uint x = unchecked(new SnesFixedPosition(slot.Position.X.Whole, slot.Position.X.Fraction).RawValue + (uint)(SnesSixteenSixteen.FromEightEight(slot.Velocity.X.SignedRawValue)));
        slot.Position = slot.Position.WithWholeX(SnesFixedPosition.WholeOf(unchecked((int)x))).WithFractionX(unchecked((ushort)x));
        short ySpeed = AccelerateWaveAxis(unchecked((short)slot.Variable), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)));
        slot.Variable = unchecked((ushort)ySpeed);
        uint y = unchecked(new SnesFixedPosition(slot.Position.Y.Whole, slot.Position.Y.Fraction).RawValue + (uint)(SnesSixteenSixteen.FromEightEight(ySpeed)));
        slot.Position = slot.Position.WithWholeY(SnesFixedPosition.WholeOf(unchecked((int)y))).WithFractionY(unchecked((ushort)y));
        shared.SetSharedCooldown(2);
        FlareCounter = 0;
        return slot.SlotIndex == 3 && (oldXSpeed < 0) != (slot.Velocity.X.SignedRawValue < 0) ? SamusComboRomData.WavePulseSound : (ushort)0;
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
