namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Etecoon's initial position, action, animation, and the way and speed it walks (#2369).</summary>
internal readonly partial record struct EscapeEtecoonInitialization(
    Position16<RoomPixels> Position,
    EscapeEtecoonAction Action,
    EscapeEtecoonSegment Animation,
    HorizontalDirection Direction,
    SignedEightEight Speed);
