namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Etecoon's initial position, behavior, animation, and speed.</summary>
internal readonly record struct EscapeEtecoonInitialization(
    Position16<RoomPixels> Position,
    EscapeEtecoonPreInstruction PreInstruction,
    ushort InstructionList,
    ushort HorizontalSpeed);
