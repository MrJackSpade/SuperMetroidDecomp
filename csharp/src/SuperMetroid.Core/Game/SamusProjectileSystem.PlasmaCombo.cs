using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    /// <summary>Original $90:D793 Plasma ring update; phase is stored in the slot's native Y-speed word.</summary>
    internal void StepPlasmaCombo(ISnesAddressSpace bus, SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared, ushort cameraX, ushort cameraY)
    {
        if (slot.PreInstruction != SamusProjectilePreInstruction.PlasmaCombo)
            throw new InvalidOperationException("Plasma update requires its own pre-instruction.");
        if ((slot.Direction & 0xf0) != 0)
        {
            ClearProjectile(slot);
            return;
        }
        shared.SetSharedCooldown(2);
        FlareCounter = 0;
        var (x, y) = ComboSineOffset(bus, slot.Variable, unchecked((ushort)slot.XVelocity));
        slot.XPosition = unchecked((ushort)(x + samus.XPosition));
        slot.YPosition = unchecked((ushort)(y + samus.YPosition));
        slot.Variable = unchecked((byte)(slot.Variable + ComboState));
        switch (slot.YVelocity)
        {
            case 0:
                slot.XVelocity = unchecked((byte)(slot.XVelocity + SamusComboRomData.PlasmaRadiusStep));
                if (slot.XVelocity >= SamusComboRomData.PlasmaOuterRadius) slot.YVelocity = 1;
                break;
            case 1:
                slot.XVelocity = unchecked((byte)(slot.XVelocity - SamusComboRomData.PlasmaRadiusStep));
                if (slot.XVelocity < SamusComboRomData.PlasmaInnerThreshold) slot.YVelocity = 2;
                break;
            case 2:
                // Only the final expansion checks visibility. The first two phases
                // intentionally keep rings alive beyond the viewport, unlike Ice.
                if (unchecked((short)(slot.XPosition - cameraX + 32)) < 0 ||
                    unchecked((short)(slot.XPosition - cameraX - 288)) >= 0 ||
                    unchecked((short)(slot.YPosition - cameraY - 16)) < 0 ||
                    unchecked((short)(slot.YPosition - cameraY - 256)) >= 0)
                    ClearProjectile(slot);
                else slot.XVelocity = unchecked((byte)(slot.XVelocity + SamusComboRomData.PlasmaRadiusStep));
                break;
            default: throw new InvalidDataException("Plasma ring reached an invalid native phase.");
        }
    }
}
