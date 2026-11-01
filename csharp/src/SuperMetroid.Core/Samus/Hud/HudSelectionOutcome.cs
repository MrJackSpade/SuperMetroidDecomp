namespace SuperMetroid.Core.Samus.Hud;

/// <summary>One frame of <c>HandleSwitchingHudSelection</c>.</summary>
/// <param name="ItemHandlerAccepted">
/// A cancel or select edge ran the switch loop, whose switched-to handler
/// accepted the now-selected item. This holds even when the selection lands back on the
/// same item, such as a cancel with nothing selected.
/// </param>
public readonly record struct HudSelectionOutcome(bool ItemHandlerAccepted);
