using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    /// <summary>Original $90:D793 Plasma ring update; phase is stored in the slot's native Y-speed word.</summary>
    internal void StepPlasmaCombo(SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
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
        var (x, y) = SamusComboMechanicsDefinitions.GetSineOffset(
            slot.Variable, unchecked((ushort)slot.Velocity.X));
        slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(x + samus.Position.X.Whole)), unchecked((ushort)(y + samus.Position.Y.Whole))));
        slot.Variable = unchecked((byte)(slot.Variable + ComboState));
        switch (slot.Velocity.Y)
        {
            case 0:
                slot.Velocity = slot.Velocity.WithX(unchecked((byte)(slot.Velocity.X + SamusComboRomData.PlasmaRadiusStep)));
                if (slot.Velocity.X >= SamusComboRomData.PlasmaOuterRadius) slot.Velocity = slot.Velocity.WithY(1);
                break;
            case 1:
                slot.Velocity = slot.Velocity.WithX(unchecked((byte)(slot.Velocity.X - SamusComboRomData.PlasmaRadiusStep)));
                if (slot.Velocity.X < SamusComboRomData.PlasmaInnerThreshold) slot.Velocity = slot.Velocity.WithY(2);
                break;
            case 2:
                // Only the final expansion checks visibility. The first two phases
                // intentionally keep rings alive beyond the viewport, unlike Ice.
                if (unchecked((short)(slot.Position.X.Whole - camera.X + 32)) < 0 ||
                    unchecked((short)(slot.Position.X.Whole - camera.X - 288)) >= 0 ||
                    unchecked((short)(slot.Position.Y.Whole - camera.Y - 16)) < 0 ||
                    unchecked((short)(slot.Position.Y.Whole - camera.Y - 256)) >= 0)
                    ClearProjectile(slot);
                else slot.Velocity = slot.Velocity.WithX(unchecked((byte)(slot.Velocity.X + SamusComboRomData.PlasmaRadiusStep)));
                break;
            default: throw new InvalidDataException("Plasma ring reached an invalid native phase.");
        }
    }
}
