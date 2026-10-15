namespace SuperMetroid.Core.Fx;

/// <summary>Signed PPU-scroll deltas produced by one accepted gameplay frame.</summary>
/// <param name="Bg1">BG1 scroll displacement.</param>
/// <param name="Bg2">BG2 scroll displacement.</param>
public readonly record struct RoomShakeFrameResult(
    Offset16<RoomPixels> Bg1,
    Offset16<Bg2Pixels> Bg2);
