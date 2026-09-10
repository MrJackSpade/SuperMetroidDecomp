using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusProjectileSystem
{
    /// <summary>WRAM $0B60: shared angle increment/phase state owned by active special beam attacks.</summary>
    public ushort ComboState { get; private set; }

    /// <summary>
    /// FireSBA ($90:CCC0): debit the selected beam cost before attempting its family.
    /// This is the allocation boundary; the normal producer must call it only after
    /// charge/input eligibility, and particles require their own pre-instruction updates.
    /// </summary>
    internal bool TryActivateCombo(ISnesAddressSpace bus, SamusState samus,
        SamusBombProjectileSystem shared, out ushort sound)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(shared);
        sound = 0;
        if (samus.SelectedHudItem != 3) return false;
        int beam = samus.EquippedBeams & 15;
        if (beam >= 12)
            throw new NotSupportedException("Out-of-table Spazer/Plasma combo dispatcher requires emulated ROM execution.");
        short remaining = unchecked((short)(samus.PowerBombs - ReadWord(bus, SamusComboRomData.Costs + beam * 2)));
        samus.PowerBombs = remaining < 0 ? (ushort)0 : (ushort)remaining;
        bool activated = beam is 1 or 2 or 4 or 8;
        if (beam == 2 && _slots[0].PreInstruction is SamusProjectilePreInstruction.IceCombo or SamusProjectilePreInstruction.IceComboOutward ||
            beam == 8 && _slots[0].PreInstruction == SamusProjectilePreInstruction.PlasmaCombo)
            activated = false;
        if (activated)
        {
            for (int i = 3; i >= 0; i--)
            {
                var slot = _slots[i];
                slot.Type = (ushort)((samus.EquippedBeams & 0x100f) | SamusComboRomData.ProjectileTag);
                slot.Direction = beam == 4 ? (ushort)5 : (ushort)0;
                slot.PreInstruction = beam switch { 1 => SamusProjectilePreInstruction.WaveCombo,
                    2 => SamusProjectilePreInstruction.IceCombo, 4 => SamusProjectilePreInstruction.SpazerCombo,
                    _ => SamusProjectilePreInstruction.PlasmaCombo };
                if (beam != 8) slot.TrailTimer = beam == 4 && i < 2 ? (ushort)0 : (ushort)4;
                slot.Variable = beam is 2 or 8 ? ReadWord(bus, SamusComboRomData.OriginAngles + i * 2) : (ushort)0;
                if (beam is 1 or 4)
                {
                    slot.XSubposition = slot.YSubposition = 0;
                    slot.XVelocity = beam == 1 ? (short)0 : (short)40;
                }
                if (beam == 4) slot.AuxiliaryPhase = 0;
                if (beam == 8) slot.XVelocity = 40;
                slot.YVelocity = beam is 1 or 2 ? (short)600 : beam == 4 ? (short)((i & 1) == 0 ? 4 : -4) : (short)0;
                if (beam == 1)
                {
                    slot.XPosition = unchecked((ushort)(samus.XPosition + (i < 2 ? 128 : -128)));
                    slot.YPosition = unchecked((ushort)(samus.YPosition + (i is 0 or 3 ? 128 : -128)));
                }
                if (beam == 4 && i >= 2) slot.Type = SamusComboRomData.SpazerTrailType;
                InitializeComboData(bus, slot, beam == 2, beam == 4 && i >= 2);
            }
            ProjectileCounter = 4;
            shared.SetSharedCooldown(bus.ReadByte(SamusProjectileRomData.Beams.UnchargedCooldowns + (_slots[0].Type & 0x3f)));
            ComboState = beam == 4 ? (ushort)0 : beam == 1 || samus.IsFacingRight(bus) ? (ushort)4 : unchecked((ushort)-4);
            sound = beam switch { 1 => 0x28, 2 => 0x23, 4 => 0x25, _ => 0x27 };
        }
        if (samus.PowerBombs == 0)
            samus.SelectedHudItem = samus.AutoCancelHudItemIndex = 0;
        return activated;
    }

    private static void InitializeComboData(ISnesAddressSpace bus, SamusProjectileSlot slot, bool ordinary, bool echo)
    {
        int table = ordinary ? SamusProjectileRomData.Beams.ChargedDataPointers :
            echo ? SamusComboRomData.EchoDataPointers : SamusComboRomData.DataPointers;
        int index = echo ? (byte)slot.Type - 34 : slot.Type & 15;
        int data = SamusProjectileRomData.Banks.Projectile | ReadWord(bus, table + index * 2);
        slot.Damage = ReadWord(bus, data);
        if ((slot.Damage & 0x8000) != 0) throw new InvalidDataException("Negative native combo projectile damage.");
        slot.InstructionPointer = ReadWord(bus, data + 2 + (ordinary || echo ? (slot.Direction & 15) * 2 : 0));
        slot.InstructionTimer = 1;
        if (ordinary)
        {
            slot.XRadius = bus.ReadByte(SamusProjectileRomData.Banks.Projectile | (slot.InstructionPointer + 4));
            slot.YRadius = bus.ReadByte(SamusProjectileRomData.Banks.Projectile | (slot.InstructionPointer + 5));
        }
    }
}
