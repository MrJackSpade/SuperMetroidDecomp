namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Tourian rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class TourianDoorDefinitions
{
    /// <summary>The door from the Tourian First room to the Metroids 1 room.</summary>
    public static DoorDefinition TourianFirstToMetroids1 { get; } = DoorDefinitions.Find(nameof(TourianFirstToMetroids1));

    /// <summary>The door from the Tourian First room to the Statues room.</summary>
    public static DoorDefinition TourianFirstToStatues { get; } = DoorDefinitions.Find(nameof(TourianFirstToStatues));

    /// <summary>The door from the Tourian First room to the Upper Tourian Save room.</summary>
    public static DoorDefinition TourianFirstToUpperTourianSave { get; } = DoorDefinitions.Find(nameof(TourianFirstToUpperTourianSave));

    /// <summary>The door from the Metroids 1 room to the Tourian First room.</summary>
    public static DoorDefinition Metroids1ToTourianFirst { get; } = DoorDefinitions.Find(nameof(Metroids1ToTourianFirst));

    /// <summary>The door from the Metroids 1 room to the Metroids 2 room.</summary>
    public static DoorDefinition Metroids1ToMetroids2 { get; } = DoorDefinitions.Find(nameof(Metroids1ToMetroids2));

    /// <summary>The door from the Metroids 2 room to the Metroids 1 room.</summary>
    public static DoorDefinition Metroids2ToMetroids1 { get; } = DoorDefinitions.Find(nameof(Metroids2ToMetroids1));

    /// <summary>The door from the Metroids 2 room to the Metroids 3 room.</summary>
    public static DoorDefinition Metroids2ToMetroids3 { get; } = DoorDefinitions.Find(nameof(Metroids2ToMetroids3));

    /// <summary>The door from the Metroids 3 room to the Metroids 2 room.</summary>
    public static DoorDefinition Metroids3ToMetroids2 { get; } = DoorDefinitions.Find(nameof(Metroids3ToMetroids2));

    /// <summary>The door from the Metroids 3 room to the Metroids 4 room.</summary>
    public static DoorDefinition Metroids3ToMetroids4 { get; } = DoorDefinitions.Find(nameof(Metroids3ToMetroids4));

    /// <summary>The door from the Metroids 4 room to the Metroids 3 room.</summary>
    public static DoorDefinition Metroids4ToMetroids3 { get; } = DoorDefinitions.Find(nameof(Metroids4ToMetroids3));

    /// <summary>The door from the Metroids 4 room to the Blue Hopper room.</summary>
    public static DoorDefinition Metroids4ToBlueHopper { get; } = DoorDefinitions.Find(nameof(Metroids4ToBlueHopper));

    /// <summary>The door from the Blue Hopper room to the Metroids 4 room.</summary>
    public static DoorDefinition BlueHopperToMetroids4 { get; } = DoorDefinitions.Find(nameof(BlueHopperToMetroids4));

    /// <summary>The door from the Blue Hopper room to the Dust Torizo room.</summary>
    public static DoorDefinition BlueHopperToDustTorizo { get; } = DoorDefinitions.Find(nameof(BlueHopperToDustTorizo));

    /// <summary>The door from the Dust Torizo room to the Blue Hopper room.</summary>
    public static DoorDefinition DustTorizoToBlueHopper { get; } = DoorDefinitions.Find(nameof(DustTorizoToBlueHopper));

    /// <summary>The door from the Dust Torizo room to the Big Boy room.</summary>
    public static DoorDefinition DustTorizoToBigBoy { get; } = DoorDefinitions.Find(nameof(DustTorizoToBigBoy));

    /// <summary>The door from the Big Boy room to the Dust Torizo room.</summary>
    public static DoorDefinition BigBoyToDustTorizo { get; } = DoorDefinitions.Find(nameof(BigBoyToDustTorizo));

    /// <summary>The door from the Big Boy room to the Seaweed room.</summary>
    public static DoorDefinition BigBoyToSeaweed { get; } = DoorDefinitions.Find(nameof(BigBoyToSeaweed));

    /// <summary>The door from the Seaweed room to the Big Boy room.</summary>
    public static DoorDefinition SeaweedToBigBoy { get; } = DoorDefinitions.Find(nameof(SeaweedToBigBoy));

    /// <summary>The door from the Seaweed room to the Tourian Eye Door room.</summary>
    public static DoorDefinition SeaweedToTourianEyeDoor { get; } = DoorDefinitions.Find(nameof(SeaweedToTourianEyeDoor));

    /// <summary>The door from the Seaweed room to the Tourian Recharge room.</summary>
    public static DoorDefinition SeaweedToTourianRecharge { get; } = DoorDefinitions.Find(nameof(SeaweedToTourianRecharge));

    /// <summary>The door from the Tourian Recharge room to the Seaweed room.</summary>
    public static DoorDefinition TourianRechargeToSeaweed { get; } = DoorDefinitions.Find(nameof(TourianRechargeToSeaweed));

    /// <summary>The door from the Mother Brain room to the Rinka Shaft room.</summary>
    public static DoorDefinition MotherBrainToRinkaShaft { get; } = DoorDefinitions.Find(nameof(MotherBrainToRinkaShaft));

    /// <summary>The door from the Mother Brain room to the Tourian Escape 1 room.</summary>
    public static DoorDefinition MotherBrainToTourianEscape1 { get; } = DoorDefinitions.Find(nameof(MotherBrainToTourianEscape1));

    /// <summary>The door from the Tourian Eye Door room to the Seaweed room.</summary>
    public static DoorDefinition TourianEyeDoorToSeaweed { get; } = DoorDefinitions.Find(nameof(TourianEyeDoorToSeaweed));

    /// <summary>The door from the Tourian Eye Door room to the Rinka Shaft room.</summary>
    public static DoorDefinition TourianEyeDoorToRinkaShaft { get; } = DoorDefinitions.Find(nameof(TourianEyeDoorToRinkaShaft));

    /// <summary>The door from the Rinka Shaft room to the Tourian Eye Door room.</summary>
    public static DoorDefinition RinkaShaftToTourianEyeDoor { get; } = DoorDefinitions.Find(nameof(RinkaShaftToTourianEyeDoor));

    /// <summary>The door from the Rinka Shaft room to the Mother Brain Save room.</summary>
    public static DoorDefinition RinkaShaftToMotherBrainSave { get; } = DoorDefinitions.Find(nameof(RinkaShaftToMotherBrainSave));

    /// <summary>The door from the Rinka Shaft room to the Mother Brain room.</summary>
    public static DoorDefinition RinkaShaftToMotherBrain { get; } = DoorDefinitions.Find(nameof(RinkaShaftToMotherBrain));

    /// <summary>The door from the Mother Brain Save room to the Rinka Shaft room.</summary>
    public static DoorDefinition MotherBrainSaveToRinkaShaft { get; } = DoorDefinitions.Find(nameof(MotherBrainSaveToRinkaShaft));

    /// <summary>The door from the Tourian Escape 1 room to the Mother Brain room.</summary>
    public static DoorDefinition TourianEscape1ToMotherBrain { get; } = DoorDefinitions.Find(nameof(TourianEscape1ToMotherBrain));

    /// <summary>The door from the Tourian Escape 1 room to the Tourian Escape 2 room.</summary>
    public static DoorDefinition TourianEscape1ToTourianEscape2 { get; } = DoorDefinitions.Find(nameof(TourianEscape1ToTourianEscape2));

    /// <summary>The door from the Tourian Escape 2 room to the Tourian Escape 1 room.</summary>
    public static DoorDefinition TourianEscape2ToTourianEscape1 { get; } = DoorDefinitions.Find(nameof(TourianEscape2ToTourianEscape1));

    /// <summary>The door from the Tourian Escape 2 room to the Tourian Escape 3 room.</summary>
    public static DoorDefinition TourianEscape2ToTourianEscape3 { get; } = DoorDefinitions.Find(nameof(TourianEscape2ToTourianEscape3));

    /// <summary>The door from the Tourian Escape 3 room to the Tourian Escape 2 room.</summary>
    public static DoorDefinition TourianEscape3ToTourianEscape2 { get; } = DoorDefinitions.Find(nameof(TourianEscape3ToTourianEscape2));

    /// <summary>The door from the Tourian Escape 3 room to the Tourian Escape 4 room.</summary>
    public static DoorDefinition TourianEscape3ToTourianEscape4 { get; } = DoorDefinitions.Find(nameof(TourianEscape3ToTourianEscape4));

    /// <summary>The door from the Tourian Escape 4 room to the Tourian Escape 3 room.</summary>
    public static DoorDefinition TourianEscape4ToTourianEscape3 { get; } = DoorDefinitions.Find(nameof(TourianEscape4ToTourianEscape3));

    /// <summary>The door from the Tourian Escape 4 room to the Climb room.</summary>
    public static DoorDefinition TourianEscape4ToClimb { get; } = DoorDefinitions.Find(nameof(TourianEscape4ToClimb));

    /// <summary>The door from the Upper Tourian Save room to the Tourian First room.</summary>
    public static DoorDefinition UpperTourianSaveToTourianFirst { get; } = DoorDefinitions.Find(nameof(UpperTourianSaveToTourianFirst));
}
