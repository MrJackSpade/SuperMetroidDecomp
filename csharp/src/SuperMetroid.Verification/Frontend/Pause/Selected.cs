using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>The selected category, selected item, equipped beams and collected beams of one selected.</summary>
/// <param name="SelectedCategory">The selected category.</param>
/// <param name="SelectedItem">The selected item.</param>
/// <param name="EquippedBeams">The equipped beams.</param>
/// <param name="CollectedBeams">The collected beams.</param>
internal readonly record struct Selected(PauseEquipmentCategory SelectedCategory, int SelectedItem, SamusBeamFlags EquippedBeams, SamusBeamFlags CollectedBeams);
