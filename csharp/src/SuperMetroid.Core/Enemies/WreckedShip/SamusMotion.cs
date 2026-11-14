namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>How Samus moved on each axis, which picks where the Wrecked Ship ghost appears around her (#1817).</summary>
/// <param name="Horizontal">How she moved left or right.</param>
/// <param name="Vertical">How she moved up or down.</param>
public readonly record struct SamusMotion(SamusAxisMotion Horizontal, SamusAxisMotion Vertical);
