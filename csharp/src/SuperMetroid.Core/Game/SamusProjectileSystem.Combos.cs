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
        SamusBeamCombination beam = new SamusBeamLoadoutWord(samus.EquippedBeams).LowNibbleCombination;
        if (!beam.IsRetail)
            throw new NotSupportedException("Out-of-table Spazer/Plasma combo dispatcher requires emulated ROM execution.");
        short remaining = unchecked((short)(samus.PowerBombs -
            SamusComboMechanicsDefinitions.GetPowerBombCost(beam)));
        samus.PowerBombs = remaining < 0 ? (ushort)0 : (ushort)remaining;
        SamusComboKind? combo = SamusComboMechanicsDefinitions.ComboKind(beam);
        // A live Ice or Plasma combo is not restarted.
        bool activated = combo is not null &&
            !(combo == SamusComboKind.Ice && _slots[0].PreInstruction is SamusProjectilePreInstruction.IceCombo or SamusProjectilePreInstruction.IceComboOutward ||
              combo == SamusComboKind.Plasma && _slots[0].PreInstruction == SamusProjectilePreInstruction.PlasmaCombo);
        if (activated)
        {
            SamusComboKind kind = combo!.Value;
            for (int i = 3; i >= 0; i--)
            {
                var slot = _slots[i];
                slot.Type = (ushort)((samus.EquippedBeams & 0x100f) | SamusComboRomData.ProjectileTag);
                slot.Direction = kind == SamusComboKind.Spazer ? (ushort)5 : (ushort)0;
                slot.PreInstruction = kind switch
                {
                    SamusComboKind.Wave => SamusProjectilePreInstruction.WaveCombo,
                    SamusComboKind.Ice => SamusProjectilePreInstruction.IceCombo,
                    SamusComboKind.Spazer => SamusProjectilePreInstruction.SpazerCombo,
                    SamusComboKind.Plasma => SamusProjectilePreInstruction.PlasmaCombo,
                    _ => throw new InvalidOperationException($"Undefined combo {kind}."),
                };
                if (kind != SamusComboKind.Plasma) slot.TrailTimer = kind == SamusComboKind.Spazer && i < 2 ? (ushort)0 : (ushort)4;
                slot.Variable = kind is SamusComboKind.Ice or SamusComboKind.Plasma
                    ? SamusComboMechanicsDefinitions.GetOriginAngle(i)
                    : (ushort)0;
                if (kind is SamusComboKind.Wave or SamusComboKind.Spazer)
                {
                    slot.XSubposition = slot.YSubposition = 0;
                    slot.XVelocity = kind == SamusComboKind.Wave ? (short)0 : (short)40;
                }
                if (kind == SamusComboKind.Spazer) slot.AuxiliaryPhase = 0;
                if (kind == SamusComboKind.Plasma) slot.XVelocity = 40;
                slot.YVelocity = kind is SamusComboKind.Wave or SamusComboKind.Ice ? (short)600
                    : kind == SamusComboKind.Spazer ? (short)((i & 1) == 0 ? 4 : -4) : (short)0;
                if (kind == SamusComboKind.Wave)
                {
                    slot.XPosition = unchecked((ushort)(samus.XPosition + (i < 2 ? 128 : -128)));
                    slot.YPosition = unchecked((ushort)(samus.YPosition + (i is 0 or 3 ? 128 : -128)));
                }
                if (kind == SamusComboKind.Spazer && i >= 2) slot.Type = SamusComboRomData.SpazerTrailType;
                InitializeComboData(slot, kind == SamusComboKind.Ice, kind == SamusComboKind.Spazer && i >= 2);
            }
            ProjectileCounter = 4;
            shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.ReadByte(SamusProjectileRomData.Beams.UnchargedCooldowns + (_slots[0].Type & 0x3f)));
            ComboState = kind == SamusComboKind.Spazer ? (ushort)0
                : kind == SamusComboKind.Wave || samus.IsFacingRight(bus) ? (ushort)4 : unchecked((ushort)-4);
            sound = kind switch
            {
                SamusComboKind.Wave => 0x28,
                SamusComboKind.Ice => 0x23,
                SamusComboKind.Spazer => 0x25,
                SamusComboKind.Plasma => 0x27,
                _ => throw new InvalidOperationException($"Undefined combo {kind}."),
            };
        }
        if (samus.PowerBombs == 0)
            samus.SelectedHudItem = samus.AutoCancelHudItemIndex = 0;
        return activated;
    }

    private static void InitializeComboData(SamusProjectileSlot slot, bool ordinary, bool echo)
    {
        int table = ordinary ? SamusProjectileRomData.Beams.ChargedDataPointers :
            echo ? SamusComboRomData.EchoDataPointers : SamusComboRomData.DataPointers;
        int index = echo ? (byte)slot.Type - 34 : slot.Type & 15;
        int data = SamusProjectileRomData.Banks.Projectile | SamusProjectileSelectionDefinitions.ReadWord(table + index * 2);
        slot.Damage = SamusProjectileDamageDefinitions.Read(data);
        if ((slot.Damage & 0x8000) != 0) throw new InvalidDataException("Negative native combo projectile damage.");
        slot.InstructionPointer = SamusProjectileSelectionDefinitions.ReadWord(data + 2 + (ordinary || echo ? (slot.Direction & 15) * 2 : 0));
        slot.InstructionTimer = 1;
        if (ordinary)
        {
            slot.XRadius = SamusProjectileRadiusDefinitions.ReadByte(SamusProjectileRomData.Banks.Projectile | (slot.InstructionPointer + 4));
            slot.YRadius = SamusProjectileRadiusDefinitions.ReadByte(SamusProjectileRomData.Banks.Projectile | (slot.InstructionPointer + 5));
        }
    }
}
