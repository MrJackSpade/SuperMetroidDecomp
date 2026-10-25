namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One step of a Chozo statue carrying Samus: the statue's velocity and where Samus is held relative to it.</summary>
/// <param name="Velocity">The statue's signed 8.8 velocity.</param>
/// <param name="SamusOffset">Where Samus is held, relative to the statue.</param>
public readonly record struct ChozoCarryStepDefinition(int Velocity, Offset16<RoomPixels> SamusOffset);
