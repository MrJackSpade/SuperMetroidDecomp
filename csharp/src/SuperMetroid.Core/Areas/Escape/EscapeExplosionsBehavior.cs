using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Areas.Escape;

/// <summary>The escape explosions species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EscapeExplosionsBehavior
{
    /// <summary>Shared queue that applies native enemy sound limits to each explosion sound.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Room pool that owns the emitted explosion sprite objects.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;

    /// <summary>Connects the escape explosion publisher to the room's shared sound and sprite owners.</summary>
    internal EscapeExplosionsBehavior(EnemyAudioQueue audio, RoomSpriteObjectPool spriteObjects)
    {
        _audio = audio;
        _spriteObjects = spriteObjects;
    }

    /// <summary>Native $8F:C1A9 sprite/sound publication through the shared object pool.</summary>
    /// <remarks>A random low nibble of eight or more leaves the inherited X selector unchanged; the caller may supply zero or a block offset.</remarks>
    public void SpawnEscapeExplosion(Position16<RoomPixels> position, int random, ushort inheritedX)
    {
        int choice = random & 15;
        // Preserve the cartridge's unchanged-X branch, not the upstream C port's bugfix.
        if (choice < 8)
        {
            inheritedX = (ushort)choice;
            if (ZebesEscapeExplosionDefinitions.ForIndex(choice).SoundEffect is SoundEffectId sound)
                _audio.QueueEnemySound(sound, SoundQueueLimit.Max6);
        }
        RoomSpriteObjectKind kind =
            ZebesEscapeExplosionDefinitions.ForIndex(inheritedX & 7).SpriteKind;
        _spriteObjects.SpawnRoomSpriteObject(position, kind, 0);
    }
}
