namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Tourian rooms' doors, named by the room they lead from.</summary>
[NumberedIdentities("Each numbered door is one door of its room, named by its place in that room's door list; every door has its own destination and is used by name, never iterated or indexed.")]
public static class TourianDoorDefinitions
{
    /// <summary>The <c>Door_TourianFirst_0</c> door.</summary>
    public static CartridgeDoorHeader TourianFirst0 { get; } = DoorDefinitions.Find(nameof(TourianFirst0));

    /// <summary>The <c>Door_TourianFirst_1</c> door.</summary>
    public static CartridgeDoorHeader TourianFirst1 { get; } = DoorDefinitions.Find(nameof(TourianFirst1));

    /// <summary>The <c>Door_TourianFirst_3</c> door.</summary>
    public static CartridgeDoorHeader TourianFirst3 { get; } = DoorDefinitions.Find(nameof(TourianFirst3));

    /// <summary>The <c>Door_Metroids1_0</c> door.</summary>
    public static CartridgeDoorHeader Metroids10 { get; } = DoorDefinitions.Find(nameof(Metroids10));

    /// <summary>The <c>Door_Metroids1_1</c> door.</summary>
    public static CartridgeDoorHeader Metroids11 { get; } = DoorDefinitions.Find(nameof(Metroids11));

    /// <summary>The <c>Door_Metroids2_0</c> door.</summary>
    public static CartridgeDoorHeader Metroids20 { get; } = DoorDefinitions.Find(nameof(Metroids20));

    /// <summary>The <c>Door_Metroids2_1</c> door.</summary>
    public static CartridgeDoorHeader Metroids21 { get; } = DoorDefinitions.Find(nameof(Metroids21));

    /// <summary>The <c>Door_Metroids3_0</c> door.</summary>
    public static CartridgeDoorHeader Metroids30 { get; } = DoorDefinitions.Find(nameof(Metroids30));

    /// <summary>The <c>Door_Metroids3_1</c> door.</summary>
    public static CartridgeDoorHeader Metroids31 { get; } = DoorDefinitions.Find(nameof(Metroids31));

    /// <summary>The <c>Door_Metroids4_0</c> door.</summary>
    public static CartridgeDoorHeader Metroids40 { get; } = DoorDefinitions.Find(nameof(Metroids40));

    /// <summary>The <c>Door_Metroids4_1</c> door.</summary>
    public static CartridgeDoorHeader Metroids41 { get; } = DoorDefinitions.Find(nameof(Metroids41));

    /// <summary>The <c>Door_BlueHopper_0</c> door.</summary>
    public static CartridgeDoorHeader BlueHopper0 { get; } = DoorDefinitions.Find(nameof(BlueHopper0));

    /// <summary>The <c>Door_BlueHopper_1</c> door.</summary>
    public static CartridgeDoorHeader BlueHopper1 { get; } = DoorDefinitions.Find(nameof(BlueHopper1));

    /// <summary>The <c>Door_DustTorizo_0</c> door.</summary>
    public static CartridgeDoorHeader DustTorizo0 { get; } = DoorDefinitions.Find(nameof(DustTorizo0));

    /// <summary>The <c>Door_DustTorizo_1</c> door.</summary>
    public static CartridgeDoorHeader DustTorizo1 { get; } = DoorDefinitions.Find(nameof(DustTorizo1));

    /// <summary>The <c>Door_BigBoy_0</c> door.</summary>
    public static CartridgeDoorHeader BigBoy0 { get; } = DoorDefinitions.Find(nameof(BigBoy0));

    /// <summary>The <c>Door_BigBoy_1</c> door.</summary>
    public static CartridgeDoorHeader BigBoy1 { get; } = DoorDefinitions.Find(nameof(BigBoy1));

    /// <summary>The <c>Door_Seaweed_0</c> door.</summary>
    public static CartridgeDoorHeader Seaweed0 { get; } = DoorDefinitions.Find(nameof(Seaweed0));

    /// <summary>The <c>Door_Seaweed_1</c> door.</summary>
    public static CartridgeDoorHeader Seaweed1 { get; } = DoorDefinitions.Find(nameof(Seaweed1));

    /// <summary>The <c>Door_Seaweed_2</c> door.</summary>
    public static CartridgeDoorHeader Seaweed2 { get; } = DoorDefinitions.Find(nameof(Seaweed2));

    /// <summary>The <c>Door_TourianRecharge_0</c> door.</summary>
    public static CartridgeDoorHeader TourianRecharge0 { get; } = DoorDefinitions.Find(nameof(TourianRecharge0));

    /// <summary>The <c>Door_MotherBrain_0</c> door.</summary>
    public static CartridgeDoorHeader MotherBrain0 { get; } = DoorDefinitions.Find(nameof(MotherBrain0));

    /// <summary>The <c>Door_MotherBrain_1</c> door.</summary>
    public static CartridgeDoorHeader MotherBrain1 { get; } = DoorDefinitions.Find(nameof(MotherBrain1));

    /// <summary>The <c>Door_TourianEyeDoor_0</c> door.</summary>
    public static CartridgeDoorHeader TourianEyeDoor0 { get; } = DoorDefinitions.Find(nameof(TourianEyeDoor0));

    /// <summary>The <c>Door_TourianEyeDoor_1</c> door.</summary>
    public static CartridgeDoorHeader TourianEyeDoor1 { get; } = DoorDefinitions.Find(nameof(TourianEyeDoor1));

    /// <summary>The <c>Door_RinkaShaft_0</c> door.</summary>
    public static CartridgeDoorHeader RinkaShaft0 { get; } = DoorDefinitions.Find(nameof(RinkaShaft0));

    /// <summary>The <c>Door_RinkaShaft_1</c> door.</summary>
    public static CartridgeDoorHeader RinkaShaft1 { get; } = DoorDefinitions.Find(nameof(RinkaShaft1));

    /// <summary>The <c>Door_RinkaShaft_2</c> door.</summary>
    public static CartridgeDoorHeader RinkaShaft2 { get; } = DoorDefinitions.Find(nameof(RinkaShaft2));

    /// <summary>The <c>Door_MotherBrainSave_0</c> door.</summary>
    public static CartridgeDoorHeader MotherBrainSave0 { get; } = DoorDefinitions.Find(nameof(MotherBrainSave0));

    /// <summary>The <c>Door_TourianEscape1_0</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape10 { get; } = DoorDefinitions.Find(nameof(TourianEscape10));

    /// <summary>The <c>Door_TourianEscape1_1</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape11 { get; } = DoorDefinitions.Find(nameof(TourianEscape11));

    /// <summary>The <c>Door_TourianEscape2_0</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape20 { get; } = DoorDefinitions.Find(nameof(TourianEscape20));

    /// <summary>The <c>Door_TourianEscape2_1</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape21 { get; } = DoorDefinitions.Find(nameof(TourianEscape21));

    /// <summary>The <c>Door_TourianEscape3_0</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape30 { get; } = DoorDefinitions.Find(nameof(TourianEscape30));

    /// <summary>The <c>Door_TourianEscape3_1</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape31 { get; } = DoorDefinitions.Find(nameof(TourianEscape31));

    /// <summary>The <c>Door_TourianEscape4_0</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape40 { get; } = DoorDefinitions.Find(nameof(TourianEscape40));

    /// <summary>The <c>Door_TourianEscape4_1</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape41 { get; } = DoorDefinitions.Find(nameof(TourianEscape41));

    /// <summary>The <c>Door_UpperTourianSave_0</c> door.</summary>
    public static CartridgeDoorHeader UpperTourianSave0 { get; } = DoorDefinitions.Find(nameof(UpperTourianSave0));
}
