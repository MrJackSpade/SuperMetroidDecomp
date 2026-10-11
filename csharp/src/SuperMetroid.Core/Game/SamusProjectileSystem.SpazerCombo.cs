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
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 8)));
            if (unchecked((short)(slot.Position.Y.Whole - cameraY - 248)) >= 0)
            {
                ClearProjectile(slot);
                return 0;
            }
        }
        else
        {
            var (x, y) = SamusComboMechanicsDefinitions.GetSineOffset(
                slot.Variable, unchecked((ushort)slot.Velocity.X));
            slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(samus.Position.X.Whole + x)), unchecked((ushort)(samus.Position.Y.Whole + y - (slot.AuxiliaryPhase == SpazerComboPhase.Circling ? 0 : 114)))));
            switch (slot.AuxiliaryPhase)
            {
                case SpazerComboPhase.Circling:
                    AdvanceSpazerAngle(slot);
                    if (slot.Variable == 128)
                    {
                        slot.Velocity = slot.Velocity.WithX(160);
                        slot.Velocity = slot.Velocity.WithY((short)((slot.SlotIndex & 1) == 0 ? 2 : -2));
                        slot.Direction = 0;
                        slot.AuxiliaryPhase = SpazerComboPhase.FlyingUpTowardsPoint;
                    }
                    break;
                case SpazerComboPhase.FlyingUpTowardsPoint:
                    if (unchecked((short)(slot.Position.Y.Whole - cameraY - 16)) < 0)
                        BeginSpazerFall(slot);
                    else
                    {
                        AdvanceSpazerAngle(slot);
                        slot.Velocity = slot.Velocity.WithX(unchecked((short)(slot.Velocity.X - 5)));
                        if (slot.Velocity.X == 0)
                        {
                            slot.Velocity = slot.Velocity.WithY((short)((slot.SlotIndex & 1) == 0 ? -2 : 2));
                            slot.Variable = unchecked((byte)(slot.Variable + 128));
                            slot.AuxiliaryPhase = SpazerComboPhase.FlyingUpAwayFromPoint;
                            shared.SetSharedCooldown(2);
                            FlareCounter = 0;
                            return slot.SlotIndex == 0 ? (ushort)0x26 : (ushort)0;
                        }
                    }
                    break;
                case SpazerComboPhase.FlyingUpAwayFromPoint:
                    if (unchecked((short)(slot.Position.Y.Whole - cameraY - 16)) < 0)
                        BeginSpazerFall(slot);
                    else
                    {
                        AdvanceSpazerAngle(slot);
                        slot.Velocity = slot.Velocity.WithX(unchecked((short)(slot.Velocity.X + 5)));
                        if (slot.Velocity.X >= 96) BeginSpazerFall(slot);
                    }
                    break;
                default: throw new InvalidOperationException($"Undefined {nameof(SpazerComboPhase)} {(int)slot.AuxiliaryPhase}.");
            }
        }
        shared.SetSharedCooldown(2);
        FlareCounter = 0;
        return 0;
    }

    private static void AdvanceSpazerAngle(SamusProjectileSlot slot) =>
        slot.Variable = unchecked((byte)(slot.Variable + slot.Velocity.Y));

    private static void BeginSpazerFall(SamusProjectileSlot slot)
    {
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + (slot.SlotIndex < 2 ? 16 : -16))));
        slot.Direction = 5;
        slot.TrailTimer = 4;
        slot.PreInstruction = SamusProjectilePreInstruction.SpazerComboFalling;
        if (slot.SlotIndex < 2)
        {
            slot.Type = SamusComboRomData.SpazerTrailType;
            InitializeComboData(slot, ordinary: false, echo: true);
        }
    }
}
