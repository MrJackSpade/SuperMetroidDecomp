namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Wrecked Ship rooms' doors, named by the room they lead from.</summary>
[NumberedIdentities("Each numbered door is one door of its room, named by its place in that room's door list; every door has its own destination and is used by name, never iterated or indexed.")]
public static class WreckedShipDoorDefinitions
{
    /// <summary>The Bowling Alley room's door 0.</summary>
    public static DoorDefinition BowlingAlley0 { get; } = DoorDefinitions.Find(nameof(BowlingAlley0));

    /// <summary>The Bowling Alley room's door 1.</summary>
    public static DoorDefinition BowlingAlley1 { get; } = DoorDefinitions.Find(nameof(BowlingAlley1));

    /// <summary>The Bowling Alley room's door 2.</summary>
    public static DoorDefinition BowlingAlley2 { get; } = DoorDefinitions.Find(nameof(BowlingAlley2));

    /// <summary>The Wrecked Ship Entrance room's door 0.</summary>
    public static DoorDefinition WreckedShipEntrance0 { get; } = DoorDefinitions.Find(nameof(WreckedShipEntrance0));

    /// <summary>The Wrecked Ship Entrance room's door 1.</summary>
    public static DoorDefinition WreckedShipEntrance1 { get; } = DoorDefinitions.Find(nameof(WreckedShipEntrance1));

    /// <summary>The Attic room's door 0.</summary>
    public static DoorDefinition Attic0 { get; } = DoorDefinitions.Find(nameof(Attic0));

    /// <summary>The Attic room's door 1.</summary>
    public static DoorDefinition Attic1 { get; } = DoorDefinitions.Find(nameof(Attic1));

    /// <summary>The Attic room's door 2.</summary>
    public static DoorDefinition Attic2 { get; } = DoorDefinitions.Find(nameof(Attic2));

    /// <summary>Wrecked Ship's Assembly Line room's door 0.</summary>
    public static DoorDefinition AssemblyLine0 { get; } = DoorDefinitions.Find(nameof(AssemblyLine0));

    /// <summary>The Wrecked Ship Main Shaft room's door 0.</summary>
    public static DoorDefinition WreckedShipMainShaft0 { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaft0));

    /// <summary>The Wrecked Ship Main Shaft room's door 1.</summary>
    public static DoorDefinition WreckedShipMainShaft1 { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaft1));

    /// <summary>The Wrecked Ship Main Shaft room's door 2.</summary>
    public static DoorDefinition WreckedShipMainShaft2 { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaft2));

    /// <summary>The Wrecked Ship Main Shaft room's door 3.</summary>
    public static DoorDefinition WreckedShipMainShaft3 { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaft3));

    /// <summary>The Wrecked Ship Main Shaft room's door 4.</summary>
    public static DoorDefinition WreckedShipMainShaft4 { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaft4));

    /// <summary>The Wrecked Ship Main Shaft room's door 5.</summary>
    public static DoorDefinition WreckedShipMainShaft5 { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaft5));

    /// <summary>The Wrecked Ship Main Shaft room's door 6.</summary>
    public static DoorDefinition WreckedShipMainShaft6 { get; } = DoorDefinitions.Find(nameof(WreckedShipMainShaft6));

    /// <summary>The Spiky Death room's door 0.</summary>
    public static DoorDefinition SpikyDeath0 { get; } = DoorDefinitions.Find(nameof(SpikyDeath0));

    /// <summary>The Spiky Death room's door 1.</summary>
    public static DoorDefinition SpikyDeath1 { get; } = DoorDefinitions.Find(nameof(SpikyDeath1));

    /// <summary>The Electric Death room's door 0.</summary>
    public static DoorDefinition ElectricDeath0 { get; } = DoorDefinitions.Find(nameof(ElectricDeath0));

    /// <summary>The Electric Death room's door 1.</summary>
    public static DoorDefinition ElectricDeath1 { get; } = DoorDefinitions.Find(nameof(ElectricDeath1));

    /// <summary>The Electric Death room's door 2.</summary>
    public static DoorDefinition ElectricDeath2 { get; } = DoorDefinitions.Find(nameof(ElectricDeath2));

    /// <summary>The Wrecked Ship Energy Tank room's door 0.</summary>
    public static DoorDefinition WreckedShipETank0 { get; } = DoorDefinitions.Find(nameof(WreckedShipETank0));

    /// <summary>The Basement room's door 0.</summary>
    public static DoorDefinition Basement0 { get; } = DoorDefinitions.Find(nameof(Basement0));

    /// <summary>The Basement room's door 1.</summary>
    public static DoorDefinition Basement1 { get; } = DoorDefinitions.Find(nameof(Basement1));

    /// <summary>The Basement room's door 2.</summary>
    public static DoorDefinition Basement2 { get; } = DoorDefinitions.Find(nameof(Basement2));

    /// <summary>The Wrecked Ship Map room's door 0.</summary>
    public static DoorDefinition WreckedShipMap0 { get; } = DoorDefinitions.Find(nameof(WreckedShipMap0));

    /// <summary>The Phantoon room's door 0.</summary>
    public static DoorDefinition Phantoon0 { get; } = DoorDefinitions.Find(nameof(Phantoon0));

    /// <summary>The Sponge Bath room's door 0.</summary>
    public static DoorDefinition SpongeBath0 { get; } = DoorDefinitions.Find(nameof(SpongeBath0));

    /// <summary>The Sponge Bath room's door 1.</summary>
    public static DoorDefinition SpongeBath1 { get; } = DoorDefinitions.Find(nameof(SpongeBath1));

    /// <summary>The Wrecked Ship West Super room's door 0.</summary>
    public static DoorDefinition WSWestSuper0 { get; } = DoorDefinitions.Find(nameof(WSWestSuper0));

    /// <summary>The Wrecked Ship East Super room's door 0.</summary>
    public static DoorDefinition WSEastSuper0 { get; } = DoorDefinitions.Find(nameof(WSEastSuper0));

    /// <summary>The Gravity Suit room's door 0.</summary>
    public static DoorDefinition GravitySuit0 { get; } = DoorDefinitions.Find(nameof(GravitySuit0));

    /// <summary>The Gravity Suit room's door 1.</summary>
    public static DoorDefinition GravitySuit1 { get; } = DoorDefinitions.Find(nameof(GravitySuit1));

    /// <summary>The Wrecked Ship Save room's door 0.</summary>
    public static DoorDefinition WreckedShipSave0 { get; } = DoorDefinitions.Find(nameof(WreckedShipSave0));
}
