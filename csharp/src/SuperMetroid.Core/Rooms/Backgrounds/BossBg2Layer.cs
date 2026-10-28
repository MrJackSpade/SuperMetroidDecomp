namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>A boss body drawn on BG2 (#1352): the tilemap its body frame shows, at the BG2 scroll that places it.</summary>
/// <param name="Tilemap">The body frame's tilemap.</param>
/// <param name="Scroll">The BG2 scroll the boss computed from its body position and the camera.</param>
public sealed record BossBg2Layer(BossBg2Tilemap Tilemap, Position16<BackgroundPixels> Scroll);
