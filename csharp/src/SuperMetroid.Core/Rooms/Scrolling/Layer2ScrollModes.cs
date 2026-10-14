namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The room state header's per-axis layer-2 scroll modes: zero copies
/// layer one, one fixes the axis, and other values are parallax multipliers over 128 whose
/// odd bit suppresses dynamic background streaming on that axis.
/// </summary>
/// <param name="Horizontal">The horizontal-axis mode.</param>
/// <param name="Vertical">The vertical-axis mode.</param>
[SpatialComponents("Spatial value type: one layer-2 parallax mode per axis, as the room header stores them.")]
public readonly record struct Layer2ScrollModes(byte Horizontal, byte Vertical);
