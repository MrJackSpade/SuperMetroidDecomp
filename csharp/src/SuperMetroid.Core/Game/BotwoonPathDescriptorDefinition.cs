namespace SuperMetroid.Core.Game;

/// <summary>One authored Botwoon movement stream, traversal direction, and destination hole.</summary>
internal readonly record struct BotwoonPathDescriptorDefinition(
    ushort PathPointer,
    short Direction,
    ushort TargetHoleByteOffset);
