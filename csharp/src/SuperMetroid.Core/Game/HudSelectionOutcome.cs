using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>One frame of <c>HandleSwitchingHudSelection</c> ($90:C4B5).</summary>
/// <param name="ItemHandlerAccepted">
/// A cancel or select edge ran the switch loop, whose switched-to handler ($90:C545-$C5C3)
/// accepted the now-selected item. This holds even when the selection lands back on the
/// same item, such as a cancel with nothing selected.
/// </param>
public readonly record struct HudSelectionOutcome(bool ItemHandlerAccepted);
