namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Etecoon's initial position, action, animation, and horizontal walking velocity.</summary>
internal readonly record struct EscapeEtecoonInitialization(
    Position16<RoomPixels> Position,
    EscapeEtecoonAction Action,
    EscapeEtecoonSegment Animation,
    SignedEightEight Velocity);
