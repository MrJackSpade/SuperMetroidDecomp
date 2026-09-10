using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    /// <summary>Runs $90:CF09/$CF7A for one Ice combo slot and returns its optional library-one sound request.</summary>
    internal ushort StepIceCombo(ISnesAddressSpace bus, SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared, ushort cameraX, ushort cameraY)
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
        ushort distance = outward ? unchecked((ushort)slot.XVelocity) : (ushort)32;
        var (x, y) = ComboSineOffset(bus, slot.Variable, distance);
        slot.XPosition = unchecked((ushort)(samus.XPosition + x));
        if (outward && (unchecked((short)(slot.XPosition - cameraX + 32)) < 0 ||
                        unchecked((short)(slot.XPosition - cameraX - 288)) >= 0))
        {
            ClearProjectile(slot);
            return 0;
        }
        slot.YPosition = unchecked((ushort)(samus.YPosition + y));
        if (outward && (unchecked((short)(slot.YPosition - cameraY - 16)) < 0 ||
                        unchecked((short)(slot.YPosition - cameraY - 256)) >= 0))
        {
            ClearProjectile(slot);
            return 0;
        }
        slot.Variable = unchecked((byte)(slot.Variable + ComboState));
        ushort sound = 0;
        if (outward)
            slot.XVelocity = unchecked((byte)(slot.XVelocity + 8));
        else
        {
            short oldLifetime = slot.YVelocity;
            slot.YVelocity = unchecked((short)(oldLifetime - 1));
            if (oldLifetime == 1)
            {
                slot.PreInstruction = SamusProjectilePreInstruction.IceComboOutward;
                slot.XVelocity = 40;
                sound = 0x24;
            }
        }
        shared.SetSharedCooldown(2);
        FlareCounter = 0;
        return sound;
    }

    /// <summary>Literal $90:CC39 two-byte hardware multiply, retaining low-byte amplitude and integer truncation.</summary>
    private static (ushort X, ushort Y) ComboSineOffset(ISnesAddressSpace bus, ushort angle, ushort amplitude)
    {
        ushort Component(int phase)
        {
            bool negative = phase >= 128;
            int index = negative ? unchecked((byte)(phase + 128)) : phase;
            int address = SamusComboRomData.PositiveSine + 2 * index;
            int magnitude = (bus.ReadByte(address) * (byte)amplitude >> 8) +
                            bus.ReadByte(address + 1) * (byte)amplitude;
            return unchecked((ushort)(negative ? -magnitude : magnitude));
        }
        return (Component(angle), Component(unchecked((byte)(angle - 64))));
    }
}
