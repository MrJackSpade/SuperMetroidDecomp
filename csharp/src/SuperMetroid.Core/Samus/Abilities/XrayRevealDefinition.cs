namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>A reveal's shape and the blocks it shows.</summary>
/// <param name="Shape">The reveal's copy or extension shape.</param>
/// <param name="Blocks">The blocks a copy shape shows; none for an extension shape, which shows the block its chain reaches.</param>
public readonly record struct XrayRevealDefinition(XrayRevealShape Shape, XrayRevealVisualBlocks? Blocks);
