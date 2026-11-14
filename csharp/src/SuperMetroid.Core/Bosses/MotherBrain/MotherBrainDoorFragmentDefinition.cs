namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Physical spawn and signed 1/256-pixel velocity for one exploded escape-door fragment.</summary>
public readonly record struct MotherBrainDoorFragmentDefinition(
    Offset16<RoomPixels> Offset,
    EightEightVector<RoomPixels> Velocity);
