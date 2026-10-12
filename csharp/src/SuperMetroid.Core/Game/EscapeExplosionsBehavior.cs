namespace SuperMetroid.Core.Game;

/// <summary>The escape explosions species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class EscapeExplosionsBehavior
{
    /// <summary>Stores audio.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Stores sprite objects.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;

    /// <summary>Initializes a new escape explosions behavior instance.</summary>
    internal EscapeExplosionsBehavior(EnemyAudioQueue audio, RoomSpriteObjectPool spriteObjects)
    {
        _audio = audio;
        _spriteObjects = spriteObjects;
    }

    /// <summary>Native $8F:C1A9 sprite/sound publication through the shared object pool.</summary>
    public void SpawnEscapeExplosion(Position16<RoomPixels> position, ushort random, ushort inheritedX)
    {
        int choice = random & 15;
        // The cartridge leaves X unchanged when the random nibble is 8..15. The
        // fourth-frame caller inherits zero; the nonblank caller leaves its block offset.
        // Preserve this quirk rather than importing the upstream C port's bugfix.
        if (choice < 8)
        {
            inheritedX = (ushort)choice;
            byte sound = ZebesEscapeExplosionDefinitions.ForIndex(choice).SoundEffect;
            if (sound != 0)
                _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, sound), 6);
        }
        RoomSpriteObjectKind kind =
            ZebesEscapeExplosionDefinitions.ForIndex(inheritedX & 7).SpriteKind;
        _spriteObjects.SpawnRoomSpriteObject(position, kind, 0);
    }
}
