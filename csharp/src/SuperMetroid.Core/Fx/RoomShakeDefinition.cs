namespace SuperMetroid.Core.Fx;

/// <summary>One cartridge earthquake type's background and enemy-projectile displacement.</summary>
/// <param name="Bg1">BG1 scroll displacement.</param>
/// <param name="Bg2">BG2 scroll displacement.</param>
/// <param name="Projectile">Enemy-projectile draw displacement.</param>
internal readonly record struct RoomShakeDefinition(
    Offset16<RoomPixels> Bg1, Offset16<Bg2Pixels> Bg2, Offset16<RoomPixels> Projectile);
