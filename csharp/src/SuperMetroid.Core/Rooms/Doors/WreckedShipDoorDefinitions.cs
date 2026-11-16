namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Wrecked Ship rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class WreckedShipDoorDefinitions
{
    /// <summary>The door from the Bowling Alley room to the West Ocean room.</summary>
    public static DoorDefinition BowlingAlleyToWestOcean { get; } = DoorDefinitions.Find(nameof(BowlingAlleyToWestOcean));

    /// <summary>The door from the Bowling Alley room to the Pre Bowling room.</summary>
    public static DoorDefinition BowlingAlleyToPreBowling { get; } = DoorDefinitions.Find(nameof(BowlingAlleyToPreBowling));

    /// <summary>The door from the Bowling Alley room to the Gravity Suit room.</summary>
    public static DoorDefinition BowlingAlleyToGravitySuit { get; } = DoorDefinitions.Find(nameof(BowlingAlleyToGravitySuit));

    /// <summary>The door from the Wrecked Ship Entrance room to the West Ocean room.</summary>
    public static DoorDefinition WreckedShipEntranceToWestOcean { get; } = DoorDefinitions.Find(nameof(WreckedShipEntranceToWestOcean));

    /// <summary>The door from the Wrecked Ship Entrance room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition WreckedShipEntranceToWreckedShipMainShaft { get; } = DoorDefinitions.Find(nameof(WreckedShipEntranceToWreckedShipMainShaft));

    /// <summary>The door from the Attic room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition AtticToWreckedShipMainShaft { get; } = DoorDefinitions.Find(nameof(AtticToWreckedShipMainShaft));

    /// <summary>The door from the Attic room to the Assembly Line room.</summary>
    public static DoorDefinition AtticToAssemblyLine { get; } = DoorDefinitions.Find(nameof(AtticToAssemblyLine));

    /// <summary>The door from the Attic room to the West Ocean room.</summary>
    public static DoorDefinition AtticToWestOcean { get; } = DoorDefinitions.Find(nameof(AtticToWestOcean));

    /// <summary>The door from the Assembly Line room to the Attic room.</summary>
    public static DoorDefinition AssemblyLineToAttic { get; } = DoorDefinitions.Find(nameof(AssemblyLineToAttic));

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Wrecked Ship Entrance room.</summary>
    public static DoorDefinition WreckedShipMainShaftToWreckedShipEntrance { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaftToWreckedShipEntrance));

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Sponge Bath room.</summary>
    public static DoorDefinition WreckedShipMainShaftToSpongeBath { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaftToSpongeBath));

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Wrecked Ship West Super room.</summary>
    public static DoorDefinition WreckedShipMainShaftToWreckedShipWestSuper { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaftToWreckedShipWestSuper));

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Basement room.</summary>
    public static DoorDefinition WreckedShipMainShaftToBasement { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaftToBasement));

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Attic room.</summary>
    public static DoorDefinition WreckedShipMainShaftToAttic { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaftToAttic));

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Wrecked Ship East Super room.</summary>
    public static DoorDefinition WreckedShipMainShaftToWreckedShipEastSuper { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaftToWreckedShipEastSuper));

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Wrecked Ship Save room.</summary>
    public static DoorDefinition WreckedShipMainShaftToWreckedShipSave { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaftToWreckedShipSave));

    /// <summary>The door from the Spiky Death room to the Sponge Bath room.</summary>
    public static DoorDefinition SpikyDeathToSpongeBath { get; } = DoorDefinitions.Find(nameof(SpikyDeathToSpongeBath));

    /// <summary>The door from the Spiky Death room to the Electric Death room.</summary>
    public static DoorDefinition SpikyDeathToElectricDeath { get; } = DoorDefinitions.Find(nameof(SpikyDeathToElectricDeath));

    /// <summary>The door from the Electric Death room to the East Ocean room.</summary>
    public static DoorDefinition ElectricDeathToEastOcean { get; } = DoorDefinitions.Find(nameof(ElectricDeathToEastOcean));

    /// <summary>The door from the Electric Death room to the Spiky Death room.</summary>
    public static DoorDefinition ElectricDeathToSpikyDeath { get; } = DoorDefinitions.Find(nameof(ElectricDeathToSpikyDeath));

    /// <summary>The door from the Electric Death room to the Wrecked Ship Energy Tank room.</summary>
    public static DoorDefinition ElectricDeathToWreckedShipEnergyTank { get; } = DoorDefinitions.Find(nameof(ElectricDeathToWreckedShipEnergyTank));

    /// <summary>The door from the Wrecked Ship Energy Tank room to the Electric Death room.</summary>
    public static DoorDefinition WreckedShipEnergyTankToElectricDeath { get; } = DoorDefinitions.Find(nameof(WreckedShipEnergyTankToElectricDeath));

    /// <summary>The door from the Basement room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition BasementToWreckedShipMainShaft { get; } = DoorDefinitions.Find(nameof(BasementToWreckedShipMainShaft));

    /// <summary>The door from the Basement room to the Wrecked Ship Map room.</summary>
    public static DoorDefinition BasementToWreckedShipMap { get; } = DoorDefinitions.Find(nameof(BasementToWreckedShipMap));

    /// <summary>The door from the Basement room to the Phantoon room.</summary>
    public static DoorDefinition BasementToPhantoon { get; } = DoorDefinitions.Find(nameof(BasementToPhantoon));

    /// <summary>The door from the Wrecked Ship Map room to the Basement room.</summary>
    public static DoorDefinition WreckedShipMapToBasement { get; } = DoorDefinitions.Find(nameof(WreckedShipMapToBasement));

    /// <summary>The door from the Phantoon room to the Basement room.</summary>
    public static DoorDefinition PhantoonToBasement { get; } = DoorDefinitions.Find(nameof(PhantoonToBasement));

    /// <summary>The door from the Sponge Bath room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition SpongeBathToWreckedShipMainShaft { get; } = DoorDefinitions.Find(nameof(SpongeBathToWreckedShipMainShaft));

    /// <summary>The door from the Sponge Bath room to the Spiky Death room.</summary>
    public static DoorDefinition SpongeBathToSpikyDeath { get; } = DoorDefinitions.Find(nameof(SpongeBathToSpikyDeath));

    /// <summary>The door from the Wrecked Ship West Super room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition WreckedShipWestSuperToWreckedShipMainShaft { get; } = DoorDefinitions.Find(nameof(WreckedShipWestSuperToWreckedShipMainShaft));

    /// <summary>The door from the Wrecked Ship East Super room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition WreckedShipEastSuperToWreckedShipMainShaft { get; } = DoorDefinitions.Find(nameof(WreckedShipEastSuperToWreckedShipMainShaft));

    /// <summary>The door from the Gravity Suit room to the West Ocean room.</summary>
    public static DoorDefinition GravitySuitToWestOcean { get; } = DoorDefinitions.Find(nameof(GravitySuitToWestOcean));

    /// <summary>The door from the Gravity Suit room to the Bowling Alley room.</summary>
    public static DoorDefinition GravitySuitToBowlingAlley { get; } = DoorDefinitions.Find(nameof(GravitySuitToBowlingAlley));

    /// <summary>The door from the Wrecked Ship Save room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition WreckedShipSaveToWreckedShipMainShaft { get; } = DoorDefinitions.Find(nameof(WreckedShipSaveToWreckedShipMainShaft));
}
