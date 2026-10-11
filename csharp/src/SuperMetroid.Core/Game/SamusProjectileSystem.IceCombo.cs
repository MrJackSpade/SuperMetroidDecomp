using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    /// <summary>Runs $90:CF09/$CF7A for one Ice combo slot and returns its optional library-one sound request.</summary>
    internal ushort StepIceCombo(ISnesAddressSpace bus, SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
    {
        bool outward = slot.PreInstruction == SamusProjectilePreInstruction.IceComboOutward;
        if (!outward && slot.PreInstruction != SamusProjectilePreInstruction.IceCombo)
            throw new InvalidOperationException("Ice combo update requires its own pre-instruction.");
        if ((slot.Direction & 0xf0) != 0)
        {
            ClearProjectile(slot);
            return outward ? (ushort)0 : (ushort)0x24;
        }
        ushort oldTimer = slot.TrailTimer;
        slot.TrailTimer = unchecked((ushort)(oldTimer - 1));
        if (oldTimer == 1)
        {
            slot.TrailTimer = 4;
            SpawnTrail(bus, slot);
        }
        ushort distance = outward ? unchecked((ushort)slot.Velocity.X) : (ushort)32;
        var (x, y) = SamusComboMechanicsDefinitions.GetSineOffset(slot.Variable, distance);
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(samus.Position.X.Whole + x)));
        if (outward && (unchecked((short)(slot.Position.X.Whole - camera.X + 32)) < 0 ||
                        unchecked((short)(slot.Position.X.Whole - camera.X - 288)) >= 0))
        {
            ClearProjectile(slot);
            return 0;
        }
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(samus.Position.Y.Whole + y)));
        if (outward && (unchecked((short)(slot.Position.Y.Whole - camera.Y - 16)) < 0 ||
                        unchecked((short)(slot.Position.Y.Whole - camera.Y - 256)) >= 0))
        {
            ClearProjectile(slot);
            return 0;
        }
        slot.Variable = unchecked((byte)(slot.Variable + ComboState));
        ushort sound = 0;
        if (outward)
            slot.Velocity = slot.Velocity.WithX(unchecked((byte)(slot.Velocity.X + 8)));
        else
        {
            short oldLifetime = slot.Velocity.Y;
            slot.Velocity = slot.Velocity.WithY(unchecked((short)(oldLifetime - 1)));
            if (oldLifetime == 1)
            {
                slot.PreInstruction = SamusProjectilePreInstruction.IceComboOutward;
                slot.Velocity = slot.Velocity.WithX(40);
                sound = 0x24;
            }
        }
        shared.SetSharedCooldown(2);
        FlareCounter = 0;
        return sound;
    }
}
