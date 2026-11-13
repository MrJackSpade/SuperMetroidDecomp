namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Owned reveal shape and its row-major blocks; extensions have no blocks.</summary>
public readonly record struct XrayRevealDefinition(XrayRevealShape Shape, int TopLeft,
    int TopRight, int BottomLeft, int BottomRight);
