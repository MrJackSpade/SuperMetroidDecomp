namespace SuperMetroid.Core.Samus.Hud;

/// <summary>One frame of HUD selection switching.</summary>
/// <param name="ItemSelectionTookEffect">
/// A cancel or select edge ran the switch loop and the now-selected item took effect, which the input phase answers by clearing
/// the charge and restoring the suit's colours unless the Grapple Beam is selected while a grapple is active. This holds even when the selection lands back on the
/// same item, such as a cancel with nothing selected.
/// </param>
public readonly record struct HudSelectionOutcome(bool ItemSelectionTookEffect);
