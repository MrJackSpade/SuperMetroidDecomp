namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>A block the x-ray extension walk has reached, with the direction and step it reached it in.</summary>
/// <param name="Block">The block reached.</param>
/// <param name="Vertical">Whether the walk was moving vertically.</param>
/// <param name="Step">The signed step the walk was taking.</param>
internal readonly record struct XrayRevealVisit(Position16<RoomBlocks> Block, bool Vertical, int Step);
