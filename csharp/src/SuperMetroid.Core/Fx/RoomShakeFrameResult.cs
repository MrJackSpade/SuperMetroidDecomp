namespace SuperMetroid.Core.Fx;

/// <summary>Signed layer-scroll deltas produced by one accepted gameplay frame.</summary>
/// <param name="Foreground">The foreground scroll displacement.</param>
/// <param name="Background">The background scroll displacement.</param>
public readonly record struct RoomShakeFrameResult(
    Offset16<RoomPixels> Foreground,
    Offset16<BackgroundPixels> Background);
