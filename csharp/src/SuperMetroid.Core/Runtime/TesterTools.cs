using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Tester conveniences: granting all equipment, unlocking Tourian, and resetting saved encounters.
/// </summary>
internal sealed class TesterTools
{
    /// <summary>Stores cgram.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Stores player.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Stores system.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>Stores address space.</summary>
    private readonly ISnesAddressSpace _addressSpace;

    /// <summary>Initializes a new tester tools instance.</summary>
    internal TesterTools(SnesCgram cgram, SamusFrameState player, Bank80SystemState system, ISnesAddressSpace addressSpace)
    {
        _cgram = cgram;
        _player = player;
        _system = system;
        _addressSpace = addressSpace;
    }
    /// <summary>reset saved encounters.</summary>
    internal void ResetSavedEncounters()
    {
        // Run after the saved mirror is restored and before any room-state or
        // enemy/PLM selection. Ordinary room transitions must never reapply this.
        foreach (AreaId area in TesterEncounterResetDefinitions.Areas)
            _system.ClearBossBits(area, BossBitMasks.AllKnown);
        _system.ClearEvent(EventNumber.ShaktoolClearedPath);

        Span<byte> doors = stackalloc byte[Bank80SystemState.DoorBitByteCount];
        for (int index = 0; index < doors.Length; index++)
            doors[index] = _system.GetOpenedDoorByteRaw(index);
        foreach (ushort bit in TesterEncounterResetDefinitions.EyeDoorBits)
            doors[bit >> 3] &= unchecked((byte)~(1 << (bit & 7)));
        _system.LoadOpenedDoorBytes(doors);
    }
    /// <summary>Stores tester inventory recipient.</summary>
    internal SamusState? testerInventoryRecipient;
    /// <summary>Whether the host's tester option grants the configured full inventory once to each initialized or restored Samus actor.</summary>
    public bool GrantAllEquipmentEnabled { get; internal set; }
    /// <summary>Whether the host's tester option bypasses the Tourian statue unlock event while preserving the room sequence's authored setup and scrolling effects.</summary>
    public bool UnlockTourianEnabled { get; internal set; }

    /// <summary>Grant once per initialized player, preserving later damage, ammo use and equipment selections.</summary>
    internal void ApplyTesterInventory(bool restoredInventory = false)
    {
        if (!GrantAllEquipmentEnabled || _player.Samus is null ||
            (!restoredInventory && ReferenceEquals(testerInventoryRecipient, _player.Samus))) return;
        testerInventoryRecipient = _player.Samus;
        _player.Samus.CollectedItems = _player.Samus.EquippedItems = (ushort)TesterInventoryDefinitions.Equipment;
        _player.Samus.CollectedBeams = (ushort)TesterInventoryDefinitions.Beams;
        _player.Samus.EquippedBeams = (ushort)(TesterInventoryDefinitions.Beams & ~SamusBeamFlags.Spazer);
        _player.Samus.Health = _player.Samus.MaxHealth = TesterInventoryDefinitions.Energy;
        _player.Samus.ReserveEnergy = _player.Samus.MaxReserveEnergy = TesterInventoryDefinitions.Reserves;
        _player.Samus.ReserveTankMode = 1;
        _player.Samus.Missiles = _player.Samus.MaxMissiles = TesterInventoryDefinitions.Missiles;
        _player.Samus.SuperMissiles = _player.Samus.MaxSuperMissiles = TesterInventoryDefinitions.SuperMissiles;
        _player.Samus.PowerBombs = _player.Samus.MaxPowerBombs = TesterInventoryDefinitions.PowerBombs;
        for (int bit = 0; bit < Bank80SystemState.ItemBitByteCount * 8; bit++)
            _system.SetCollectedItemBit(bit);
        _player.Samus.Presentation.LoadSuitPalette(_addressSpace, _cgram);
    }
}
