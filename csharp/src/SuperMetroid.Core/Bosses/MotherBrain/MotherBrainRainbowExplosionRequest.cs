namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One explosion the rainbow beam requests, at its offset from the beam.</summary>
public readonly partial record struct MotherBrainRainbowExplosionRequest(
    Offset16<RoomPixels> Offset);
