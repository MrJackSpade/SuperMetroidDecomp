using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Escape;

/// <summary>One random Zebes-escape explosion sprite and its optional sound.</summary>
/// <param name="SpriteKind">The explosion sprite spawned.</param>
/// <param name="SoundEffect">The library-two sound queued with it, or none.</param>
internal readonly record struct ZebesEscapeExplosionDefinition(
    RoomSpriteObjectKind SpriteKind,
    SoundEffectId? SoundEffect);
