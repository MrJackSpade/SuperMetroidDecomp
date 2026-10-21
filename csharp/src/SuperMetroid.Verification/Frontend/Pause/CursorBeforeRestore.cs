using SuperMetroid.Core.Frontend.Pause;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>The selected category and selected item of one cursor before restore.</summary>
/// <param name="SelectedCategory">The selected category.</param>
/// <param name="SelectedItem">The selected item.</param>
internal readonly record struct CursorBeforeRestore(PauseEquipmentCategory SelectedCategory, int SelectedItem);
