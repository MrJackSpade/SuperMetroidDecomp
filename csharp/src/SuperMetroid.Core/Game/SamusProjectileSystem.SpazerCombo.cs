using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    /// <summary>Runs $90:DB06/$DC9C, including paired-hit cleanup and retagging the primary particles as falling trails.</summary>
    internal ushort StepSpazerCombo(ISnesAddressSpace bus, SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared, ushort cameraY)
    {
        bool falling = slot.PreInstruction == SamusProjectilePreInstruction.SpazerComboFalling;
        if (!falling && slot.PreInstruction != SamusProjectilePreInstruction.SpazerCombo)
            throw new InvalidOperationException("Spazer update requires its own pre-instruction.");
        if ((slot.Direction & 0xf0) != 0)
        {
            int partner = slot.SlotIndex ^ 2;
            ClearProjectile(slot);
            if (!falling) ClearProjectile(_slots[partner]);
            return 0;
        }
        ushort previousTimer = slot.TrailTimer;
        slot.TrailTimer = unchecked((ushort)(previousTimer - 1));
        if (previousTimer == 1)
        {
            slot.TrailTimer = 4;
            SpawnTrail(bus, slot);
        }
        if (falling)
        {
            slot.YPosition = unchecked((ushort)(slot.YPosition + 8));
            if (unchecked((short)(slot.YPosition - cameraY - 248)) >= 0)
            {
                ClearProjectile(slot);
                return 0;
            }
        }
        else
        {
            var (x, y) = ComboSineOffset(bus, slot.Variable, unchecked((ushort)slot.XVelocity));
            slot.XPosition = unchecked((ushort)(samus.XPosition + x));
            slot.YPosition = unchecked((ushort)(samus.YPosition + y - (slot.AuxiliaryPhase == 0 ? 0 : 114)));
            switch (slot.AuxiliaryPhase)
            {
                case 0:
                    AdvanceSpazerAngle(slot);
                    if (slot.Variable == 128)
                    {
                        slot.XVelocity = 160;
                        slot.YVelocity = (short)((slot.SlotIndex & 1) == 0 ? 2 : -2);
                        slot.Direction = 0;
                        slot.AuxiliaryPhase = 2;
                    }
                    break;
                case 2:
                    if (unchecked((short)(slot.YPosition - cameraY - 16)) < 0)
                        BeginSpazerFall(bus, slot);
                    else
                    {
                        AdvanceSpazerAngle(slot);
                        slot.XVelocity = unchecked((short)(slot.XVelocity - 5));
                        if (slot.XVelocity == 0)
                        {
                            slot.YVelocity = (short)((slot.SlotIndex & 1) == 0 ? -2 : 2);
                            slot.Variable = unchecked((byte)(slot.Variable + 128));
                            slot.AuxiliaryPhase = 4;
                            shared.SetSharedCooldown(2);
                            FlareCounter = 0;
                            return slot.SlotIndex == 0 ? (ushort)0x26 : (ushort)0;
                        }
                    }
                    break;
                case 4:
                    if (unchecked((short)(slot.YPosition - cameraY - 16)) < 0)
                        BeginSpazerFall(bus, slot);
                    else
                    {
                        AdvanceSpazerAngle(slot);
                        slot.XVelocity = unchecked((short)(slot.XVelocity + 5));
                        if (slot.XVelocity >= 96) BeginSpazerFall(bus, slot);
                    }
                    break;
                default: throw new InvalidDataException("Spazer reached an invalid native phase.");
            }
        }
        shared.SetSharedCooldown(2);
        FlareCounter = 0;
        return 0;
    }

    private static void AdvanceSpazerAngle(SamusProjectileSlot slot) =>
        slot.Variable = unchecked((byte)(slot.Variable + slot.YVelocity));

    private static void BeginSpazerFall(ISnesAddressSpace bus, SamusProjectileSlot slot)
    {
        slot.XPosition = unchecked((ushort)(slot.XPosition + (slot.SlotIndex < 2 ? 16 : -16)));
        slot.Direction = 5;
        slot.TrailTimer = 4;
        slot.PreInstruction = SamusProjectilePreInstruction.SpazerComboFalling;
        if (slot.SlotIndex < 2)
        {
            slot.Type = SamusComboRomData.SpazerTrailType;
            InitializeComboData(bus, slot, ordinary: false, echo: true);
        }
    }
}
