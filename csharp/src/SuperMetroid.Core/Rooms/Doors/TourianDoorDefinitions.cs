namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Tourian rooms' doors, named by the room they lead from.</summary>
[NumberedIdentities("Each numbered door is one door of its room, named by its place in that room's door list; every door has its own destination and is used by name, never iterated or indexed.")]
public static class TourianDoorDefinitions
{
    /// <summary>The Tourian First room's door 0.</summary>
    public static DoorDefinition TourianFirst0 { get; } = DoorDefinitions.Find(nameof(TourianFirst0));

    /// <summary>The Tourian First room's door 1.</summary>
    public static DoorDefinition TourianFirst1 { get; } = DoorDefinitions.Find(nameof(TourianFirst1));

    /// <summary>The Tourian First room's door 3.</summary>
    public static DoorDefinition TourianFirst3 { get; } = DoorDefinitions.Find(nameof(TourianFirst3));

    /// <summary>The Metroids 1 room's door 0.</summary>
    public static DoorDefinition Metroids10 { get; } = DoorDefinitions.Find(nameof(Metroids10));

    /// <summary>The Metroids 1 room's door 1.</summary>
    public static DoorDefinition Metroids11 { get; } = DoorDefinitions.Find(nameof(Metroids11));

    /// <summary>The Metroids 2 room's door 0.</summary>
    public static DoorDefinition Metroids20 { get; } = DoorDefinitions.Find(nameof(Metroids20));

    /// <summary>The Metroids 2 room's door 1.</summary>
    public static DoorDefinition Metroids21 { get; } = DoorDefinitions.Find(nameof(Metroids21));

    /// <summary>The Metroids 3 room's door 0.</summary>
    public static DoorDefinition Metroids30 { get; } = DoorDefinitions.Find(nameof(Metroids30));

    /// <summary>The Metroids 3 room's door 1.</summary>
    public static DoorDefinition Metroids31 { get; } = DoorDefinitions.Find(nameof(Metroids31));

    /// <summary>The Metroids 4 room's door 0.</summary>
    public static DoorDefinition Metroids40 { get; } = DoorDefinitions.Find(nameof(Metroids40));

    /// <summary>The Metroids 4 room's door 1.</summary>
    public static DoorDefinition Metroids41 { get; } = DoorDefinitions.Find(nameof(Metroids41));

    /// <summary>The Blue Hopper room's door 0.</summary>
    public static DoorDefinition BlueHopper0 { get; } = DoorDefinitions.Find(nameof(BlueHopper0));

    /// <summary>The Blue Hopper room's door 1.</summary>
    public static DoorDefinition BlueHopper1 { get; } = DoorDefinitions.Find(nameof(BlueHopper1));

    /// <summary>The Dust Torizo room's door 0.</summary>
    public static DoorDefinition DustTorizo0 { get; } = DoorDefinitions.Find(nameof(DustTorizo0));

    /// <summary>The Dust Torizo room's door 1.</summary>
    public static DoorDefinition DustTorizo1 { get; } = DoorDefinitions.Find(nameof(DustTorizo1));

    /// <summary>The Big Boy room's door 0.</summary>
    public static DoorDefinition BigBoy0 { get; } = DoorDefinitions.Find(nameof(BigBoy0));

    /// <summary>The Big Boy room's door 1.</summary>
    public static DoorDefinition BigBoy1 { get; } = DoorDefinitions.Find(nameof(BigBoy1));

    /// <summary>The Seaweed room's door 0.</summary>
    public static DoorDefinition Seaweed0 { get; } = DoorDefinitions.Find(nameof(Seaweed0));

    /// <summary>The Seaweed room's door 1.</summary>
    public static DoorDefinition Seaweed1 { get; } = DoorDefinitions.Find(nameof(Seaweed1));

    /// <summary>The Seaweed room's door 2.</summary>
    public static DoorDefinition Seaweed2 { get; } = DoorDefinitions.Find(nameof(Seaweed2));

    /// <summary>The Tourian Recharge room's door 0.</summary>
    public static DoorDefinition TourianRecharge0 { get; } = DoorDefinitions.Find(nameof(TourianRecharge0));

    /// <summary>The Mother Brain room's door 0.</summary>
    public static DoorDefinition MotherBrain0 { get; } = DoorDefinitions.Find(nameof(MotherBrain0));

    /// <summary>The Mother Brain room's door 1.</summary>
    public static DoorDefinition MotherBrain1 { get; } = DoorDefinitions.Find(nameof(MotherBrain1));

    /// <summary>The Tourian Eye Door room's door 0.</summary>
    public static DoorDefinition TourianEyeDoor0 { get; } = DoorDefinitions.Find(nameof(TourianEyeDoor0));

    /// <summary>The Tourian Eye Door room's door 1.</summary>
    public static DoorDefinition TourianEyeDoor1 { get; } = DoorDefinitions.Find(nameof(TourianEyeDoor1));

    /// <summary>The Rinka Shaft room's door 0.</summary>
    public static DoorDefinition RinkaShaft0 { get; } = DoorDefinitions.Find(nameof(RinkaShaft0));

    /// <summary>The Rinka Shaft room's door 1.</summary>
    public static DoorDefinition RinkaShaft1 { get; } = DoorDefinitions.Find(nameof(RinkaShaft1));

    /// <summary>The Rinka Shaft room's door 2.</summary>
    public static DoorDefinition RinkaShaft2 { get; } = DoorDefinitions.Find(nameof(RinkaShaft2));

    /// <summary>The Mother Brain Save room's door 0.</summary>
    public static DoorDefinition MotherBrainSave0 { get; } = DoorDefinitions.Find(nameof(MotherBrainSave0));

    /// <summary>The Tourian Escape 1 room's door 0.</summary>
    public static DoorDefinition TourianEscape10 { get; } = DoorDefinitions.Find(nameof(TourianEscape10));

    /// <summary>The Tourian Escape 1 room's door 1.</summary>
    public static DoorDefinition TourianEscape11 { get; } = DoorDefinitions.Find(nameof(TourianEscape11));

    /// <summary>The Tourian Escape 2 room's door 0.</summary>
    public static DoorDefinition TourianEscape20 { get; } = DoorDefinitions.Find(nameof(TourianEscape20));

    /// <summary>The Tourian Escape 2 room's door 1.</summary>
    public static DoorDefinition TourianEscape21 { get; } = DoorDefinitions.Find(nameof(TourianEscape21));

    /// <summary>The Tourian Escape 3 room's door 0.</summary>
    public static DoorDefinition TourianEscape30 { get; } = DoorDefinitions.Find(nameof(TourianEscape30));

    /// <summary>The Tourian Escape 3 room's door 1.</summary>
    public static DoorDefinition TourianEscape31 { get; } = DoorDefinitions.Find(nameof(TourianEscape31));

    /// <summary>The Tourian Escape 4 room's door 0.</summary>
    public static DoorDefinition TourianEscape40 { get; } = DoorDefinitions.Find(nameof(TourianEscape40));

    /// <summary>The Tourian Escape 4 room's door 1.</summary>
    public static DoorDefinition TourianEscape41 { get; } = DoorDefinitions.Find(nameof(TourianEscape41));

    /// <summary>The Upper Tourian Save room's door 0.</summary>
    public static DoorDefinition UpperTourianSave0 { get; } = DoorDefinitions.Find(nameof(UpperTourianSave0));
}
