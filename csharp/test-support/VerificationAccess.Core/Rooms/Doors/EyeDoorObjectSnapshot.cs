using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification view of one of an eye door's three room objects: its eye, body or bottom.</summary>
/// <param name="Block">The room block the object stands at.</param>
/// <param name="RoomArgument">The object's room argument word, or null for a part whose record carries no argument.</param>
/// <param name="Component">Which of the door's objects this is.</param>
/// <param name="HitCounter">The eye's hits; zero for the body and bottom.</param>
/// <param name="Behaviour">The typed object.</param>
/// <param name="FrameBehavior">The native pre-instruction the object's wait stands for: the eye's missile wake, the part's opened-door wake, or the spawn's default.</param>
internal readonly record struct EyeDoorObjectSnapshot(
    Position16<RoomBlocks> Block,
    ushort? RoomArgument,
    EyeDoorComponent Component,
    byte HitCounter,
    RoomObjectBehaviour? Behaviour,
    ushort FrameBehavior);
