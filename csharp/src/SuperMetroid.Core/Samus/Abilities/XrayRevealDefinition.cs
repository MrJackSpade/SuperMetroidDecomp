namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Owned native reveal command and its row-major metatile arguments; extensions have no arguments.</summary>
public readonly record struct XrayRevealDefinition(XrayRevealCommand Command, int TopLeft,
    int TopRight, int BottomLeft, int BottomRight);
