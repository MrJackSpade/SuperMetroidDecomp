using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Areas.Escape;

/// <summary>The escape explosions species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EscapeExplosionsBehavior
{
    /// <summary>Shared queue that applies enemy sound limits to each explosion sound.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The room's sprite objects, which the explosions join.</summary>
    private readonly RoomSpriteObjects _spriteObjects;

    /// <summary>Connects the escape explosion publisher to the room's shared sound and sprite owners.</summary>
    internal EscapeExplosionsBehavior(EnemyAudioQueue audio, RoomSpriteObjects spriteObjects)
    {
        _audio = audio;
        _spriteObjects = spriteObjects;
    }

    /// <summary>Spawns an escape explosion's sprite object and requests its sound.</summary>
    /// <param name="position">Where the explosion goes off.</param>
    /// <param name="random">The random sample that draws the explosion.</param>
    /// <param name="kindLeftOpen">The kind the explosion's place gives it when the draw names no explosion; such an explosion makes no sound.</param>
    public void SpawnEscapeExplosion(Position16<RoomPixels> position, int random, RoomSpriteObjectKind kindLeftOpen)
    {
        RoomSpriteObjectKind kind = kindLeftOpen;
        if (ZebesEscapeExplosions.Drawn(random) is { } drawn)
        {
            kind = drawn.SpriteKind;
            if (drawn.SoundEffect is SoundEffectId sound)
                _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);
        }
        _spriteObjects.SpawnRoomSpriteObject(position, kind, default);
    }
}
