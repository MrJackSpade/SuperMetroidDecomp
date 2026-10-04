using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    private SamusState? testerInventoryRecipient;
    public bool GrantAllEquipmentEnabled { get; private set; }
    public bool UnlockTourianEnabled { get; private set; }

    /// <summary>Grant once per initialized player, preserving later damage, ammo use and equipment selections.</summary>
    internal void ApplyTesterInventory(bool restoredInventory = false)
    {
        if (!GrantAllEquipmentEnabled || Samus is null ||
            (!restoredInventory && ReferenceEquals(testerInventoryRecipient, Samus))) return;
        testerInventoryRecipient = Samus;
        Samus.CollectedItems = Samus.EquippedItems = (ushort)TesterInventoryDefinitions.Equipment;
        Samus.CollectedBeams = (ushort)TesterInventoryDefinitions.Beams;
        Samus.EquippedBeams = (ushort)(TesterInventoryDefinitions.Beams & ~SamusBeamFlags.Spazer);
        Samus.Health = Samus.MaxHealth = TesterInventoryDefinitions.Energy;
        Samus.ReserveEnergy = Samus.MaxReserveEnergy = TesterInventoryDefinitions.Reserves;
        Samus.ReserveTankMode = 1;
        Samus.Missiles = Samus.MaxMissiles = TesterInventoryDefinitions.Missiles;
        Samus.SuperMissiles = Samus.MaxSuperMissiles = TesterInventoryDefinitions.SuperMissiles;
        Samus.PowerBombs = Samus.MaxPowerBombs = TesterInventoryDefinitions.PowerBombs;
        for (int bit = 0; bit < Bank80SystemState.ItemBitByteCount * 8; bit++)
            System.SetCollectedItemBit(bit);
        Samus.LoadSuitPalette(_addressSpace, Cgram);
    }
}
