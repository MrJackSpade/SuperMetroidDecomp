namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where one exploded escape-door fragment spawns and how fast it flies.</summary>
public readonly partial record struct MotherBrainDoorFragmentDefinition(
    Offset16<RoomPixels> Offset,
    PixelVelocity<RoomPixels> Velocity);
