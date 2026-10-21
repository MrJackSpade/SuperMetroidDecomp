using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's last displayed body frame and the tilemap resolved for it.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="Tilemap">The tilemap.</param>
internal readonly record struct KraidBodyTilemapCache(KraidBodyFrame Frame, BossBg2Tilemap Tilemap);
