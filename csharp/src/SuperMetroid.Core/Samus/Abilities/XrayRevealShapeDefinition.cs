using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One X-ray revealed-block shape: how many blocks it copies over the room's, and the area it is limited to.</summary>
/// <param name="CopiedBlocks">The blocks the shape copies over the room's; an extension copies none and reveals as the block it extends does.</param>
/// <param name="OnlyIn">The area whose rooms alone the shape reveals in, or none for a shape that reveals in every area.</param>
internal sealed record XrayRevealShapeDefinition(int CopiedBlocks, AreaId? OnlyIn = null);
