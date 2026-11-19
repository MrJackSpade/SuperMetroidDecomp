namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Wrecked Ship rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class WreckedShipDoorDefinitions
{
    /// <summary>The door from the Bowling Alley room to the West Ocean room.</summary>
    public static DoorDefinition BowlingAlleyToWestOcean { get; } = new("BowlingAlleyToWestOcean", RoomDefinitions.WestOcean, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(126, 22), new(7, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bowling Alley room to the Pre Bowling room.</summary>
    public static DoorDefinition BowlingAlleyToPreBowling { get; } = new("BowlingAlleyToPreBowling", RoomDefinitions.PreBowling, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bowling Alley room to the Gravity Suit room.</summary>
    public static DoorDefinition BowlingAlleyToGravitySuit { get; } = new("BowlingAlleyToGravitySuit", RoomDefinitions.GravitySuit, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship Entrance room to the West Ocean room.</summary>
    public static DoorDefinition WreckedShipEntranceToWestOcean { get; } = new("WreckedShipEntranceToWestOcean", RoomDefinitions.WestOcean, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(126, 70), new(7, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship Entrance room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition WreckedShipEntranceToWreckedShipMainShaft { get; } = new("WreckedShipEntranceToWreckedShipMainShaft", RoomDefinitions.WreckedShipMainShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(65, 54), new(4, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Attic room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition AtticToWreckedShipMainShaft { get; } = new("AtticToWreckedShipMainShaft", RoomDefinitions.WreckedShipMainShaft, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(70, 2), new(4, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Attic room to the Assembly Line room.</summary>
    public static DoorDefinition AtticToAssemblyLine { get; } = new("AtticToAssemblyLine", RoomDefinitions.AssemblyLine, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Attic room to the West Ocean room.</summary>
    public static DoorDefinition AtticToWestOcean { get; } = new("AtticToWestOcean", RoomDefinitions.WestOcean, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(126, 6), new(7, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Assembly Line room to the Attic room.</summary>
    public static DoorDefinition AssemblyLineToAttic { get; } = new("AssemblyLineToAttic", RoomDefinitions.Attic, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(110, 6), new(6, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Wrecked Ship Entrance room.</summary>
    public static DoorDefinition WreckedShipMainShaftToWreckedShipEntrance { get; } = new("WreckedShipMainShaftToWreckedShipEntrance", RoomDefinitions.WreckedShipEntrance, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.WreckedShipEntranceStartsTreadmillLeftwards);

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Sponge Bath room.</summary>
    public static DoorDefinition WreckedShipMainShaftToSpongeBath { get; } = new("WreckedShipMainShaftToSpongeBath", RoomDefinitions.SpongeBath, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Wrecked Ship West Super room.</summary>
    public static DoorDefinition WreckedShipMainShaftToWreckedShipWestSuper { get; } = new("WreckedShipMainShaftToWreckedShipWestSuper", RoomDefinitions.WreckedShipWestSuper, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Basement room.</summary>
    public static DoorDefinition WreckedShipMainShaftToBasement { get; } = new("WreckedShipMainShaftToBasement", RoomDefinitions.Basement, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(38, 2), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Attic room.</summary>
    public static DoorDefinition WreckedShipMainShaftToAttic { get; } = new("WreckedShipMainShaftToAttic", RoomDefinitions.Attic, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(70, 14), new(4, 0), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Wrecked Ship East Super room.</summary>
    public static DoorDefinition WreckedShipMainShaftToWreckedShipEastSuper { get; } = new("WreckedShipMainShaftToWreckedShipEastSuper", RoomDefinitions.WreckedShipEastSuper, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship Main Shaft room to the Wrecked Ship Save room.</summary>
    public static DoorDefinition WreckedShipMainShaftToWreckedShipSave { get; } = new("WreckedShipMainShaftToWreckedShipSave", RoomDefinitions.WreckedShipSave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spiky Death room to the Sponge Bath room.</summary>
    public static DoorDefinition SpikyDeathToSpongeBath { get; } = new("SpikyDeathToSpongeBath", RoomDefinitions.SpongeBath, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.SpongeBathOpensScreen1x0TopAligned);

    /// <summary>The door from the Spiky Death room to the Electric Death room.</summary>
    public static DoorDefinition SpikyDeathToElectricDeath { get; } = new("SpikyDeathToElectricDeath", RoomDefinitions.ElectricDeath, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Electric Death room to the East Ocean room.</summary>
    public static DoorDefinition ElectricDeathToEastOcean { get; } = new("ElectricDeathToEastOcean", RoomDefinitions.EastOcean, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 70), new(0, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Electric Death room to the Spiky Death room.</summary>
    public static DoorDefinition ElectricDeathToSpikyDeath { get; } = new("ElectricDeathToSpikyDeath", RoomDefinitions.SpikyDeath, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Electric Death room to the Wrecked Ship Energy Tank room.</summary>
    public static DoorDefinition ElectricDeathToWreckedShipEnergyTank { get; } = new("ElectricDeathToWreckedShipEnergyTank", RoomDefinitions.WreckedShipEnergyTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship Energy Tank room to the Electric Death room.</summary>
    public static DoorDefinition WreckedShipEnergyTankToElectricDeath { get; } = new("WreckedShipEnergyTankToElectricDeath", RoomDefinitions.ElectricDeath, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ElectricDeathOpensScreen0x0);

    /// <summary>The door from the Basement room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition BasementToWreckedShipMainShaft { get; } = new("BasementToWreckedShipMainShaft", RoomDefinitions.WreckedShipMainShaft, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(70, 125), new(4, 7), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.WreckedShipMainShaftOpensScreens4x6And4x7);

    /// <summary>The door from the Basement room to the Wrecked Ship Map room.</summary>
    public static DoorDefinition BasementToWreckedShipMap { get; } = new("BasementToWreckedShipMap", RoomDefinitions.WreckedShipMap, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Basement room to the Phantoon room.</summary>
    public static DoorDefinition BasementToPhantoon { get; } = new("BasementToPhantoon", RoomDefinitions.Phantoon, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship Map room to the Basement room.</summary>
    public static DoorDefinition WreckedShipMapToBasement { get; } = new("WreckedShipMapToBasement", RoomDefinitions.Basement, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Phantoon room to the Basement room.</summary>
    public static DoorDefinition PhantoonToBasement { get; } = new("PhantoonToBasement", RoomDefinitions.Basement, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 6), new(4, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.BasementClosesScreen3x0OpensScreen4x0TopAligned);

    /// <summary>The door from the Sponge Bath room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition SpongeBathToWreckedShipMainShaft { get; } = new("SpongeBathToWreckedShipMainShaft", RoomDefinitions.WreckedShipMainShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 70), new(4, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Sponge Bath room to the Spiky Death room.</summary>
    public static DoorDefinition SpongeBathToSpikyDeath { get; } = new("SpongeBathToSpikyDeath", RoomDefinitions.SpikyDeath, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship West Super room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition WreckedShipWestSuperToWreckedShipMainShaft { get; } = new("WreckedShipWestSuperToWreckedShipMainShaft", RoomDefinitions.WreckedShipMainShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(65, 102), new(4, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wrecked Ship East Super room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition WreckedShipEastSuperToWreckedShipMainShaft { get; } = new("WreckedShipEastSuperToWreckedShipMainShaft", RoomDefinitions.WreckedShipMainShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 102), new(5, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.WreckedShipMainShaftOpensScreen5x6TopAligned);

    /// <summary>The door from the Gravity Suit room to the West Ocean room.</summary>
    public static DoorDefinition GravitySuitToWestOcean { get; } = new("GravitySuitToWestOcean", RoomDefinitions.WestOcean, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 54), new(5, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Gravity Suit room to the Bowling Alley room.</summary>
    public static DoorDefinition GravitySuitToBowlingAlley { get; } = new("GravitySuitToBowlingAlley", RoomDefinitions.BowlingAlley, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(17, 38), new(1, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.BowlingAlleyClosesScreens0x1And1x1And2x1And3x1And4x1And5x1);

    /// <summary>The door from the Wrecked Ship Save room to the Wrecked Ship Main Shaft room.</summary>
    public static DoorDefinition WreckedShipSaveToWreckedShipMainShaft { get; } = new("WreckedShipSaveToWreckedShipMainShaft", RoomDefinitions.WreckedShipMainShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 54), new(4, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);
}
