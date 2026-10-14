namespace SuperMetroid.Core.Game;

/// <summary>One random Zebes-escape explosion sprite and its optional sound.</summary>
internal readonly record struct ZebesEscapeExplosionDefinition(
    RoomSpriteObjectKind SpriteKind,
    byte SoundEffect);
