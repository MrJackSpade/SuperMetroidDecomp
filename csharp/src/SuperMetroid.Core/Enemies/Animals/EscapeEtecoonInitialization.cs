namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Etecoon's initial position, behavior, animation, and walking velocity (only its X is meaningful).</summary>
internal readonly record struct EscapeEtecoonInitialization(
    Position16<RoomPixels> Position,
    EscapeEtecoonPreInstruction PreInstruction,
    EscapeEtecoonProgram Program,
    EightEightVector<RoomPixels> Velocity);
