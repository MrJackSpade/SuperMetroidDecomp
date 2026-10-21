using SuperMetroid.Core.Frontend.Pause;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>The screen mode, selected category, selected item and map scroll of one pause backdrops selection.</summary>
/// <param name="ScreenMode">The screen mode.</param>
/// <param name="SelectedCategory">The selected category.</param>
/// <param name="SelectedItem">The selected item.</param>
/// <param name="MapScroll">The map scroll.</param>
internal readonly record struct PauseBackdropsSelection(PauseScreenMode ScreenMode, PauseEquipmentCategory SelectedCategory, int SelectedItem, Position16<MapPixels> MapScroll);
