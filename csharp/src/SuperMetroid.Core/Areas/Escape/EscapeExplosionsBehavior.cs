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
    /// <remarks>A random value whose remainder modulo 16 is eight or more leaves the inherited X selector unchanged; the caller may supply zero or a block offset.</remarks>
    public void SpawnEscapeExplosion(Position16<RoomPixels> position, int random, int inheritedX)
    {
        int choice = ZebesEscapeDefinitions.ExplosionChoice.Of(random);
        // Preserve the game's unchanged-X branch, not the upstream C rewrite's bugfix.
        if (choice < ZebesEscapeExplosionDefinitions.Choices.Length)
        {
            inheritedX = choice;
            if (ZebesEscapeExplosions.ForIndex(choice).SoundEffect is SoundEffectId sound)
                _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);
        }
        RoomSpriteObjectKind kind =
            ZebesEscapeExplosions.ForIndex(inheritedX & 7).SpriteKind;
        _spriteObjects.SpawnRoomSpriteObject(position, kind, default);
    }
}
