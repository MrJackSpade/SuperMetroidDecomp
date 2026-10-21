using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>The equipped items, collected items, equipped beams and collected beams of one inventory.</summary>
/// <param name="EquippedItems">The equipped items.</param>
/// <param name="CollectedItems">The collected items.</param>
/// <param name="EquippedBeams">The equipped beams.</param>
/// <param name="CollectedBeams">The collected beams.</param>
internal readonly record struct Inventory(SamusEquipmentFlags EquippedItems, SamusEquipmentFlags CollectedItems, SamusBeamFlags EquippedBeams, SamusBeamFlags CollectedBeams);
