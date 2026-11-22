using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Escape;

/// <summary>One random Zebes-escape explosion sprite and its optional sound.</summary>
/// <param name="SpriteKind">The explosion sprite spawned.</param>
/// <param name="SoundEffect">The enemy-library sound queued with it, or none.</param>
internal readonly partial record struct ZebesEscapeExplosionDefinition(
    RoomEffectActorKind SpriteKind,
    SoundEffectId? SoundEffect);
