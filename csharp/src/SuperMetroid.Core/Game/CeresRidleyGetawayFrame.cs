namespace SuperMetroid.Core.Game;

/// <summary>
/// One authored Mode 7 getaway frame. <paramref name="Velocity"/> holds the native signed words
/// of the $A6:B00F X table (subtracted from the horizontal offset, so the image moves right) and
/// the $A6:AF2F Y table (added to the vertical offset).
/// </summary>
public readonly record struct CeresRidleyGetawayFrame(ushort Zoom, Offset16<Mode7Pixels> Velocity);
