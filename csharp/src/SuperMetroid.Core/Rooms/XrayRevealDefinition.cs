namespace SuperMetroid.Core.Rooms;

/// <summary>Owned native reveal command and its row-major metatile arguments; extensions have no arguments.</summary>
public readonly record struct XrayRevealDefinition(XrayRevealCommand Command, ushort TopLeft,
    ushort TopRight, ushort BottomLeft, ushort BottomRight);
