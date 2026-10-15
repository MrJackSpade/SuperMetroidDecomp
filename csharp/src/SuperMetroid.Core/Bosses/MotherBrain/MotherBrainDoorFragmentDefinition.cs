namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Physical spawn and signed 8.8 velocity for one exploded escape-door fragment.</summary>
internal readonly record struct MotherBrainDoorFragmentDefinition(
    Offset16<RoomPixels> Offset,
    EightEightVector<RoomPixels> Velocity);
