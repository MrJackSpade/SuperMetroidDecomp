namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>One eight-byte row from the ROM table at <c>$88:AEC1</c>.</summary>
public readonly record struct SkyScrollSection(
    ushort TopPosition,
    ushort Subspeed,
    ushort Speed,
    int DataSlot);
