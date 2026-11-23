namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Tourian rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static partial class TourianDoorDefinitions
{
    /// <summary>The door from the Tourian First room to the Metroids 1 room.</summary>
    public static DoorDefinition TourianFirstToMetroids1 { get; } = new("TourianFirstToMetroids1", RoomDefinitions.Metroids1, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 6), new(5, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian First room to the Statues room.</summary>
    public static DoorDefinition TourianFirstToStatues { get; } = new("TourianFirstToStatues", RoomDefinitions.Statues, new(DoorDirection.Up, DoorClosingBehavior.None), null, new(0, 1), DoorSamusSpeed.None, DoorSetupDefinitions.StatuesOpensScreens0x0And0x1) { ExchangesExploredMaps = true };

    /// <summary>The door from the Tourian First room to the Upper Tourian Save room.</summary>
    public static DoorDefinition TourianFirstToUpperTourianSave { get; } = new("TourianFirstToUpperTourianSave", RoomDefinitions.UpperTourianSave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Metroids 1 room to the Tourian First room.</summary>
    public static DoorDefinition Metroids1ToTourianFirst { get; } = new("Metroids1ToTourianFirst", RoomDefinitions.TourianFirst, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Metroids 1 room to the Metroids 2 room.</summary>
    public static DoorDefinition Metroids1ToMetroids2 { get; } = new("Metroids1ToMetroids2", RoomDefinitions.Metroids2, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Metroids 2 room to the Metroids 1 room.</summary>
    public static DoorDefinition Metroids2ToMetroids1 { get; } = new("Metroids2ToMetroids1", RoomDefinitions.Metroids1, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Metroids 2 room to the Metroids 3 room.</summary>
    public static DoorDefinition Metroids2ToMetroids3 { get; } = new("Metroids2ToMetroids3", RoomDefinitions.Metroids3, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Metroids 3 room to the Metroids 2 room.</summary>
    public static DoorDefinition Metroids3ToMetroids2 { get; } = new("Metroids3ToMetroids2", RoomDefinitions.Metroids2, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Metroids 3 room to the Metroids 4 room.</summary>
    public static DoorDefinition Metroids3ToMetroids4 { get; } = new("Metroids3ToMetroids4", RoomDefinitions.Metroids4, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Metroids 4 room to the Metroids 3 room.</summary>
    public static DoorDefinition Metroids4ToMetroids3 { get; } = new("Metroids4ToMetroids3", RoomDefinitions.Metroids3, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 6), new(5, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Metroids 4 room to the Blue Hopper room.</summary>
    public static DoorDefinition Metroids4ToBlueHopper { get; } = new("Metroids4ToBlueHopper", RoomDefinitions.BlueHopper, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(22, 3), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Blue Hopper room to the Metroids 4 room.</summary>
    public static DoorDefinition BlueHopperToMetroids4 { get; } = new("BlueHopperToMetroids4", RoomDefinitions.Metroids4, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 30), new(0, 1), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Blue Hopper room to the Dust Torizo room.</summary>
    public static DoorDefinition BlueHopperToDustTorizo { get; } = new("BlueHopperToDustTorizo", RoomDefinitions.DustTorizo, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Dust Torizo room to the Blue Hopper room.</summary>
    public static DoorDefinition DustTorizoToBlueHopper { get; } = new("DustTorizoToBlueHopper", RoomDefinitions.BlueHopper, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Dust Torizo room to the Big Boy room.</summary>
    public static DoorDefinition DustTorizoToBigBoy { get; } = new("DustTorizoToBigBoy", RoomDefinitions.BigBoy, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Boy room to the Dust Torizo room.</summary>
    public static DoorDefinition BigBoyToDustTorizo { get; } = new("BigBoyToDustTorizo", RoomDefinitions.DustTorizo, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Boy room to the Seaweed room.</summary>
    public static DoorDefinition BigBoyToSeaweed { get; } = new("BigBoyToSeaweed", RoomDefinitions.Seaweed, new(DoorDirection.Left, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Seaweed room to the Big Boy room.</summary>
    public static DoorDefinition SeaweedToBigBoy { get; } = new("SeaweedToBigBoy", RoomDefinitions.BigBoy, new(DoorDirection.Right, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Seaweed room to the Tourian Eye Door room.</summary>
    public static DoorDefinition SeaweedToTourianEyeDoor { get; } = new("SeaweedToTourianEyeDoor", RoomDefinitions.TourianEyeDoor, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Seaweed room to the Tourian Recharge room.</summary>
    public static DoorDefinition SeaweedToTourianRecharge { get; } = new("SeaweedToTourianRecharge", RoomDefinitions.TourianRecharge, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Recharge room to the Seaweed room.</summary>
    public static DoorDefinition TourianRechargeToSeaweed { get; } = new("TourianRechargeToSeaweed", RoomDefinitions.Seaweed, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mother Brain room to the Rinka Shaft room.</summary>
    public static DoorDefinition MotherBrainToRinkaShaft { get; } = new("MotherBrainToRinkaShaft", RoomDefinitions.RinkaShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mother Brain room to the Tourian Escape 1 room.</summary>
    public static DoorDefinition MotherBrainToTourianEscape1 { get; } = new("MotherBrainToTourianEscape1", RoomDefinitions.TourianEscape1, new(DoorDirection.Left, DoorClosingBehavior.EscapeGateCloses), new(31, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Eye Door room to the Seaweed room.</summary>
    public static DoorDefinition TourianEyeDoorToSeaweed { get; } = new("TourianEyeDoorToSeaweed", RoomDefinitions.Seaweed, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Eye Door room to the Rinka Shaft room.</summary>
    public static DoorDefinition TourianEyeDoorToRinkaShaft { get; } = new("TourianEyeDoorToRinkaShaft", RoomDefinitions.RinkaShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Rinka Shaft room to the Tourian Eye Door room.</summary>
    public static DoorDefinition RinkaShaftToTourianEyeDoor { get; } = new("RinkaShaftToTourianEyeDoor", RoomDefinitions.TourianEyeDoor, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Rinka Shaft room to the Mother Brain Save room.</summary>
    public static DoorDefinition RinkaShaftToMotherBrainSave { get; } = new("RinkaShaftToMotherBrainSave", RoomDefinitions.MotherBrainSave, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Rinka Shaft room to the Mother Brain room.</summary>
    public static DoorDefinition RinkaShaftToMotherBrain { get; } = new("RinkaShaftToMotherBrain", RoomDefinitions.MotherBrain, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mother Brain Save room to the Rinka Shaft room.</summary>
    public static DoorDefinition MotherBrainSaveToRinkaShaft { get; } = new("MotherBrainSaveToRinkaShaft", RoomDefinitions.RinkaShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Escape 1 room to the Mother Brain room.</summary>
    public static DoorDefinition TourianEscape1ToMotherBrain { get; } = new("TourianEscape1ToMotherBrain", RoomDefinitions.MotherBrain, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Escape 1 room to the Tourian Escape 2 room.</summary>
    public static DoorDefinition TourianEscape1ToTourianEscape2 { get; } = new("TourianEscape1ToTourianEscape2", RoomDefinitions.TourianEscape2, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 3), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Escape 2 room to the Tourian Escape 1 room.</summary>
    public static DoorDefinition TourianEscape2ToTourianEscape1 { get; } = new("TourianEscape2ToTourianEscape1", RoomDefinitions.TourianEscape1, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 12), new(0, 0), DoorSamusSpeed.PixelsPerStep(2), DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Escape 2 room to the Tourian Escape 3 room.</summary>
    public static DoorDefinition TourianEscape2ToTourianEscape3 { get; } = new("TourianEscape2ToTourianEscape3", RoomDefinitions.TourianEscape3, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Escape 3 room to the Tourian Escape 2 room.</summary>
    public static DoorDefinition TourianEscape3ToTourianEscape2 { get; } = new("TourianEscape3ToTourianEscape2", RoomDefinitions.TourianEscape2, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Escape 3 room to the Tourian Escape 4 room.</summary>
    public static DoorDefinition TourianEscape3ToTourianEscape4 { get; } = new("TourianEscape3ToTourianEscape4", RoomDefinitions.TourianEscape4, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Escape 4 room to the Tourian Escape 3 room.</summary>
    public static DoorDefinition TourianEscape4ToTourianEscape3 { get; } = new("TourianEscape4ToTourianEscape3", RoomDefinitions.TourianEscape3, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 6), new(5, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Tourian Escape 4 room to the Climb room.</summary>
    public static DoorDefinition TourianEscape4ToClimb { get; } = new("TourianEscape4ToClimb", RoomDefinitions.Climb, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 134), new(0, 8), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ClimbClosesScreen1x8OpensScreen0x8TopAligned) { ExchangesExploredMaps = true };

    /// <summary>The door from the Upper Tourian Save room to the Tourian First room.</summary>
    public static DoorDefinition UpperTourianSaveToTourianFirst { get; } = new("UpperTourianSaveToTourianFirst", RoomDefinitions.TourianFirst, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);
}
